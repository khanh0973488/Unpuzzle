using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class Screw : MonoBehaviour
{
    private Vector3 targetPosLevitate = new Vector3(0f, 0.13f, -0.4f);
    private Vector3 startPos;
    private Quaternion startRot;

    float rotSpeed1 = 630f;
    float rotSpeed2 = 165f;

    bool isScrewLevitating = false;

    ScrewManager currentScrewManager = null;

    ShowcaseParent showcaseParent = null;

    private void Start()
    {
        showcaseParent = GetComponentInParent<ShowcaseParent>();
    }

    public void SetScrewManager(ScrewManager newScrewManager)
    {
        currentScrewManager = newScrewManager;
    }

    public ScrewManager GetCurrentScrewManager()
    {
        return currentScrewManager;
    }

    public void MoveAboveHole(Transform holeTransform)
    {
        StopAllCoroutines();

        transform.parent = holeTransform.parent;
        startPos = holeTransform.position;
        transform.position = startPos + targetPosLevitate;
        Vector3 targetPos = transform.position - targetPosLevitate;

        ChangeLevitation();
    }

    public bool ChangeLevitation(bool isReturningToSamePosition = false)
    {
        transform.parent = showcaseParent.transform;

        if (isScrewLevitating)
        {
            isScrewLevitating = false;

            StopAllCoroutines();

            StartCoroutine(MoveAndRotateIn(isReturningToSamePosition));
            return true;
        }
        else
        {
            isScrewLevitating = true;

            if (GetCurrentScrewManager() != null)
                GetCurrentScrewManager().ReplaceToDummyScrew(this);
            else
                SpawnDummyScrew();

            StopAllCoroutines();

            StartCoroutine(MoveAndRotateOut());
            return false;
        }
    }

    #region Dummy Screw Management

    GameObject currentDummyScrewObject = null;

    void SpawnDummyScrew()
    {
        TryToDestroyDummyScrew();

        currentDummyScrewObject = Instantiate(new GameObject("DummyScrew"), transform);
        currentDummyScrewObject.transform.parent = null;
        currentDummyScrewObject.AddComponent<Rigidbody>().isKinematic = true;
        currentDummyScrewObject.AddComponent<CapsuleCollider>();

        CapsuleCollider dummyCollider = currentDummyScrewObject.GetComponent<CapsuleCollider>();
        CapsuleCollider thisCollider = GetComponent<CapsuleCollider>();
        dummyCollider.isTrigger = false;
        dummyCollider.center = thisCollider.center;
        dummyCollider.radius = thisCollider.radius;
        dummyCollider.height = thisCollider.height;
        dummyCollider.direction = thisCollider.direction;
    }

    void TryToDestroyDummyScrew()
    {
        if (currentDummyScrewObject != null)
        {
            Destroy(currentDummyScrewObject);
            currentDummyScrewObject = null;
        }
    }

    #endregion

    IEnumerator MoveAndRotateOut()
    {
        GetComponent<Collider>().enabled = false;

        startPos = transform.position;
        startRot = transform.rotation;
        Vector3 targetPos = transform.position + targetPosLevitate;

        while (isScrewLevitating)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 1.8f * Time.deltaTime);
            transform.Rotate(Vector3.up, rotSpeed1 * Time.deltaTime, Space.Self);
            transform.Rotate(Vector3.right, rotSpeed2 * Time.deltaTime, Space.World);

            if (Vector3.Distance(transform.position, targetPos) < 0.001f)
                break;

            yield return new WaitForEndOfFrame();
        }

        transform.position = targetPos;

        GetComponent<Collider>().enabled = true;
    }

    IEnumerator MoveAndRotateIn(bool isReturningToSamePosition = false)
    {
        GetComponent<Collider>().enabled = false;

        Vector3 targetPos = startPos;

        if (!isScrewLevitating)
            targetPos = transform.position - targetPosLevitate;

        while (!isScrewLevitating)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 1.8f * Time.deltaTime);
            transform.Rotate(Vector3.up, -rotSpeed1 * Time.deltaTime, Space.Self);
            transform.Rotate(Vector3.right, -rotSpeed2 * Time.deltaTime, Space.World);

            if (Vector3.Distance(transform.position, targetPos) < 0.001f)
                break;

            yield return new WaitForEndOfFrame();
        }

        transform.position = targetPos;
        transform.rotation = startRot;

        if (isReturningToSamePosition && currentScrewManager != null)
            currentScrewManager.UndoDummyScrewReplace();
        else if (!isReturningToSamePosition && currentScrewManager != null)
            currentScrewManager.RemoveScrew(gameObject);

        GetComponent<Collider>().enabled = true;
        TryToDestroyDummyScrew();
    }
}
}