using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class BoltMover : MonoBehaviour
    {
        public GameObject boxBlockPrefab;
        private bool canClickOnBolt = true;
        Vector3 startPos;
        Transform boltEnd;
        Transform boltStart;

        private void Start()
        {
            startPos = transform.position;

            boltEnd = transform.Find("BoltEnd");
            boltStart = transform.Find("BoltStart");

            //boltStart.GetComponent<MeshRenderer>().enabled = false;
            //boltEnd.GetComponent<MeshRenderer>().enabled = false;

            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<CapsuleCollider>().isTrigger = true;

            Invoke("ResetCollider", 0.1f);
        }

        private void OnEnable()
        {
            boltEnd = transform.Find("BoltEnd");
            boltStart = transform.Find("BoltStart");

            //boltStart.GetComponent<MeshRenderer>().enabled = false;
            //boltEnd.GetComponent<MeshRenderer>().enabled = false;
        }

        private bool canSpawnBoxBlock = false;
        private bool canSpawnBoxBlockAfterStart = true;

        private void ResetCollider()
        {
            GetComponent<CapsuleCollider>().enabled = true;
            Invoke("ResetCollider2", 0.1f);
        }

        private void ResetCollider2()
        {
            GetComponent<CapsuleCollider>().isTrigger = false;
            Invoke("StopSpawningBoxBlock", 0.1f);
        }

        private void StopSpawningBoxBlock()
        {
            canSpawnBoxBlockAfterStart = false;
        }

        private void LateUpdate()
        {
            if (canClickOnBolt) return;

            Ray ray = new Ray(boltStart.position, boltStart.forward);
            RaycastHit hitInfo;

            float rayLength = .28f;

            if (Physics.Raycast(ray, out hitInfo, rayLength /*, mask, QueryTriggerInteraction.Ignore*/))
            {
                string hitObjectName = hitInfo.collider.gameObject.name.ToString().ToLower();
                if (hitObjectName.Contains("bolt") ||
                    hitObjectName.Contains("nut") ||
                    hitObjectName.Contains("block"))
                {
                    StopAllCoroutines();
                    Debug.Log("Colliding_V3_So_Start_Moving_Back");
                    StartCoroutine(MoveBackToStartingPos());

                    Debug.DrawLine(ray.origin, hitInfo.point, Color.red);
                }
            }
            else
            {
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayLength, Color.green);
            }

        }

        public void TryToRemoveBolt()
        {
            if (SkillButton.currentlyUsingSkill != null &&
        (SkillButton.currentlyUsingSkill.skillType == SkillButton.SkillType.NutsRemoveBolts))
            {
                foreach (NutMoveOnBolt nut in GetComponentsInChildren<NutMoveOnBolt>())
                    nut.Explode();

                SkillButton.currentlyUsingSkill.RemovedBolt();
                StartCoroutine(RemoveBolt());
                return;
            }

            if (!canClickOnBolt)
                return;

            canClickOnBolt = false;

            if (transform.GetComponentInChildren<NutMoveOnBolt>() == null)
                StartCoroutine(RemoveBolt());
            else
                StartCoroutine(FailRemovingBolt());
        }

        IEnumerator RemoveBolt()
        {
            foreach (ScrewBox screwBox in FindObjectsOfType<ScrewBox>(false))
                screwBox.RemoveHoldingBolt(this);

            //GetComponentInParent<CollectedStacksCounter>().IncreaseCollectedPieces();

            while (true)
            {
                transform.Translate(Vector3.forward * 8f * Time.deltaTime, Space.Self);

                yield return null;
            }

            canClickOnBolt = true;
        }

        IEnumerator FailRemovingBolt()
        {
            Vector3 targetPos = transform.position + transform.forward * 1f;

            float previousDistance = 1000f;

            while (Vector3.Distance(transform.position, targetPos) > 0.1f && !canMoveBack)
            {
                if (Vector3.Distance(transform.position, targetPos) > previousDistance)
                    break;
                else
                {
                    previousDistance = Vector3.Distance(transform.position, targetPos);
                    Debug.Log("DIST: " + Vector3.Distance(transform.position, targetPos));

                    transform.Translate(Vector3.forward * 4f * Time.deltaTime, Space.Self);

                    yield return null;
                }
            }

            StartCoroutine(MoveBackToStartingPos());
        }

        bool isMovingBack = false;

        IEnumerator MoveBackToStartingPos()
        {
            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Clickable_FailedMatch();

            Debug.Log("_Start_Moving_Back");
            Vector3 targetPos = startPos;

            float previousDistance = 10000f;

            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                if (Vector3.Distance(transform.position, targetPos) > previousDistance)
                    break;
                else
                {
                    previousDistance = Vector3.Distance(transform.position, targetPos);
                    transform.Translate(-Vector3.forward * 6f * Time.deltaTime);

                    yield return null;
                }
            }

            transform.position = targetPos;

            canMoveBack = false;
            canClickOnBolt = true;
        }

        bool canMoveBack = false;

        public void MoveBack()
        {
            canMoveBack = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetComponent<BoltMover>() != null || collision.gameObject.name.Contains("Bolt"))
            {
                //StopAllCoroutines();
                Debug.Log("Colliding_V2_So_Start_Moving_Back");
                //StartCoroutine(MoveBackToStartingPos());
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<BoltMover>() != null && !other.gameObject.name.ToLower().Contains("end") && !other.gameObject.name.ToLower().Contains("start") && GetComponentInParent<ShowcaseParent>().IsInGameMode())
            {
                //StopAllCoroutines();
                Debug.Log("Colliding_V1_So_Start_Moving_Back");
                if (other.transform.parent.GetSiblingIndex() > transform.parent.GetSiblingIndex())
                    canSpawnBoxBlock = true;

                if (canSpawnBoxBlock && canSpawnBoxBlockAfterStart)
                {
                    CapsuleCollider myCollider = GetComponent<CapsuleCollider>();
                    CapsuleCollider otherCollider = other as CapsuleCollider;

                    if (otherCollider != null)
                    {
                        CapsuleCollider collider1 = GetComponent<CapsuleCollider>();
                        CapsuleCollider collider2 = other.GetComponent<CapsuleCollider>();

                        // Calculate axes (assuming Z-axis alignment)
                        Vector3 axis1 = transform.forward;
                        Vector3 axis2 = other.transform.forward;

                        // Calculate central axes positions
                        Vector3 center1 = transform.position + collider1.center;
                        Vector3 center2 = other.transform.position + collider2.center;

                        // Project center2 onto the axis of center1
                        Vector3 projection1 = center1 + Vector3.Project(center2 - center1, axis1);

                        // Project center1 onto the axis of center2
                        Vector3 projection2 = center2 + Vector3.Project(center1 - center2, axis2);

                        // Calculate midpoint of projections
                        Vector3 midpoint = (projection1 + projection2) / 2;
                        midpoint = new Vector3(midpoint.x, midpoint.y, transform.position.z);



                        GameObject tempBlockBox = Instantiate(boxBlockPrefab, transform);
                        tempBlockBox.transform.localPosition = Vector3.zero;
                        tempBlockBox.transform.position = midpoint;

                        tempBlockBox.transform.parent = transform.parent.parent;
                    }
                }
            }
        }
    }
}