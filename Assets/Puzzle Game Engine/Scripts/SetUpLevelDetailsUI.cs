using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class SetUpLevelDetailsUI : MonoBehaviour
    {
        public GameObject levelObject;
        public GameObject levelClearedPanel;
        public GameObject levelFailedPanel;
        public TextMeshPro levelText;

        // Hàm hiển thị panel khi level cleared
        public void ShowLevelCleared()
        {
            levelClearedPanel.SetActive(true);
        }

        // Hàm hiển thị panel khi level failed (chậm hơn 1 giây)
        public void ShowLevelFailed()
        {
            StartCoroutine(ShowLevelFailedWithDelay(1f));
        }

        private IEnumerator ShowLevelFailedWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            levelFailedPanel.SetActive(true);
        }
    }
}