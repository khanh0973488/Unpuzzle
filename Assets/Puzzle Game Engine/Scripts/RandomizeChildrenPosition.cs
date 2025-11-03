using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class RandomizeChildrenPosition : MonoBehaviour
    {
        public bool randomizePosition = false;
        public bool resetPosition = false;

        [Space]
        private List<Vector3> startPos = new List<Vector3>();
        public Vector3 minPositionOffset;
        public Vector3 maxPositionOffset;


        private void Update()
        {
            if (randomizePosition)
            {
                randomizePosition = false;

                if (startPos.Count < transform.childCount)
                    startPos = new List<Vector3>();

                if (startPos.Count <= 0)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        startPos.Add(transform.GetChild(i).position);
                    }
                }

                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).position = startPos[i];

                    Vector3 randomPosOffset = new Vector3(
                    Random.Range(minPositionOffset.x, maxPositionOffset.x),
                    Random.Range(minPositionOffset.y, maxPositionOffset.y),
                    Random.Range(minPositionOffset.z, maxPositionOffset.z));

                    transform.GetChild(i).transform.position += randomPosOffset;
                }
            }

            if (resetPosition)
            {
                resetPosition = false;

                for (int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).position = startPos[i];
            }
        }
    }
}
