using System;
using UnityEngine;
using UnityEngine.UI;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class MoneyChooserUI : MonoBehaviour
    {
        [Range(1, 3)]
        public int displayMoneyIndex;

        public GameObject[] moneysUI;
        public GameObject[] moneys1Toggles;
        public GameObject[] moneys2Toggles;
        public GameObject[] moneys3Toggles;

        private void OnValidate()
        {
            UpdateMoneyUI();
        }

        private void UpdateMoneyUI()
        {
            // Guard against unassigned references in the Inspector
            if (moneysUI == null || moneysUI.Length < 3)
                return;

            // Local helper to safely set active on arrays that might be null or contain nulls
            void SetActiveForArray(GameObject[] objects, bool isActive)
            {
                if (objects == null)
                    return;

                for (int i = 0; i < objects.Length; i++)
                {
                    var obj = objects[i];
                    if (obj != null)
                        obj.SetActive(isActive);
                }
            }

            switch (displayMoneyIndex)
            {
                case 1:
                    if (moneysUI[0] != null) moneysUI[0].SetActive(true);
                    if (moneysUI[1] != null) moneysUI[1].SetActive(false);
                    if (moneysUI[2] != null) moneysUI[2].SetActive(false);

                    SetActiveForArray(moneys1Toggles, true);
                    SetActiveForArray(moneys2Toggles, false);
                    SetActiveForArray(moneys3Toggles, false);
                    break;

                case 2:
                    if (moneysUI[0] != null) moneysUI[0].SetActive(false);
                    if (moneysUI[1] != null) moneysUI[1].SetActive(true);
                    if (moneysUI[2] != null) moneysUI[2].SetActive(false);

                    SetActiveForArray(moneys1Toggles, false);
                    SetActiveForArray(moneys2Toggles, true);
                    SetActiveForArray(moneys3Toggles, false);
                    break;

                case 3:
                    if (moneysUI[0] != null) moneysUI[0].SetActive(false);
                    if (moneysUI[1] != null) moneysUI[1].SetActive(false);
                    if (moneysUI[2] != null) moneysUI[2].SetActive(true);

                    SetActiveForArray(moneys1Toggles, false);
                    SetActiveForArray(moneys2Toggles, false);
                    SetActiveForArray(moneys3Toggles, true);
                    break;
            }
        }

        public void OverWriteValue_MoneyIndex(Slider slider)
        {
            displayMoneyIndex = Mathf.RoundToInt(slider.value);

            UpdateMoneyUI();
        }
    }
}
