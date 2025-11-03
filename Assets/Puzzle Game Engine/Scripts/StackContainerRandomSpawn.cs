using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;
using Unity.VisualScripting;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class StackContainerRandomSpawn : MonoBehaviour
    {
        public bool spawn = false;
        public bool spawnAtStart = false;
        public bool doesHaveOnEnableMethod = true;
        public bool setStackToDefaultLayer = true;

        [Space]
        [Header("If Spawning Specific Prefab (Not Based On Grid Types")]
        public bool isSpawningSpecificPrefab = false;
        public GameObject specificPrefab;

        [Space]
        [Header("If Not Spawning Specific Prefab")]
        public GridTypes.GridType gridType = GridTypes.GridType.Square;
        public GridTypes gridTypesScriptableObject;

        [Space]
        [Header("Spawn Properties")]
        [Range(1, 10)]
        public int countOfStackToSpawnMin;
        [Range(1, 10)]
        public int countOfStackToSpawnMax;

        private int countOfStackToSpawn;

        [Space]
        public Vector3 spawnDirection = new Vector3(0, 1f, 0);
        [Range(0.05f, 1f)]
        public float distanceOfStackedObjects;

        #region Deciding Which Prefab To Spawn

        private GameObject gridPrefabSquare;
        private GameObject gridPrefabHexagon;
        private GameObject gridPrefabRectangle;
        private GameObject gridPrefabCoin;

        private void TryToAssignSpawnablePrefabs()
        {
            if (gridPrefabSquare == null) gridPrefabSquare = gridTypesScriptableObject.gridPrefabSquare;
            if (gridPrefabHexagon == null) gridPrefabHexagon = gridTypesScriptableObject.gridPrefabHexagon;
            if (gridPrefabRectangle == null) gridPrefabRectangle = gridTypesScriptableObject.gridPrefabRectangle;
            if (gridPrefabCoin == null) gridPrefabCoin = gridTypesScriptableObject.gridPrefabCoin;

            if (isSpawningSpecificPrefab && specificPrefab != null)
            {
                gridPrefabSquare = gridPrefabHexagon = gridPrefabRectangle = gridPrefabCoin = specificPrefab;
            }
        }

        #endregion

        private void OnValidate()
        {
            if (updateColorsManually)
                ColorizeStackManually();
        }

        IEnumerator Start()
        {
            TryToAssignSpawnablePrefabs();

            yield return new WaitForSeconds(0.1f);
            if (spawnAtStart)
                spawn = true;

            if (Application.isPlaying)
                TryToAddChildrenToGrid();
        }

        private void OnEnable()
        {
            if (!doesHaveOnEnableMethod)
                return;

            TryToAssignSpawnablePrefabs();

            if (spawnAtStart)
                spawn = true;

            if (Application.isPlaying)
                TryToAddChildrenToGrid();
        }

        private void Update()
        {
            if (spawn)
            {
                spawn = false;

                TryToAssignSpawnablePrefabs();

                DestroyAllChildren();

                SpawnChildren();

                TryToAddAnimation();

                ColorizeStack();
            }
        }

        private void TryToAddAnimation()
        {
            if (Application.isPlaying)
            {
                if (GetComponentInParent<SpawnerOfStackContainer>() != null)
                    GetComponentInParent<SpawnerOfStackContainer>().AddSpawnAnim(GetComponentInParent<SpawnerOfStackContainer>().transform.GetChild(0).gameObject);

                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).GetComponent<RotationAnimation>() != null)
                        transform.GetChild(i).GetComponent<RotationAnimation>().PlaySelectedAnimation();
                    if (transform.GetChild(i).GetComponent<PositionAnimation>() != null)
                        transform.GetChild(i).GetComponent<PositionAnimation>().PlaySelectedAnimation();
                }
            }
        }

        private void SpawnChildren()
        {
            countOfStackToSpawn = Random.Range(countOfStackToSpawnMin, countOfStackToSpawnMax + 1);

            for (int i = 0; i < countOfStackToSpawn; i++)
            {
                GameObject prefabToSpawn = gridPrefabSquare;
                GameObject tempSpawnedObj = null;
                Vector3 targetPosition;

                switch (gridType)
                {
                    case GridTypes.GridType.Square:
                        prefabToSpawn = gridPrefabSquare;
                        break;

                    case GridTypes.GridType.Hexagon:
                        prefabToSpawn = gridPrefabHexagon;
                        break;

                    case GridTypes.GridType.Rectangle:
                        prefabToSpawn = gridPrefabRectangle;
                        break;

                    case GridTypes.GridType.Coin:
                        prefabToSpawn = gridPrefabCoin;
                        break;
                }

                Debug.Log("SPAWNING CHILD: " + prefabToSpawn.name);

                tempSpawnedObj = Instantiate(prefabToSpawn, transform);

                Debug.Log("SPAWNING CHILD_2: " + tempSpawnedObj.name);

                targetPosition = transform.position/*localPosition*/ + (spawnDirection * distanceOfStackedObjects * (transform.childCount /*- 1*/));
                tempSpawnedObj.transform.position = targetPosition;

                tempSpawnedObj.transform.localRotation = Quaternion.Euler(tempSpawnedObj.GetComponent<ParabolicJump>().localRotationForced);

                if (setStackToDefaultLayer)
                    tempSpawnedObj.layer = 0;
            }

            if (GetComponent<Animation>() != null)
                GetComponent<Animation>().Play();
        }

        private void DestroyAllChildren()
        {
            for (int i = (transform.childCount) - (1); i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        #region Colorizing

        [Space]
        [Header("Coloring Stacks")]
        public StackColors stackColorsScriptableObject;

        [Space]
        [Header("Coloring Stacks Manually")]
        public bool updateColorsManually = false;
        public List<StackColors.StackColor> stackColorsList;

        [Space]
        [Header("Coloring Stacks Randomly")]
        public int maxDifferentColorsInStack;
        public int minConsequentColorsAboveEachOther;

        public void ColorizeStack()
        {
            Color selectedRandomColor = Color.white;
            if (Application.isPlaying)
            {
                if (GetComponentInParent<UnlockedColors>() != null)
                    selectedRandomColor = GetComponentInParent<UnlockedColors>().GetRandomColor();
                else
                    selectedRandomColor = stackColorsScriptableObject.GetRandomColor();
            }
            else
                selectedRandomColor = stackColorsScriptableObject.GetRandomColor();

            if (fixColor != Color.white)
                selectedRandomColor = fixColor;

            Color newColor = Color.white;

            int i = 0;
            int coloredCount = 0;
            foreach (ColorManager colorManager in GetComponentsInChildren<ColorManager>())
            {
                if ((fixColor == Color.white) && Random.Range(0, 3) == 0 && coloredCount >= minConsequentColorsAboveEachOther && (i + minConsequentColorsAboveEachOther) <= transform.childCount)
                {
                    Debug.Log("Changing color to selected random color: " + selectedRandomColor);

                    do
                    {
                        if (Application.isPlaying)
                        {
                            if (GetComponentInParent<UnlockedColors>() != null)
                            {
                                newColor = GetComponentInParent<UnlockedColors>().GetRandomColor();

                                if (GetComponentInParent<UnlockedColors>().unlockedColorsAtStart <= 1)
                                    break;
                            }
                            else
                            {
                                if (stackColorsScriptableObject.colors.Length <= 1)
                                    break;

                                newColor = stackColorsScriptableObject.GetRandomColor();
                            }
                        }
                        else
                        {
                            if (stackColorsScriptableObject.colors.Length <= 1)
                                break;

                            newColor = stackColorsScriptableObject.GetRandomColor();
                        }
                    } while (newColor == selectedRandomColor);

                    selectedRandomColor = newColor;
                    coloredCount = 0;
                }

                colorManager.ChangeColor(selectedRandomColor);

                colorManager.SetUpNumberInStackBasedOnColor(selectedRandomColor);

                i++;
                coloredCount++;
            }
        }
        private void UpdateStackCoolorsListCount()
        {
            while (stackColorsList.Count != transform.childCount)
            {
                if (transform.childCount > stackColorsList.Count)
                    stackColorsList.Add(StackColors.StackColor.Blue);
                else
                    stackColorsList.RemoveAt(stackColorsList.Count - 1);
            }
        }

        private void ColorizeStackManually()
        {
            UpdateStackCoolorsListCount();

            int i = 0;
            Color[] colorArray = stackColorsScriptableObject.colors;
            ColorManager[] colorManagers = GetComponentsInChildren<ColorManager>();

            // Ensure we don't exceed the bounds of stackColorsList or colorArray
            int maxCount = Mathf.Min(colorManagers.Length, stackColorsList.Count);

            foreach (ColorManager colorManager in colorManagers)
            {
                if (i >= maxCount) break; // Exit loop if we've reached the max count

                int colorIndex = (int)stackColorsList[i];
                if (colorIndex >= 0 && colorIndex < colorArray.Length) // Ensure index is within range
                {
                    colorManager.ChangeColorInEditor(colorArray[colorIndex]);
                }
                else
                {
                    Debug.LogWarning($"Index {colorIndex} is out of range for colors array. Skipping.");
                }

                i++;
            }
        }


        Color fixColor = Color.white;

        public void SetFixedSpawnColorOnce(Color spawnColor)
        {
            Invoke(nameof(SetBackSpawnColorToDefault), 2f);

            fixColor = spawnColor;
        }

        private void SetBackSpawnColorToDefault()
        {
            fixColor = Color.white;
        }

        #endregion

        #region Selecting Grid as Parent for Spawned Children

        [Space]
        [Header("Spawn Children For Grid")]
        public bool addChildrenToGridAtStart = false;
        public bool isOrderReversed;
        public bool getGridParentFromFollowerScript;
        public Transform gridAsParentOfChildren;

        private void TryToAddChildrenToGrid()
        {
            if (getGridParentFromFollowerScript && GetComponentInParent<FollowObject>() != null)
            {
                gridAsParentOfChildren = GetComponentInParent<FollowObject>().objectToFollow.transform;
            }

            if (addChildrenToGridAtStart && gridAsParentOfChildren != null)
            {
                if (isOrderReversed)
                {
                    List<Transform> children = new List<Transform>();

                    for (int i = 0; i < transform.childCount; i++)
                        children.Add(transform.GetChild(i));

                    for (int i = 0; i < children.Count; i++)
                        children[i].parent = gridAsParentOfChildren;
                }
                else
                {
                    for (int i = (transform.childCount) - (1); i >= 0; i--)
                        transform.GetChild(i).parent = gridAsParentOfChildren;
                }

                if (gridAsParentOfChildren.GetComponent<CheckNeighbours>() != null)
                    gridAsParentOfChildren.GetComponent<CheckNeighbours>().StartCheckNeighbourColorsCoroutine(1f);

                Destroy(gameObject);
            }
        }

        #endregion

        #region Spawn Counts

        int minSpawnAtStart;
        int maxSpawnAtStart;

        public void SetFixedSpawnCountOnce(int spawnCount)
        {
            minSpawnAtStart = countOfStackToSpawnMin;
            maxSpawnAtStart = countOfStackToSpawnMax;

            Invoke(nameof(SetBackSpawnCountToDefault), 2f);

            countOfStackToSpawnMin = countOfStackToSpawnMax = spawnCount;
        }

        private void SetBackSpawnCountToDefault()
        {
            countOfStackToSpawnMin = minSpawnAtStart;
            countOfStackToSpawnMax = maxSpawnAtStart;
        }

        #endregion
    }
}