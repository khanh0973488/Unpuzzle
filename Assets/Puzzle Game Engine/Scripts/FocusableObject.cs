using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class FocusableObject : MonoBehaviour
    {
        public Transform parentOfHolderOfOtherFocusableObjects;
        public bool isFocusedOnThisAtStart = false;
        public int categoryIndex = 0;

        private Transform currentCamHolder;

        private void Awake()
        {
            currentCamHolder = transform.Find("CamHolder");
        }

        public Transform GetCurrentCamHolder()
        {
            return currentCamHolder;
        }

        private void Start()
        {
            if (isFocusedOnThisAtStart)
                MainCameraController.Instance.FocusOnObject(currentCamHolder);
        }

        public Transform GetPreviousFocusableObject()
        {
            List<Transform> listOfFocusableObjects = new List<Transform>();

            foreach (FocusableObject focusable in parentOfHolderOfOtherFocusableObjects.GetComponentsInChildren<FocusableObject>())
                listOfFocusableObjects.Add(focusable.transform);

            int currentIndexInList = listOfFocusableObjects.IndexOf(transform);

            if (currentIndexInList == 0) return null;

            return listOfFocusableObjects[currentIndexInList - 1].Find("CamHolder");
        }

        public Transform GetNextFocusableObject()
        {
            List<Transform> listOfFocusableObjects = new List<Transform>();

            foreach (FocusableObject focusable in parentOfHolderOfOtherFocusableObjects.GetComponentsInChildren<FocusableObject>())
                listOfFocusableObjects.Add(focusable.transform);

            int currentIndexInList = listOfFocusableObjects.IndexOf(transform);

            if (currentIndexInList == listOfFocusableObjects.Count - 1) return null;

            return listOfFocusableObjects[currentIndexInList + 1].Find("CamHolder");
        }
    }
}
