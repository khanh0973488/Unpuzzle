using HyperPuzzleEngine;
using System.Collections;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class CanBottle : MonoBehaviour
    {
        [Header("Jump Settings")]
        public float jumpHeight = 2f;   // The height the object will jump
        public float jumpDuration = 1f; // How long the jump will last

        [HideInInspector] public bool isJumpingToBox = false;

        public void JumpToTarget(Transform target)
        {
            transform.SetParent(target);
            StartCoroutine(JumpToPosition(target));
        }

        private IEnumerator JumpToPosition(Transform target)
        {
            isJumpingToBox = true;

            Vector3 startPosition = transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < jumpDuration)
            {
                Vector3 targetPosition = target.position;
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / jumpDuration);
                float height = Mathf.Sin(Mathf.PI * t) * jumpHeight; // Creates the arc
                transform.position = Vector3.Lerp(startPosition, targetPosition, t) + Vector3.up * height;
                yield return null;
            }

            //transform.position = target.position;
            transform.localPosition = Vector3.zero;

            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_BottleJam_BottleJumpEnd();

            isJumpingToBox = false;
        }
    }
}