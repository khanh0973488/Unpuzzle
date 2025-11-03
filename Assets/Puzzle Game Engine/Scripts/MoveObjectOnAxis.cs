using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class MoveObjectOnAxis : MonoBehaviour
    {
        public enum MoveAxis { X, Y, Z }

        [Header("Movement Settings")]
        public MoveAxis moveAxis = MoveAxis.X;
        public float moveAmount = 1.0f;
        public float moveSpeed = 0.0f;

        public void MoveInstantly()
        {
            Vector3 newPosition = transform.position;
            switch (moveAxis)
            {
                case MoveAxis.X:
                    newPosition.x += moveAmount;
                    break;
                case MoveAxis.Y:
                    newPosition.y += moveAmount;
                    break;
                case MoveAxis.Z:
                    newPosition.z += moveAmount;
                    break;
            }
            transform.position = newPosition;

            //Disable every domino points on children except for the top child
            if (transform.childCount > 1)
            {
                foreach (SpriteRenderer dominoSprite in transform.GetChild(transform.childCount - 2).GetComponentsInChildren<SpriteRenderer>())
                    dominoSprite.enabled = false;
            }
        }

        public IEnumerator MoveBySpeedCoroutine()
        {
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition;

            switch (moveAxis)
            {
                case MoveAxis.X:
                    targetPosition.x += moveAmount;
                    break;
                case MoveAxis.Y:
                    targetPosition.y += moveAmount;
                    break;
                case MoveAxis.Z:
                    targetPosition.z += moveAmount;
                    break;
            }

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                if (moveSpeed == 0)
                {
                    MoveInstantly();
                    yield break;
                }

                Vector3 direction = (targetPosition - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;

                if (Vector3.Distance(transform.position, targetPosition) < moveSpeed * Time.deltaTime)
                {
                    transform.position = targetPosition;
                    yield break;
                }

                yield return null;
            }

            transform.position = targetPosition;
        }

        public void StartMoveBySpeed()
        {
            StartCoroutine(MoveBySpeedCoroutine());
        }
    }
}