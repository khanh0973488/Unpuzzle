using System.Collections;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class SelectableAfterPlaced : MonoBehaviour
    {
        bool isLevitating = false;

        [Header("Levitation")]
        public float levitatingSpeed = 10f;

        public bool canSelectAtStart = true;
        [HideInInspector] public bool canSelect = true;

        public bool isLevitatingToWorldPosition = false;
        public Vector3 levitateAmountLocalOffset = new Vector3(0f, 1f, 0f);
        public Vector3 levitateAmountRealWorldPosition = new Vector3(0f, 1f, 0f);

        Vector3 positionBeforeLevitating;

        bool canLevitate = true;

        [Space]
        public bool outlineWhileSelected = true;

        private void Start()
        {
            canSelect = canSelectAtStart;
        }

        private void OnMouseDown()
        {
            if (!canSelect) return;

            Debug.Log("clickedOn");

            if (GetComponentInParent<ShowcaseParent>().GetComponentInChildren<DealStackButton>() != null &&
                (GetComponentInParent<ShowcaseParent>().GetComponentInChildren<DealStackButton>().IsDealingCards())/*|| GetComponentInParent<ShowcaseParent>().IsSelectableLevitating()*/)
            {
                return;
            }

            if (GetComponentInParent<ShowcaseParent>().IsAnyStackJumping())
            {
                return;
            }

            if (GetComponentInParent<ShowcaseParent>().IsSelectableLevitating())
            {
                //This stack is already levitating - Clicked on same stack
                if (GetComponentInParent<GridChildrenAreSelectable>().IsLevitating())
                {
                    GetComponentInParent<GridChildrenAreSelectable>().LevitateChildren();
                }
                else
                {
                    //Try to jump - Clicked on other stack
                    GetComponentInParent<GridChildrenAreSelectable>().TryToJump();
                }
            }
            else
            {
                GetComponentInParent<GridChildrenAreSelectable>().LevitateChildren();
            }
        }

        public bool IsLevitating()
        {
            return isLevitating;
        }

        public void SetLevitating(bool newValue)
        {
            isLevitating = newValue;
        }

        public void ChangeLevitation(Vector3 targetEndPos)
        {
            if (!canLevitate) return;

            canLevitate = false;

            StartCoroutine(Move(targetEndPos));
        }

        IEnumerator Move(Vector3 target)
        {
            if (!isLevitating)
                positionBeforeLevitating = transform.position;

            Debug.Log("STARTING TO LEVITATE");
            SetOutline(true);

            Vector3 startPos = transform.position;
            Vector3 targetPos = transform.position;

            if (GetComponent<RotateConstantly>() != null)
            {
                GetComponent<RotateConstantly>().rotatingDifferentDirection = isLevitating;
                GetComponent<RotateConstantly>().StartToRotate();
            }

            if (!isLevitating)
            {
                targetPos = CalculateLevitationTargetPos(targetPos, startPos);

                if (target != Vector3.one * 444f)
                    targetPos = target;

                SetOutline(true);
            }
            else
            {
                if (GetComponent<RotateAndMoveAround>() != null)
                {
                    GetComponent<RotateAndMoveAround>().StopRotatingAndMoving();

                    startPos = GetComponent<RotateAndMoveAround>().positionAtStart;
                }

                targetPos = positionBeforeLevitating;

                if (target != Vector3.one * 444f)
                    targetPos = target;

                SetOutline(false);
            }

            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, levitatingSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            if (GetComponent<RotateConstantly>() != null)
                GetComponent<RotateConstantly>().StopRotating();

            transform.position = targetPos;

            if (!isLevitating)
            {
                if (GetComponent<RotateAndMoveAround>() != null)
                    GetComponent<RotateAndMoveAround>().StartToRotateAndMove();
            }

            isLevitating = !isLevitating;

            if (!isLevitating)
                transform.localRotation = Quaternion.Euler(GetComponent<ParabolicJump>().localRotationForced);

            canLevitate = true;
        }

        private Vector3 CalculateLevitationTargetPos(Vector3 targetPos, Vector3 startPos)
        {
            if (!isLevitatingToWorldPosition)
                targetPos = startPos + levitateAmountLocalOffset;
            else
            {
                targetPos = startPos;
                if (levitateAmountRealWorldPosition.x != 0f)
                    targetPos = new Vector3(levitateAmountRealWorldPosition.x, targetPos.y, targetPos.z);
                if (levitateAmountRealWorldPosition.y != 0f)
                    targetPos = new Vector3(targetPos.x, levitateAmountRealWorldPosition.y, targetPos.z);
                if (levitateAmountRealWorldPosition.z != 0f)
                    targetPos = new Vector3(targetPos.x, targetPos.y, levitateAmountRealWorldPosition.z);
            }

            return targetPos;
        }

        public void SetOutline(bool activating)
        {
            if (outlineWhileSelected)
                transform.GetChild(0).gameObject.SetActive(activating);
        }
    }
}
