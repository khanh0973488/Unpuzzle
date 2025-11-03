using HyperPuzzleEngine;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class SphereCaster : MonoBehaviour
    {
        public bool showSphereCastGizmo = false;
        public float sphereRadius = 1.27f;
        public Vector3 sphereCastDirection = new Vector3(0f, 1f, 0f);
        public float castDistance = 0.04f;
        public LayerMask layerMask = 6; // Optionally specify layerMask to restrict hit detections

        private List<GameObject> collidingObjects = new List<GameObject>();

        public GameObject[] CastSphere(Vector3 posToCastAt)
        {
            collidingObjects = new List<GameObject>();

            RaycastHit[] hits;
            // Transform direction from local space to world space
            Vector3 direction = transform.TransformDirection(sphereCastDirection.normalized);

            // SphereCastAll to detect all colliders on the specified path
            hits = Physics.SphereCastAll(posToCastAt, sphereRadius, direction, castDistance, layerMask);

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.GetInstanceID() != transform.GetInstanceID())
                {
                    collidingObjects.Add(hit.collider.gameObject);
                }
            }

            return collidingObjects.ToArray();
        }

        void OnDrawGizmos()
        {
            if (showSphereCastGizmo)
            {
                // Define the sphere cast direction and distance
                Vector3 castDirection = transform.TransformDirection(sphereCastDirection.normalized) * castDistance;

                // Draw the starting sphere
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, sphereRadius);

                // Draw the ending sphere
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position + castDirection, sphereRadius);

                // Optionally, draw a line between the centers of the start and end spheres
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + castDirection);
            }
        }
    }
}