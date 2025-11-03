using UnityEngine;

namespace HyperPuzzleEngine
{
    public class OnClickRelation : MonoBehaviour
    {
        // Target Objects
        [Header("Target Objects")]
        public GameObject[] targetObjects;

        // Debug Line Options
        [Space]
        [Header("Debug Line Options")]
        public float lineWidth = 0.1f;
        public bool debugLine = true;

        // Relation Methods
        [Space]
        [Header("Relation Methods")]
        public bool jumpFlip = false;

        private void Start()
        {
            foreach (GameObject target in targetObjects)
                target.GetComponent<Collider>().enabled = false;
        }

        private void OnDrawGizmos()
        {
            if (debugLine && targetObjects != null)
            {
                foreach (GameObject target in targetObjects)
                {
                    if (target == null) continue;

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position, target.transform.position);

                    // Draw a thicker line by drawing multiple parallel lines
                    Vector3 direction = (target.transform.position - transform.position).normalized;
                    Vector3 perpendicular = Vector3.Cross(direction, Vector3.up) * lineWidth;

                    Gizmos.DrawLine(transform.position + perpendicular, target.transform.position + perpendicular);
                    Gizmos.DrawLine(transform.position - perpendicular, target.transform.position - perpendicular);
                }
            }
        }

        public void EvaluateRelationMethods()
        {
            if (jumpFlip && targetObjects != null)
            {
                foreach (GameObject target in targetObjects)
                {
                    if (target == null) continue;

                    JumpFlip jumpAndFlipComponent = target.GetComponent<JumpFlip>();
                    if (jumpAndFlipComponent != null)
                    {
                        jumpAndFlipComponent.JumpAndFlipObject();
                    }
                    else
                    {
                        Debug.LogWarning("JumpAndFlip component not found on target object: " + target.name);
                    }
                }
            }
        }
    }
}