using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class SpaceChildrenEvenly : MonoBehaviour
    {
        public bool checkAlways = false;
        public bool isCentered = false;
        public Vector3 distance;

        private void OnValidate()
        {
            OrderChildren();
        }

        public void OrderChildren()
        {
            if (transform.childCount == 0)
                return;

            // Calculate the total offset based on number of children and distance
            Vector3 totalOffset = (transform.childCount - 1) * distance;
            Vector3 startPosition = isCentered ? -totalOffset / 2 : Vector3.zero;

            // Position each child
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                child.localPosition = startPosition + i * distance;
            }
        }

        private void Update()
        {
            if (checkAlways)
            {
                OrderChildren();
            }
        }
    }
}