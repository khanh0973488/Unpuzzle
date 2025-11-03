using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class RotateAndMoveAround : MonoBehaviour
    {
        [HideInInspector] public Quaternion rotationAtStart;
        [HideInInspector] public Vector3 positionAtStart;

        public bool canRotateAtStart = false;
        private bool isRotating = false;

        public bool canMove = true;
        public bool canRotate = true;

        [Space]
        [Header("Rotation Randomization")]
        public Vector3 minRot;
        public Vector3 maxRot;
        [Header("Position Randomization")]
        public Vector3 minPos;
        public Vector3 maxPos;

        private Quaternion targetRotation;
        private Vector3 targetPosition;

        [Space]
        [Header("Speeds")]
        public float rotationSpeed = 0.5f; // Speed of rotation
        public float movementSpeed = 3.0f; // Speed of movement

        private void Start()
        {
            if (canRotateAtStart)
                StartToRotateAndMove();
        }

        public void StopRotatingAndMoving()
        {
            isRotating = false;
            transform.position = positionAtStart;
            transform.rotation = rotationAtStart;
        }

        public void StartToRotateAndMove()
        {
            rotationAtStart = transform.rotation;
            positionAtStart = transform.position;
            isRotating = true;
            SelectNewTargets();
        }

        private void SelectNewTargets()
        {
            // Adding a random rotation to the initial rotation
            targetRotation = rotationAtStart * Quaternion.Euler(
                Random.Range(minRot.x, maxRot.x),
                Random.Range(minRot.y, maxRot.y),
                Random.Range(minRot.z, maxRot.z)
            );

            // Adding a random offset to the initial position
            targetPosition = positionAtStart + new Vector3(
                Random.Range(minPos.x, maxPos.x),
                Random.Range(minPos.y, maxPos.y),
                Random.Range(minPos.z, maxPos.z)
            );

            // Automatically reselect targets after a random interval
            if (IsInvoking(nameof(SelectNewTargets)))
                CancelInvoke(nameof(SelectNewTargets));
            Invoke(nameof(SelectNewTargets), Random.Range(0.6f, 2f));
        }

        private void LateUpdate()
        {
            if (isRotating)
            {
                if (canRotate)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
                    {
                        transform.rotation = targetRotation;
                    }
                }

                if (canMove)
                {
                    transform.position = Vector3.Lerp(transform.position, targetPosition, movementSpeed * Time.deltaTime);
                    if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
                    {
                        transform.position = targetPosition;
                    }
                }

                // Check if both rotation and position have reached the targets
                if (Quaternion.Angle(transform.rotation, targetRotation) < 1f || Vector3.Distance(transform.position, targetPosition) < 0.001f)
                {
                    SelectNewTargets();
                }
            }
        }
    }
}
