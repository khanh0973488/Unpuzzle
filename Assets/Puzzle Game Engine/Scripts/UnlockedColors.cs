using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class UnlockedColors : MonoBehaviour
    {
        public StackColors stackColors;

        public int unlockedColorsAtStart = 1;

        [Space]
        [Header("Saving Unlocked Colors")]
        public bool canSaveUnlockedColors = true;
        public string savePrefixPlayerPrefString = "unlocked_colors";

        [SerializeField] private List<Color> unlockedColors = new List<Color>();

        private void Awake()
        {
            LoadColors();
        }

        public Color[] GetUnlockedColors()
        {
            return unlockedColors.ToArray();
        }

        private void LoadColors()
        {
            Debug.Log("Unlocking Colors.. Start..");

            List<Color> unlockedColorsHelper = new List<Color>();

            int currentlyUnlockedColors = PlayerPrefs.GetInt(gameObject.name + savePrefixPlayerPrefString, unlockedColorsAtStart);
            unlockedColorsAtStart = currentlyUnlockedColors;
            for (int i = 0; i < currentlyUnlockedColors; i++)
            {
                if (i >= stackColors.colors.Length)
                    break;

                Debug.Log("Unlocking Color: " + stackColors.colors[i]);
                unlockedColorsHelper.Add(stackColors.colors[i]);
            }

            unlockedColors = unlockedColorsHelper;
        }

        public void UnlockNextColor()
        {
            unlockedColorsAtStart++;

            if (canSaveUnlockedColors)
                PlayerPrefs.SetInt(gameObject.name + savePrefixPlayerPrefString, unlockedColorsAtStart);

            LoadColors();

            RespawnStacks();
        }

        private void RespawnStacks()
        {
            foreach (StackContainerRandomSpawn stackSpawner in GetComponentsInChildren<StackContainerRandomSpawn>())
                stackSpawner.spawn = true;
        }

        public Color GetRandomColor()
        {
            Debug.Log("Returning Random Color of Colors count: " + unlockedColors.Count);

            if (unlockedColors.Count == 0)
                unlockedColors.Add(stackColors.colors[0]);

            int randomColorIndex;
            if (unlockedColors.Count == 1)
                randomColorIndex = 0;
            else
                randomColorIndex = Random.Range(0, unlockedColors.Count);

            Debug.Log("Returning Color Index: " + randomColorIndex);

            return unlockedColors[randomColorIndex];
        }

        public Color GetNextColor(Color previousColor)
        {
            // Find the color in the unlockedColors list with a tolerance
            int prevColorIndex = unlockedColors.FindIndex(c => AreColorsCloseEnough(c, previousColor));
            if (prevColorIndex != -1) // Found a close enough color
            {
                prevColorIndex++;

                if (prevColorIndex >= unlockedColors.Count)
                {
                    UnlockNextColor();
                }

                if (prevColorIndex >= unlockedColors.Count)
                    prevColorIndex--;

                return unlockedColors[prevColorIndex];
            }

            // If no close enough color is found, return white
            return Color.white;
        }

        // New helper method to check if two colors are similar within a tolerance
        private bool AreColorsCloseEnough(Color color1, Color color2, float tolerance = 0.01f)
        {
            return Mathf.Abs(color1.r - color2.r) <= tolerance &&
                   Mathf.Abs(color1.g - color2.g) <= tolerance &&
                   Mathf.Abs(color1.b - color2.b) <= tolerance;
        }

        //UI Changes
        [Space]
        [Header("UI Overwrites")]
        public TMP_Dropdown selectedDropdownColor;
        public Slider selectedSliderColor;
        public Toggle[] enabledColorToggles;

        public void UnlockColorsBasedOnToggles()
        {
            unlockedColors = new List<Color>();

            for (int i = 0; i < enabledColorToggles.Length; i++)
            {
                if (enabledColorToggles[i].isOn)
                {
                    unlockedColors.Add(stackColors.colors[i]);
                }
            }

            unlockedColorsAtStart = unlockedColors.Count;
        }

        public Color GetSelectedDropdownColor()
        {
            return stackColors.colors[(int)selectedDropdownColor.value];
        }

        public Color GetSelectedSliderColor()
        {
            return stackColors.colors[(int)selectedSliderColor.value];
        }
    }
}
