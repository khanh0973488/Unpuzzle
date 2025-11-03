using System.Collections;
using UnityEngine;
using HyperPuzzleEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace HyperPuzzleEngine
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Reload")]
        public bool reloadAfterLevelFailed = false;
        public float reloadDelay = 2f;

        [Header("Fail Handling")]
        [Tooltip("Extra small delay before showing Fail to allow any final win updates to complete in the same frame.")]
        public float outOfMovesFailGraceDelay = 0.1f;

        [HideInInspector] public int tempLevelIndex = 0;

        [Space]
        public GameObject checkForLevelsInChildren;

        List<GameObject> levels = new List<GameObject>();
        List<GameObject> levelClearedPanels = new List<GameObject>();
        List<GameObject> levelFailedPanels = new List<GameObject>();

        int levelIndexAtStart;

        [Space]
        public UnityEvent OnLoadedNextLevel;

        bool alreadyClearedPanel = false;
        bool alreadyLoadedNextLevel = false;

        void Start()
        {
            SetUpLevelDetails();
            LoadCurrentLevel();
        }

        private void SetUpLevelDetails()
        {
            levels.Clear();
            levelClearedPanels.Clear();
            levelFailedPanels.Clear();

            int i = 0;

            if (checkForLevelsInChildren == null) return;

            foreach (SetUpLevelDetailsUI level in checkForLevelsInChildren.GetComponentsInChildren<SetUpLevelDetailsUI>(true))
            {
                if (level.GetComponentInParent<LevelCreator>(true) == null)
                {
                    levels.Add(level.levelObject);
                    levelClearedPanels.Add(level.levelClearedPanel);
                    levelFailedPanels.Add(level.levelFailedPanel);
                    level.levelText.text = "Level " + (i + 1).ToString();

                    i++;
                }
            }
        }

        private void LoadCurrentLevel()
        {
            tempLevelIndex = PlayerPrefs.GetInt(gameObject.name + "_Level", 0);

            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i] != null)
                {
                    levels[i].gameObject.SetActive(false);

                    if (i == tempLevelIndex)
                        levels[tempLevelIndex].gameObject.SetActive(true);
                }
            }


            levelIndexAtStart = tempLevelIndex;
        }

        public void LoadNextLevel(float delay)
        {
            if (alreadyLoadedNextLevel) return;
            alreadyLoadedNextLevel = true;
            Invoke(nameof(CanLoadNextLevelAgain), 3f);

            StartCoroutine(LoadingNextLevel(delay));
        }

        IEnumerator LoadingNextLevel(float delay)
        {
            //Loads next level only if player has not done it already in this game session

            yield return new WaitForSeconds(delay);

            int potentialNextLevelIndex = tempLevelIndex + 1;

            if (potentialNextLevelIndex >= levels.Count)
                potentialNextLevelIndex = 0;

            if (potentialNextLevelIndex != levelIndexAtStart)
            {
                foreach (GameObject panel in levelFailedPanels)
                    panel.SetActive(false);
                foreach (GameObject panel in levelClearedPanels)
                    panel.SetActive(false);

                tempLevelIndex++;

                if (tempLevelIndex >= levels.Count)
                    tempLevelIndex = 0;

                for (int i = 0; i < levels.Count; i++)
                    levels[i].gameObject.SetActive(false);

                levels[tempLevelIndex].gameObject.SetActive(true);

                PlayerPrefs.SetInt(gameObject.name + "_Level", tempLevelIndex);
            }
            else
            {
                PlayerPrefs.SetInt(gameObject.name + "_Level", potentialNextLevelIndex);
            }

            OnLoadedNextLevel.Invoke();

            Transform blockClick = transform.Find("BlockClick");
            if (blockClick != null)
                blockClick.gameObject.SetActive(false);
        }

        #region Cleared and Failed Level Panels

        public void ActivateLevelClearedPanel()
        {
            if (alreadyClearedPanel) return;
            alreadyClearedPanel = true;
            Invoke(nameof(CanLoadNextLevelAgain), 3f);

            Debug.Log("Level Cleared For Game: " + gameObject.name);
            if (levelClearedPanels.Count > tempLevelIndex)
                levelClearedPanels[tempLevelIndex].SetActive(true);
            if (levelFailedPanels.Count > tempLevelIndex)
                levelFailedPanels[tempLevelIndex].SetActive(false);

            if (GetComponent<SoundsManagerForTemplate>() != null)
                GetComponent<SoundsManagerForTemplate>().PlaySound_Level_Won();

            Transform blockClick = transform.Find("BlockClick");
            if (blockClick != null)
                blockClick.gameObject.SetActive(true);
        }

        /// <summary>
        /// Called when moves run out. Decides whether the level is actually won or lost
        /// based on current collection progress and remaining movable blocks.
        /// </summary>
        public void DecideOutcomeOnOutOfMoves()
        {
            CollectedStacksCounter collectedCounter = GetComponent<CollectedStacksCounter>();

            if (collectedCounter != null)
            {
                int neededToCollect = collectedCounter.GetCurrentlyNeededToCollect();
                int currentlyCollected = collectedCounter.GetCurrentlyCollected();

                if (currentlyCollected >= neededToCollect)
                {
                    ActivateLevelClearedPanel();
                    return;
                }
            }

            // Kiểm tra điều kiện thua mới: còn move nhưng tất cả block còn lại đều bị ẩn
            if (HasRemainingMovableBlocksButAllHidden())
            {
                ActivateLevelFailedPanel();
                return;
            }

            ActivateLevelFailedPanel();
        }

        /// <summary>
        /// Hàm mới: Xử lý khi move = 0, thua ngay lập tức
        /// </summary>
        public void OnMovesReachedZero()
        {
            Debug.Log("Move = 0 - Game thua!");
            ActivateLevelFailedPanel();
        }

        /// <summary>
        /// Hàm mới: Xử lý khi 2 block đối nhau va chạm vào nhau - thua
        /// </summary>
        public void OnBlocksCollided()
        {
            Debug.Log("2 block đối nhau va chạm - Game thua!");
            ActivateLevelFailedPanel();
        }

        /// <summary>
        /// Kiểm tra xem còn block nào có thể di chuyển (active và canMove = true) hay không
        /// </summary>
        private bool HasRemainingMovableBlocks()
        {
            // Tìm tất cả các Block component trong scene hiện tại
            Block[] allBlocks = FindObjectsOfType<Block>();
            
            foreach (Block block in allBlocks)
            {
                // Kiểm tra block có active và có thể di chuyển không
                if (block.gameObject.activeInHierarchy && block.canMove)
                {
                    // Kiểm tra block có bị lock không
                    LockBlock lockBlock = block.GetComponent<LockBlock>();
                    if (lockBlock == null || !lockBlock.isLocked)
                    {
                        return true; // Còn ít nhất 1 block có thể di chuyển
                    }
                }
            }
            
            return false; // Không còn block nào có thể di chuyển
        }

        /// <summary>
        /// Kiểm tra xem còn block nào có thể di chuyển nhưng tất cả đều bị lock (ẩn)
        /// </summary>
        private bool HasRemainingMovableBlocksButAllHidden()
        {
            // Tìm tất cả các Block component trong scene hiện tại
            Block[] allBlocks = FindObjectsOfType<Block>();
            bool hasMovableBlocks = false;
            bool hasUnlockedMovableBlocks = false;
            
            foreach (Block block in allBlocks)
            {
                // Kiểm tra block có thể di chuyển và đang active không
                if (block.canMove && block.gameObject.activeInHierarchy)
                {
                    hasMovableBlocks = true; // Có block có thể di chuyển
                    
                    // Kiểm tra block có bị lock không
                    LockBlock lockBlock = block.GetComponent<LockBlock>();
                    if (lockBlock == null || !lockBlock.isLocked)
                    {
                        hasUnlockedMovableBlocks = true; // Có block không bị lock có thể di chuyển
                    }
                }
            }
            
            // Trả về true nếu có block có thể di chuyển nhưng tất cả đều bị lock
            return hasMovableBlocks && !hasUnlockedMovableBlocks;
        }

        /// <summary>
        /// Kiểm tra điều kiện thua: còn move nhưng tất cả block còn lại đều bị ẩn
        /// Có thể gọi từ bên ngoài để kiểm tra bất kỳ lúc nào
        /// </summary>
        public void CheckForHiddenBlocksLossCondition()
        {
            // Kiểm tra xem đã thắng chưa
            CollectedStacksCounter collectedCounter = GetComponent<CollectedStacksCounter>();
            if (collectedCounter != null)
            {
                int neededToCollect = collectedCounter.GetCurrentlyNeededToCollect();
                int currentlyCollected = collectedCounter.GetCurrentlyCollected();

                if (currentlyCollected >= neededToCollect)
                {
                    return; // Đã thắng, không cần kiểm tra điều kiện thua
                }
            }

            // Kiểm tra điều kiện thua: còn move nhưng tất cả block còn lại đều bị ẩn
            if (HasRemainingMovableBlocksButAllHidden())
            {
                ActivateLevelFailedPanel();
            }
        }

        public void ActivateLevelFailedPanel()
        {
            StartCoroutine(ActivateLevelFailedPanelDeferred());
        }

        private IEnumerator ActivateLevelFailedPanelDeferred()
        {
            // Allow any last-frame win updates (e.g., counters) to complete
            if (outOfMovesFailGraceDelay > 0f)
                yield return new WaitForSeconds(outOfMovesFailGraceDelay);
            else
                yield return null;

            // Safety check: if the level is actually cleared, don't show Failed; show Cleared instead
            CollectedStacksCounter collectedCounter = GetComponent<CollectedStacksCounter>();
            if (collectedCounter != null)
            {
                int neededToCollect = collectedCounter.GetCurrentlyNeededToCollect();
                int currentlyCollected = collectedCounter.GetCurrentlyCollected();

                if (currentlyCollected >= neededToCollect)
                {
                    ActivateLevelClearedPanel();
                    yield break;
                }
            }

            if (alreadyClearedPanel) yield break;
            alreadyClearedPanel = true;
            Invoke(nameof(CanLoadNextLevelAgain), 3f);

            if (levelClearedPanels.Count > tempLevelIndex)
                levelClearedPanels[tempLevelIndex].SetActive(false);
            if (levelFailedPanels.Count > tempLevelIndex)
                levelFailedPanels[tempLevelIndex].SetActive(true);

            if (GetComponent<SoundsManagerForTemplate>() != null)
                GetComponent<SoundsManagerForTemplate>().PlaySound_Level_Failed();

            if (reloadAfterLevelFailed)
                Invoke(nameof(ReloadGame), reloadDelay);

            Transform blockClick = transform.Find("BlockClick");
            if (blockClick != null)
                blockClick.gameObject.SetActive(true);
        }

        private void ReloadGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void CanClearLevelAgain()
        {
            alreadyClearedPanel = false;
        }

        private void CanLoadNextLevelAgain()
        {
            alreadyLoadedNextLevel = alreadyClearedPanel = false;
        }

        #endregion
    }
}