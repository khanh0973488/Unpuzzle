using UnityEngine;
using System.Collections;
using HyperPuzzleEngine;
using UnityEngine.Events;

namespace HyperPuzzleEngine
{
    public class ParabolicJump : MonoBehaviour
    {
        public bool canSpawnParabolicJumpEndPrefab = true;
        [Space]
        [Header("Nuts Movement Behavior")]
        public bool simplyMovingWithoutJump = false;
        public bool yMovementIsFixed = false;
        public bool simulatingLevitateDownAtEnd = false;

        [Space]
        public float simpleMovementSpeed = 5f;

        #region Private Variables

        private float minJumpHeight = 1f;
        private float maxJumpHeight = 4f;
        private float jumpDuration = 0.4f;
        private Vector3 startPosition;
        private Vector3 previousPosition;
        private float startTime;
        private bool isJumping = false;

        Vector3 currentTarget;
        Quaternion currentRotationAtStart;
        bool currentIsLocalPos;

        #endregion

        [HideInInspector] public Transform parentAtStartOfJump;

        [Space]
        [Header("Rotation Offsets In Different Events")]
        public Vector3 applyRotationOffsetAtDealing = Vector3.zero;
        public Vector3 applyRotationOffsetAtStackChanging = Vector3.zero;
        public Vector3 localRotationForced = Vector3.zero;
        public Vector3 targetPositionOffset = Vector3.zero;

        [Space]
        public bool changeRotationAtFinishedJump = false;
        public Vector3 applyRotationAtFinishedJump = Vector3.zero;

        [Space]
        public UnityEvent OnFinishedJump;

        #region Main Jumping Mechanics

        public void SimpleJump(Vector3 targetPos, bool isLocalPos)
        {
            foreach (ProgressBar stackProgressBar in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<ProgressBar>())
                stackProgressBar.CheckProgress();

            if (GetComponent<RotateAndMoveAround>() != null)
                GetComponent<RotateAndMoveAround>().StopRotatingAndMoving();

            if (GetComponent<RotateConstantly>() != null)
                GetComponent<RotateConstantly>().StopRotating();

            targetPos = targetPos + targetPositionOffset;

            if (GetComponent<SelectableAfterPlaced>() != null)
                GetComponent<SelectableAfterPlaced>().SetOutline(false);

            if (simplyMovingWithoutJump)
                StartCoroutine(JumpToTarget(targetPos, isLocalPos, false, false));
            else
                StartCoroutine(JumpToTarget(targetPos, isLocalPos));

            if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_StartedToJump();
        }

        public void SimpleMove(Vector3 targetPos, bool isLocalPos)
        {
            targetPos = targetPos + targetPositionOffset;

            StartCoroutine(JumpToTarget(targetPos, isLocalPos, false, false));
        }

        IEnumerator JumpToTarget(Vector3 target, bool isLocalPos = false, bool canJump = true, bool canRotate = true)
        {
            isJumping = true;

            Quaternion rotationAtStart = transform.rotation;

            float realWorldFixedPosOnY = 0f;
            if (GetComponent<SelectableAfterPlaced>() != null)
                realWorldFixedPosOnY = GetComponent<SelectableAfterPlaced>().levitateAmountRealWorldPosition.y;

            if (yMovementIsFixed)
                target = new Vector3(target.x, realWorldFixedPosOnY, target.z);

            startPosition = isLocalPos ? transform.localPosition : transform.position;
            previousPosition = startPosition;

            float horizontalDistance = Vector3.Distance(new Vector3(startPosition.x, 0, startPosition.z), new Vector3(target.x, 0, target.z));
            float verticalDistance = Mathf.Abs(target.y - startPosition.y);

            float jumpHeight = Mathf.Lerp(minJumpHeight, maxJumpHeight, horizontalDistance / 10);
            jumpHeight = Mathf.Max(jumpHeight, verticalDistance * 0.5f);

            if (!canJump) jumpHeight = 0;

            startTime = Time.time;

            currentTarget = target;
            currentRotationAtStart = rotationAtStart;
            currentIsLocalPos = isLocalPos;

            if (parentAtStartOfJump != null)
            {
                if (parentAtStartOfJump.GetComponent<CheckNeighbours>() != null)
                    parentAtStartOfJump.GetComponent<CheckNeighbours>().TryToDisableHighlight();
            }

            #region First Reach Fixed Pos On Y

            while (yMovementIsFixed && Mathf.Abs(transform.position.y - realWorldFixedPosOnY) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, realWorldFixedPosOnY, transform.position.z), simpleMovementSpeed * Time.deltaTime);
                yield return null;
            }

            #endregion

            //Forcing Sort Nuts game's nuts to move fast
            if (gameObject.name.Contains("Grid_Nut2")) jumpDuration = 0.15f;

            while (Time.time - startTime < jumpDuration)
            {
                float t = (Time.time - startTime) / jumpDuration;
                Vector3 jumpArc = CalculateJumpArc(startPosition, target, jumpHeight, t);

                if (canJump)
                {
                    if (isLocalPos)
                        transform.localPosition = jumpArc;
                    else
                        transform.position = jumpArc;
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, target, simpleMovementSpeed * Time.deltaTime);
                }

                if (canRotate)
                {
                    // Update orientation to always face forward along the jump arc
                    Vector3 direction = jumpArc - previousPosition;
                    if (direction != Vector3.zero)
                    {
                        transform.LookAt(previousPosition);

                        if (parentAtStartOfJump == null || parentAtStartOfJump.GetComponent<OccupiedGrid>() == null)
                        {
                            if (applyRotationOffsetAtDealing != Vector3.zero)
                                transform.Rotate(applyRotationOffsetAtDealing);
                            else
                                transform.Rotate(Vector3.forward, 90f);
                        }
                        else
                        {
                            if (applyRotationOffsetAtStackChanging != Vector3.zero)
                                transform.Rotate(applyRotationOffsetAtStackChanging);
                            else
                                transform.Rotate(Vector3.forward, 90f);
                        }
                    }
                }

                previousPosition = jumpArc;

                yield return null;
            }

            // Snap to the exact target position and finish the jump
            transform.position = target;
            FinishJumping();
        }

        #endregion

        public void FinishJumping()
        {
            StopAllCoroutines();

            transform.position = currentTarget;  // Ensure exact positioning at the end

            if (localRotationForced == Vector3.zero)
            {
                transform.rotation = currentRotationAtStart;

                if (applyRotationOffsetAtDealing != Vector3.zero)
                    transform.Rotate(applyRotationOffsetAtDealing);
            }
            else
                transform.localRotation = Quaternion.Euler(localRotationForced);

            if (currentIsLocalPos) transform.localPosition = currentTarget;

            CheckIfStackHasMatchingColors();

            ReCheckNeighbours();

            if (simulatingLevitateDownAtEnd)
            {
                GetComponent<SelectableAfterPlaced>().SetLevitating(true);
                GetComponent<SelectableAfterPlaced>().ChangeLevitation(GetComponentInParent<GridChildrenAreSelectable>().GetPos(transform.GetSiblingIndex() + 1) + targetPositionOffset);
            }

            if (changeRotationAtFinishedJump)
            {
                transform.localRotation = Quaternion.identity;
                transform.Rotate(applyRotationAtFinishedJump);
            }

            isJumping = false;
            Debug.Log("Finished Jumping....");

            if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Jumped();

            if (GetComponentInParent<ShowcaseParent>() != null && GetComponentInParent<ShowcaseParent>().GetComponentInChildren<ProgressBar>() != null)
            {
                foreach (ProgressBar stackProgressBar in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<ProgressBar>())
                    stackProgressBar.CheckProgress();
            }

            if (GetComponentInParent<DominoPointsManager>() != null) GetComponentInParent<DominoPointsManager>().RotateBasedOnMatchablePointsSide();

            OnFinishedJump.Invoke();

            if (GetComponentInParent<CheckForMoreTargetPositions>() != null)
                GetComponentInParent<CheckForMoreTargetPositions>().CheckIfAnyTargetIsFree();
        }

        public bool IsJumping()
        {
            return isJumping;
        }

        void CheckIfStackHasMatchingColors()
        {
            if (transform.parent != null && transform.parent.childCount - 1 == transform.GetSiblingIndex())
            {
                if (transform.parent != null)
                {
                    transform.parent.GetComponentInParent<CheckNeighbours>().StartCheckMatchingColorsInStack();
                }
            }
        }

        void ReCheckNeighbours()
        {
            if (transform.parent == null) return;

            if ((transform.parent.childCount - 1) == transform.GetSiblingIndex())
            {
                if (parentAtStartOfJump != null)
                {
                    if (parentAtStartOfJump.GetComponentInParent<CheckNeighbours>() != null)
                        parentAtStartOfJump.GetComponentInParent<CheckNeighbours>().StartCheckNeighbourColorsCoroutine();
                }

                if (transform.parent != null)
                {
                    transform.parent.GetComponentInParent<CheckNeighbours>().StartCheckNeighbourColorsCoroutine();
                }
            }
        }

        private Vector3 CalculateJumpArc(Vector3 start, Vector3 target, float height, float time)
        {
            float x = Mathf.Lerp(start.x, target.x, time);
            float y = Mathf.Lerp(start.y, target.y, time) + Mathf.Sin(time * Mathf.PI) * height;
            float z = Mathf.Lerp(start.z, target.z, time);

            return new Vector3(x, y, z);
        }
    }
}
