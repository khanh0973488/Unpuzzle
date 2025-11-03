using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class StackContainer : MonoBehaviour
    {
        public bool spawn = false;
        public bool setStackToDefaultLayer = true;

        [Space]
        public GridTypes.GridType gridType = GridTypes.GridType.Square;
        [Range(1, 10)]
        public int countOfStackToSpawn;
        [Range(0.05f, 1f)]
        public float distanceOfStackedObjects;

        private GameObject gridPrefabSquare;
        private GameObject gridPrefabHexagon;

        public GridTypes gridTypesScriptableObject;

        private void Start()
        {
            if (gridPrefabHexagon == null) gridPrefabSquare = gridTypesScriptableObject.gridPrefabSquare;
            if (gridPrefabHexagon == null) gridPrefabHexagon = gridTypesScriptableObject.gridPrefabHexagon;
        }

        private void Update()
        {
            if (spawn)
            {
                spawn = false;

                if (gridPrefabHexagon == null) gridPrefabSquare = gridTypesScriptableObject.gridPrefabSquare;
                if (gridPrefabHexagon == null) gridPrefabHexagon = gridTypesScriptableObject.gridPrefabHexagon;

                DestroyAllChildren();

                SpawnChildren();

                ColorizeStack();
            }
        }

        private void SpawnChildren()
        {
            for (int i = 0; i < countOfStackToSpawn; i++)
            {
                GameObject tempSpawnedObj = null;
                Vector3 targetPosition;

                switch (gridType)
                {
                    case GridTypes.GridType.Square:
                        tempSpawnedObj = Instantiate(gridPrefabSquare, transform);
                        targetPosition = transform.position/*localPosition*/ + (Vector3.up * distanceOfStackedObjects * (transform.childCount /*- 1*/));
                        tempSpawnedObj.transform.position = targetPosition;
                        break;

                    case GridTypes.GridType.Hexagon:
                        tempSpawnedObj = Instantiate(gridPrefabHexagon, transform);
                        targetPosition = transform.position/*localPosition*/ + (Vector3.up * distanceOfStackedObjects * (transform.childCount /*- 1*/));
                        tempSpawnedObj.transform.position = targetPosition;
                        break;
                }

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
        public List<StackColors.StackColor> stackColorsList;

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

        private void ColorizeStack()
        {
            UpdateStackCoolorsListCount();

            int i = 0;
            foreach (ColorManager colorManager in GetComponentsInChildren<ColorManager>())
            {
                colorManager.ChangeColor(stackColorsScriptableObject.colors[(int)stackColorsList[i]]);
                i++;
            }
        }

        #endregion

        #region Spawn Counts

        int spawnAtStart;

        public void SetFixedSpawnCountOnce(int spawnCount)
        {
            spawnAtStart = countOfStackToSpawn;

            Invoke(nameof(SetBackSpawnCountToDefault), 2f);

            countOfStackToSpawn = spawnCount;
        }

        private void SetBackSpawnCountToDefault()
        {
            countOfStackToSpawn = spawnAtStart;
        }

        #endregion
    }
}