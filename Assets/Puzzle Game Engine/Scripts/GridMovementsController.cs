using UnityEngine;

namespace HyperPuzzleEngine
{
    public class GridMovementsController : MonoBehaviour
    {
        public float xStep = 1f;     // Step size for X-axis
        public float yStep = 1f;     // Step size for Y-axis
        public float zStep = 1f;     // Step size for Z-axis

        public Vector3 GetClosestGridPoint(Vector3 position)
        {
            Vector3 gridOrigin = transform.position;
            float x = Mathf.Round((position.x - gridOrigin.x) / xStep) * xStep + gridOrigin.x;
            float y = Mathf.Round((position.y - gridOrigin.y) / yStep) * yStep + gridOrigin.y;
            float z = Mathf.Round((position.z - gridOrigin.z) / zStep) * zStep + gridOrigin.z;

            return new Vector3(x, y, z);
        }

        public Vector3 GetClosestGridPoint(Transform transform)
        {
            return GetClosestGridPoint(transform.position);
        }
    }
}