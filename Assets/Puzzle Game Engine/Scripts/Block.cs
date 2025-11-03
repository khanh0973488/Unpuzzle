using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

namespace HyperPuzzleEngine
{
    public class Block : MonoBehaviour
    {
        public UnityEvent OnThisCanBeCleared;
        public UnityEvent OnHitAnotherBlock;

        [Space]
        public Animation redErrorOutlineAnim;

        public enum MoveDirection
        {
            Forward,
            Back,
            Up,
            Down,
            Right,
            Left
        }

        public enum MovementState
        {
            None,
            MovingForward,
            Bouncing
        }

        [Space]
        [Header("Movements")]
        [Tooltip("Enable/disable block movement functionality")]
        public bool canMove = true;
        
        [Header("Collision Rules")]
        [Tooltip("Nếu bật: cho phép block đối nhau va chạm bình thường. Nếu tắt: block đối nhau va chạm sẽ thua.")]
        public bool allowOppositeCollision = true;
        
        public MoveDirection movementDirection = MoveDirection.Forward;
        public float moveDistance = 1f;
        public float moveSpeed = 1.25f;        // Giảm từ 5f xuống 1.25f (1/4)
        public float moveSpeedFastest = 2.5f;  // Giảm từ 10f xuống 2.5f (1/4)
        public float reachFastMoveSpeedInSeconds = 1f;
        public float bounceSpeed = 1.25f;      // Giảm từ 5f xuống 1.25f (1/4)
        [Space]
        public string collidingObjectsName; // Name pattern to identify collidable objects

        private bool isMoving = false;
        private Vector3 moveDirection;

        private MovementState movementState = MovementState.None;

        [Space]
        public float bounceAmount;
        public float rayCheckDistanceForNextBlockOnCollision;

        private MovesConstraint movesConstraint;
        private PossibleGridPositions possibleGridPositions;
        private GridMovementsController gridController;
        private LockBlock lockedBlock;
        private DragRotate dragRotateObj;
        private LockBlock[] lockedBlocks;

        private bool increasedCounter = false;

		[Header("Rotate When Other Block Moves")]
		public bool rotateOnOtherBlockMove = false;
		public Transform rotateOnOtherMoveTarget;
		public Space rotationSpaceOther = Space.Self;

		private static event Action<Block> OnAnyBlockMoveStarted;

        private void Start()
        {
            movesConstraint = GetComponentInParent<ShowcaseParent>().GetComponentInChildren<MovesConstraint>();
            possibleGridPositions = GetComponentInParent<ShowcaseParent>().GetComponentInChildren<PossibleGridPositions>();
            gridController = GetComponentInParent<ShowcaseParent>().GetComponentInChildren<GridMovementsController>();
            lockedBlock = GetComponent<LockBlock>();
            dragRotateObj = GetComponentInParent<DragRotate>();
            lockedBlocks = GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<LockBlock>();
        }

		private void OnEnable()
		{
			OnAnyBlockMoveStarted += HandleOtherBlockMoveStarted;
		}

		private void OnDisable()
		{
			OnAnyBlockMoveStarted -= HandleOtherBlockMoveStarted;
		}

		private void HandleOtherBlockMoveStarted(Block mover)
		{
			if (!rotateOnOtherBlockMove) return;
			if (mover == this) return;
			Transform target = rotateOnOtherMoveTarget != null ? rotateOnOtherMoveTarget : transform;
			
			// Random rotation: 90, -90, or 180 degrees
			float[] rotationOptions = { 90f, -90f, 180f };
			float zRotation = rotationOptions[UnityEngine.Random.Range(0, rotationOptions.Length)];
			
			Vector3 rotation = new Vector3(0f, 0f, zRotation);
			target.Rotate(rotation, rotationSpaceOther);
		}

        public void OnMouseUpAsButton()
        {
            if (lockedBlock != null && lockedBlock.isLocked) return;
            if (!canMove) return; // Check if movement is enabled

            if (!isMoving)
            {
                DecreaseAvailableMoves(1);
                moveDirection = GetDirectionVector(movementDirection);

				#region Check If Can Be Cleared

                Ray ray = new Ray(transform.position, moveDirection);
                RaycastHit hitInfo;

                float rayLength = 10f;

                if (Physics.Raycast(ray, out hitInfo, rayLength /*, mask, QueryTriggerInteraction.Ignore*/))
                {
                    if (!hitInfo.collider.gameObject.name.Contains(collidingObjectsName))
                    {
                        OnThisCanBeCleared.Invoke();
                        IncreaseCollectedCount();
                    }
                    else
                    {
                        if (hitInfo.collider.transform.parent != transform.parent)
                        {
                            OnThisCanBeCleared.Invoke();
                            IncreaseCollectedCount();
                        }
                    }
                }
                else
                {
                    OnThisCanBeCleared.Invoke();
                    IncreaseCollectedCount();
                }

				#endregion

				// Reduce hidden counters when a block starts moving (no sensor required)
				if (LockBlock.TryMarkDecreased(LockBlock.DecreaseSource.Move))
					ReduceLockedBlockCounter();

				OnAnyBlockMoveStarted?.Invoke(this);
				StartCoroutine(MoveForward());
            }
        }

        public void UnParent()
        {
            transform.parent = transform.parent.parent;
        }

        public void PlayClearedAnimation()
        {
            if (GetComponent<Animation>() != null)
                GetComponent<Animation>().Play("MovableCubeCleared");

            Destroy(gameObject, 3f);
        }

        public void PlayFailedAnimation()
        {
            if (GetComponent<Animation>() != null)
                GetComponent<Animation>().Play("MovableCubeFailed");
        }

        public void RemoveColliderAndSetNonClickable()
        {
            GetComponent<Collider>().enabled = false;
            isCleared = true;
        }

        bool isCleared = false;

        private Vector3 GetDirectionVector(MoveDirection dir)
        {
            switch (dir)
            {
                case MoveDirection.Forward:
                    return transform.forward;
                case MoveDirection.Back:
                    return -transform.forward;
                case MoveDirection.Up:
                    return transform.up;
                case MoveDirection.Down:
                    return -transform.up;
                case MoveDirection.Right:
                    return transform.right;
                case MoveDirection.Left:
                    return -transform.right;
                default:
                    return transform.forward;
            }
        }

        public IEnumerator MoveForward()
        {
            GetComponent<Collider>().enabled = false;

            Debug.Log("clicked.............");
            isMoving = true;
            movementState = MovementState.MovingForward;
            Vector3 targetPosition = transform.position + moveDirection * moveDistance;

            GetComponent<Collider>().enabled = true;

            float elapsedTime = 0f; // Track time elapsed since the start of the movement

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                // Calculate the current speed using linear interpolation
                float currentSpeed = Mathf.Lerp(moveSpeed, moveSpeedFastest, elapsedTime / reachFastMoveSpeedInSeconds);

                // Update elapsed time
                elapsedTime += Time.deltaTime;

                // Move towards the target position using the current speed
                Vector3 nextPosition = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
                transform.position = nextPosition;

                yield return null; // Wait for the next frame
            }

            // Move to grid position
            yield return StartCoroutine(MoveToGridPosition(moveSpeed));

            // Snap to grid at the very end
            transform.position = gridController.GetClosestGridPoint(transform.position);

            isMoving = false;
            movementState = MovementState.None;
        }
        IEnumerator MoveToGridPosition(float speed)
        {
            Vector3 gridPosition = gridController.GetClosestGridPoint(transform.position);

            while (Vector3.Distance(transform.position, gridPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, gridPosition, speed * Time.deltaTime);
                yield return null;
            }

            // Ensure final position is exact by snapping to grid
            transform.position = gridPosition;
        }

        Transform latestCollidingPossiblePosition;

        private void OnTriggerEnter(Collider other)
        {
            if (isMoving)
            {
                // Check if the collided object's name contains "PossiblePosition"
                if (other.gameObject.name.Contains("PossiblePosition"))
                {
                    bool canUsePosition = true;

                    // Define the size and offset of the box cast to check for overlapping blocks
                    Vector3 boxCastSize = new Vector3(transform.lossyScale.x / 2f, transform.lossyScale.y / 2f, transform.lossyScale.z / 2f); // Adjust this size based on your grid and block dimensions
                    Vector3 castPosition = other.transform.position;

                    // Perform a box cast to detect nearby colliders
                    Collider[] hitColliders = Physics.OverlapBox(castPosition, boxCastSize / 2, Quaternion.identity);
                    foreach (Collider hitCollider in hitColliders)
                    {
                        // Ignore the current block itself
                        if (hitCollider.gameObject == gameObject) continue;

                        // Check if the collider belongs to another "Block" object
                        Block block = hitCollider.GetComponent<Block>();
                        if (block != null && !block.isMoving)
                        {
                            // If another block is detected and is not moving, we cannot use this position
                            canUsePosition = false;
                            break;
                        }
                    }

                    // If the position is clear (either no block or only moving blocks), set latestCollidingPossiblePosition
                    if (canUsePosition)
                    {
                        latestCollidingPossiblePosition = other.transform;
                    }
                }

                // Check if the collided object's name contains the specified string
                if (other.gameObject.name.Contains(collidingObjectsName))
                {
                    if (isCleared) return;

                    if (movementState == MovementState.MovingForward)
                    {
                        OnHitAnotherBlock.Invoke();

                        GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Block_HitObstacle();

                        Block otherBlock = other.GetComponent<Block>();
                        if (otherBlock != null)
                        {
                            // Kiểm tra điều kiện thua: 2 block đối nhau va chạm (nếu không được cho phép)
                            if (AreBlocksOpposite(this, otherBlock))
                            {
                                // Kiểm tra xem block này có cho phép đối đầu không
                                if (!allowOppositeCollision)
                                {
                                    LevelManager levelManager = GetComponentInParent<LevelManager>();
                                    if (levelManager != null)
                                    {
                                        levelManager.OnBlocksCollided();
                                    }
                                }
                            }

                            if (otherBlock.redErrorOutlineAnim != null)
                                otherBlock.redErrorOutlineAnim.Play();
                        }

                        // Stop current movement
                        StopAllCoroutines();

                        // Get the transform of the target "PossiblePosition" using PossibleGridPositions script
                        Transform possiblePositionTransform = possibleGridPositions.FindPossiblePosition(transform.position, moveDirection);

                        if (possiblePositionTransform != null)
                        {
                            //StartCoroutine(MoveToPossiblePosition(possiblePositionTransform.position));
                            StartCoroutine(MoveToPossibleTransform(latestCollidingPossiblePosition));
                        }
                        else
                        {
                            // If no possible position is found, move back slightly as a fallback
                            StartCoroutine(MoveBackAfterCollision());
                        }

                        // Start bounce on the collided block
                        StartBounce(other.gameObject, moveDirection);

                        isMoving = false;
                        movementState = MovementState.None;
                    }
                }
            }
        }

        IEnumerator MoveToPossiblePosition(Vector3 targetPosition)
        {
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, bounceSpeed * Time.deltaTime);
                yield return null;
            }

            // Snap to grid at the very end
            //transform.position = gridController.GetClosestGridPoint(transform.position);
            transform.position = targetPosition;
        }

        IEnumerator MoveToPossibleTransform(Transform targetTransform)
        {
            while (Vector3.Distance(transform.position, targetTransform.position) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, bounceSpeed * Time.deltaTime);
                yield return null;
            }

            // Snap to grid at the very end
            //transform.position = gridController.GetClosestGridPoint(transform.position);
            transform.position = targetTransform.position;
        }

        IEnumerator MoveBackAfterCollision()
        {
            // Move back by bounceAmount in the opposite direction
            Vector3 oppositeDirection = -moveDirection;
            Vector3 targetPosition = transform.position + oppositeDirection * bounceAmount * 2f;

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, bounceSpeed * Time.deltaTime);
                yield return null;
            }

            // Move to grid position
            yield return StartCoroutine(MoveToGridPosition(bounceSpeed));

            // Snap to grid at the very end
            transform.position = gridController.GetClosestGridPoint(transform.position);
        }

        void StartBounce(GameObject collidedBlock, Vector3 direction)
        {
            Block blockScript = collidedBlock.GetComponent<Block>();
            if (blockScript != null)
            {
                blockScript.Bounce(direction);
            }
        }

        public void Bounce(Vector3 direction)
        {
            if (!isMoving)
            {
                StartCoroutine(BounceCoroutine(direction));
            }
        }

        IEnumerator BounceCoroutine(Vector3 direction)
        {
            isMoving = true;
            movementState = MovementState.Bouncing;
            Vector3 startPosition = transform.localPosition;
            Vector3 bouncePosition = startPosition + direction * bounceAmount;

            // Calculate time to reach bouncePosition
            float timeToBouncePosition = Vector3.Distance(startPosition, bouncePosition) / bounceSpeed;

            // Before starting to move towards bouncePosition, do a raycast
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, rayCheckDistanceForNextBlockOnCollision))
            {
                // Check if the hit object is a block we should interact with
                if (hit.collider.gameObject.name.Contains(collidingObjectsName))
                {
                    Block nextBlock = hit.collider.gameObject.GetComponent<Block>();
                    if (nextBlock != null)
                    {
                        // Schedule the next block's bounce to start when this block starts moving back
                        nextBlock.BounceAfterDelay(direction, timeToBouncePosition);
                    }
                }
            }

            // Move to bounce position
            while (Vector3.Distance(transform.localPosition, bouncePosition) > 0.01f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, bouncePosition, bounceSpeed * Time.deltaTime);
                yield return null;
            }

            // Now we start moving back to start position
            while (Vector3.Distance(transform.localPosition, startPosition) > 0.01f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPosition, bounceSpeed * Time.deltaTime);
                yield return null;
            }

            // Move to grid position with same speed
            //yield return StartCoroutine(MoveToGridPosition(bounceSpeed));

            // Snap to grid at the very end
            //transform.position = gridController.GetClosestGridPoint(transform.position);
            transform.localPosition = startPosition;

            isMoving = false;
            movementState = MovementState.None;
        }

        public void BounceAfterDelay(Vector3 direction, float delay)
        {
            if (!isMoving)
            {
                StartCoroutine(BounceAfterDelayCoroutine(direction, delay));
            }
        }

        IEnumerator BounceAfterDelayCoroutine(Vector3 direction, float delay)
        {
            yield return new WaitForSeconds(delay);
            Bounce(direction);
        }

        void DecreaseAvailableMoves(int decreaseMovesBy)
        {
            if (movesConstraint != null)
                movesConstraint.UpdateMoves(-decreaseMovesBy);
        }

        public void IncreaseCollectedCount()
        {
            if (increasedCounter) return;
            increasedCounter = true;

            GetComponentInParent<CollectedStacksCounter>().IncreaseCollectedPieces(1, gameObject.GetInstanceID());
        }

        public void FixCentralPivotFor3dRotatableObject()
        {
            if (dragRotateObj != null)
            {
                dragRotateObj.RepositionCentralPivot(transform);
            }
        }

        public void ReduceLockedBlockCounter()
        {
            foreach (LockBlock lockedBlock in lockedBlocks)
                lockedBlock.ReduceLockedCounter();
        }

        public void PlayTappedSoundEffect()
        {
            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Block_Tapped();
        }

        public bool IsMoving()
        {
            return movementState != MovementState.None;
        }

		/// <summary>
		/// Force-set the internal moving flag/state. Use with care.
		/// </summary>
		public void SetMovingFlag(bool moving)
		{
			isMoving = moving;
			movementState = moving ? MovementState.MovingForward : MovementState.None;
		}

		/// <summary>
		/// Programmatically start a forward move if the block is allowed to move and is idle.
		/// Returns true if a move was started.
		/// </summary>
		public bool TryStartMove()
		{
			if (lockedBlock != null && lockedBlock.isLocked) return false;
			if (!canMove) return false;
			if (isMoving) return false;

			DecreaseAvailableMoves(1);
			moveDirection = GetDirectionVector(movementDirection);

			// Hidden counters logic mirrors click start
			if (LockBlock.TryMarkDecreased(LockBlock.DecreaseSource.Move))
				ReduceLockedBlockCounter();

			OnAnyBlockMoveStarted?.Invoke(this);
			StartCoroutine(MoveForward());
			return true;
		}

        /// <summary>
        /// Get the current movement direction vector based on the block's movementDirection enum
        /// </summary>
        public Vector3 GetCurrentMovementDirection()
        {
            return GetDirectionVector(movementDirection);
        }

        /// <summary>
        /// Kiểm tra xem 2 block có đối nhau không (di chuyển ngược chiều nhau)
        /// </summary>
        private bool AreBlocksOpposite(Block block1, Block block2)
        {
            Vector3 direction1 = block1.GetDirectionVector(block1.movementDirection);
            Vector3 direction2 = block2.GetDirectionVector(block2.movementDirection);
            
            // Kiểm tra xem 2 block có di chuyển ngược chiều nhau không
            // Ví dụ: Forward vs Back, Up vs Down, Right vs Left
            return Vector3.Dot(direction1, direction2) < -0.9f; // Gần như ngược chiều nhau
        }

		/// <summary>
		/// Redirect this block mid-move: rotate by the given local axis/angle and immediately
		/// continue moving one grid step using the same movementDirection relative to the new rotation.
		/// Does not consume an extra move.
		/// </summary>
		public void RedirectDuringMove(Vector3 localAxis, float rotationDegrees)
		{
			if (!isMoving || movementState != MovementState.MovingForward) return;

			// Stop current forward step cleanly
			StopAllCoroutines();

			// Apply rotation
			Vector3 axis = localAxis.sqrMagnitude < Mathf.Epsilon ? new Vector3(0f, 0f, 1f) : localAxis.normalized;
			Quaternion delta = Quaternion.AngleAxis(rotationDegrees, axis);
			transform.localRotation = transform.localRotation * delta;

			// Recompute move direction based on current enum relative to new rotation
			moveDirection = GetDirectionVector(movementDirection);

			// Reset flags and start a fresh forward step from current position
			isMoving = false;
			movementState = MovementState.None;
			StartCoroutine(MoveForward());
		}
    }
}

