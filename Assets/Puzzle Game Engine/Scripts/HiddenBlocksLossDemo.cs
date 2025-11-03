using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    /// <summary>
    /// Demo script để minh họa điều kiện thua mới: còn move nhưng tất cả block còn lại đều bị ẩn
    /// </summary>
    public class HiddenBlocksLossDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [Tooltip("Số lượng block sẽ bị ẩn khi nhấn nút demo")]
        public int blocksToHide = 2;
        
        [Tooltip("Nút để kích hoạt demo (có thể gán trong Inspector)")]
        public KeyCode demoKey = KeyCode.H;
        
        private LevelManager levelManager;
        
        private void Start()
        {
            levelManager = FindObjectOfType<LevelManager>();
            if (levelManager == null)
            {
                Debug.LogWarning("HiddenBlocksLossDemo: Không tìm thấy LevelManager!");
            }
        }
        
        private void Update()
        {
            // Nhấn phím H để demo điều kiện thua
            if (Input.GetKeyDown(demoKey))
            {
                DemoHiddenBlocksLoss();
            }
        }
        
        /// <summary>
        /// Demo: Lock một số block để kích hoạt điều kiện thua
        /// </summary>
        public void DemoHiddenBlocksLoss()
        {
            if (levelManager == null)
            {
                Debug.LogWarning("HiddenBlocksLossDemo: LevelManager không tồn tại!");
                return;
            }
            
            // Tìm tất cả các Block có thể di chuyển
            Block[] allBlocks = FindObjectsOfType<Block>();
            int lockedCount = 0;
            
            foreach (Block block in allBlocks)
            {
                if (lockedCount >= blocksToHide) break;
                
                // Chỉ lock block có thể di chuyển và chưa bị lock
                if (block.canMove && block.gameObject.activeInHierarchy)
                {
                    LockBlock lockBlock = block.GetComponent<LockBlock>();
                    if (lockBlock == null)
                    {
                        // Thêm LockBlock component nếu chưa có
                        lockBlock = block.gameObject.AddComponent<LockBlock>();
                    }
                    
                    if (!lockBlock.isLocked)
                    {
                        lockBlock.isLocked = true;
                        lockBlock.boxesToClear = 1; // Cần clear 1 block để unlock
                        lockBlock.UpdateLockedState();
                        lockedCount++;
                        Debug.Log($"HiddenBlocksLossDemo: Đã lock block {block.name}");
                    }
                }
            }
            
            Debug.Log($"HiddenBlocksLossDemo: Đã lock {lockedCount} block. Kiểm tra điều kiện thua...");
            
            // Kiểm tra điều kiện thua
            levelManager.CheckForHiddenBlocksLossCondition();
        }
        
        /// <summary>
        /// Hiển thị thông tin về các block hiện tại
        /// </summary>
        [ContextMenu("Show Block Status")]
        public void ShowBlockStatus()
        {
            Block[] allBlocks = FindObjectsOfType<Block>();
            int totalBlocks = 0;
            int activeBlocks = 0;
            int movableBlocks = 0;
            int unlockedMovableBlocks = 0;
            int lockedMovableBlocks = 0;
            
            foreach (Block block in allBlocks)
            {
                totalBlocks++;
                
                if (block.gameObject.activeInHierarchy)
                {
                    activeBlocks++;
                    
                    if (block.canMove)
                    {
                        movableBlocks++;
                        
                        LockBlock lockBlock = block.GetComponent<LockBlock>();
                        if (lockBlock == null || !lockBlock.isLocked)
                        {
                            unlockedMovableBlocks++;
                        }
                        else
                        {
                            lockedMovableBlocks++;
                        }
                    }
                }
            }
            
            Debug.Log($"Block Status:\n" +
                     $"Total Blocks: {totalBlocks}\n" +
                     $"Active Blocks: {activeBlocks}\n" +
                     $"Movable Blocks: {movableBlocks}\n" +
                     $"Unlocked Movable Blocks: {unlockedMovableBlocks}\n" +
                     $"Locked Movable Blocks: {lockedMovableBlocks}\n" +
                     $"Loss Condition: {(movableBlocks > 0 && unlockedMovableBlocks == 0 ? "YES" : "NO")}");
        }
    }
}
