using UnityEngine;
using HyperPuzzleEngine;
using UnityEditor;

namespace HyperPuzzleEngine
{
    [ExecuteInEditMode]
    public class FollowObject : MonoBehaviour
    {
        public Transform objectToFollow;
        public Vector3 followOffset = Vector3.zero;

        private void LateUpdate()
        {
            if (objectToFollow != null)
                transform.position = objectToFollow.position + followOffset;
            else
            {
                if (!TryToFindCloseGrid())

                    DeferDestroy(gameObject);
            }
        }

        private bool TryToFindCloseGrid()
        {
            foreach (CheckNeighbours grid in transform.parent.GetComponentsInChildren<CheckNeighbours>())
            {
                if (Vector3.Distance(grid.transform.position, transform.position) <=
                    (Vector3.Distance(Vector3.zero, followOffset) + 0.2f))
                {
                    objectToFollow = grid.transform;
                }
            }

            return objectToFollow != null;
        }

        private void DeferDestroy(GameObject obj)
        {
            if (obj == null) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Defer destruction in Edit Mode using delayCall
                EditorApplication.delayCall += () =>
                {
                    if (obj != null)
                    {
                        DestroyImmediate(obj);
                    }
                };
            }
            else
#endif
            {
                // Destroy immediately in Play Mode
                Destroy(obj);
            }
        }
    }
}
