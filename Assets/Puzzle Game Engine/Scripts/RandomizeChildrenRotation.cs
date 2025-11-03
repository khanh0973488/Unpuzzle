using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class RandomizeChildrenRotation : MonoBehaviour
    {
        public bool randomizeRotation = false;
        public bool resetRotation = false;

        [Space]
        private List<Quaternion> startRot = new List<Quaternion>();
        public Vector3 minRotationOffset;
        public Vector3 maxRotationOffset;

        private void Update()
        {
            if (randomizeRotation)
            {
                randomizeRotation = false;

                if (startRot.Count < transform.childCount)
                    startRot = new List<Quaternion>();

                if (startRot.Count <= 0)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        startRot.Add(transform.GetChild(i).rotation);
                    }
                }

                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).rotation = startRot[i];

                    Vector3 randomRotOffset = new Vector3(
                        Random.Range(minRotationOffset.x, maxRotationOffset.x),
                        Random.Range(minRotationOffset.y, maxRotationOffset.y),
                        Random.Range(minRotationOffset.z, maxRotationOffset.z));

                    transform.GetChild(i).Rotate(randomRotOffset);
                }
            }

            if (resetRotation)
            {
                resetRotation = false;

                for (int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).rotation = startRot[i];
            }
        }
    }
}
