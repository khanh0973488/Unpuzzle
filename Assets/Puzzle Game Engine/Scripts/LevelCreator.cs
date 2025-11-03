using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class LevelCreator : MonoBehaviour
    {
        public enum LevelCreatorType
        {
            HexaSortCreator,
            NutsSortCreator,
            ScrewsJamCreator
        }

        public LevelCreatorType levelCreatorType = LevelCreatorType.HexaSortCreator;

        private Animation anim;
        private ShowcaseParent showcase;

        public GameObject[] objectsToDisableOnLevelCreatorMode;

        public Animation gameModeAnim;

        [Space]
        [Header("Grid Generators")]
        public GridGenerator gridGeneratorLevelCreator;
        public GridGenerator gridGeneratorTestGame;
        public GridGenerator gridGeneratorNewLevelPrefab;

        [Space]
        [Header("Object Holders")]
        public GameObject objectsHolderLevelCreator;
        public GameObject objectsHolderTestGame;
        public GameObject objectsHolderNewLevelPrefab;

        private void Start()
        {
            anim = GetComponent<Animation>();
            showcase = GetComponentInParent<ShowcaseParent>();
        }

        public void OpenLevelCreator()
        {
            gameModeAnim.Play("CloseGameMode");

            if (showcase.GetComponentInChildren<TutorialHandManager>() != null)
                showcase.GetComponentInChildren<TutorialHandManager>().HideTutorial(true);

            for (int i = 0; i < objectsToDisableOnLevelCreatorMode.Length; i++)
                objectsToDisableOnLevelCreatorMode[i].SetActive(false);

            showcase.SetGameMode(false);
        }

        public void CloseLevelCreator()
        {
            gameModeAnim.Play("OpenGameMode");

            for (int i = 0; i < objectsToDisableOnLevelCreatorMode.Length; i++)
                objectsToDisableOnLevelCreatorMode[i].SetActive(true);

            foreach (GridGenerator gridGenerator in GetComponentsInChildren<GridGenerator>())
                gridGenerator.DeleteCurrentGrids();

            foreach (SpawnerOfStackContainer stackContainerSpawner in GetComponentsInChildren<SpawnerOfStackContainer>())
                stackContainerSpawner.DeleteCurrentContainer();

            showcase.SetGameMode(true);
        }

        private IEnumerator RemoveSelectablePhysicalButtonComponentFromGrids(Transform gridsParent, float timeDelay)
        {
            yield return new WaitForSeconds(timeDelay);

            foreach (PhysicalButton selectable in gridsParent.GetComponentsInChildren<PhysicalButton>())
            {
                Destroy(selectable);
            }
        }

        private IEnumerator SetUpOccupiedComponentsForGrids(Transform gridsParent, float timeDelay)
        {
            yield return new WaitForSeconds(timeDelay);

            #region Destroying Grids

            List<Transform> spawnedGridsByLevelGenerator = gridGeneratorLevelCreator.GetSpawnedGrids();
            List<Transform> destroyableGrids = new List<Transform>();

            for (int i = gridsParent.childCount - 1; i >= 0; i--)
            {
                if (spawnedGridsByLevelGenerator[i] == null)
                    destroyableGrids.Add(gridsParent.GetChild(i));
            }

            for (int i = destroyableGrids.Count - 1; i >= 0; i--)
            {
                destroyableGrids[i].parent = null;
                Destroy(destroyableGrids[i].gameObject);
            }

            ////Checking if grid is destroyed
            //for (int i = gridsParent.childCount - 1; i >= 0; i--)
            //{
            //    bool tempChildIsDeleted = true;
            //    for (int j = 0; j < gridGeneratorLevelCreator.transform.childCount; j++)
            //    {

            //        if (Vector3.Distance(gridsParent.GetChild(i).position, gridGeneratorLevelCreator.transform.GetChild(j).position) < 0.1f)
            //        {
            //            tempChildIsDeleted = false;
            //            break;
            //        }
            //    }

            //    if (tempChildIsDeleted)
            //    {
            //        Destroy(gridsParent.GetChild(i).gameObject, 0.01f);
            //        gridsParent.GetChild(i).transform.parent = null;
            //    }
            //}

            #endregion

            yield return null;

            #region Setting Up Uccupied Grids

            for (int i = 0; i < gridsParent.childCount; i++)
            {
                gridsParent.GetChild(i).GetComponent<OccupiedGrid>().lockType =
                gridGeneratorLevelCreator.transform.GetChild(i).GetComponent<OccupiedGrid>().lockType;

                if (gridGeneratorLevelCreator.transform.GetChild(i).GetComponent<OccupiedGrid>().IsOccupied())
                    gridsParent.GetChild(i).GetComponent<OccupiedGrid>().setGridOccupied = true;
                else
                    gridsParent.GetChild(i).GetComponent<OccupiedGrid>().setGridFree = true;

            }

            #endregion

            yield return null;

            #region Setting Up Locked Grid Prices

            for (int i = 0; i < gridsParent.childCount; i++)
            {
                if (gridGeneratorLevelCreator.transform.GetChild(i).GetComponentInChildren<UnlockableGrid>(true) != null)
                {
                    gridsParent.GetChild(i).GetComponentInChildren<UnlockableGrid>(true).ChangePrice(
                    gridGeneratorLevelCreator.transform.GetChild(i).GetComponentInChildren<UnlockableGrid>(true).GetTempPrice());

                    gridsParent.GetChild(i).GetComponentInChildren<UnlockableGrid>(true).GetComponent<Collider>().enabled = true;
                }
            }

            #endregion
        }

        public void SpawnGridsForTestGame()
        {
            //To avoid spawning issues because of animation is still playing
            Invoke(nameof(SpawnGridsForTestGameRealSpawn), 0.8f);
            if (gridGeneratorTestGame != null)
                gridGeneratorTestGame.DeleteCurrentGrids();
        }

        public void SpawnGridsForLevelPrefab()
        {
            //To avoid spawning issues because of animation is still playing
            Invoke(nameof(SpawnGridsForNewLevelPrefab), 0.8f);
            if (gridGeneratorTestGame != null)
                gridGeneratorTestGame.DeleteCurrentGrids();
        }

        private void SpawnGridsForTestGameRealSpawn()
        {
            GridGenerator tempGrid;
            tempGrid = gridGeneratorTestGame;

            switch (levelCreatorType)
            {
                case LevelCreatorType.HexaSortCreator:
                    tempGrid.columns = gridGeneratorLevelCreator.columns;
                    tempGrid.rows = gridGeneratorLevelCreator.rows;
                    tempGrid.gridType = gridGeneratorLevelCreator.gridType;
                    tempGrid.spawnPattern = gridGeneratorLevelCreator.spawnPattern;
                    tempGrid.columnDistance = gridGeneratorLevelCreator.columnDistance;
                    tempGrid.rowDistance = gridGeneratorLevelCreator.rowDistance;
                    tempGrid.outerRings = gridGeneratorLevelCreator.outerRings;

                    tempGrid.spawnGrid = true;

                    StartCoroutine(RemoveSelectablePhysicalButtonComponentFromGrids(tempGrid.transform, .5f));
                    StartCoroutine(SetUpOccupiedComponentsForGrids(tempGrid.transform, .5f));
                    break;

                case LevelCreatorType.NutsSortCreator:
                    for (int i = tempGrid.transform.childCount - 1; i >= 0; i--)
                        Destroy(tempGrid.transform.GetChild(i).gameObject);

                    List<Transform> nuts = new List<Transform>();
                    List<GameObject> nutsOriginalParentsToDestroy = new List<GameObject>();

                    foreach (SelectableAfterPlaced nut in gridGeneratorLevelCreator.GetComponentsInChildren<SelectableAfterPlaced>())
                    {
                        if (!nut.transform.parent.gameObject.name.ToLower().Contains("nutspawner")) break;

                        if (!nutsOriginalParentsToDestroy.Contains(nut.transform.parent.gameObject))
                            nutsOriginalParentsToDestroy.Add(nut.transform.parent.gameObject);
                        nuts.Add(nut.transform);

                        nut.canSelectAtStart = true;
                    }

                    for (int i = 0; i < gridGeneratorLevelCreator.transform.childCount; i++)
                    {
                        if (gridGeneratorLevelCreator.transform.GetChild(i).Find("NutSpawner") != null)
                        {
                            if (!nutsOriginalParentsToDestroy.Contains(gridGeneratorLevelCreator.transform.GetChild(i).Find("NutSpawner").gameObject))
                                nutsOriginalParentsToDestroy.Add(gridGeneratorLevelCreator.transform.GetChild(i).Find("NutSpawner").gameObject);
                        }
                    }

                    List<int> nutSiblingIndexes = new List<int>();

                    for (int i = 0; i < nuts.Count; i++)
                        nutSiblingIndexes.Add(nuts[i].GetSiblingIndex());

                    for (int i = 0; i < nuts.Count; i++)
                        nuts[i].parent = nuts[i].parent.parent;

                    for (int i = 0; i < nutsOriginalParentsToDestroy.Count; i++)
                    {
                        nutsOriginalParentsToDestroy[i].transform.parent = null;
                        Destroy(nutsOriginalParentsToDestroy[i]);
                    }

                    for (int i = 0; i < nuts.Count; i++)
                        nuts[i].SetSiblingIndex(nutSiblingIndexes[i]);

                    gridGeneratorLevelCreator.spawnGridAtStart = false;
                    GameObject newGridGenerator = Instantiate(gridGeneratorLevelCreator, tempGrid.transform).gameObject;
                    newGridGenerator.transform.position = gridGeneratorLevelCreator.transform.position;

                    foreach (PhysicalButton button in newGridGenerator.GetComponentsInChildren<PhysicalButton>())
                        Destroy(button);
                    foreach (SelectableAfterPlaced selectable in newGridGenerator.GetComponentsInChildren<SelectableAfterPlaced>())
                        selectable.canSelect = true;

                    for (int i = 0; i < newGridGenerator.transform.childCount; i++)
                    {
                        if (newGridGenerator.transform.GetChild(i).GetComponent<BoxCollider>() != null) ;
                        newGridGenerator.transform.GetChild(i).GetComponent<BoxCollider>().enabled = true;
                    }

                    break;

                case LevelCreatorType.ScrewsJamCreator:
                    GameObject tempObjectsHolder;
                    tempObjectsHolder = objectsHolderTestGame;

                    for (int i = tempObjectsHolder.transform.childCount - 1; i >= 0; i--)
                        Destroy(tempObjectsHolder.transform.GetChild(i).gameObject);


                    List<Quaternion> screwRotations = new List<Quaternion>();

                    foreach (Screw screw in objectsHolderLevelCreator.GetComponentsInChildren<Screw>(true))
                        screwRotations.Add(screw.transform.rotation);

                    objectsHolderLevelCreator.GetComponent<LayersManager>().loadAllLayersOnStart = true;

                    //objectsHolderLevelCreator.spawnGridAtStart = false;
                    GameObject newObjectsHolderScrews = Instantiate(objectsHolderLevelCreator, tempObjectsHolder.transform).gameObject;
                    newObjectsHolderScrews.transform.position = objectsHolderLevelCreator.transform.position;

                    int j = 0;

                    foreach (Screw screw in tempObjectsHolder.GetComponentsInChildren<Screw>(true))
                    {
                        screw.transform.rotation = screwRotations[j];
                        j++;
                    }

                    foreach (PhysicalButton shapeButton in tempObjectsHolder.GetComponentsInChildren<PhysicalButton>(true))
                    {
                        if (shapeButton.gameObject.name.ToLower().Contains("shape"))
                        {
                            shapeButton.gameObject.AddComponent<ScrewManager>();
                            Destroy(shapeButton);
                        }
                    }

                    foreach (ColorManager shapeColorManager in tempObjectsHolder.GetComponentsInChildren<ColorManager>(true))
                    {
                        if (shapeColorManager.gameObject.name.ToLower().Contains("shape"))
                            Destroy(shapeColorManager);
                    }

                    foreach (DestroyGameobject shapeDestroyObj in tempObjectsHolder.GetComponentsInChildren<DestroyGameobject>(true))
                    {
                        if (shapeDestroyObj.gameObject.name.ToLower().Contains("shape"))
                            Destroy(shapeDestroyObj);
                    }
                    break;
            }
        }

        private void SpawnGridsForNewLevelPrefab()
        {
            GridGenerator tempGrid;
            tempGrid = gridGeneratorNewLevelPrefab;

            switch (levelCreatorType)
            {
                case LevelCreatorType.HexaSortCreator:
                    tempGrid.columns = gridGeneratorLevelCreator.columns;
                    tempGrid.rows = gridGeneratorLevelCreator.rows;
                    tempGrid.gridType = gridGeneratorLevelCreator.gridType;
                    tempGrid.spawnPattern = gridGeneratorLevelCreator.spawnPattern;
                    tempGrid.columnDistance = gridGeneratorLevelCreator.columnDistance;
                    tempGrid.rowDistance = gridGeneratorLevelCreator.rowDistance;
                    tempGrid.outerRings = gridGeneratorLevelCreator.outerRings;

                    tempGrid.spawnGrid = true;

                    StartCoroutine(RemoveSelectablePhysicalButtonComponentFromGrids(tempGrid.transform, .5f));
                    StartCoroutine(SetUpOccupiedComponentsForGrids(tempGrid.transform, .5f));
                    break;

                case LevelCreatorType.NutsSortCreator:
                    for (int i = tempGrid.transform.childCount - 1; i >= 0; i--)
                        Destroy(tempGrid.transform.GetChild(i).gameObject);

                    GameObject newGridGenerator = Instantiate(gridGeneratorLevelCreator, tempGrid.transform).gameObject;
                    //newGridGenerator.transform.position = gridGeneratorLevelCreator.transform.position;

                    foreach (PhysicalButton button in newGridGenerator.GetComponentsInChildren<PhysicalButton>())
                        Destroy(button);
                    foreach (SelectableAfterPlaced selectable in newGridGenerator.GetComponentsInChildren<SelectableAfterPlaced>())
                        selectable.canSelect = true;

                    for (int i = 0; i < newGridGenerator.transform.childCount; i++)
                    {
                        if (newGridGenerator.transform.GetChild(i).GetComponent<BoxCollider>() != null) ;
                        newGridGenerator.transform.GetChild(i).GetComponent<BoxCollider>().enabled = true;
                    }

                    break;

                case LevelCreatorType.ScrewsJamCreator:
                    GameObject tempObjectsHolder;
                    tempObjectsHolder = objectsHolderNewLevelPrefab;

                    for (int i = tempObjectsHolder.transform.childCount - 1; i >= 0; i--)
                        Destroy(tempObjectsHolder.transform.GetChild(i).gameObject);


                    List<Quaternion> screwRotations = new List<Quaternion>();

                    foreach (Screw screw in objectsHolderLevelCreator.GetComponentsInChildren<Screw>(true))
                        screwRotations.Add(screw.transform.rotation);

                    objectsHolderLevelCreator.GetComponent<LayersManager>().loadAllLayersOnStart = true;

                    GameObject newObjectsHolderScrews = Instantiate(objectsHolderLevelCreator, tempObjectsHolder.transform).gameObject;
                    //newObjectsHolderScrews.transform.position = objectsHolderLevelCreator.transform.position;

                    int j = 0;

                    foreach (Screw screw in tempObjectsHolder.GetComponentsInChildren<Screw>(true))
                    {
                        screw.transform.rotation = screwRotations[j];
                        j++;
                    }

                    foreach (PhysicalButton shapeButton in tempObjectsHolder.GetComponentsInChildren<PhysicalButton>(true))
                    {
                        if (shapeButton.gameObject.name.ToLower().Contains("shape"))
                        {
                            shapeButton.gameObject.AddComponent<ScrewManager>();
                            Destroy(shapeButton);
                        }
                    }

                    foreach (ColorManager shapeColorManager in tempObjectsHolder.GetComponentsInChildren<ColorManager>(true))
                    {
                        if (shapeColorManager.gameObject.name.ToLower().Contains("shape"))
                            shapeColorManager.loadColorAtStart = true;
                        if (shapeColorManager.GetColorIndex() < 0)
                            shapeColorManager.SaveColorByIndex(0);

                    }

                    foreach (DestroyGameobject shapeDestroyObj in tempObjectsHolder.GetComponentsInChildren<DestroyGameobject>(true))
                    {
                        if (shapeDestroyObj.gameObject.name.ToLower().Contains("shape"))
                            Destroy(shapeDestroyObj);
                    }

                    foreach (ScrewManager screwManager in tempObjectsHolder.GetComponentsInChildren<ScrewManager>(true))
                        screwManager.ResetScrews();

                    break;
            }

        }
    }
}