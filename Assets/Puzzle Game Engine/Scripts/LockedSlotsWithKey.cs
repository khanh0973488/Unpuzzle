using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class LockedSlotsWithKey : MonoBehaviour
    {
        public bool spawnLockedSlots = false;

        [Space]
        public Transform parentOfSpawn;
        public GameObject lockedSlotPrefab;

        public int countOfLockedSlots;
        int countOfMaxSlotsAtStart;
        public Vector3 spawnDirection;
        public Vector3 spawnOffset;

        private CheckNeighbours thisCheckNeighboursScript;

        private List<Transform> lockedSlots;

        private bool isFirstUnlock = true;

        private void Update()
        {
            if (spawnLockedSlots)
            {
                spawnLockedSlots = false;

                DestroyAllChildren();

                SpawnLockedSlotPrefabs();
            }
        }

        private void DestroyAllChildren()
        {
            for (int i = (parentOfSpawn.childCount) - (1); i >= 0; i--)
                DestroyImmediate(parentOfSpawn.GetChild(i).gameObject);
        }

        void Start()
        {
            thisCheckNeighboursScript = GetComponentInParent<CheckNeighbours>();
            countOfMaxSlotsAtStart = thisCheckNeighboursScript.maxCountOfStackChildren;

            if (Application.isPlaying)
            {
                UpdateCheckNeighboursSlots();

                if (parentOfSpawn == null)
                    parentOfSpawn = transform;

                lockedSlots = new List<Transform>();
                for (int i = 0; i < parentOfSpawn.childCount; i++)
                    lockedSlots.Add(parentOfSpawn.GetChild(i));
            }
        }

        private void UpdateCheckNeighboursSlots()
        {
            thisCheckNeighboursScript.maxCountOfStackChildren = countOfMaxSlotsAtStart - countOfLockedSlots;
        }

        private void SpawnLockedSlotPrefabs()
        {
            thisCheckNeighboursScript = GetComponentInParent<CheckNeighbours>();

            for (int i = 0; i < countOfLockedSlots; i++)
            {
                Vector3 targetPosition = thisCheckNeighboursScript.GetNextPosition();

                GameObject newLockedSlot = Instantiate(lockedSlotPrefab, parentOfSpawn);
                newLockedSlot.transform.position = targetPosition;
                newLockedSlot.transform.rotation = Quaternion.identity;
            }
        }

        public void UnlockSlot(bool isUnlockingInReversedOrder = false)
        {
            if (isFirstUnlock)
            {
                isFirstUnlock = false;

                lockedSlots = new List<Transform>();
                for (int i = 0; i < parentOfSpawn.childCount; i++)
                    lockedSlots.Add(parentOfSpawn.GetChild(i));
            }

            if (lockedSlots.Count <= 0)
            {
                Debug.Log("NO MORE SLOTS TO OPEN");
                return;
            }

            int tempSlotIndex = 0;

            if (isUnlockingInReversedOrder)
                tempSlotIndex = lockedSlots.Count - 1;

            lockedSlots[tempSlotIndex].GetComponent<Animation>().Play();
            lockedSlots.RemoveAt(tempSlotIndex);

            if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Key_Unlocked();

            countOfLockedSlots--;
            UpdateCheckNeighboursSlots();
        }
    }
}
