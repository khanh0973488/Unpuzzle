#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;
using System.Linq;
using System;

namespace HyperPuzzleEngine
{
    [ExecuteInEditMode]
    public class LevelManagerUnscrewJam : MonoBehaviour
    {
        [Header("Container Settings")]
        public bool updateContainers = false;
        [Range(1, 14)]
        public int containerCount = 1;
        public Transform containerHolder;
        public GameObject containerPrefab;
        public List<StackColors.StackColor> stackColorsList = new List<StackColors.StackColor>();

        [Header("Screw Settings")]
        [Space]
        public bool updateScrews = false;
        public Transform bigCubesHolder;
        public int holesPerContainer = 3;

        [Header("Checking Overlapping Screws")]
        [Space]
        public bool checkOverlappingScrewsAfterSpawn = true;
        public Vector3 overlapBoxSize;

        private void OnValidate()
        {
            // Only make updates in edit mode, not during play mode
            if (Application.isPlaying) return;

            if (updateContainers)
            {
                updateContainers = false;
                UpdateStackColorsList();
                UpdateContainers();
            }

            if (updateScrews)
            {
                updateScrews = false;
                SpawnScrews();
                UpdateScrews();

                if (checkOverlappingScrewsAfterSpawn) CheckOverlapingScrews();
            }
        }

        void UpdateScrews()
        {
            CubeWithScrews[] cubesWithScrews = bigCubesHolder.GetComponentsInChildren<CubeWithScrews>();
            foreach (var cube in cubesWithScrews)
                cube.UpdateScrews();
        }

        private void UpdateStackColorsList()
        {
            // Adjust the stackColorsList size to match containerCount without resetting existing colors
            while (stackColorsList.Count < containerCount)
            {
                stackColorsList.Add(StackColors.StackColor.Blue); // Add default color or desired color
            }
            while (stackColorsList.Count > containerCount)
            {
                stackColorsList.RemoveAt(stackColorsList.Count - 1); // Remove excess elements
            }
        }

        private void UpdateContainers()
        {
            if (containerHolder == null || containerPrefab == null) return;

            int currentContainerCount = containerHolder.childCount;

            if (currentContainerCount < containerCount)
            {
                for (int i = currentContainerCount; i < containerCount; i++)
                {
                    GameObject newContainer = Instantiate(containerPrefab, containerHolder);
                    newContainer.name = $"Container_{i + 1}";
                }
            }
            else if (currentContainerCount > containerCount)
            {
                List<GameObject> containersToRemove = new List<GameObject>();
                for (int i = currentContainerCount - 1; i >= containerCount; i--)
                {
                    containersToRemove.Add(containerHolder.GetChild(i).gameObject);
                }

#if UNITY_EDITOR
                EditorApplication.delayCall += () =>
                {
                    foreach (var container in containersToRemove)
                    {
                        DestroyImmediate(container);
                    }
                };
#endif
            }

            for (int i = 0; i < containerCount; i++)
            {
                var container = containerHolder.GetChild(i).GetComponent<ContainerHolesHolder>();
                if (container != null)
                {
                    container.stackColor = stackColorsList[i];
                    container.ColorizeManually();
                }
            }
        }

        private void SpawnScrews()
        {
            if (bigCubesHolder == null) return;

            // Calculate the exact number of screws needed
            int totalScrewsNeeded = containerCount * holesPerContainer;
            int screwsAssigned = 0;

            CubeWithScrews[] cubesWithScrews = bigCubesHolder.GetComponentsInChildren<CubeWithScrews>();
            int maxScrewsPerCube = 6;

            // Step 1: Disable all screws in every cube initially
            foreach (var cube in cubesWithScrews)
            {
                for (int i = 0; i < cube.screws.Length; i++)
                {
                    cube.screws[i] = false;
                }
                //cube.UpdateScrews();
            }

            // Step 2: Plan semi-even screw distribution across cubes
            List<int> screwsPerCubePlan = new List<int>();
            int remainingScrews = totalScrewsNeeded;
            int cubeCount = cubesWithScrews.Length;

            // Populate screwsPerCubePlan with a semi-even distribution
            for (int i = 0; i < cubeCount; i++)
            {
                int screwsForThisCube = Mathf.Min(remainingScrews / (cubeCount - i), maxScrewsPerCube);
                screwsPerCubePlan.Add(screwsForThisCube);
                remainingScrews -= screwsForThisCube;
            }

            Debug.Log("PLAN for SCREWS: List length:" + screwsPerCubePlan.Count);
            for (int i = 0; i < screwsPerCubePlan.Count; i++)
                Debug.Log("PLAN for SCREWS: " + i + " .element: " + screwsPerCubePlan[i]);

            // Step 3: Assign screws based on the screwsPerCubePlan
            for (int cubeIndex = 0; cubeIndex < cubesWithScrews.Length; cubeIndex++)
            {
                var cube = cubesWithScrews[cubeIndex];
                int screwsForThisCube = screwsPerCubePlan[cubeIndex];

                // Generate a list of all possible screw indexes and shuffle it for randomness
                List<int> availableIndexes = Enumerable.Range(0, cube.screws.Length).ToList();
                List<int> randomIndexes = availableIndexes.OrderBy(x => UnityEngine.Random.value).Take(screwsForThisCube).ToList();

                foreach (int index in randomIndexes)
                {
                    // Enable screws at selected random indexes
                    if (screwsAssigned < totalScrewsNeeded)
                    {
                        cube.screws[index] = true;
                        screwsAssigned++;
                    }
                }

                // Update the cube to apply screw assignments
                //cube.UpdateScrews();

                // Break if we've assigned all required screws
                if (screwsAssigned >= totalScrewsNeeded) break;
            }

            // Debug: Log the total screws assigned for verification
            Debug.Log($"Total screws assigned: {screwsAssigned}/{totalScrewsNeeded}");

            // Step 4: Assign random colors to enabled screws
            AssignColorsToScrews(cubesWithScrews);
        }


        private void AssignColorsToScrews(CubeWithScrews[] cubesWithScrews)
        {
            Dictionary<StackColors.StackColor, int> colorScrewCounts = CalculateScrewColorRequirements();

            foreach (var cube in cubesWithScrews)
            {
                for (int i = 0; i < cube.screws.Length; i++)
                {
                    if (cube.screws[i])  // Only assign colors to enabled screws
                    {
                        StackColors.StackColor selectedColor = GetRandomAvailableColor(colorScrewCounts);
                        if (colorScrewCounts[selectedColor] > 0)
                        {
                            cube.StackColorsListOfScrews[i] = selectedColor;
                            colorScrewCounts[selectedColor]--;
                        }
                        else
                        {
                            Debug.LogWarning("Ran out of screws for the specified colors.");
                        }
                    }
                }

                // Update cube to reflect color assignments
                //cube.UpdateScrews();
            }
        }


        private StackColors.StackColor GetRandomAvailableColor(Dictionary<StackColors.StackColor, int> colorScrewCounts)
        {
            List<StackColors.StackColor> availableColors = colorScrewCounts
                .Where(kv => kv.Value > 0)
                .Select(kv => kv.Key)
                .ToList();

            if (availableColors.Count == 0)
            {
                Debug.LogWarning("No colors available for screw assignment.");
                return colorScrewCounts.Keys.First();
            }

            return availableColors[UnityEngine.Random.Range(0, availableColors.Count)];
        }

        private List<int> SelectRandomIndexes(int maxIndex, int count)
        {
            List<int> availableIndexes = new List<int>();
            for (int i = 0; i < maxIndex; i++) availableIndexes.Add(i);
            availableIndexes.Shuffle();

            return availableIndexes.Take(count).ToList();
        }

        private Dictionary<StackColors.StackColor, int> CalculateScrewColorRequirements()
        {
            Dictionary<StackColors.StackColor, int> colorRequirements = new Dictionary<StackColors.StackColor, int>();

            for (int i = 0; i < containerCount; i++)
            {
                var container = containerHolder.GetChild(i).GetComponent<ContainerHolesHolder>();
                if (container == null) continue;

                StackColors.StackColor color = container.stackColor;
                int screwsNeeded = holesPerContainer;

                if (!colorRequirements.ContainsKey(color))
                {
                    colorRequirements[color] = 0;
                }

                colorRequirements[color] += screwsNeeded;
            }

            return colorRequirements;
        }

        #region Dealing With Overlapping Screws

        void CheckOverlapingScrews()
        {
            // Get all active screws in the hierarchy
            var screws = GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<ScrewForJam>();
            bool foundOverlap = false;

            foreach (var screw in screws)
            {
                // Get the position and rotation of the screw, and check for overlapping screws
                Vector3 screwPosition = screw.transform.position;
                Quaternion screwRotation = screw.transform.rotation;
                Collider[] overlappingColliders = Physics.OverlapBox(screwPosition, overlapBoxSize / 2, screwRotation);

                // Filter overlapping colliders to only those that are screws and not the same as this one
                var overlappingScrews = overlappingColliders
                    .Select(c => c.GetComponent<ScrewForJam>())
                    .Where(s => s != null && s != screw)
                    .ToList();

                Debug.Log($"Overlap_Checking screw at position {screwPosition} with rotation {screwRotation.eulerAngles}");
                Debug.Log($"Overlap_Overlap count for this screw: {overlappingScrews.Count}");

                if (overlappingScrews.Count > 0)
                {
                    foundOverlap = true;
                    Debug.Log("Overlap_Found overlapping screws!");

                    // Randomly select one screw from the overlapping pair to disable
                    ScrewForJam screwToDisable = UnityEngine.Random.value > 0.5f ? screw : overlappingScrews[0];
                    CubeWithScrews cube = screwToDisable.GetComponentInParent<CubeWithScrews>();

                    // Get the index of this screw in the cube
                    int screwIndex = cube.GetIndexOfScrewBasedOnScrewParent(screwToDisable.transform.parent);
                    Debug.Log($"Overlap_Selected screw at index {screwIndex} of cube '{cube.name}' to disable.");

                    if (screwIndex != -1 && cube.screws[screwIndex])
                    {
                        // Store the color of the screw to be disabled
                        StackColors.StackColor disabledScrewColor = cube.StackColorsListOfScrews[screwIndex];

                        // Disable this screw
                        cube.screws[screwIndex] = false;
                        Debug.Log($"Overlap_Disabling screw at index {screwIndex} in cube '{cube.name}'.");

                        // Attempt to find a new, non-overlapping screw in this or the overlapping screw's cube
                        if (!TryEnableNonOverlappingScrew(cube, screwIndex, disabledScrewColor))
                        {
                            Debug.Log($"Overlap_No suitable non-overlapping screw found in '{cube.name}', trying other cube.");
                            // If no suitable screw is found in the first cube, try the other overlapping screw's cube
                            CubeWithScrews otherCube = overlappingScrews[0].GetComponentInParent<CubeWithScrews>();
                            TryEnableNonOverlappingScrew(otherCube, -1, disabledScrewColor); // No excluded index for the other cube
                        }

                        // Update cube to apply changes
                        cube.UpdateScrews();
                    }
                }
            }

            if (!foundOverlap)
            {
                Debug.Log("Overlap_No overlapping screws detected.");
            }
        }

        private bool TryEnableNonOverlappingScrew(CubeWithScrews cube, int excludedIndex, StackColors.StackColor colorToApply)
        {
            Debug.Log($"Overlap_Attempting to enable a non-overlapping screw in cube '{cube.name}' excluding index {excludedIndex}.");

            for (int i = 0; i < cube.screws.Length; i++)
            {
                // Skip the excluded index and only consider disabled screws
                if (i == excludedIndex || cube.screws[i]) continue;

                // Check for overlaps at the current screw position
                Transform screwTransform = cube.transform.GetChild(i);
                Vector3 screwPosition = screwTransform.position;
                Quaternion screwRotation = screwTransform.rotation;
                Collider[] overlappingColliders = Physics.OverlapBox(screwPosition, overlapBoxSize / 2, screwRotation);

                bool isOverlapping = overlappingColliders.Any(collider =>
                    collider.GetComponent<ScrewForJam>() != null && collider.GetComponent<ScrewForJam>().transform != screwTransform);

                Debug.Log($"Overlap_Checking potential new screw at index {i} in '{cube.name}': Overlap found? {isOverlapping}");

                // If no overlap found, enable this screw and set its color
                if (!isOverlapping)
                {
                    cube.screws[i] = true;
                    cube.StackColorsListOfScrews[i] = colorToApply; // Apply the color from the disabled screw
                    Debug.Log($"Overlap_Enabled screw at index {i} in '{cube.name}' with color '{colorToApply}' and no overlaps.");
                    return true;
                }
                else
                {
                    Debug.Log($"Overlap_Screw at index {i} in '{cube.name}' could not be enabled due to overlap.");
                }
            }

            Debug.Log($"Overlap_No non-overlapping screws could be enabled in '{cube.name}'.");
            // Return false if no non-overlapping screw could be enabled
            return false;
        }


        private void OnDrawGizmos()
        {
            // Draw overlap box for each active screw's position and rotation
            var screws = GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<ScrewForJam>();
            Gizmos.color = Color.red;

            foreach (var screw in screws)
            {
                if (screw.gameObject.activeInHierarchy)
                {
                    Gizmos.matrix = Matrix4x4.TRS(screw.transform.position, screw.transform.rotation, Vector3.one);
                    Gizmos.DrawWireCube(Vector3.zero, overlapBoxSize);
                }
            }

            // Reset the Gizmos matrix to avoid affecting other Gizmos
            Gizmos.matrix = Matrix4x4.identity;
        }

        #endregion
    }
}