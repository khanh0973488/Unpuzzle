using HyperPuzzleEngine;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.GraphicRaycaster;

namespace HyperPuzzleEngine
{
    public class MovableObject : MonoBehaviour
    {
        [Header("Animations to Play on Blocking Object When Blocked")]
        public bool isMovingBlockingObjectIfBlocked = true;
        public bool isShakingBlockingObjectIfBlocked = true;

        [Space]
        public string nameOfObjectWhichIsBlockingWay = "cube";

        [Space]
        [Header("Movement Properties")]
        public float movementSpeed = 10f;
        public Vector3 direction = Vector3.forward;

        [Space]
        [Header("Movement Events")]
        public UnityEvent OnWayIsBlocked;
        public UnityEvent OnMove;

        private RayCaster rayCaster;
        private MovesConstraint movesConstraint;

        private GameObject blockingObject = null;

        private void Start()
        {
            rayCaster = GetComponent<RayCaster>();
            movesConstraint = GetComponentInParent<ShowcaseParent>().GetComponentInChildren<MovesConstraint>();
        }

        public void TryToMoveObject()
        {
            if (rayCaster != null)
            {
                GameObject[] collidedObjects = rayCaster.CastRay(transform.position);

                for (int i = 0; i < collidedObjects.Length; i++)
                {
                    if (collidedObjects[i].name.ToLower().Contains(nameOfObjectWhichIsBlockingWay.ToLower()))
                    {
                        blockingObject = collidedObjects[i];
                        OnWayIsBlocked.Invoke();
                        return;
                    }
                }

                OnMove.Invoke();
            }
        }

        public void PlayBlockedAnimationsOnBlockingObject()
        {
            if (blockingObject == null) return;

            if (isMovingBlockingObjectIfBlocked && blockingObject.GetComponent<PositionAnimation>() != null)
                blockingObject.GetComponent<PositionAnimation>().PlaySelectedAnimation();
            if (isShakingBlockingObjectIfBlocked && blockingObject.GetComponent<RotationAnimation>() != null)
                blockingObject.GetComponent<RotationAnimation>().Shake();
        }

        public void MoveForwardConstantly()
        {
            Destroy(gameObject, 2f);
            StartCoroutine(Moving());
        }

        IEnumerator Moving()
        {
            while (true)
            {
                transform.Translate(direction * movementSpeed * Time.deltaTime);

                yield return new WaitForEndOfFrame();
            }
        }

        public void DecreaseAvailableMoves(int decreaseMovesBy)
        {
            if (movesConstraint != null)
                movesConstraint.UpdateMoves(-decreaseMovesBy);
        }
    }
}