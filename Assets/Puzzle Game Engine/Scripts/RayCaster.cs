using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class RayCaster : MonoBehaviour
{
    public bool showRayGizmo = false;
    public Color rayColor = Color.red;
    public float rayLength = 0.04f;
    public Vector3 rayDirection = new Vector3(0f, 1f, 0f);
    public LayerMask layerMask = 6; // Optionally specify layerMask to restrict hit detections

    private List<GameObject> collidingObjects = new List<GameObject>();

    public GameObject[] CastRay(Vector3 posToCastAt)
    {
        collidingObjects = new List<GameObject>();

        RaycastHit[] hits;
        // Transform direction from local space to world space
        Vector3 direction = transform.TransformDirection(rayDirection.normalized);

        // RaycastAll to detect all colliders on the specified path
        hits = Physics.RaycastAll(posToCastAt, direction, rayLength, layerMask);

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
        if (showRayGizmo)
        {
            // Define the ray direction and length
            Vector3 castDirection = (transform.TransformDirection(rayDirection.normalized)) * rayLength;

            // Optionally, draw a line for the ray
            Gizmos.color = rayColor;
            Gizmos.DrawLine(transform.position, transform.position + castDirection);
        }
    }
}
}