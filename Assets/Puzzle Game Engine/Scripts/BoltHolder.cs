using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class BoltHolder : MonoBehaviour
    {
        public StackColors color;
        public GameObject boltNutPrefab;

        private Color[] colorsToChooseFrom; // Define this array in the inspector with your desired colors
        private int currentIndex = 0;

        private PlaceSelectedObject objectPlacer;

        void Start()
        {
            if (GetComponentInParent<ShowcaseParent>().IsInGameMode())
                Destroy(this);
            else
            {
                objectPlacer = GetComponentInParent<ShowcaseParent>().GetComponentInChildren<PlaceSelectedObject>();
                colorsToChooseFrom = color.colors;

                // Disable all children except the first
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(i == 0);
                }
            }
        }

        void Update()
        {
            if (objectPlacer.GetCurrentlySelectedObject() != gameObject) return;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    //ChangeChildrenColor(scroll);
                    ActivateChild(scroll);
                }
            }
        }

        void ActivateChild(float scrollDirection)
        {
            // Deactivate current child
            //transform.GetChild(currentIndex).gameObject.SetActive(false);
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(false);

            // Determine new index
            if (scrollDirection > 0)
            {
                currentIndex = (currentIndex + 1) % transform.childCount;
            }
            else
            {
                currentIndex = (currentIndex - 1 + transform.childCount) % transform.childCount;
            }

            // Activate new current child
            transform.GetChild(currentIndex).gameObject.SetActive(true);
        }

        void ChangeChildrenColor(float scrollDirection)
        {
            // Update color index based on scroll direction
            int colorIndex = (currentIndex + (scrollDirection > 0 ? 1 : -1) + colorsToChooseFrom.Length) % colorsToChooseFrom.Length;
            currentIndex = colorIndex; // Update current index to new color index for consistency

            // Apply color to all children
            for (int i = 0; i < transform.childCount; i++)
            {
                Renderer renderer = transform.GetChild(i).GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = colorsToChooseFrom[colorIndex];
                }
            }
        }
    }
}