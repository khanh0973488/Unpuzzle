using UnityEngine;

namespace HyperPuzzleEngine
{
    public class JumpFlip : MonoBehaviour
    {
        public enum JumpSpace { Local, Global }
        public enum JumpAxis { X, Y, Z }

        public JumpSpace jumpSpace = JumpSpace.Global;
        public JumpAxis jumpAxis = JumpAxis.Y;
        public float jumpHeight = 2.0f;
        public float jumpDuration = 0.5f;
        private bool isJumping = false;

        public void JumpAndFlipObject()
        {
            if (!isJumping)
            {
                StartCoroutine(JumpAndFlipCoroutine());
            }
        }

        private System.Collections.IEnumerator JumpAndFlipCoroutine()
        {
            isJumping = true;
            float elapsedTime = 0f;
            Vector3 startPosition = (jumpSpace == JumpSpace.Local) ? transform.localPosition : transform.position;
            Quaternion startRotation = transform.localRotation;

            // Determine the target position based on the axis selected
            Vector3 jumpDirection = Vector3.zero;
            switch (jumpAxis)
            {
                case JumpAxis.X:
                    jumpDirection = new Vector3(jumpHeight, 0, 0);
                    break;
                case JumpAxis.Y:
                    jumpDirection = new Vector3(0, jumpHeight, 0);
                    break;
                case JumpAxis.Z:
                    jumpDirection = new Vector3(0, 0, jumpHeight);
                    break;
            }

            Vector3 targetPosition = startPosition + jumpDirection;
            Quaternion targetRotation = startRotation * Quaternion.Euler(0, 180f, 0);

            // Jump up and rotate
            while (elapsedTime < jumpDuration)
            {
                float t = elapsedTime / jumpDuration;
                t = t * t * (3f - 2f * t); // Smoothstep for smoother movement

                if (jumpSpace == JumpSpace.Local)
                {
                    transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
                }
                else
                {
                    transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                }

                transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure final position and rotation
            if (jumpSpace == JumpSpace.Local)
            {
                transform.localPosition = targetPosition;
            }
            else
            {
                transform.position = targetPosition;
            }
            transform.localRotation = targetRotation;

            // Jump down
            elapsedTime = 0f;
            while (elapsedTime < jumpDuration)
            {
                float t = elapsedTime / jumpDuration;
                t = t * t * (3f - 2f * t); // Smoothstep for smoother movement

                if (jumpSpace == JumpSpace.Local)
                {
                    transform.localPosition = Vector3.Lerp(targetPosition, startPosition, t);
                }
                else
                {
                    transform.position = Vector3.Lerp(targetPosition, startPosition, t);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure final position
            if (jumpSpace == JumpSpace.Local)
            {
                transform.localPosition = startPosition;
            }
            else
            {
                transform.position = startPosition;
            }
            isJumping = false;

            GetComponent<Collider>().enabled = true;
        }
    }
}