using HyperPuzzleEngine;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class SortCardsLevelCheck : MonoBehaviour
    {
        public StackColors stackColors;

        [Header("Control")]
        public bool updateCounter; // Public bool to trigger update

        [Space]
        [Header("Card Counters")]
        public int countOfCardsTotal;
        public int stacksToBeCollected;

        [System.Serializable]
        public struct CardTypeCounter
        {
            public StackColors.StackColor colorType; // Color Type from the enum
            public int count; // Count of the color
        }

        [SerializeField]
        public List<CardTypeCounter> cardTypes = new List<CardTypeCounter>();

        [Header("Match Requirements")]
        public int cardsNeededToMatch = 3; // The number of cards needed to match

        [System.Serializable]
        public struct MissingCardsCounter
        {
            public StackColors.StackColor colorType; // Color Type
            public int missingCount; // Number of cards needed to match the threshold
        }

        [SerializeField]
        public List<MissingCardsCounter> missingCardTypes = new List<MissingCardsCounter>();

        private void Update()
        {
            if (updateCounter)
            {
                updateCounter = false;
                UpdateCardCounts();
                UpdateMissingCardCounts();
            }
        }

        private void UpdateCardCounts()
        {
            cardTypes.Clear();
            var colorManagers = GetComponentsInChildren<ColorManager>();

            Dictionary<StackColors.StackColor, int> colorCountDict = new Dictionary<StackColors.StackColor, int>();
            foreach (StackColors.StackColor color in System.Enum.GetValues(typeof(StackColors.StackColor)))
            {
                colorCountDict[color] = 0;
            }

            foreach (var colorManager in colorManagers)
            {
                Color childColor = colorManager.GetColor();
                int colorIndex = stackColors.GetIndexOfColor(childColor);

                if (colorIndex >= 0 && colorIndex < stackColors.colors.Length)
                {
                    StackColors.StackColor stackColor = (StackColors.StackColor)colorIndex;
                    colorCountDict[stackColor]++;
                }
            }

            foreach (var entry in colorCountDict)
            {
                cardTypes.Add(new CardTypeCounter
                {
                    colorType = entry.Key,
                    count = entry.Value
                });
            }

            countOfCardsTotal = colorManagers.Length;
            stacksToBeCollected = countOfCardsTotal / cardsNeededToMatch;

            Debug.Log("Card counts updated.");
        }

        private void UpdateMissingCardCounts()
        {
            missingCardTypes.Clear();

            foreach (var cardType in cardTypes)
            {
                int remainder = cardType.count % cardsNeededToMatch;
                if (remainder != 0)
                {
                    int missingCount = cardsNeededToMatch - remainder;

                    missingCardTypes.Add(new MissingCardsCounter
                    {
                        colorType = cardType.colorType,
                        missingCount = missingCount
                    });
                }
            }

            Debug.Log("Missing card counts updated.");
        }
    }
}