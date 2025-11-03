using UnityEngine;
using HyperPuzzleEngine;
using System.Collections;

namespace HyperPuzzleEngine
{
    public class Rotate : MonoBehaviour
    {
        public bool onlyRotateIfGameIsStill = true;
        public bool onlyRotateIfGameIsInFocus = true;
        public float dragSpeed = 2f;  // Controls the sensitivity of the drag
        public float snapRotationDegreeOnY = 60f;

        private Vector2 fingerDownPosition;
        private Vector2 fingerUpPosition;
        private float screen_width;
        private bool isDragging = false;
        private float lastXPosition;
        private Quaternion targetRotation;
        private ShowcaseParent showcaseParent;

        private Camera mainCam;

        void Start()
        {
            mainCam = Camera.main;
            screen_width = Screen.width;
            targetRotation = transform.rotation; // Initialize target rotation
            showcaseParent = GetComponentInParent<ShowcaseParent>();
        }

        void Update()
        {
            if (Input.GetMouseButton(0) && onlyRotateIfGameIsStill)
            {
                if (onlyRotateIfGameIsInFocus && !MainCameraController.Instance.IsFocusingOnTemplate(showcaseParent)) return;

                if (showcaseParent.GetComponentsInChildren<CheckNeighboursQueue>().Length <= 0) return;

                if (showcaseParent.IsAnyStackJumping()) return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 highestChildPosOnCanvas = Vector2.one * -Mathf.Infinity;
                Vector2 lowestChildPosOnCanvas = Vector2.one * Mathf.Infinity;

                for (int i = 0; i < transform.childCount; i++)
                {
                    if (mainCam.WorldToScreenPoint(transform.GetChild(i).position).y > highestChildPosOnCanvas.y)
                        highestChildPosOnCanvas = mainCam.WorldToScreenPoint(transform.GetChild(i).position);

                    if (mainCam.WorldToScreenPoint(transform.GetChild(i).position).y < lowestChildPosOnCanvas.y)
                        lowestChildPosOnCanvas = mainCam.WorldToScreenPoint(transform.GetChild(i).position);
                }

                if (!(Input.mousePosition.y < highestChildPosOnCanvas.y && Input.mousePosition.y > lowestChildPosOnCanvas.y))
                    return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                fingerDownPosition = Input.mousePosition;
                isDragging = true;
                lastXPosition = fingerDownPosition.x;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                SnapRotation();
            }

            if (isDragging)
            {
                fingerUpPosition = Input.mousePosition;
                float currentXPosition = fingerUpPosition.x;
                float xDiff = currentXPosition - lastXPosition;
                float rotationAmount = xDiff * dragSpeed / screen_width;

                transform.Rotate(0f, rotationAmount, 0f, Space.World);
                lastXPosition = currentXPosition;
            }
            else
            {
                // Smoothly interpolate to the target rotation using Slerp
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
            }
        }

        void SnapRotation()
        {
            float yRotation = transform.eulerAngles.y;
            float targetYRotation = Mathf.Round(yRotation / snapRotationDegreeOnY) * snapRotationDegreeOnY;
            targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);
        }
    }
}
