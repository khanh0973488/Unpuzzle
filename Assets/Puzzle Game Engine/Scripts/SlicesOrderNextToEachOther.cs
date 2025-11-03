using System.Collections;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class SlicesOrderNextToEachOther : MonoBehaviour
    {
        Transform tempMovingChild;
        Transform tempSwitchingDisabledChild;
        Quaternion tempTargetLocalRot;

        Vector3 startLocalPosOfSlice;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.1f);
            startLocalPosOfSlice = GetComponentInChildren<Slice>(true).transform.localPosition;
        }

        public void TryToOrderSlices()
        {
            StartCoroutine(OrderSlices());
        }

        IEnumerator OrderSlices()
        {
            while (true)
            {
                if (AreAllSlicesArrivedToStartPos() && !IsAnySliceMoving())
                    break;

                yield return new WaitForSeconds(0.1f);
            }

            do
            {
                foreach (Slice slice in GetComponentsInChildren<Slice>(true))
                    slice.transform.localPosition = startLocalPosOfSlice;

                int tempActiveChildrenCount = GetActiveSlicesCount();

                if (tempActiveChildrenCount > 0)
                {
                    if (!AreSlicesNextToEachOther())
                    {
                        tempMovingChild.SetSiblingIndex(tempMovingChild.GetSiblingIndex() - 1);
                        do
                        {
                            float step = 610f * Time.deltaTime; //Degrees per second
                            tempMovingChild.localRotation = Quaternion.RotateTowards(tempMovingChild.localRotation, tempTargetLocalRot, step);
                            yield return new WaitForEndOfFrame();

                        } while (tempMovingChild.localRotation != tempTargetLocalRot);

                        tempMovingChild.localRotation = tempTargetLocalRot;
                        tempSwitchingDisabledChild.Rotate(Vector3.up, 360f / transform.childCount);
                    }
                }

                if (AreSlicesNextToEachOther())
                {
                    break;
                }

            } while (true);

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).localRotation = Quaternion.identity;
                transform.GetChild(i).Rotate(Vector3.up, 360f / transform.childCount * (i + 1));
            }
        }

        private bool IsAnySliceMoving()
        {
            foreach (Slice slice in GetComponentsInChildren<Slice>(false))
            {
                if (slice.IsMoving())
                    return true;
            }

            return false;
        }

        private bool AreAllSlicesArrivedToStartPos()
        {
            foreach (Slice slice in GetComponentsInChildren<Slice>(false))
            {
                if (Vector3.Distance(slice.transform.localPosition, startLocalPosOfSlice) > 0.01f)
                    return false;
            }

            return true;
        }

        private int GetActiveSlicesCount()
        {
            int activeChildrenCount = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    activeChildrenCount++;
                }
            }

            return activeChildrenCount;
        }

        private bool AreSlicesNextToEachOther()
        {
            bool allIsNextToEachOther = true;

            int prevIndex = -1;
            int tempIndex = -1;

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    if (prevIndex == -1)
                        prevIndex = i;
                    else
                        prevIndex = tempIndex;
                    tempIndex = i;

                    if ((Mathf.Abs(tempIndex - prevIndex) > 1))
                    {
                        if (prevIndex == 0)
                        {
                            if (!transform.GetChild(transform.childCount - 1).gameObject.activeInHierarchy)
                            {
                                SetUpVariables(prevIndex, tempIndex);

                                return false;
                            }
                        }
                        else
                        {
                            SetUpVariables(prevIndex, tempIndex);

                            return false;
                        }
                    }
                }
            }

            return allIsNextToEachOther;
        }

        private void SetUpVariables(int prevIndex, int tempIndex)
        {
            tempMovingChild = transform.GetChild(tempIndex);
            tempSwitchingDisabledChild = transform.GetChild(tempIndex - 1);
            tempTargetLocalRot = tempSwitchingDisabledChild.localRotation;
        }
    }
}
