using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using HyperPuzzleEngine;
using System.Linq;
using System.Collections.Generic;

namespace HyperPuzzleEngine
{
    public class CollectedPiecesCounter : MonoBehaviour
    {
        public UnityEvent OnCollected;

        private string playerPrefsPrefix = "MoneyCount_";

        public delegate void MoneyUpdatedHandler(int newMoneyAmount);
        public event MoneyUpdatedHandler OnMoneyUpdated;

        int tempCount = 0;
        TextMeshProUGUI counter;

        [Space]
        public Animation animToPlay;
        public CollectedEffects collectedEffects;
        public GameObject moneySpawnerUI;

        [Space]
        [Header("Toggles")]
        public bool onlyInvokeEventsIfToggleIsOn = false;
        public Toggle animToggle;
        public Toggle particleToggle;
        public Toggle moveToggle;

        private ShowcaseParent showcaseParent;

        private List<string> increaseMoneyViaCheckNeighboursScriptNames = new List<string>
        {
            "#2 Sort Cards",
            "#3 Sort Coins",
            "#4 Sort Cake",
            "#8 Sort Coins 2"
        };

        private void OnEnable()
        {
            showcaseParent = GetComponentInParent<ShowcaseParent>();
            counter = GetComponent<TextMeshProUGUI>();
            counter.text = "0";

            LoadCollectedCount();

            foreach (UnlockableGrid unlockableGrid in showcaseParent.GetComponentsInChildren<UnlockableGrid>(true))
                unlockableGrid.SubscribeForMoneyManager(this);
        }

        public void IncreaseCollectedCounter(int increaseBy = 1, bool isCallingFromCheckNeighboursScript = false)
        {
            if (isCallingFromCheckNeighboursScript)
            {
                bool foundMatchingGameName = false;

                for (int i = 0; i < increaseMoneyViaCheckNeighboursScriptNames.Count; i++)
                {
                    if (showcaseParent.name.Contains(increaseMoneyViaCheckNeighboursScriptNames[i]))
                    {
                        foundMatchingGameName = true;
                        break;
                    }
                }

                if (!foundMatchingGameName) return;
            }

            tempCount += increaseBy;
            tempCount = Math.Clamp(tempCount, 0, 999);
            if (counter == null)
                counter = GetComponent<TextMeshProUGUI>();
            counter.text = tempCount.ToString();

            // Trigger the event
            OnMoneyUpdated?.Invoke(tempCount);

            OnCollected.Invoke();
        }

        public void IncreaseCollectedCounter(int increaseBy = 1)
        {
            tempCount += increaseBy;
            tempCount = Math.Clamp(tempCount, 0, 999);
            if (counter == null)
                counter = GetComponent<TextMeshProUGUI>();
            counter.text = tempCount.ToString();

            // Trigger the event
            OnMoneyUpdated?.Invoke(tempCount);

            OnCollected.Invoke();
        }

        public void IncreaseCollectedCounterWithoutEffects(int increaseBy = 1)
        {
            tempCount += increaseBy;
            tempCount = Math.Clamp(tempCount, 0, 999);
            if (counter == null)
                counter = GetComponent<TextMeshProUGUI>();
            counter.text = tempCount.ToString();

            // Trigger the event
            OnMoneyUpdated?.Invoke(tempCount);

            SaveCollectedCount();
        }

        public int GetCurrentMoneyCount()
        {
            return tempCount;
        }

        #region Saving and Loading 

        public void SaveCollectedCount()
        {
            PlayerPrefs.SetInt(playerPrefsPrefix + GetComponentInParent<ShowcaseParent>().name + gameObject.name, tempCount);
        }

        public void LoadCollectedCount()
        {
            tempCount = PlayerPrefs.GetInt(playerPrefsPrefix + GetComponentInParent<ShowcaseParent>().name + gameObject.name, 0);
            IncreaseCollectedCounterWithoutEffects(0);
        }

        #endregion

        #region Money Amination and Particles and Effects

        public void PlayAnimation()
        {
            if (onlyInvokeEventsIfToggleIsOn)
            {
                if (animToggle != null)
                {
                    if (!animToggle.isOn)
                        return;
                }
                else
                    return;
            }

            animToPlay.Play();
        }

        public void PlayParticle()
        {
            if (onlyInvokeEventsIfToggleIsOn)
            {
                if (particleToggle != null)
                {
                    if (!particleToggle.isOn)
                        return;
                }
                else
                    return;
            }

            collectedEffects.PlayCollectedEffect();
        }

        public void Move()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (onlyInvokeEventsIfToggleIsOn)
            {
                if (moveToggle != null)
                {
                    if (!moveToggle.isOn)
                        return;
                }
                else
                    return;
            }

            moneySpawnerUI.SetActive(true);
        }

        #endregion
    }
}