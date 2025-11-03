using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class RotateConstantly : MonoBehaviour
    {
        public bool isRotatingOnLocalAxis = false;

        [Space]
        [HideInInspector] public Quaternion rotationAtStart;
        [HideInInspector] public Vector3 positionAtStart;

        public bool isRotating = false;
        [HideInInspector] public bool rotatingDifferentDirection = false;

        public Vector3 rotateAmount;

        [Space]
        public float rotationSpeed = 0.5f; // Speed of rotation

        public bool IsRotating()
        {
            return isRotating;
        }

        public void StopRotating()
        {
            if (!isRotating) return;

            isRotating = false;
        }

        public void StartToRotate()
        {
            isRotating = true;
        }

        private void LateUpdate()
        {
            if (isRotating)
            {
                if (isRotatingOnLocalAxis)
                {
                    if (rotatingDifferentDirection)
                        transform.Rotate(-rotateAmount * rotationSpeed * Time.deltaTime, Space.Self);
                    else
                        transform.Rotate(rotateAmount * rotationSpeed * Time.deltaTime, Space.Self);
                }
                else
                {
                    if (rotatingDifferentDirection)
                        transform.Rotate(-rotateAmount * rotationSpeed * Time.deltaTime, Space.World);
                    else
                        transform.Rotate(rotateAmount * rotationSpeed * Time.deltaTime, Space.World);
                }
            }
        }
    }
}
