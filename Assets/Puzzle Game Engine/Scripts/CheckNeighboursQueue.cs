using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;
using System.Collections;

namespace HyperPuzzleEngine
{
    public class CheckNeighboursQueue : MonoBehaviour
    {
        [Header("Fail Condition: All Grids Have Children")]
        [Space]
        public bool checkIfAllGridsAreFilled = false;
        public Transform gridsToCheckParent;
        public float checkIfGridsAreFilledInterval = 1f;

        [Header("Only For Debugging")]
        [SerializeField] List<CheckNeighbours> queue = new List<CheckNeighbours>();
        [SerializeField] CheckNeighbours currentlyChecking = null;

        private ShowcaseParent showcaseParent;

        private int realTestCount = 0;
        private int previousRealTestCount = 0;
        private CheckNeighbours previouslyCheckedNeighbour = null;

        IEnumerator Start()
        {
            showcaseParent = GetComponentInParent<ShowcaseParent>();

            if (checkIfAllGridsAreFilled)
                StartCoroutine(CheckFilledGrids());

            while (true)
            {
                yield return new WaitForSeconds(0.4f);

                //Checking continously

                bool neighbourMatched = false;

                if (MainCameraController.Instance.IsFocusingOnTemplate(showcaseParent) && !showcaseParent.IsAnyStackJumping())
                {
                    if (currentlyChecking == previouslyCheckedNeighbour && realTestCount == previousRealTestCount)
                    {
                        neighbourMatched = true;
                    }

                    yield return new WaitForSeconds(0.4f);

                    if (neighbourMatched)
                    {
                        if (currentlyChecking == previouslyCheckedNeighbour && realTestCount == previousRealTestCount)
                        {
                            currentlyChecking = null;

                            yield return new WaitForSeconds(0.2f);

                            if (!showcaseParent.IsAnyStackJumping())
                            {
                                if (!TryToDoCheck())
                                {
                                    foreach (CheckNeighbours grid in gridsToCheckParent.GetComponentsInChildren<CheckNeighbours>(false))
                                    {
                                        if (grid.transform.childCount > 0 && !queue.Contains(grid) && queue.Count == 0)
                                        {
                                            if (!showcaseParent.IsAnyStackJumping())
                                            {
                                                Debug.Log("Adding To Queue_V2: " + grid.gameObject.name);

                                                queue.Add(grid);
                                                TryToDoCheck();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

        IEnumerator CheckFilledGrids()
        {
            LevelManager levelManager = GetComponentInParent<LevelManager>();

            while (checkIfAllGridsAreFilled)
            {
                yield return new WaitForSeconds(checkIfGridsAreFilledInterval);

                if (showcaseParent.IsInGameMode())
                {
                    bool foundEmptyGrid = false;
                    bool canReloadLevel = true;

                    foreach (CheckNeighbours grid in gridsToCheckParent.GetComponentsInChildren<CheckNeighbours>(false))
                    {
                        if (grid.transform.childCount == 0)
                        {
                            Debug.Log("QUEUE_Found Empty Grid.");
                            foundEmptyGrid = true;
                            break;
                        }
                    }

                    if (!foundEmptyGrid)
                    {
                        if (showcaseParent.IsAnyStackJumping())
                        {
                            Debug.Log("QUEUE_Stack Is Jumping");
                            canReloadLevel = false;
                        }

                        //if (currentlyChecking != null)
                        //canReloadLevel = false;

                        //if (queue.Count > 0)
                        //canReloadLevel = false;

                        if (canReloadLevel)
                            levelManager.ActivateLevelFailedPanel();
                    }
                }
            }
        }

        public List<CheckNeighbours> GetQueue()
        {
            return queue;
        }

        public void AddToQueue(CheckNeighbours newCheck)
        {
            if (!MainCameraController.Instance.IsFocusingOnTemplate(showcaseParent)) return;

            RemoveIncavtiveGrids();

            Debug.Log("Adding To Queue: " + newCheck.gameObject.name);

            queue.Add(newCheck);

            TryToDoCheck();
        }

        private void RemoveIncavtiveGrids()
        {
            //if (!currentlyChecking.gameObject.activeInHierarchy)
            //currentlyChecking = null;

            foreach (CheckNeighbours grid in queue)
            {
                if (!grid.gameObject.activeInHierarchy)
                    queue.Remove(grid);
            }
        }

        public void RemoveFromQueue(CheckNeighbours removableCheck)
        {
            if (!MainCameraController.Instance.IsFocusingOnTemplate(showcaseParent)) return;

            if (!queue.Contains(removableCheck)) return;

            RemoveIncavtiveGrids();

            Debug.Log("Removing From Queue: " + removableCheck.gameObject.name);

            if (currentlyChecking == removableCheck)
                currentlyChecking = null;
            queue.Remove(removableCheck);

            TryToDoCheck();
        }

        public bool TryToDoCheck()
        {
            if (!MainCameraController.Instance.IsFocusingOnTemplate(showcaseParent)) return false;

            RemoveIncavtiveGrids();

            if ((currentlyChecking == null) && queue.Count > 0)
            {
                Debug.Log("Starting Real Test");

                previouslyCheckedNeighbour = currentlyChecking = queue[0];
                currentlyChecking.StartCheckNeighbourColorsCoroutine(0f, true);

                realTestCount++;
                previousRealTestCount = realTestCount;

                return true;
            }
            else
            {
                Debug.Log("Cannot Start Real Test");
                return false;
            }
        }

        public void ResetQueue()
        {
            queue = new List<CheckNeighbours>();
            currentlyChecking = null;
        }
    }
}
