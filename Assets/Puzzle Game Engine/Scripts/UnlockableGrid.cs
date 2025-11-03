using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class UnlockableGrid : MonoBehaviour
    {
        public int price;
        public bool needToPayFullAmount = true;

        [Space]
        public bool updateColorIfCanBuy = true;
        public Color buyableColor;
        private Color defaultColor;
        private MeshRenderer renderer;

        [Space]
        public GameObject notBuyableGameobject;

        [Space]
        public UnlockableGrid[] conditionallyUnlockableGrids;

        private int tempPrice;

        private TextMeshPro priceText;

        private CollectedPiecesCounter currentMoneyManager;

        [Space]
        [Header("Events")]
        public UnityEvent OnPurchased;
        public UnityEvent OnFailedPurchase;

        private OccupiedGrid thisGrid;

        int countOfConditionalLocks = 0;

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (currentMoneyManager != null)
            {
                currentMoneyManager.OnMoneyUpdated -= HandleMoneyUpdated;
            }
        }

        private void OnMouseDown()
        {
            TryToBuyGrid();
        }

        #region Subscribing To Money Changes

        private void HandleMoneyUpdated(int currentMoneyCount)
        {
            Colorize();
        }

        public void SubscribeForMoneyManager(CollectedPiecesCounter newMoneyManager)
        {
            if (currentMoneyManager != null)
                currentMoneyManager.OnMoneyUpdated -= HandleMoneyUpdated;

            currentMoneyManager = newMoneyManager;
            currentMoneyManager.OnMoneyUpdated += HandleMoneyUpdated;
        }

        #endregion

        void Start()
        {
            thisGrid = GetComponentInParent<OccupiedGrid>();
            if (thisGrid == null)
            {
                if (GetComponentInParent<FollowObject>() != null)
                {
                    thisGrid = GetComponentInParent<FollowObject>().objectToFollow.GetComponent<OccupiedGrid>();

                    if (thisGrid != null)
                    {
                        thisGrid.setGridOccupied = true;
                    }
                }
            }

            renderer = GetComponent<MeshRenderer>();
            defaultColor = renderer.material.color;

            currentMoneyManager = GetComponentInParent<ShowcaseParent>().GetComponentInChildren<CollectedPiecesCounter>(false);
            priceText = GetComponentInChildren<TextMeshPro>();

            tempPrice = price;
            UpdatePriceText();

            Colorize();

            SetConditionallyUnlockableGridsUnlockable(false);

            if (currentMoneyManager != null)
                currentMoneyManager.OnMoneyUpdated += HandleMoneyUpdated;
        }

        public void Colorize()
        {
            if (updateColorIfCanBuy)
            {
                int currentMoneyCount = 0;
                if (currentMoneyManager != null)
                    currentMoneyCount = currentMoneyManager.GetCurrentMoneyCount();

                if (renderer != null)
                {
                    if (needToPayFullAmount && (tempPrice <= currentMoneyCount))
                        renderer.material.color = buyableColor;
                    else
                        renderer.material.color = defaultColor;
                }
            }
        }

        public void ChangePrice(int newPrice)
        {
            price = tempPrice = newPrice;
            UpdatePriceText();
            Colorize();
        }

        private void UpdatePriceText()
        {
            if (priceText == null)
                priceText = GetComponentInChildren<TextMeshPro>(true);
            if (priceText != null)
                priceText.text = tempPrice.ToString();
        }

        public int GetTempPrice()
        {
            return tempPrice;
        }

        #region Purchasing 

        public void TryToBuyGrid()
        {
            int currentMoneyCount = currentMoneyManager.GetCurrentMoneyCount();

            Debug.Log("PURCHASE_1");

            if (notBuyableGameobject.activeInHierarchy)
            {
                Debug.Log("PURCHASE_4");

                if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                    GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Grid_FailedToUnlock();

                OnFailedPurchase.Invoke();
                return;
            }

            if (needToPayFullAmount && (tempPrice <= currentMoneyCount))
            {
                Debug.Log("PURCHASE_2");
                Buy(currentMoneyCount);
            }
            else if (!needToPayFullAmount)
            {
                Debug.Log("PURCHASE_3");
                Buy(currentMoneyCount);
            }
            else
            {
                Debug.Log("PURCHASE_4");

                if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                    GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Grid_FailedToUnlock();

                OnFailedPurchase.Invoke();
            }
        }

        private void Buy(int payAmount = 1)
        {
            if (payAmount > tempPrice)
            {
                payAmount = tempPrice;
            }

            currentMoneyManager.IncreaseCollectedCounterWithoutEffects(-payAmount);

            tempPrice -= payAmount;
            UpdatePriceText();

            if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Grid_Unlocked();

            OnPurchased.Invoke();
        }

        #endregion

        public void UnlockParentGrid()
        {
            thisGrid.UnlockThisGrid(transform.parent.gameObject);
        }

        public void PlayAnimationInParentGrid(string animName)
        {
            if (transform.parent.GetComponentInParent<AnimationPlayer>() != null)
                transform.parent.GetComponentInParent<AnimationPlayer>().PlayAnimation(animName);
            else
            {
                if (transform.parent.GetComponent<FollowObject>() != null)
                {
                    if (transform.parent.GetComponent<FollowObject>().objectToFollow.GetComponentInParent<AnimationPlayer>() != null)
                        transform.parent.GetComponent<FollowObject>().objectToFollow.GetComponentInParent<AnimationPlayer>().PlayAnimation(animName);
                }
            }
        }

        public void SetThisGridUnlockable(bool isUnlockable)
        {
            if (notBuyableGameobject != null)
                notBuyableGameobject.SetActive(!isUnlockable);
        }

        //Conditionally Unlockable Grid = Locked Grid Which Can Be Unlocked Only After This Grid Has Been Unlocked
        #region Managing Conditionally Unlockable Grids

        public void SetConditionallyUnlockableGridsUnlockable(bool isUnlockable)
        {
            foreach (UnlockableGrid grid in conditionallyUnlockableGrids)
            {
                if (grid == null) break;

                if (isUnlockable)
                    grid.countOfConditionalLocks--;
                else
                    grid.countOfConditionalLocks++;

                grid.notBuyableGameobject.SetActive(!isUnlockable);

                if (grid.countOfConditionalLocks > 0)
                    grid.notBuyableGameobject.SetActive(true);
            }
        }

        public void AddConditionallyUnlockableGrid(UnlockableGrid newGrid)
        {
            for (int i = 0; i < conditionallyUnlockableGrids.Length; i++)
            {
                if (conditionallyUnlockableGrids[i] == newGrid)
                    return;
            }

            List<UnlockableGrid> list = new List<UnlockableGrid>();
            for (int i = 0; i < conditionallyUnlockableGrids.Length; i++)
            {
                if (conditionallyUnlockableGrids[i] != null)
                    list.Add(conditionallyUnlockableGrids[i]);
            }

            list.Add(newGrid);

            conditionallyUnlockableGrids = list.ToArray();
            newGrid.SetThisGridUnlockable(false);
        }

        public void RemoveConditionallyUnlockableGrid(UnlockableGrid newGrid)
        {
            for (int i = 0; i < conditionallyUnlockableGrids.Length; i++)
            {
                if (conditionallyUnlockableGrids[i] == newGrid)
                {
                    List<UnlockableGrid> list = new List<UnlockableGrid>();
                    for (int j = 0; j < conditionallyUnlockableGrids.Length; j++)
                    {
                        if (j != i)
                        {
                            if (conditionallyUnlockableGrids[i] != null)
                                list.Add(conditionallyUnlockableGrids[j]);
                        }
                    }

                    conditionallyUnlockableGrids = list.ToArray();
                    newGrid.SetThisGridUnlockable(true);
                    return;
                }
            }
        }

        #endregion
    }
}
