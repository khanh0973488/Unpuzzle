using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class LookAtObject : MonoBehaviour
    {
        public Transform objectToLookAt;
        public Vector3 rotationOffset;

        private Vector3 target;

        [Space]
        [Header("Rotation Constraints")]
        public bool fixRotationOnX = false;
        public bool fixRotationOnY = false;
        public bool fixRotationOnZ = false;

        void Update()
        {
            target = objectToLookAt.position;

            if (!fixRotationOnX) target.x = transform.position.x;
            if (!fixRotationOnY) target.y = transform.position.y;
            if (!fixRotationOnZ) target.z = transform.position.z;

            transform.LookAt(target);
            transform.Rotate(rotationOffset);
        }
    }
}
