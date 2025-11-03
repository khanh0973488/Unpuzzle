using HyperPuzzleEngine;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class PlaceSelectedObject : MonoBehaviour
    {
        public LayerMask mouseRaycastLayer;
        public float gridSnapAmount = 0.1f;
        public Vector3 placementOffset;

        public GameObject specificObjectToPlacePrefab;
        public Vector3 rotateSpecificObjectAfterRotation;

        public GameObject[] objectsPrefabsList;

        private GameObject currentlySelectedObject;
        private bool canPlace = false;
        private bool isPlacingSpecificObject = false;

        private float rotationSpeed = 220f; // Degrees per second
        private float targetRotationZ = 0;
        private float scaleStep = 0.1f; // Change in scale per mouse wheel notch
        private float minScale = 0.5f; // Minimum scale limit
        private float maxScale = 2.0f; // Maximum scale limit

        private LayersManager layerManager;

        private void Start()
        {
            layerManager = GetComponentInParent<ShowcaseParent>().GetComponentInChildren<LayersManager>();
        }

        public bool IsPlacingObject()
        {
            return (currentlySelectedObject != null);
        }

        public void SelectObject(int indexOfObject)
        {
            canPlace = false;
            isPlacingSpecificObject = false;

            if (currentlySelectedObject != null)
            {
                Destroy(currentlySelectedObject);
            }

            // Instantiate the selected object with an initial position and offset
            currentlySelectedObject = Instantiate(objectsPrefabsList[indexOfObject], transform.position + placementOffset, Quaternion.identity);

            targetRotationZ = 0f;

            currentlySelectedObject.transform.parent = layerManager.GetCurrentLayerTransform();

            Invoke(nameof(CanPlaceObject), 0.15f);
        }

        public void SelectSpecificObject()
        {
            canPlace = false;
            isPlacingSpecificObject = true;

            if (currentlySelectedObject != null)
            {
                Destroy(currentlySelectedObject);
            }

            // Instantiate the selected object with an initial position and offset
            currentlySelectedObject = Instantiate(specificObjectToPlacePrefab, transform.position + placementOffset, Quaternion.identity);
            currentlySelectedObject.transform.Rotate(rotateSpecificObjectAfterRotation);

            targetRotationZ = 0f;

            currentlySelectedObject.transform.parent = layerManager.GetCurrentLayerTransform();

            Invoke(nameof(CanPlaceObject), 0.15f);
        }

        private void CanPlaceObject()
        {
            canPlace = true;
        }

        private void Update()
        {
            if (currentlySelectedObject != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 10f, mouseRaycastLayer))
                {
                    Vector3 gridPosition = hit.point + placementOffset;
                    gridPosition.x = Mathf.Round(gridPosition.x / gridSnapAmount) * gridSnapAmount;
                    gridPosition.y = Mathf.Round(gridPosition.y / gridSnapAmount) * gridSnapAmount;

                    currentlySelectedObject.transform.position = gridPosition + placementOffset;
                }

                // Rotate with right mouse button
                if (Input.GetMouseButtonDown(1)) // Right mouse button
                {
                    targetRotationZ += 45f; // Adjust this value as needed
                    targetRotationZ = Mathf.Round(targetRotationZ / 45f) * 45f;
                }

                // Smoothly rotate towards the target rotation
                Quaternion currentRotation = currentlySelectedObject.transform.rotation;
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetRotationZ);

                if (isPlacingSpecificObject)
                    targetRotation = Quaternion.Euler
                        (0f + rotateSpecificObjectAfterRotation.x,
                        0f + rotateSpecificObjectAfterRotation.y,
                        targetRotationZ + rotateSpecificObjectAfterRotation.z);

                currentlySelectedObject.transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Choose color with mouse wheel
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0)
                {
                    ColorManager objectColorManager = currentlySelectedObject.GetComponent<ColorManager>();
                    if (objectColorManager == null)
                        objectColorManager = currentlySelectedObject.GetComponentInChildren<ColorManager>(false);

                    int currentIndex = objectColorManager.stackColors.GetIndexOfColor(objectColorManager.GetColor());
                    int maxIndex = objectColorManager.stackColors.colors.Length - 1;

                    // Calculate the new index based on scroll direction
                    if (scroll > 0)
                    {
                        // Scroll up
                        if (currentIndex >= maxIndex)
                        {
                            currentIndex = 0; // Wrap to the start if it reaches the end
                        }
                        else
                        {
                            currentIndex++;
                        }
                    }
                    else if (scroll < 0)
                    {
                        // Scroll down
                        if (currentIndex <= 0)
                        {
                            currentIndex = maxIndex; // Wrap to the end if it reaches the start
                        }
                        else
                        {
                            currentIndex--;
                        }
                    }

                    // Get the next color based on the new index
                    Debug.Log("TEST_ CURRENT COLOR INDEX: " + currentIndex + "__MAXINDEX: " + maxIndex);
                    Color newColor = objectColorManager.stackColors.colors[currentIndex];
                    //objectColorManager.currentColorIndex = currentIndex; // Update the current index

                    // Change the color of the object
                    objectColorManager.ChangeColor(newColor);
                    objectColorManager.SetCurrentColorAsDefault();
                    objectColorManager.SaveColorByIndex();
                }

                if (Input.GetMouseButtonUp(0) && canPlace)
                {
                    currentlySelectedObject = null;
                    canPlace = false;
                }
            }
        }

        public GameObject GetCurrentlySelectedObject()
        {
            return currentlySelectedObject;
        }
    }
}