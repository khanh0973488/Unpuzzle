using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Eldvmo.Ripples;

namespace HyperPuzzleEngine
{
    /// <summary>
    /// Component that can be added to any block to make it behave like a bomb
    /// When hit by another block, it will explode and destroy surrounding blocks
    /// </summary>
    public class BombComponent : MonoBehaviour
    {
        [Header("Bomb Settings")]
        [Tooltip("Enable bomb functionality")]
        public bool isBomb = false;

        [Tooltip("Radius of explosion - how far the bomb can destroy blocks")]
        public float explosionRadius = 2f;

        [Tooltip("Delay before explosion after being hit")]
        public float explosionDelay = 0.1f;

        [Tooltip("Whether the bomb itself should be destroyed after explosion")]
        public bool destroySelfAfterExplosion = true;

        [Tooltip("Layers to check for blocks to destroy")]
        public LayerMask targetLayers = -1;

        [Header("Destruction Settings")]
        [Tooltip("Spawn particle effect when destroying blocks")]
        public bool spawnParticleOnDestroy = true;

        [Tooltip("Increase collected pieces counter when destroying blocks")]
        public bool increaseCollectedPiecesCounterOnDestroy = true;

        [Header("Visual Effects")]
        [Tooltip("Particle effect to spawn at bomb position during explosion")]
        public GameObject explosionEffect;

        [Tooltip("Sound effect to play during explosion")]
        public AudioClip explosionSound;

        [Header("Vortex Effect Settings")]
        [Tooltip("Set vortex size to this value when bomb explodes (for RingRipple_Lite shader)")]
        public float vortexSizeOnExplosion = 0.2f;

        [Tooltip("Automatically find and update RingRipple_Lite shader materials")]
        public bool updateVortexSizeOnExplosion = true;

        [Tooltip("Duration to animate vortex size from current value to target (seconds)")]
        public float vortexSizeAnimationDuration = 1f;

        [Tooltip("Animation curve for vortex size growth (X: time 0-1, Y: size 0-1)")]
        public AnimationCurve vortexSizeAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Block Rotation Effect")]
        [Tooltip("Enable rotation effect - blocks rotate towards center clockwise")]
        public bool enableBlockRotation = true;

        [Tooltip("Use bounds center as rotation center (if false, uses transform.position)")]
        public bool useRenderBoundsCenter = true;

        [Tooltip("Delay before starting rotation effect (seconds)")]
        public float rotationStartDelay = 0.5f;

        [Tooltip("Rotation speed (degrees per second)")]
        public float rotationSpeed = 180f;

        [Tooltip("Pull speed towards center (units per second)")]
        public float pullSpeedTowardsCenter = 2f;

        [Tooltip("Duration of rotation effect (seconds)")]
        public float rotationDuration = 2f;

        [Tooltip("Minimum distance to stop pulling (blocks closer than this stop rotating)")]
        public float rotationStopDistance = 0.3f;

        [Tooltip("Spiral direction: true = expand outward (rộng ra), false = contract inward (thu vào)")]
        public bool spiralOutward = false;

        [Tooltip("Maximum radius when spiraling outward (chỉ dùng khi spiralOutward = true)")]
        public float maxSpiralRadius = 5f;

        [Tooltip("Animation curve for rotation effect (X: time 0-1, Y: effect 0-1)")]
        public AnimationCurve rotationAnimationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        [Tooltip("Extra spin up factor over time (0 = constant speed, 2 = up to 3x)")]
        public float spinUpFactor = 2f;

        [Tooltip("Tighten spiral radius faster towards center (higher = tighter)")]
        public float radiusDecayExponent = 3f;

        [Tooltip("Optional curve controlling radius decay (overrides exponent if set)")]
        public AnimationCurve radiusDecayCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Y Sinking Effect")]
        [Tooltip("Enable Y sinking effect - blocks sink down when rotating (like being sucked into water)")]
        public bool enableYSinking = true;

        [Tooltip("Maximum Y sinking amount (units to sink down)")]
        public float maxYSinkingAmount = 3f;

        [Tooltip("Animation curve for Y sinking (X: time 0-1, Y: sinking 0-1)")]
        public AnimationCurve ySinkingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Scale Effect")]
        [Tooltip("Enable scale shrinking effect - blocks shrink when rotating")]
        public bool enableScaleShrinking = true;

        [Tooltip("Minimum scale (blocks will shrink to this size, 0.2 = 20% of original size)")]
        public float minScale = 0.2f;

        [Tooltip("Animation curve for scale shrinking (X: time 0-1, Y: scale 0-1)")]
        public AnimationCurve scaleShrinkingCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private bool hasExploded = false;
        private AudioSource audioSource;
        private Block blockComponent;
        private List<GameObject> rotatingBlocks = new List<GameObject>();
        private GameObject triggeringBlock = null; // Block đâm vào để kích hoạt bomb
        [Header("Self Rotation")]
        [Tooltip("Enable each block to spin around its own Y axis while rotating toward the center")]
        public bool enableSelfRotation = true;

        [Tooltip("Speed of self rotation in degrees per second")]
        public float selfRotationSpeed = 360f;


        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            blockComponent = GetComponent<Block>();

            // Khởi tạo animation curve mặc định nếu chưa có
            if (vortexSizeAnimationCurve == null || vortexSizeAnimationCurve.keys.Length == 0)
            {
                vortexSizeAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isBomb || hasExploded) return;

            if (IsValidTarget(other.gameObject))
            {
                triggeringBlock = other.gameObject; // Lưu lại block kích hoạt
                StartCoroutine(ExplodeWithDelay());
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isBomb || hasExploded) return;

            if (IsValidTarget(collision.gameObject))
            {
                triggeringBlock = collision.gameObject; // Lưu lại block kích hoạt
                StartCoroutine(ExplodeWithDelay());
            }
        }

        /// <summary>
        /// Explodes with a delay, destroying surrounding blocks
        /// </summary>
        private IEnumerator ExplodeWithDelay()
        {
            hasExploded = true;

            // Disable block movement if it has Block component
            if (blockComponent != null)
            {
                blockComponent.RemoveColliderAndSetNonClickable();
            }

            // Animate vortex size to target value when bomb is activated
            if (updateVortexSizeOnExplosion)
            {
                StartCoroutine(AnimateVortexSize(vortexSizeOnExplosion, vortexSizeAnimationDuration));
            }

            // Start rotating blocks towards center
            if (enableBlockRotation)
            {
                StartCoroutine(RotateBlocksToCenter());
            }

            // Wait for explosion delay
            yield return new WaitForSeconds(explosionDelay);

            // Perform explosion
            Explode();
        }

        /// <summary>
        /// Performs the actual explosion, destroying blocks in radius
        /// </summary>
        private void Explode()
        {
            // Play explosion sound
            if (explosionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(explosionSound);
            }

            // Spawn explosion effect at bomb position
            if (explosionEffect != null)
            {
                GameObject effect = Instantiate(explosionEffect, transform.position, transform.rotation);
                Destroy(effect, 3f); // Clean up after 3 seconds
            }

            // Find all colliders within explosion radius
            Collider[] collidersInRange = Physics.OverlapSphere(transform.position, explosionRadius, targetLayers);

            List<GameObject> blocksToDestroy = new List<GameObject>();

            // Collect valid blocks to destroy
            foreach (Collider col in collidersInRange)
            {
                if (IsValidTarget(col.gameObject) && col.gameObject != gameObject)
                {
                    blocksToDestroy.Add(col.gameObject);
                }
            }

            // Process all collected blocks - unlock locked blocks, destroy others
            foreach (GameObject block in blocksToDestroy)
            {
                ProcessBlock(block);
            }

            // Count the bomb itself as collected if it should be destroyed
            if (destroySelfAfterExplosion)
            {
                // Count the bomb block itself as collected
                if (increaseCollectedPiecesCounterOnDestroy)
                {
                    Block bombBlockComponent = GetComponent<Block>();
                    if (bombBlockComponent != null)
                    {
                        Debug.Log($"BombComponent: Counting bomb itself as collected");
                        bombBlockComponent.IncreaseCollectedCount();
                    }
                    else
                    {
                        Debug.Log($"BombComponent: No Block component on bomb, using fallback counter");
                        CollectedStacksCounter counter = GetComponentInParent<CollectedStacksCounter>();
                        if (counter != null)
                        {
                            counter.IncreaseCollectedPieces(1, gameObject.GetInstanceID());
                        }
                    }
                }

                Destroy(gameObject, 0.1f); // Small delay to ensure other blocks are destroyed first
            }
            else
            {
                // Reset explosion state so it can explode again
                hasExploded = false;
            }
        }

        /// <summary>
        /// Processes a target block - unlocks if locked, destroys if not locked
        /// </summary>
        private void ProcessBlock(GameObject targetBlock)
        {
            if (targetBlock == null) return;

            // Check if the block is locked
            LockBlock lockBlockComponent = targetBlock.GetComponent<LockBlock>();
            if (lockBlockComponent != null && lockBlockComponent.isLocked)
            {
                // Block is locked - instantly unlock it by bomb (set to 0 regardless of current count)
                Debug.Log($"BombComponent: Instantly unlocking locked block {targetBlock.name} by bomb");
                lockBlockComponent.UnlockByBomb();
                return;
            }

            // Block is not locked - destroy it normally
            DestroyBlock(targetBlock);
        }

        /// <summary>
        /// Destroys a target block using the same logic as RotatingBlade
        /// </summary>
        private void DestroyBlock(GameObject targetBlock)
        {
            if (targetBlock == null) return;

            // Get the Block component to trigger OnThisCanBeCleared event
            Block blockComponent = targetBlock.GetComponent<Block>();
            if (blockComponent != null)
            {
                blockComponent.OnThisCanBeCleared.Invoke();
            }

            // Spawn particle effect
            if (spawnParticleOnDestroy)
            {
                GameObject spawnedParticle = Instantiate(Resources.Load("MovableCubeDestroyParticle") as GameObject, transform);
                spawnedParticle.transform.parent = null;
                spawnedParticle.transform.position = targetBlock.transform.position;

                MeshRenderer targetRenderer = targetBlock.GetComponent<MeshRenderer>();
                if (targetRenderer != null)
                {
                    spawnedParticle.GetComponent<ParticleSystemRenderer>().material = targetRenderer.material;
                }

                Destroy(spawnedParticle, 2f);
            }

            // Increase collected pieces counter using Block's method
            if (increaseCollectedPiecesCounterOnDestroy)
            {
                Block targetBlockComponent = targetBlock.GetComponent<Block>();
                if (targetBlockComponent != null)
                {
                    targetBlockComponent.IncreaseCollectedCount();
                }
                else
                {
                    // Fallback to direct counter if no Block component
                    CollectedStacksCounter counter = GetComponentInParent<CollectedStacksCounter>();
                    if (counter != null)
                    {
                        counter.IncreaseCollectedPieces(1, targetBlock.GetInstanceID());
                    }
                }
            }

            // Play sound effect
            SoundsManagerForTemplate soundManager = GetComponentInParent<SoundsManagerForTemplate>();
            if (soundManager != null)
            {
                soundManager.PlaySound_Block_DestroyedByObstacle();
            }

            // Reduce locked block counter
            ReduceLockedBlockCounter();

            // Destroy the target block
            Destroy(targetBlock);
        }

        /// <summary>
        /// Reduces the locked block counter using the same logic as RotatingBlade
        /// </summary>
        private void ReduceLockedBlockCounter()
        {
            if (!LockBlock.TryMarkDecreased(LockBlock.DecreaseSource.Blade)) return;

            ShowcaseParent showcaseParent = GetComponentInParent<ShowcaseParent>();
            if (showcaseParent != null)
            {
                foreach (LockBlock lockedBlock in showcaseParent.GetComponentsInChildren<LockBlock>())
                {
                    lockedBlock.ReduceLockedCounter();
                }
            }
        }

        /// <summary>
        /// Checks if a GameObject is a valid target for destruction
        /// </summary>
        private bool IsValidTarget(GameObject target)
        {
            return target != null && target.name.Contains("MovableCube");
        }

        /// <summary>
        /// Xoay các block trong phạm vi về tâm hoặc rộng ra theo chiều kim đồng hồ
        /// </summary>
        /// <summary>
        /// Xoay các block trong phạm vi về tâm hoặc rộng ra theo chiều kim đồng hồ
        /// </summary>
        private IEnumerator RotateBlocksToCenter()
        {
            // Chờ một khoảng thời gian trước khi bắt đầu xoay
            if (rotationStartDelay > 0f)
            {
                yield return new WaitForSeconds(rotationStartDelay);
            }

            // Xác định tâm xoay
            Vector3 centerPosition;
            if (useRenderBoundsCenter)
            {
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    centerPosition = renderer.bounds.center;
                    Debug.Log($"BombComponent: Sử dụng bounds center làm tâm xoay: {centerPosition}");
                }
                else
                {
                    Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
                    if (childRenderers.Length > 0)
                    {
                        Bounds combinedBounds = childRenderers[0].bounds;
                        for (int i = 1; i < childRenderers.Length; i++)
                        {
                            combinedBounds.Encapsulate(childRenderers[i].bounds);
                        }
                        centerPosition = combinedBounds.center;
                        Debug.Log($"BombComponent: Sử dụng combined bounds center làm tâm xoay: {centerPosition}");
                    }
                    else
                    {
                        centerPosition = transform.position;
                        Debug.LogWarning($"BombComponent: Không tìm thấy Renderer, sử dụng transform.position: {centerPosition}");
                    }
                }
            }
            else
            {
                centerPosition = transform.position;
                Debug.Log($"BombComponent: Sử dụng transform.position làm tâm xoay: {centerPosition}");
            }

            rotatingBlocks.Clear();

            // Đảm bảo block kích hoạt được thêm vào danh sách
            if (triggeringBlock != null && IsValidTarget(triggeringBlock) && triggeringBlock != gameObject)
            {
                if (!rotatingBlocks.Contains(triggeringBlock))
                {
                    rotatingBlocks.Add(triggeringBlock);
                    Block triggerBlock = triggeringBlock.GetComponent<Block>();
                    if (triggerBlock != null)
                    {
                        triggerBlock.SetMovingFlag(false);
                        triggerBlock.StopAllCoroutines();
                        triggerBlock.canMove = false;
                    }
                }
            }

            // Tìm tất cả các block trong phạm vi
            Collider[] collidersInRange = Physics.OverlapSphere(centerPosition, explosionRadius, targetLayers);
            foreach (Collider col in collidersInRange)
            {
                if (IsValidTarget(col.gameObject) && col.gameObject != gameObject)
                {
                    if (!rotatingBlocks.Contains(col.gameObject))
                    {
                        rotatingBlocks.Add(col.gameObject);
                        Block block = col.gameObject.GetComponent<Block>();
                        if (block != null)
                        {
                            block.SetMovingFlag(false);
                            block.StopAllCoroutines();
                            block.canMove = false;
                        }
                    }
                }
            }

            if (rotatingBlocks.Count == 0)
            {
                yield break;
            }

            // Lưu trạng thái ban đầu của các block
            Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();
            Dictionary<GameObject, float> initialAngles = new Dictionary<GameObject, float>();
            Dictionary<GameObject, float> blockStartTimes = new Dictionary<GameObject, float>();
            Dictionary<GameObject, float> initialYPositions = new Dictionary<GameObject, float>();
            Dictionary<GameObject, Vector3> initialScales = new Dictionary<GameObject, Vector3>();

            foreach (GameObject block in rotatingBlocks)
            {
                if (block == null) continue;

                Vector3 blockPos = block.transform.position;
                Vector3 dirToCenter = centerPosition - blockPos;
                float distToCenter = dirToCenter.magnitude;

                if (distToCenter <= rotationStopDistance)
                {
                    if (dirToCenter.magnitude < 0.01f)
                        dirToCenter = new Vector3(1, 0, 0) * rotationStopDistance;
                    else
                        dirToCenter = dirToCenter.normalized * rotationStopDistance;

                    blockPos = centerPosition - dirToCenter;
                    distToCenter = rotationStopDistance;
                }

                initialPositions[block] = blockPos;
                initialYPositions[block] = blockPos.y;
                initialScales[block] = block.transform.localScale;
                initialAngles[block] = Mathf.Atan2(dirToCenter.z, dirToCenter.x) * Mathf.Rad2Deg;
                blockStartTimes[block] = 0f;
            }

            float elapsedTime = 0f;

            while (elapsedTime < rotationDuration && rotatingBlocks.Count > 0)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / rotationDuration);

                float curveValue = rotationAnimationCurve != null && rotationAnimationCurve.keys.Length > 0
                    ? rotationAnimationCurve.Evaluate(progress)
                    : 1f;

                // Liên tục thêm block mới vào (nếu có)
                Collider[] newCollidersInRange = Physics.OverlapSphere(centerPosition, explosionRadius, targetLayers);
                foreach (Collider col in newCollidersInRange)
                {
                    if (IsValidTarget(col.gameObject) && col.gameObject != gameObject)
                    {
                        if (!rotatingBlocks.Contains(col.gameObject))
                        {
                            rotatingBlocks.Add(col.gameObject);
                            Block block = col.gameObject.GetComponent<Block>();
                            if (block != null)
                            {
                                block.SetMovingFlag(false);
                                block.StopAllCoroutines();
                                block.canMove = false;
                            }
                            if (!initialPositions.ContainsKey(col.gameObject))
                            {
                                initialPositions[col.gameObject] = col.transform.position;
                                initialYPositions[col.gameObject] = col.transform.position.y;
                                initialScales[col.gameObject] = col.transform.localScale;
                                Vector3 directionToCenter = centerPosition - col.transform.position;
                                initialAngles[col.gameObject] = Mathf.Atan2(directionToCenter.z, directionToCenter.x) * Mathf.Rad2Deg;
                                blockStartTimes[col.gameObject] = elapsedTime;
                            }
                        }
                    }
                }

                rotatingBlocks.RemoveAll(block => block == null);

                foreach (GameObject block in rotatingBlocks)
                {
                    if (block == null) continue;

                    Block blockComponent = block.GetComponent<Block>();
                    if (blockComponent != null)
                    {
                        blockComponent.SetMovingFlag(false);
                        blockComponent.StopAllCoroutines();
                        blockComponent.canMove = false;
                    }

                    Vector3 currentPosition = block.transform.position;
                    Vector3 directionToCenter = centerPosition - currentPosition;
                    float distanceToCenter = directionToCenter.magnitude;

                    if (distanceToCenter <= rotationStopDistance)
                    {
                        distanceToCenter = rotationStopDistance;
                        directionToCenter = directionToCenter.normalized * rotationStopDistance;
                        if (directionToCenter.magnitude < 0.01f)
                            directionToCenter = new Vector3(1, 0, 0) * rotationStopDistance;
                    }

                    if (!initialPositions.ContainsKey(block))
                    {
                        initialPositions[block] = block.transform.position;
                        initialYPositions[block] = block.transform.position.y;
                        initialScales[block] = block.transform.localScale;
                        initialAngles[block] = Mathf.Atan2(directionToCenter.z, directionToCenter.x) * Mathf.Rad2Deg;
                        blockStartTimes[block] = elapsedTime;
                    }

                    float initialDistance = Vector3.Distance(initialPositions[block], centerPosition);
                    float initialAngle = initialAngles[block];
                    float initialY = initialYPositions[block];
                    Vector3 initialScale = initialScales[block];

                    float blockElapsedTime = elapsedTime - blockStartTimes[block];
                    float blockProgress = Mathf.Clamp01(blockElapsedTime / rotationDuration);

                    float speedMultiplier = 1f + spinUpFactor * blockProgress;
                    float currentRotationSpeed = rotationSpeed * speedMultiplier;

                    float radiusT = blockProgress;
                    float currentRadius;
                    if (spiralOutward)
                    {
                        if (radiusDecayCurve != null && radiusDecayCurve.keys.Length > 0)
                            radiusT = radiusDecayCurve.Evaluate(blockProgress);
                        currentRadius = Mathf.Lerp(initialDistance, maxSpiralRadius, Mathf.Clamp01(radiusT));
                    }
                    else
                    {
                        if (radiusDecayCurve != null && radiusDecayCurve.keys.Length > 0)
                            radiusT = radiusDecayCurve.Evaluate(blockProgress);
                        else
                            radiusT = 1f - Mathf.Exp(-radiusDecayExponent * blockProgress);

                        currentRadius = Mathf.Lerp(initialDistance, rotationStopDistance, radiusT);
                        currentRadius = Mathf.Max(currentRadius, rotationStopDistance);
                    }

                    float totalRotation = -currentRotationSpeed * blockElapsedTime * curveValue;
                    float newAngle = initialAngle + totalRotation;

                    float radians = newAngle * Mathf.Deg2Rad;
                    Vector3 newDirection = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians));
                    Vector3 targetPosition = centerPosition + newDirection * currentRadius;

                    float targetY = initialY;
                    if (enableYSinking)
                    {
                        float sinkingProgress = ySinkingCurve != null && ySinkingCurve.keys.Length > 0
                            ? ySinkingCurve.Evaluate(blockProgress)
                            : blockProgress;
                        float ySinkAmount = maxYSinkingAmount * sinkingProgress;
                        targetY = initialY - ySinkAmount;
                    }
                    targetPosition.y = targetY;

                    Vector3 targetScale = initialScale;
                    if (enableScaleShrinking)
                    {
                        float scaleProgress = scaleShrinkingCurve != null && scaleShrinkingCurve.keys.Length > 0
                            ? scaleShrinkingCurve.Evaluate(blockProgress)
                            : blockProgress;
                        float scaleValue = Mathf.Lerp(1f, minScale, scaleProgress);
                        targetScale = initialScale * scaleValue;
                    }

                    // --- Di chuyển & Scale ---
                    block.transform.position = targetPosition;
                    block.transform.localScale = Vector3.Lerp(block.transform.localScale, targetScale, Time.deltaTime * pullSpeedTowardsCenter * curveValue);

                    // --- Self rotation quanh trục Y ---
                    if (enableSelfRotation)
                    {
                        float distanceFactor = Mathf.InverseLerp(rotationStopDistance, explosionRadius, distanceToCenter);
                        float dynamicSpeed = Mathf.Lerp(selfRotationSpeed * 2f, selfRotationSpeed * 0.5f, distanceFactor);
                        block.transform.Rotate(Vector3.up, dynamicSpeed * Time.deltaTime, Space.World);

                    }
                }

                yield return null;
            }

            int blockCount = rotatingBlocks.Count;
            foreach (GameObject block in rotatingBlocks)
            {
                if (block == null) continue;
                Vector3 directionToCenter = centerPosition - block.transform.position;
                float distance = directionToCenter.magnitude;

                if (spiralOutward)
                {
                    if (distance < maxSpiralRadius)
                    {
                        Vector3 finalPos = centerPosition - directionToCenter.normalized * maxSpiralRadius;
                        block.transform.position = finalPos;
                    }
                }
                else
                {
                    if (distance > rotationStopDistance)
                    {
                        float targetDistance = Mathf.Max(rotationStopDistance, distance * 0.3f);
                        Vector3 finalPos = centerPosition - directionToCenter.normalized * targetDistance;
                        block.transform.position = finalPos;
                    }
                }
            }

            rotatingBlocks.Clear();
            Debug.Log($"BombComponent: Đã hoàn thành xoay {blockCount} blocks {(spiralOutward ? "rộng ra" : "về tâm")}");
        }


        /// <summary>
        /// Animate vortex size từ giá trị hiện tại lên target value
        /// </summary>
        private IEnumerator AnimateVortexSize(float targetSize, float duration)
        {
            // Tìm VortexAtPosition component trên cùng GameObject
            VortexAtPosition vortexComponent = GetComponent<VortexAtPosition>();

            if (vortexComponent == null)
            {
                Debug.LogWarning($"BombComponent: Không tìm thấy VortexAtPosition component trên {gameObject.name}");
                yield break;
            }

            // Dùng reflection để lấy ripplePlane từ VortexAtPosition (vì nó là private field)
            FieldInfo ripplePlaneField = typeof(VortexAtPosition).GetField("ripplePlane", BindingFlags.NonPublic | BindingFlags.Instance);

            if (ripplePlaneField == null)
            {
                Debug.LogWarning($"BombComponent: Không tìm thấy field 'ripplePlane' trong VortexAtPosition");
                yield break;
            }

            MeshRenderer ripplePlane = ripplePlaneField.GetValue(vortexComponent) as MeshRenderer;

            if (ripplePlane == null || ripplePlane.material == null)
            {
                Debug.LogWarning($"BombComponent: Ripple plane không được gán hoặc không có material");
                yield break;
            }

            Material mat = ripplePlane.material;

            // Kiểm tra xem material có property _VortexSize không
            if (!mat.HasProperty("_VortexSize"))
            {
                Debug.LogWarning($"BombComponent: Material không có property '_VortexSize'");
                yield break;
            }

            // Lấy giá trị hiện tại của _VortexSize
            float startSize = mat.GetFloat("_VortexSize");

            // Nếu đã đạt target, không cần animate
            if (Mathf.Approximately(startSize, targetSize))
            {
                yield break;
            }

            float elapsedTime = 0f;

            // Animate từ startSize đến targetSize
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / duration);

                // Áp dụng animation curve (nếu có, nếu không dùng linear)
                float curveValue = progress;
                if (vortexSizeAnimationCurve != null && vortexSizeAnimationCurve.keys.Length > 0)
                {
                    curveValue = vortexSizeAnimationCurve.Evaluate(progress);
                }

                // Lerp từ startSize đến targetSize
                float currentSize = Mathf.Lerp(startSize, targetSize, curveValue);

                // Set giá trị mới
                mat.SetFloat("_VortexSize", currentSize);

                yield return null;
            }

            // Đảm bảo đạt đúng target value
            mat.SetFloat("_VortexSize", targetSize);
            Debug.Log($"BombComponent: Đã animate _VortexSize từ {startSize} lên {targetSize} cho {gameObject.name}");
        }

        /// <summary>
        /// Set vortex size ngay lập tức (không animate)
        /// </summary>
        private void SetVortexSize(float size)
        {
            // Tìm VortexAtPosition component trên cùng GameObject
            VortexAtPosition vortexComponent = GetComponent<VortexAtPosition>();

            if (vortexComponent == null)
            {
                Debug.LogWarning($"BombComponent: Không tìm thấy VortexAtPosition component trên {gameObject.name}");
                return;
            }

            // Dùng reflection để lấy ripplePlane từ VortexAtPosition (vì nó là private field)
            FieldInfo ripplePlaneField = typeof(VortexAtPosition).GetField("ripplePlane", BindingFlags.NonPublic | BindingFlags.Instance);

            if (ripplePlaneField == null)
            {
                Debug.LogWarning($"BombComponent: Không tìm thấy field 'ripplePlane' trong VortexAtPosition");
                return;
            }

            MeshRenderer ripplePlane = ripplePlaneField.GetValue(vortexComponent) as MeshRenderer;

            if (ripplePlane == null || ripplePlane.material == null)
            {
                Debug.LogWarning($"BombComponent: Ripple plane không được gán hoặc không có material");
                return;
            }

            Material mat = ripplePlane.material;

            // Kiểm tra xem material có property _VortexSize không
            if (mat.HasProperty("_VortexSize"))
            {
                mat.SetFloat("_VortexSize", size);
                Debug.Log($"BombComponent: Set _VortexSize = {size} cho ripple plane của {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"BombComponent: Material không có property '_VortexSize'");
            }
        }

        /// <summary>
        /// Draw explosion radius in editor for debugging
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (isBomb)
            {
                // Xác định tâm để vẽ gizmos
                Vector3 centerPosition;
                if (useRenderBoundsCenter)
                {
                    Renderer renderer = GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        centerPosition = renderer.bounds.center;
                    }
                    else
                    {
                        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
                        if (childRenderers.Length > 0)
                        {
                            Bounds combinedBounds = childRenderers[0].bounds;
                            for (int i = 1; i < childRenderers.Length; i++)
                            {
                                combinedBounds.Encapsulate(childRenderers[i].bounds);
                            }
                            centerPosition = combinedBounds.center;
                        }
                        else
                        {
                            centerPosition = transform.position;
                        }
                    }
                }
                else
                {
                    centerPosition = transform.position;
                }

                // Vẽ explosion radius
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(centerPosition, explosionRadius);

                // Vẽ maxSpiralRadius nếu spiralOutward được bật
                if (spiralOutward)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(centerPosition, maxSpiralRadius);
                }

                // Vẽ điểm tâm
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(centerPosition, 0.1f);
            }
        }
    }
}