using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HyperPuzzleEngine
{
    [ExecuteInEditMode]
    public class FillUpConveyorBeltColorsRandomly : MonoBehaviour
    {
        public bool spawnBottlesRandomly = false;

        [Space]
        public int countOfBottlesPerBox = 4;
        private BoxJumpController boxJumpController;

        void Update()
        {
            if (spawnBottlesRandomly)
            {
                SpawnBottlesRandomly();
            }
        }

        public void SpawnBottlesRandomly()
        {
            spawnBottlesRandomly = false;

            // Access BoxJumpController to get the box presence and color data
            boxJumpController = GetComponentInParent<ShowcaseParent>().GetComponentInChildren<BoxJumpController>();
            if (boxJumpController == null)
            {
                Debug.LogError("BoxJumpController not found!");
                return;
            }

            // Dictionary to hold the required count of each color based on the box colors
            Dictionary<StackColors.StackColor, int> colorBottleCounts = new Dictionary<StackColors.StackColor, int>();

            // Count bottles from middle line
            foreach (var box in boxJumpController.middleLineBoxPresence)
            {
                if (box.hasBox)
                {
                    if (colorBottleCounts.ContainsKey(box.boxColor))
                    {
                        colorBottleCounts[box.boxColor] += countOfBottlesPerBox;
                    }
                    else
                    {
                        colorBottleCounts[box.boxColor] = countOfBottlesPerBox;
                    }
                }
            }

            // Count bottles from bottom line
            foreach (var box in boxJumpController.bottomLineBoxPresence)
            {
                if (box.hasBox)
                {
                    if (colorBottleCounts.ContainsKey(box.boxColor))
                    {
                        colorBottleCounts[box.boxColor] += countOfBottlesPerBox;
                    }
                    else
                    {
                        colorBottleCounts[box.boxColor] = countOfBottlesPerBox;
                    }
                }
            }

            int totalBottlesNeeded = colorBottleCounts.Values.Sum();

            // Get all conveyor belts and calculate the approximate number of bottles per conveyor
            ConveyorBelt[] conveyors = GetComponentsInChildren<ConveyorBelt>();
            int bottlesPerConveyor = totalBottlesNeeded / conveyors.Length;
            int remainingBottles = totalBottlesNeeded % conveyors.Length;

            foreach (ConveyorBelt conveyor in conveyors)
            {
                // Randomly set each conveyor's total can count close to the average
                conveyor.totalCans = bottlesPerConveyor + (remainingBottles-- > 0 ? 1 : 0);

                // Reset the list for colors
                conveyor.selectedCanColors = new List<StackColors.StackColor>();

                // Fill the conveyor's bottles with colors based on remaining colorBottleCounts
                List<StackColors.StackColor> conveyorColors = new List<StackColors.StackColor>();
                for (int i = 0; i < conveyor.totalCans; i++)
                {
                    // Choose a random color with available bottles
                    StackColors.StackColor selectedColor = ChooseRandomAvailableColor(colorBottleCounts);
                    conveyorColors.Add(selectedColor);
                    colorBottleCounts[selectedColor]--;
                }

                // Assign colors randomly within the conveyor
                conveyor.selectedCanColors = conveyorColors.OrderBy(_ => Random.value).ToList();

                // Trigger the conveyor to fill up with cans
                conveyor.fillUpLineWithCans = true;
            }

            // Log the total number of each color spawned across all conveyors in a single line
            Dictionary<StackColors.StackColor, int> totalSpawnedColors = new Dictionary<StackColors.StackColor, int>();
            foreach (var conveyor in conveyors)
            {
                foreach (var color in conveyor.selectedCanColors)
                {
                    if (totalSpawnedColors.ContainsKey(color))
                    {
                        totalSpawnedColors[color]++;
                    }
                    else
                    {
                        totalSpawnedColors[color] = 1;
                    }
                }
            }

            // Build the summary message
            int totalBottlesSpawned = totalSpawnedColors.Values.Sum();
            string summaryMessage = $"Spawned {totalBottlesSpawned} bottles in total. ";

            // Append each color count to the summary message
            foreach (var colorCount in totalSpawnedColors)
            {
                summaryMessage += $"{colorCount.Value} {colorCount.Key.ToString().ToLower()}, ";
            }

            // Trim the trailing comma and space
            summaryMessage = summaryMessage.TrimEnd(',', ' ');

            // Output the single-line summary to the console
            Debug.Log(summaryMessage);
        }

        // Helper method to choose a random color with available bottles
        private StackColors.StackColor ChooseRandomAvailableColor(Dictionary<StackColors.StackColor, int> colorBottleCounts)
        {
            var availableColors = colorBottleCounts.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
            return availableColors[Random.Range(0, availableColors.Count)];
        }
    }
}