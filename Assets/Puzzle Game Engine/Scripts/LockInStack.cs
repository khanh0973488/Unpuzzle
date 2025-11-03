using System.Collections;
using UnityEngine;
using HyperPuzzleEngine;
using System;

namespace HyperPuzzleEngine
{
    public class LockInStack : MonoBehaviour
    {
        public Vector3 lockedPosOffset;
        public bool isMovingToFinalPosition = true;
        public bool deleteSelectableAfterPlaced = true;

        [Space]
        [Header("Enable/Disable Event After Locked")]
        public GameObject[] enableObjectsOnLocking;
        public GameObject[] disableObjectsOnLocking;

        float movementSpeed = 0.5f;

        public void LockIn()
        {
            Debug.Log("LOCKING_V2_" + gameObject.name);

            if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_LockIn_Match();

            if (GetComponent<SelectableAfterPlaced>() != null && deleteSelectableAfterPlaced)
                Destroy(GetComponent<SelectableAfterPlaced>());

            GetComponent<SelectableAfterPlaced>().canSelect = false;

            for (int i = 0; i < enableObjectsOnLocking.Length; i++)
                enableObjectsOnLocking[i].gameObject.SetActive(true);

            for (int i = 0; i < disableObjectsOnLocking.Length; i++)
                disableObjectsOnLocking[i].gameObject.SetActive(false);

            if (isMovingToFinalPosition)
                StartCoroutine(MoveToLockedPosition());
        }

        IEnumerator MoveToLockedPosition()
        {
            Vector3 targetPos = transform.position + lockedPosOffset;
            while (Vector3.Distance(targetPos, transform.position) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, movementSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            IncreaseCollectedStacksCounter();
        }

        private void IncreaseCollectedStacksCounter()
        {
            GetComponentInParent<CollectedStacksCounter>().IncreaseCollectedPieces();
        }
    }
}
