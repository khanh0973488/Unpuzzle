using UnityEngine;
using System.Collections.Generic;

namespace HyperPuzzleEngine
{
    public class TurnRedirectTile : MonoBehaviour
    {
        [Header("Filter")]
        public string redBlockNameContains = "red";

        [Header("Rotation")]
        public Vector3 localRotationAxis = new Vector3(0f, 0f, 1f); // Z axis

        [Header("Target Direction")]
        public DirectionOption targetDirection = DirectionOption.Left;

        [Header("Behavior")]
        public bool singleUse = false;
        private bool consumed = false;

        [Header("Center Alignment")]
        public float centerAlignmentThreshold = 0.05f;

        [Header("Raycast Check")]
        public float raycastDistance = 1f;
        public LayerMask obstacleLayerMask = -1;

        private readonly HashSet<int> redirectedIds = new HashSet<int>();
        private readonly Dictionary<int, Vector3> entryDirections = new Dictionary<int, Vector3>();

        public enum DirectionOption
        {
            Left = 180,    // -X (trái)
            Right = 0,     // +X (phải)
            Up = 90,       // +Z (lên/trên)
            Down = 270     // -Z (xuống/dưới)
        }

        private void OnTriggerEnter(Collider other)
        {
            if (consumed) return;

            Block block = other.GetComponent<Block>();
            if (block == null) return;

            int id = block.gameObject.GetInstanceID();
            if (redirectedIds.Contains(id)) return;

            if (!string.IsNullOrEmpty(redBlockNameContains))
            {
                if (!other.gameObject.name.ToLower().Contains(redBlockNameContains.ToLower()))
                    return;
            }

            Debug.Log("đang ở trên ô hồng: ENTER");

            if (block.IsMoving())
                entryDirections[id] = block.GetCurrentMovementDirection();
        }

        private void OnTriggerStay(Collider other)
        {
            if (consumed) return;

            Block block = other.GetComponent<Block>();
            if (block == null) return;
            Debug.Log("đang ở trên ô hồng: STAY");
            int id = block.gameObject.GetInstanceID();
            Debug.Log("id: " + id);
            if (redirectedIds.Contains(id))
            {
                Debug.Log("đã được redirect");
                
                // Kiểm tra raycast để quyết định có gọi OnMouseUpAsButton hay không
                if (IsPathBlocked(block))
                {
                    Debug.Log("đường đi bị chặn - không gọi OnMouseUpAsButton (block sẽ dừng lại)");
                    return;
                }
                else
                {
                    Debug.Log("đường đi không bị chặn - gọi OnMouseUpAsButton (block sẽ tự di chuyển tiếp)");
                    block.OnMouseUpAsButton();
                    return;
                }
            }

            // Debug current moving state and force moving true while on pink tile
            Debug.Log(block.IsMoving() ? "đang di chuyển" : "đang không di chuyển");

            // If the block is idle while overlapping this tile, kick off a move forward
            if (!block.IsMoving())
            {
                // Store entry direction immediately after starting to move
                if (block.TryStartMove())
                {
                    entryDirections[id] = block.GetCurrentMovementDirection();
                }
                else
                {
                    // If we couldn't start move (e.g., after impact), keep moving flag true
                    block.SetMovingFlag(true);
                }
            }

            if (!entryDirections.ContainsKey(id)) return;

            Vector3 tileCenter = GetComponent<Collider>().bounds.center;
            Vector3 blockCenter = other.bounds.center;
            Vector3 diff = blockCenter - tileCenter;
            diff.y = 0f;

            if (diff.magnitude <= centerAlignmentThreshold)
            {
                Vector3 entryDir = entryDirections[id];
                float requiredRotation = CalculateRequiredRotation(entryDir);

                block.RedirectDuringMove(localRotationAxis, requiredRotation);
                redirectedIds.Add(id);

                if (singleUse) consumed = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Block block = other.GetComponent<Block>();
            if (block == null) return;

            int id = block.gameObject.GetInstanceID();
            redirectedIds.Remove(id);
            entryDirections.Remove(id);

            Debug.Log("ko đang ở trên ô hồng: EXIT");

            // When leaving the pink tile, do not force the moving flag
        }

        private float CalculateRequiredRotation(Vector3 entryDirection)
        {
            entryDirection.Normalize();

            // Tính góc từ hướng vào (entry direction)
            float entryAngle = Mathf.Atan2(entryDirection.z, entryDirection.x) * Mathf.Rad2Deg;
            if (entryAngle < 0) entryAngle += 360f;

            // Góc mục tiêu từ enum
            float targetExitDirection = (float)targetDirection;

            // Tính góc cần xoay
            float rotationNeeded = targetExitDirection - entryAngle;

            // Chuẩn hóa về khoảng [-180, 180]
            while (rotationNeeded > 180f) rotationNeeded -= 360f;
            while (rotationNeeded < -180f) rotationNeeded += 360f;

            // Xác định hướng vào và hướng ra
            bool isEntryVertical = Mathf.Abs(entryDirection.z) > Mathf.Abs(entryDirection.x);
            bool isTargetVertical = (targetDirection == DirectionOption.Up || targetDirection == DirectionOption.Down);

            // Áp dụng flip logic khác nhau cho từng trường hợp
            if (isEntryVertical && !isTargetVertical)
            {
                // Entry từ trên/dưới → Target trái/phải
                rotationNeeded = -rotationNeeded;
            }
            else if (!isEntryVertical && isTargetVertical)
            {
                // Entry từ trái/phải → Target trên/dưới
                rotationNeeded = -rotationNeeded;
            }
            // Các trường hợp còn lại giữ nguyên:
            // - Entry ngang → Target ngang (Left/Right → Left/Right)
            // - Entry dọc → Target dọc (Up/Down → Up/Down)

            return rotationNeeded;
        }

        private bool IsPathBlocked(Block block)
        {
            // Lấy vị trí hiện tại của block
            Vector3 blockPosition = block.transform.position;
            
            // Tính toán hướng target dựa trên targetDirection
            Vector3 targetDirection = GetTargetDirection();
            
            // Thực hiện raycast từ vị trí block theo hướng target
            Ray ray = new Ray(blockPosition, targetDirection);
            RaycastHit hit;
            
            Debug.DrawRay(blockPosition, targetDirection * raycastDistance, Color.red, 0.1f);
            
            if (Physics.Raycast(ray, out hit, raycastDistance, obstacleLayerMask))
            {
                // LƯU Ý: Logic loại trừ này CHỈ dành cho TurnRedirectTile script này
                // KHÔNG áp dụng cho các script khác để tránh ảnh hưởng đến logic game khác
                // Loại trừ chính block hiện tại và các object không phải vật cản
                if (hit.collider.gameObject == block.gameObject || 
                    hit.collider.name.Contains("PossiblePosition") ||
                    hit.collider.name.Contains("TurnRedirect") ||
                    hit.collider.name.Contains("Tile"))
                {
                    Debug.Log($"Raycast hit ignored: {hit.collider.name} (không phải vật cản)");
                    return false; // Không coi là bị chặn
                }
                
                Debug.Log($"Raycast hit vật cản thực sự: {hit.collider.name} at distance {hit.distance}");
                return true; // Có vật cản thực sự
            }
            
            return false; // Không có vật cản
        }

        private Vector3 GetTargetDirection()
        {
            switch (targetDirection)
            {
                case DirectionOption.Left:
                    return Vector3.left;
                case DirectionOption.Right:
                    return Vector3.right;
                case DirectionOption.Up:
                    return Vector3.forward;
                case DirectionOption.Down:
                    return Vector3.back;
                default:
                    return Vector3.forward;
            }
        }
    }
}