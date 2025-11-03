using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class PossibleGridPositions : MonoBehaviour
    {
        public Vector3 boxSize = new Vector3(1, 1, 1);
        public int maxAttempts = 5;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, boxSize);
        }

        public Transform FindPossiblePosition(Vector3 startPosition, Vector3 direction)
        {
            Vector3 currentPosition = startPosition;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Move in the opposite direction of the given direction, starting immediately from the offset
                currentPosition -= direction.normalized * boxSize.magnitude;

                // Perform the box cast at the current position
                Collider[] colliders = Physics.OverlapBox(currentPosition, boxSize / 2, Quaternion.identity);

                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject.name.Contains("PossiblePosition"))
                    {
                        return collider.transform;
                    }
                }
            }

            // If no matching object is found within maxAttempts, return null
            return null;
        }

        public Transform FindPossiblePosition(Transform startTransform, Vector3 direction)
        {
            return FindPossiblePosition(startTransform.position, direction);
        }
    }
}