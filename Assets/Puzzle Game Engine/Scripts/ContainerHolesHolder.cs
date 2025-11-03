using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class ContainerHolesHolder : MonoBehaviour
    {
        [SerializeField] private string holeNameIdentifier = "HolePrefab";
        private List<Transform> holes = new List<Transform>();
        private List<bool> holeStatus = new List<bool>(); // Keeps track of which holes are occupied or empty

        void Start()
        {
            InitializeHoles();
        }

        // This function initializes the holes list by finding all children that match the identifier
        private void InitializeHoles()
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains(holeNameIdentifier))
                {
                    holes.Add(child);
                    holeStatus.Add(false); // All holes are initially empty
                }
            }
        }

        // This function returns the transform of the next empty hole
        public Transform GetNextEmptyHole()
        {
            for (int i = 0; i < holeStatus.Count; i++)
            {
                if (!holeStatus[i]) // If the hole is not occupied
                {
                    return holes[i];
                }
            }

            return null; // No empty hole found
        }

        // This function reserves the next empty hole (sets its status to occupied)
        public bool ReserveNextEmptyHole()
        {
            for (int i = 0; i < holeStatus.Count; i++)
            {
                if (!holeStatus[i]) // If the hole is not occupied
                {
                    holeStatus[i] = true; // Set the status to occupied
                    return true;
                }
            }

            return false; // No empty hole found to reserve
        }

        // This function un-reserves a hole, making it available again
        public bool UnReserveHole(Transform hole)
        {
            int index = holes.IndexOf(hole);
            if (index != -1 && holeStatus[index])
            {
                holeStatus[index] = false; // Set the status to empty
                return true;
            }
            return false; // The specified hole was not found or wasn't occupied
        }

        public bool DoesHaveScrew(Transform screwToCheck)
        {
            return screwsAddedToThisContainer.Contains(screwToCheck);
        }

        List<Transform> screwsAddedToThisContainer = new List<Transform>();

        public void AddScrew(Transform screwToAdd)
        {
            if (!screwsAddedToThisContainer.Contains(screwToAdd)) screwsAddedToThisContainer.Add(screwToAdd);
        }


        [Space]
        [Header("Coloring Containers")]
        public StackColors stackColorsScriptableObject;
        public StackColors.StackColor stackColor;

        #region Colorizing Containers

        public void ColorizeManually()
        {
            if (stackColorsScriptableObject == null || stackColorsScriptableObject.colors == null)
            {
                Debug.LogWarning("stackColorsScriptableObject or its colors array is not assigned.");
                return;
            }

            foreach (ColorManager colorM in GetComponentsInChildren<ColorManager>())
            {
                colorM.ChangeColorInEditor(stackColorsScriptableObject.colors[(int)stackColor]);
            }
        }


        private void OnValidate()
        {
            ColorizeManually();
        }

        public Color GetContainerColor()
        {
            return stackColorsScriptableObject.colors[(int)stackColor];
        }

        #endregion
    }
}