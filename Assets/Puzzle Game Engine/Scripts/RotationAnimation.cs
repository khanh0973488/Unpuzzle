using System.Collections;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class RotationAnimation : MonoBehaviour
    {
        public Vector3 rotationAxis = Vector3.up;  // Axis to rotate around, can be set in the inspector
        public float duration = 0.4f;  // Total duration of the shake
        public float magnitude = 35f;  // Maximum rotation angle

        [Tooltip("Define multipliers for the shake animation curve. Each element represents a fraction of the maximum rotation magnitude applied at evenly spaced intervals during the shake duration. The last multiplier should typically be 0 to return to the starting rotation.")]
        public float[] multipliers = new float[] { 1f, -0.8f, 0.5f, 0f };

        public enum RotationAnimationType
        {
            Shake
        }

        public RotationAnimationType animationType;

        public void PlaySelectedAnimation()
        {
            switch (animationType)
            {
                case RotationAnimationType.Shake:
                    Shake();
                    break;
            }
        }

        public void Shake()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(ShakeRoutine());
        }

        Quaternion startRot;
        bool gotStartRot = false;

        private IEnumerator ShakeRoutine()
        {
            if (!gotStartRot)
            {
                gotStartRot = true;
                startRot = transform.rotation;
            }

            float elapsed = 0.0f;

            Quaternion[] targetRotations = new Quaternion[multipliers.Length];

            // Pre-calculate target rotations based on the multipliers
            for (int i = 0; i < multipliers.Length; i++)
            {
                float angle = magnitude * multipliers[i];
                Quaternion rotation = Quaternion.AngleAxis(angle, rotationAxis);
                targetRotations[i] = startRot * rotation;
            }

            Quaternion currentRot = startRot;  // Start from the original rotation
            int nextTarget = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float relativeTime = elapsed / duration;
                float segmentDuration = duration / multipliers.Length;
                float segmentElapsed = elapsed % segmentDuration;
                float lerpFactor = segmentElapsed / segmentDuration;

                // Update the next target index based on the relative time
                int prevTarget = nextTarget;
                nextTarget = Mathf.FloorToInt(relativeTime * multipliers.Length);

                // Ensure we do not go out of bounds
                if (nextTarget >= targetRotations.Length)
                    break;

                // When the target changes, start interpolating from the last current rotation
                if (prevTarget != nextTarget)
                {
                    currentRot = transform.rotation;
                }

                // Smoothly interpolate from the current rotation to the next target rotation
                transform.rotation = Quaternion.Slerp(currentRot, targetRotations[nextTarget], lerpFactor);

                yield return null;
            }

            // Optionally, reset rotation to the start to prevent drift
            transform.rotation = startRot;
        }
    }
}
