using System.Collections;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class ScaleDownAndDestroy : MonoBehaviour
    {
        // Duration for scaling down
        public float duration = 0.1f;

        void Start()
        {
            // Start the scaling coroutine
            StartCoroutine(ScaleDown());
        }

        IEnumerator ScaleDown()
        {
            // Time passed
            float time = 0;
            // Initial scale
            Vector3 initialScale = transform.localScale;
            // Scale down to zero
            Vector3 targetScale = Vector3.zero;

            // While the time is less than the duration
            while (time < duration)
            {
                // Increase the time by the time that has passed since the last frame
                time += Time.deltaTime;
                // Calculate the scale based on the time passed
                transform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);
                // Wait for the next frame
                yield return null;
            }

            // Ensure the scale is set to zero exactly
            transform.localScale = targetScale;
            // Destroy the GameObject
            Destroy(gameObject);
        }
    }
}
