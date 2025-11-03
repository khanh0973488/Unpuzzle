using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteInEditMode]
    public class GridGenerator : MonoBehaviour
    {
        [Tooltip("Enable This To Spawn Grid")]
        public bool spawnGrid = false;

        [Space]
        [Header("Specfic Prefab (Not Using Grid Types)")]
        public bool isSpawningSpecificPrefab = false;
        public GameObject specificGridPrefab;

        private List<Transform> spawnedGridsInSpawner = new List<Transform>();

        public enum SpawnPattern
        {
            Linear,
            Circular
        }

        [Space]
        public GridTypes gridTypesScriptableObject;
        public Material gridMat;
        [Header("Grid Spawn Options")]
        public GridTypes.GridType gridType = GridTypes.GridType.Square;
        public SpawnPattern spawnPattern = SpawnPattern.Linear;

        [Space]
        public bool rotateHexagonAtCircularSpawn = false;
        public bool centerPivot = true;
        public bool spawnGridAtStart = false;
        public bool isUsingVisualInsteadOfOwnMesh = false;
        public bool isLockedGridClickable = true;

        [Space(25)]
        [Header("Grid Parameters - Spawn Pattern")]
        [Header("   Ring Pattern")]
        [Range(0, 5)]
        public int outerRings = 1; // For circular patterns
        [Header("   Linear Pattern")]
        [Range(0, 10)]
        public int columns = 1;
        [Range(0, 10)]
        public int rows = 1;
        [Space(25)]
        [Header("Grid Parameters - Spacing")]
        [Range(0.7f, 4f)]
        public float columnDistance = 1.0f;
        [Range(0.7f, 4f)]
        public float rowDistance = 1.0f;

        GameObject gridPrefabSquare;
        GameObject gridPrefabHexagon;
        GameObject gridPrefabRectangle;
        GameObject gridPrefabCoin;

        public List<Transform> GetSpawnedGrids()
        {
            return spawnedGridsInSpawner;
        }

        private void TryToAssignSpawnablePrefabs()
        {
            if (gridPrefabSquare == null) gridPrefabSquare = gridTypesScriptableObject.gridPrefabSquare;
            if (gridPrefabHexagon == null) gridPrefabHexagon = gridTypesScriptableObject.gridPrefabHexagon;
            if (gridPrefabRectangle == null) gridPrefabRectangle = gridTypesScriptableObject.gridPrefabRectangle;
            if (gridPrefabCoin == null) gridPrefabCoin = gridTypesScriptableObject.gridPrefabCoin;

            if (isSpawningSpecificPrefab && specificGridPrefab != null)
            {
                gridPrefabSquare = gridPrefabHexagon = gridPrefabRectangle = gridPrefabCoin = specificGridPrefab;
            }
        }

        void Update()
        {
            if (spawnGrid)
            {
                spawnedGridsInSpawner = new List<Transform>();

                TryToAssignSpawnablePrefabs();

                spawnGrid = false;
                UpdateGrid();

                if (isUsingVisualInsteadOfOwnMesh)
                {
                    //Disable this grid's mesh renderer and collider
                    foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>(true))
                        renderer.enabled = false;
                    foreach (Collider collider in GetComponentsInChildren<Collider>(true))
                        collider.enabled = false;
                }
            }
        }

        private void Start()
        {
            if (spawnGridAtStart) spawnGrid = true;

            TryToAssignSpawnablePrefabs();
        }

        private void DeleteDuplicatedGrids()
        {
            List<Vector3> spawnPoisions = new List<Vector3>();
            List<Vector3> duplicatedPositions = new List<Vector3>();

            for (int i = 0; i < transform.childCount; i++)
            {
                if (spawnPoisions.Contains(transform.GetChild(i).position))
                    duplicatedPositions.Add(transform.GetChild(i).position);

                spawnPoisions.Add(transform.GetChild(i).position);
            }


            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (duplicatedPositions.Contains(transform.GetChild(i).position))
                {
                    Debug.Log("Found Duplicate Grid Spawns");

                    duplicatedPositions.Remove(transform.GetChild(i).position);
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }

        public void DeleteCurrentGrids()
        {
            spawnedGridsInSpawner = new List<Transform>();

            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }


        void UpdateGrid()
        {
            DeleteCurrentGrids();

            switch (gridType)
            {
                case GridTypes.GridType.Square:
                    GenerateSquareGrid_Linear();
                    break;

                case GridTypes.GridType.Hexagon:
                    if (spawnPattern == SpawnPattern.Linear) GenerateHexagonGrid_Linear();
                    else if (spawnPattern == SpawnPattern.Circular) GenerateHexagonGrid_Circular();
                    break;

                case GridTypes.GridType.Rectangle:
                    GenerateRectangleGrid_Linear();
                    break;

                case GridTypes.GridType.Coin:
                    GenerateCoinGrid_Linear();
                    break;
            }

            CenterPivot();

            DeleteDuplicatedGrids();

            foreach (FollowObject visual in GetComponentsInChildren<FollowObject>(true))
                visual.transform.parent = transform;
        }

        #region Square

        void GenerateSquareGrid_Linear()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (gridPrefabSquare != null)
                    {
                        GameObject newChild = Instantiate(gridPrefabSquare, transform);
                        spawnedGridsInSpawner.Add(newChild.transform);
                        newChild.transform.localPosition = new Vector3(j * columnDistance, 0, i * rowDistance);
                        newChild.GetComponent<MeshRenderer>().material = gridMat;
                    }
                }
            }
            CenterPivot();
        }

        #endregion

        #region Rectangle

        void GenerateRectangleGrid_Linear()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (gridPrefabRectangle != null)
                    {
                        GameObject newChild = Instantiate(gridPrefabRectangle, transform);
                        spawnedGridsInSpawner.Add(newChild.transform);
                        newChild.transform.localPosition = new Vector3(j * columnDistance, 0, i * rowDistance);
                        newChild.GetComponent<MeshRenderer>().material = gridMat;
                    }
                }
            }
            CenterPivot();
        }

        #endregion

        #region Coin

        void GenerateCoinGrid_Linear()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (gridPrefabCoin != null)
                    {
                        GameObject newChild = Instantiate(gridPrefabCoin, transform);
                        spawnedGridsInSpawner.Add(newChild.transform);
                        newChild.transform.localPosition = new Vector3(j * columnDistance, 0, i * rowDistance);
                        newChild.GetComponent<MeshRenderer>().material = gridMat;
                    }
                }
            }
            CenterPivot();
        }

        #endregion

        #region Hexagon

        void GenerateHexagonGrid_Linear()
        {
            float hexWidth = columnDistance;
            // Calculate vertical distance based on hexagon width, use this to make it evenly distributed
            //float hexHeight = Mathf.Sqrt(3) / 2 * hexWidth; 
            float hexHeight = rowDistance;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (gridPrefabHexagon != null)
                    {
                        GameObject newChild = Instantiate(gridPrefabHexagon, transform);
                        spawnedGridsInSpawner.Add(newChild.transform);
                        //newChild.transform.Rotate(Vector3.forward, 30f);
                        newChild.GetComponent<MeshRenderer>().material = gridMat;

                        // Offset every other row by half the width of a hex
                        float offsetX = (i % 2 == 0) ? j * hexWidth : j * hexWidth + hexWidth / 2;
                        newChild.transform.localPosition = new Vector3(offsetX, 0, i * hexHeight);
                    }
                }
            }
            CenterPivot();
        }

        void GenerateHexagonGrid_Circular()
        {
            // Calculate the distance between hexagon centers
            float hexWidth = columnDistance;
            float hexHeight = Mathf.Sqrt(3) * hexWidth * 0.5f;

            // Dictionary to track spawned positions
            Dictionary<Vector3, bool> spawnedPositions = new Dictionary<Vector3, bool>();

            // Helper method to spawn a hexagon and track its position
            void SpawnHexagon(Vector3 position)
            {
                if (gridPrefabHexagon != null && !spawnedPositions.ContainsKey(position))
                {
                    GameObject tempSpawnedHexagon = Instantiate(gridPrefabHexagon, position, Quaternion.identity, transform);
                    spawnedGridsInSpawner.Add(tempSpawnedHexagon.transform);
                    tempSpawnedHexagon.GetComponent<MeshRenderer>().material = gridMat;
                    if (rotateHexagonAtCircularSpawn)
                    {
                        tempSpawnedHexagon.transform.Rotate(Vector3.right, -90f);
                        tempSpawnedHexagon.transform.Rotate(Vector3.forward, 30f);
                    }
                    spawnedPositions[position] = true;
                }
            }

            // Instantiate the center hexagon
            SpawnHexagon(Vector3.zero);

            // Instantiate hexagon rings
            for (int r = 1; r <= outerRings; r++)
            {
                // Starting point for this ring
                Vector3 currentPos = new Vector3(r * hexWidth * 0.75f, 0.0f, -r * hexHeight * 0.5f);

                // Loop through the 6 sides of the ring
                for (int side = 0; side < 6; side++)
                {
                    // Loop through each hexagon in the side (minus the corners)
                    for (int j = 0; j < r; j++)
                    {
                        // Spawn the hexagon at the current position
                        SpawnHexagon(currentPos);

                        // Move to the next hexagon position
                        // Offset direction depends on the current side of the hexagon
                        Vector3 directionOffset = Vector3.zero;
                        switch (side)
                        {
                            case 0: directionOffset = new Vector3(-hexWidth * 0.75f, 0.0f, hexHeight * 0.5f); break;
                            case 1: directionOffset = new Vector3(-hexWidth * 0.75f, 0.0f, -hexHeight * 0.5f); break;
                            case 2: directionOffset = new Vector3(0.0f, 0.0f, -hexHeight); break;
                            case 3: directionOffset = new Vector3(hexWidth * 0.75f, 0.0f, -hexHeight * 0.5f); break;
                            case 4: directionOffset = new Vector3(hexWidth * 0.75f, 0.0f, hexHeight * 0.5f); break;
                            case 5: directionOffset = new Vector3(0.0f, 0.0f, hexHeight); break;
                        }
                        currentPos += directionOffset;
                    }
                }
            }
            SpawnMissingHexagons(hexWidth, hexHeight);

            CenterPivot();
        }

        // Helper method to spawn a hexagon and track its position
        void SpawnHexagon(Vector3 position)
        {
            if (gridPrefabHexagon != null)
            {
                GameObject tempSpawnedHexagon = Instantiate(gridPrefabHexagon, position, Quaternion.identity, transform);
                spawnedGridsInSpawner.Add(tempSpawnedHexagon.transform);

                if (rotateHexagonAtCircularSpawn)
                {
                    tempSpawnedHexagon.transform.Rotate(Vector3.right, -90f);
                    tempSpawnedHexagon.transform.Rotate(Vector3.forward, 30f);
                }
                tempSpawnedHexagon.GetComponent<MeshRenderer>().material = gridMat;
            }
        }

        private void SpawnMissingHexagons(float hexWidth, float hexHeight)
        {
            // Brute-force corrections for any missing hexagons
            // This part needs to be adjusted based on the specific positions that are missing
            // Add correction like this for each missing hexagon:
            if (outerRings >= 1)
                SpawnHexagon(new Vector3(0f, 0.0f, -2 * hexHeight * 0.5f));
            if (outerRings >= 2)
            {
                SpawnHexagon(new Vector3(0f, 0.0f, -6 * hexHeight * 0.5f));

                SpawnHexagon(new Vector3(-1f * hexWidth * 0.75f, 0.0f, -5f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(1f * hexWidth * 0.75f, 0.0f, -5f * hexHeight * 0.5f));
            }
            if (outerRings >= 3)
            {
                SpawnHexagon(new Vector3(0f, 0.0f, -10 * hexHeight * 0.5f));

                SpawnHexagon(new Vector3(-2f * hexWidth * 0.75f, 0.0f, -8f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(2f * hexWidth * 0.75f, 0.0f, -8f * hexHeight * 0.5f));

                SpawnHexagon(new Vector3(-1f * hexWidth * 0.75f, 0.0f, -9f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(1f * hexWidth * 0.75f, 0.0f, -9f * hexHeight * 0.5f));
            }
            if (outerRings >= 4)
            {
                // The hexagon directly below the center for the 4th ring
                SpawnHexagon(new Vector3(0f, 0.0f, -14 * hexHeight * 0.5f));

                // The two hexagons adjacent to the above, but one step closer to the center
                SpawnHexagon(new Vector3(-3f * hexWidth * 0.75f, 0.0f, -11f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(3f * hexWidth * 0.75f, 0.0f, -11f * hexHeight * 0.5f));

                // Additionally, there should be two more missing on the next step closer to the center,
                // following the pattern that every new ring adds an additional pair of adjacent missing hexagons
                SpawnHexagon(new Vector3(-2f * hexWidth * 0.75f, 0.0f, -12f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(2f * hexWidth * 0.75f, 0.0f, -12f * hexHeight * 0.5f));

                SpawnHexagon(new Vector3(-1f * hexWidth * 0.75f, 0.0f, -13f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(1f * hexWidth * 0.75f, 0.0f, -13f * hexHeight * 0.5f));
            }
            if (outerRings >= 5)
            {
                // The hexagon directly below the center for the 4th ring
                SpawnHexagon(new Vector3(0f, 0.0f, -14 * hexHeight * 0.5f));

                // The two hexagons adjacent to the above, but one step closer to the center
                SpawnHexagon(new Vector3(-3f * hexWidth * 0.75f, 0.0f, -11f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(3f * hexWidth * 0.75f, 0.0f, -11f * hexHeight * 0.5f));

                // Additionally, there should be two more missing on the next step closer to the center,
                // following the pattern that every new ring adds an additional pair of adjacent missing hexagons
                SpawnHexagon(new Vector3(-2f * hexWidth * 0.75f, 0.0f, -12f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(2f * hexWidth * 0.75f, 0.0f, -12f * hexHeight * 0.5f));

                SpawnHexagon(new Vector3(-1f * hexWidth * 0.75f, 0.0f, -13f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(1f * hexWidth * 0.75f, 0.0f, -13f * hexHeight * 0.5f));
            }
            if (outerRings >= 5)
            {
                // Directly below the center for the 5th ring
                SpawnHexagon(new Vector3(0f, 0.0f, -18 * hexHeight * 0.5f));

                // First pair stepping inwards from the 4th ring's outermost pair
                SpawnHexagon(new Vector3(-4f * hexWidth * 0.75f, 0.0f, -14f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(4f * hexWidth * 0.75f, 0.0f, -14f * hexHeight * 0.5f));

                // Second pair stepping inwards from the 4th ring's second pair
                SpawnHexagon(new Vector3(-3f * hexWidth * 0.75f, 0.0f, -15f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(3f * hexWidth * 0.75f, 0.0f, -15f * hexHeight * 0.5f));

                // Third pair stepping inwards from the 4th ring's innermost pair
                SpawnHexagon(new Vector3(-2f * hexWidth * 0.75f, 0.0f, -16f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(2f * hexWidth * 0.75f, 0.0f, -16f * hexHeight * 0.5f));

                // Fourth pair, a new innermost pair for the 5th ring
                SpawnHexagon(new Vector3(-1f * hexWidth * 0.75f, 0.0f, -17f * hexHeight * 0.5f));
                SpawnHexagon(new Vector3(1f * hexWidth * 0.75f, 0.0f, -17f * hexHeight * 0.5f));
            }
        }

        #endregion


        void CenterPivot()
        {
            if (centerPivot)
            {
                Vector3 centroid = Vector3.zero;
                foreach (Transform child in transform)
                {
                    centroid += child.localPosition;
                }
                centroid /= transform.childCount;

                foreach (Transform child in transform)
                {
                    child.localPosition -= centroid;
                }
            }
        }

        #region Overwrite Properties By UI

        [Space]
        [Header("Overwrite Values")]
        public bool canOverwriteValuesByUI = true;
        public UnityEvent OnOverwriteValue;

        public void SpawnGrid()
        {
            spawnGrid = true;
        }

        public void OverwriteValue_Columns(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            columns = Mathf.RoundToInt(slider.value);

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_Rows(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            rows = Mathf.RoundToInt(slider.value);

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_Distance_Column(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            columnDistance = (slider.value);

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_Distance_Row(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            rowDistance = (slider.value);

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_Outer_Rings(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            outerRings = Mathf.RoundToInt(slider.value);

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_GridType(TMP_Dropdown dropdown)
        {
            if (!canOverwriteValuesByUI) return;

            gridType = (GridTypes.GridType)dropdown.value;

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_SpawnPattern(TMP_Dropdown dropdown)
        {
            if (!canOverwriteValuesByUI) return;

            spawnPattern = (SpawnPattern)dropdown.value;

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_SpawnPattern(int dropdownValue)
        {
            if (!canOverwriteValuesByUI) return;

            spawnPattern = (SpawnPattern)dropdownValue;

            OnOverwriteValue.Invoke();
        }

        public void OverwriteOccupiedLockTypeInChildren(TMP_Dropdown dropdown)
        {
            foreach (OccupiedGrid occupied in GetComponentsInChildren<OccupiedGrid>())
            {
                occupied.OverwriteValue_LockType(dropdown);
            }
        }

        #endregion
    }
}