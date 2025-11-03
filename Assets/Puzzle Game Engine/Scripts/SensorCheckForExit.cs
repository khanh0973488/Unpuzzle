using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HyperPuzzleEngine
{
    public class SensorCheckForExit : MonoBehaviour
    {
        public string nameOfObjectToCheck;

        public UnityEvent OnObjectExit;

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<ShowcaseParent>() == GetComponentInParent<ShowcaseParent>()
                && other.name.ToLower().Contains(nameOfObjectToCheck.ToLower()))
            {
                OnObjectExit.Invoke();
                other.GetComponent<Block>().IncreaseCollectedCount();
                other.gameObject.SetActive(false);
                
                // Kiểm tra điều kiện thua sau khi ẩn block
                LevelManager levelManager = GetComponentInParent<LevelManager>();
                if (levelManager != null)
                {
                    levelManager.CheckForHiddenBlocksLossCondition();
                }
            }
        }

        // public void ReduceLockedBlockCounter()
        // {
        //     if (!LockBlock.TryMarkDecreased(LockBlock.DecreaseSource.Exit)) return;
        //     foreach (LockBlock lockedBlock in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<LockBlock>())
        //         lockedBlock.ReduceLockedCounter();
        // }
    }
}