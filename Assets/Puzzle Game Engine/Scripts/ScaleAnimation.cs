using System.Collections;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class ScaleAnimation : MonoBehaviour
    {
        public bool playOnEnable = false;
        public bool resetOnEnd = true;

        [Space]
        public Vector3 direction = Vector3.one;  // Direction to scale, can be set in the inspector
        public float duration = 0.4f;  // Total duration of the scale animation
        public float magnitude = 3f;  // Maximum scale multiplier

        public bool isSnappingToFirstMultiplier = false;
        [Tooltip("Define multipliers for the scale animation curve. Each element represents a fraction of the maximum scale multiplier applied at evenly spaced intervals during the scale duration. The last multiplier should typically be 1 to return to the original scale.")]
        public float[] multipliers = new float[] { 1f, 0.8f, 0.5f, 1f };

        private void OnEnable()
        {
            if (playOnEnable) PlaySelectedAnimation();
        }

        public enum ScaleAnimationType
        {
            Scale
        }

        public ScaleAnimationType animationType;

        public void PlaySelectedAnimation()
        {
            switch (animationType)
            {
                case ScaleAnimationType.Scale:
                    Scale();
                    break;
                default:
                    break;
            }
        }

        public void Scale()
        {
            StartCoroutine(ScaleRoutine());
        }

        Vector3 startScale;
        bool gotStartScale = false;

        private IEnumerator ScaleRoutine()
        {
            if (!gotStartScale)
            {
                gotStartScale = true;
                startScale = transform.localScale;
            }
            float elapsed = 0.0f;

            Vector3[] targetScales = new Vector3[multipliers.Length];

            // Pre-calculate target scales based on the multipliers
            for (int i = 0; i < multipliers.Length; i++)
            {
                float scaleValue = magnitude * multipliers[i];
                targetScales[i] = startScale + direction.normalized * scaleValue;
            }

            Vector3 currentScale = startScale;  // Start from the original scale
            int nextTarget = 0;

            if (isSnappingToFirstMultiplier)
            {
                currentScale = transform.localScale = targetScales[0];
            }

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
                if (nextTarget >= targetScales.Length)
                    break;

                // When the target changes, start interpolating from the last current scale
                if (prevTarget != nextTarget)
                {
                    currentScale = transform.localScale;
                }

                // Smoothly interpolate from the current scale to the next target scale
                transform.localScale = Vector3.Lerp(currentScale, targetScales[nextTarget], lerpFactor);

                yield return null;
            }


            if (resetOnEnd)
            {
                // Optionally, reset scale to the start to prevent drift
                transform.localScale = startScale;
            }
        }
    }
}
