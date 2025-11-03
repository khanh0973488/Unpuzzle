using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace HyperPuzzleEngine
{
    public class SkillButton : MonoBehaviour
    {
        public enum SkillType
        {
            HexaHammer,
            NutsRemoveNuts,
            NutsRemoveBolts
        }
        public SkillType skillType = SkillType.HexaHammer;

        [Space]
        public static SkillButton currentlyUsingSkill = null;
        public static Transform currentlyTappedPiece = null;

        public int canUseFromLevel = 2;
        public int canUseForFreeThisTimes = 1;
        public int priceToUse = 15;
        public bool canUsedByWatchingAd = false;

        [Space]
        public TextMeshProUGUI bottomText;
        public GameObject skillTutorialText;
        public Image lockedImage;

        private bool canUseInThisLevel = false;
        private int canUseForFreeCount;

        private CollectedPiecesCounter thisMoneyManager;

        [Space]
        [Header("Physical Visuals")]
        public GameObject hammerPrefab;
        public Vector3 hammerSpawnPosOffset;

        private void Start()
        {
            canUseForFreeCount = canUseForFreeThisTimes;
            thisMoneyManager = GetComponentInParent<ShowcaseParent>().GetComponentInChildren<CollectedPiecesCounter>();

            Invoke(nameof(CheckIfCanUseSkill), 0.5f);
        }

        public void ResetUse()
        {
            canUseForFreeCount = canUseForFreeThisTimes;
            CheckIfCanUseSkill();
        }

        public void CheckIfCanUseSkill()
        {
            if (GetComponentInParent<LevelManager>() != null && (GetComponentInParent<LevelManager>().tempLevelIndex + 1) >= canUseFromLevel)
            {
                if (canUseForFreeCount > 0)
                    bottomText.text = canUseForFreeCount.ToString();
                else
                {
                    if (priceToUse > 0)
                        bottomText.text = "$" + priceToUse.ToString();
                    else
                        bottomText.text = "";
                }

                lockedImage.gameObject.SetActive(false);
                canUseInThisLevel = true;
            }
            else
            {
                bottomText.text = "LVL " + canUseFromLevel.ToString();
                lockedImage.gameObject.SetActive(true);
                canUseInThisLevel = false;
            }
        }

        public void TryToUseSkill()
        {
            if (currentlyUsingSkill == this)
            {
                if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                    GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Grid_FailedToUnlock();
                return;
            }

            if (currentlyUsingSkill != null)
            {
                if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                    GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Grid_FailedToUnlock();
                return;
            }

            if (skillType == SkillType.HexaHammer)
            {
                if (!IsThereAreAnyStacksPlaced())
                {
                    if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                        GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Grid_FailedToUnlock();
                    return;
                }
            }

            if (canUseInThisLevel)
            {
                if (canUseForFreeCount > 0)
                {
                    canUseForFreeCount--;

                    UseSkill();
                    return;
                }
                else
                {
                    if (canUsedByWatchingAd)
                    {
                        UseSkill();
                        return;
                    }
                    else if (thisMoneyManager.GetCurrentMoneyCount() >= priceToUse)
                    {
                        thisMoneyManager.IncreaseCollectedCounterWithoutEffects(-priceToUse);

                        UseSkill();
                        return;
                    }
                }
            }

            if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Grid_FailedToUnlock();
        }

        private bool IsThereAreAnyStacksPlaced()
        {
            foreach (CheckNeighbours grid in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<CheckNeighbours>())
            {
                if (grid.GetTopChildColor(grid.transform) != Color.white)
                    return true;
            }

            return false;
        }

        private void UseSkill()
        {
            Debug.Log("USING SKILL");

            if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Button_Pressed();

            currentlyUsingSkill = this;

            skillTutorialText.SetActive(true);

            if (skillType == SkillType.HexaHammer)
            {
                foreach (CheckNeighbours grid in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<CheckNeighbours>())
                {
                    if (grid.GetTopChildColor(grid.transform) != Color.white)
                        grid.transform.GetChild(grid.transform.childCount - 1).AddComponent<ClickableByHammer>();
                }
            }
            if (skillType == SkillType.NutsRemoveNuts)
            {
                GetComponentInParent<ShowcaseParent>().GetComponentInChildren<ShowRemoveIcons>().ShowRemoveIconsForNuts();
            }
            if (skillType == SkillType.NutsRemoveBolts)
            {
                GetComponentInParent<ShowcaseParent>().GetComponentInChildren<ShowRemoveIcons>().ShowRemoveIconsForBolts();
            }

            CheckIfCanUseSkill();
        }

        public void SpawnHammer(Transform clickedPiece)
        {
            currentlyTappedPiece = clickedPiece;

            Vector3 spawnPos = clickedPiece.position;

            foreach (ClickableByHammer clickable in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<ClickableByHammer>())
                Destroy(clickable);

            skillTutorialText.SetActive(false);

            Instantiate(hammerPrefab, spawnPos + hammerSpawnPosOffset, Quaternion.identity);

            currentlyUsingSkill = null;
        }

        int removableNuts = 2;

        public void RemovedNut(NutMoveOnBolt currentlyRemovedNut)
        {
            removableNuts--;

            if (removableNuts > 0)
                GetComponentInParent<ShowcaseParent>().GetComponentInChildren<ShowRemoveIcons>().DisableCurrentRemoveIcon(currentlyRemovedNut);
            else
            {
                GetComponentInParent<ShowcaseParent>().GetComponentInChildren<ShowRemoveIcons>().DisableAllRemoveIcons();
                removableNuts = 2;
                currentlyUsingSkill = null;
            }
        }

        public void RemovedBolt()
        {
            GetComponentInParent<ShowcaseParent>().GetComponentInChildren<ShowRemoveIcons>().DisableAllRemoveIcons();
            currentlyUsingSkill = null;
        }
    }
}