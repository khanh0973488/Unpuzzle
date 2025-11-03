using HyperPuzzleEngine;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class CubeWithScrews : MonoBehaviour
    {
        public bool[] screws;

        [Space]
        [Header("Coloring Screws")]
        public StackColors stackColorsScriptableObject;

        [SerializeField]
        private List<StackColors.StackColor> stackColorsListOfScrews;

        // Public property to access and modify stackColorsListOfScrews safely
        public List<StackColors.StackColor> StackColorsListOfScrews
        {
            get
            {
                // Ensure the list is initialized before accessing it
                if (stackColorsListOfScrews == null)
                {
                    stackColorsListOfScrews = new List<StackColors.StackColor>();
                }
                return stackColorsListOfScrews;
            }
            set
            {
                if (!Application.isPlaying)
                {
                    stackColorsListOfScrews = value;
                }
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                UpdateStackColorsListCount();
            }
        }

        #region Colorizing Screws

        private void UpdateStackColorsListCount()
        {
            if (Application.isPlaying) return;

            int requiredCount = screws != null && screws.Length > 0 ? screws.Length : transform.childCount;

            while (stackColorsListOfScrews.Count < requiredCount)
            {
                stackColorsListOfScrews.Add(StackColors.StackColor.Blue);
            }

            while (stackColorsListOfScrews.Count > requiredCount)
            {
                stackColorsListOfScrews.RemoveAt(stackColorsListOfScrews.Count - 1);
            }
        }

        private void ColorizeScrewsManually()
        {
            if (Application.isPlaying) return;

            UpdateStackColorsListCount();

            for (int i = 0; i < screws.Length; i++)
            {
                var colorManager = transform.GetChild(i).GetComponentInChildren<ColorManager>();
                if (colorManager != null && screws[i] == true)
                    colorManager.ChangeColorInEditor(stackColorsScriptableObject.colors[(int)stackColorsListOfScrews[i]]);
            }
        }

        #endregion

        public void UpdateScrews()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("UPDATED SCREW BOX");

                for (int i = 0; i < screws.Length; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(screws[i]);
                }

                ColorizeScrewsManually();
                Debug.Log("Updated Screw");
            }
        }

        public StackColors.StackColor GetColorOfScrew(Transform child)
        {
            int indexOfChild = child.GetSiblingIndex();
            return stackColorsListOfScrews[indexOfChild];
        }

        public void TryToMakeCubeFall()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeInHierarchy) return;
            }

            DragRotate pivotObject = GetComponentInParent<DragRotate>();
            GetComponent<Rigidbody>().isKinematic = false;
            transform.parent = null;

            pivotObject.RepositionCentralPivot();
        }

        public int GetIndexOfScrewBasedOnScrewParent(Transform screwParentHolder)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i) == screwParentHolder)
                    return i;
            }
            return -1;
        }
    }
}