using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class MatchSymbolWithColor : MonoBehaviour
    {
        public Sprite[] symbols;
        public StackColors possibleColors;

        void Start()
        {
            SetUpSymbolBasedOnColor();
        }

        public void SetUpSymbolBasedOnColor()
        {
            ColorManager colorManager = GetComponent<ColorManager>();

            for (int i = 0; i < possibleColors.colors.Length; i++)
            {
                if (possibleColors.colors[i] == colorManager.GetColor())
                {
                    foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
                        renderer.sprite = symbols[i];

                    break;
                }
            }
        }

        public void SetUpSymbolBasedOnColor(Color color)
        {
            int indexOfColor = 0;
            for (int i = 0; i < possibleColors.colors.Length; i++)
            {
                if (color == possibleColors.colors[i])
                {
                    indexOfColor = i;
                    break;
                }
            }

            foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
                renderer.sprite = symbols[indexOfColor];
        }
    }
}