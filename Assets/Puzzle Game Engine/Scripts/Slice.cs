using System.Collections;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
public class Slice : MonoBehaviour
{
    private bool isMoving = false;

    public bool IsMoving()
    {
        return isMoving;
    }

    float movementSpeed = 4f;
    float rotationSpeed = 5f;

    Transform parentAtStartOfJump;

    public void MoveSlice(Transform targetSlice, Vector3 startPos, Quaternion startRot)
    {
        parentAtStartOfJump = targetSlice.transform.parent.parent;
        targetSlice.GetComponent<Slice>().DisableSlice();
        StartCoroutine(Move(startPos, startRot));
    }

    IEnumerator Move(Vector3 startPos, Quaternion startRot)
    {
        isMoving = true;

        Vector3 endPos = transform.position;
        Quaternion endRot = transform.rotation;

        transform.position = startPos;
        transform.rotation = startRot;

        while (Vector3.Distance(transform.position, endPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, movementSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, endRot, rotationSpeed * Time.deltaTime);

            yield return new WaitForEndOfFrame();
        }

        transform.position = endPos;
        transform.rotation = endRot;

        isMoving = false;

        if (GetComponentInParent<SoundsManagerForTemplate>() != null)
            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Slices_PieceMoved();

        CheckIfStackHasMatchingColors();

        ReCheckNeighbours();
    }

    void ReCheckNeighbours()
    {
        //At the end of jump, re-check neighbours - ONLY IF THIS IS THE LATEST JUMPER -
        if ((transform.parent.childCount - 1) == transform.GetSiblingIndex())
        {
            if (parentAtStartOfJump != null)
            {
                Debug.Log("Re-Checking parent neighbours: P1-" + parentAtStartOfJump.name);

                if (parentAtStartOfJump.GetComponentInParent<CheckNeighbours>() != null)
                    parentAtStartOfJump.GetComponentInParent<CheckNeighbours>().StartCheckNeighbourColorsCoroutine();
            }

            if (transform.parent != null)
            {
                Debug.Log("Re-Checking parent neighbours: P2-" + transform.parent.name);
                transform.parent.GetComponentInParent<CheckNeighbours>().StartCheckNeighbourColorsCoroutine();
            }
        }
    }

    void CheckIfStackHasMatchingColors()
    {
        Debug.Log("Checking Matching Colors: P2-" + transform.parent.name);

        if (parentAtStartOfJump != null && !parentAtStartOfJump.GetComponentInParent<CheckNeighbours>().HasSlices())
            parentAtStartOfJump.GetComponentInParent<CheckNeighbours>().transform.GetChild(0).gameObject.AddComponent<ScaleDownAndDestroy>();

        transform.parent.GetComponentInParent<CheckNeighbours>().StartCheckMatchingSlicesInStack();
    }

    public void DisableSlice()
    {
        transform.parent.gameObject.SetActive(false);
        
        // Kiểm tra điều kiện thua sau khi ẩn block
        LevelManager levelManager = GetComponentInParent<LevelManager>();
        if (levelManager != null)
        {
            levelManager.CheckForHiddenBlocksLossCondition();
        }
    }
}
}
