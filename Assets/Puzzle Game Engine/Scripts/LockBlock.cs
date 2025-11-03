using UnityEngine;
using TMPro;
namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class LockBlock : MonoBehaviour
    {
        public enum DecreaseSource { Move, Exit, Blade }
        public static float lastDecreaseTime = -999f;
        public static float decreaseCooldownSeconds = 0.1f;
        public static DecreaseSource lastDecreaseSource;
        public static bool TryMarkDecreased(DecreaseSource source)
        {
            if (Application.isPlaying)
            {
                if (Time.time - lastDecreaseTime < decreaseCooldownSeconds)
                    return false;
                lastDecreaseTime = Time.time;
                lastDecreaseSource = source;
                return true;
            }
            return true;
        }
        [Header("Lock Settings")]
        public bool isLocked = true;
        public GameObject lockedBlock;
        private TextMeshPro[] lockTexts;
        [Range(1, 50)]
        public int boxesToClear = 5;

        // Dropdown để chọn kiểu hiển thị text
        public enum LockDisplayMode
        {
            ShowNumber,
            ShowQuestionMark
        }
        [Tooltip("Chọn cách hiển thị text trên ô khóa: số lượng còn lại hoặc dấu ?")]
        public LockDisplayMode displayMode = LockDisplayMode.ShowNumber;

        // Thêm cờ để xác định lock chỉ mở bằng bomb
        [Tooltip("Nếu bật, ô khóa này CHỈ có thể mở bằng bomb, không bị ảnh hưởng bởi counter")]
        public bool bombOnlyUnlock = false;

        [Header("Locked Block Global Rotation")]
        public Quaternion lockedBlockGlobalRotation = Quaternion.identity;

        [Header("Text Rotation Fix")]
        [Tooltip("Rotation để fix text hiển thị đúng hướng")]
        public Vector3 textRotationFix = new Vector3(0, 180, 0);

        private void OnValidate()
        {
            if (lockedBlock != null)
                lockTexts = lockedBlock.GetComponentsInChildren<TextMeshPro>();

            // Tự động set bombOnlyUnlock = true khi chọn ShowQuestionMark
            if (displayMode == LockDisplayMode.ShowQuestionMark)
                bombOnlyUnlock = true;

            UpdateLockedState();
        }

        private void Start()
        {
            if (lockedBlock != null)
                lockTexts = lockedBlock.GetComponentsInChildren<TextMeshPro>();
            UpdateLockedState();
        }

        public void UpdateLockedState()
        {
            if (lockedBlock != null)
            {
                lockedBlock.SetActive(isLocked);
                lockedBlock.transform.rotation = lockedBlockGlobalRotation;
                if (isLocked && lockTexts != null)
                {
                    string displayText = displayMode == LockDisplayMode.ShowNumber
                        ? boxesToClear.ToString()
                        : "?";
                    foreach (var text in lockTexts)
                    {
                        text.text = displayText;
                        // Fix rotation của text để hiển thị đúng hướng
                        text.transform.localRotation = Quaternion.Euler(textRotationFix);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            // Đảm bảo text luôn đúng rotation trong khi chạy
            if (isLocked && lockTexts != null)
            {
                foreach (var text in lockTexts)
                {
                    if (text != null)
                        text.transform.localRotation = Quaternion.Euler(textRotationFix);
                }
            }
        }

        public void ReduceLockedCounter()
        {
            // Không giảm counter nếu bombOnlyUnlock = true
            if (isLocked && !bombOnlyUnlock)
            {
                boxesToClear = Mathf.Max(0, boxesToClear - 1);
                if (lockTexts != null)
                {
                    string displayText = displayMode == LockDisplayMode.ShowNumber
                        ? boxesToClear.ToString()
                        : "?";
                    foreach (var text in lockTexts)
                    {
                        text.text = displayText;
                        text.transform.localRotation = Quaternion.Euler(textRotationFix);
                    }
                }
                if (boxesToClear == 0)
                {
                    isLocked = false;
                    UpdateLockedState();
                }
                
                // Kiểm tra điều kiện thua sau khi giảm counter (có thể unlock block)
                LevelManager levelManager = GetComponentInParent<LevelManager>();
                if (levelManager != null)
                {
                    levelManager.CheckForHiddenBlocksLossCondition();
                }
            }
        }

        public void UnlockByBomb()
        {
            if (isLocked)
            {
                Debug.Log($"LockBlock: Unlocking {gameObject.name} by bomb explosion");
                boxesToClear = 0;
                isLocked = false;
                UpdateLockedState();
                
                // Kiểm tra điều kiện thua sau khi unlock block
                LevelManager levelManager = GetComponentInParent<LevelManager>();
                if (levelManager != null)
                {
                    levelManager.CheckForHiddenBlocksLossCondition();
                }
            }
        }
    }
}