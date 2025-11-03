using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class CheckForMoreTargetPositions : MonoBehaviour
    {
        List<Transform> allTargetPositionsInChildren = new List<Transform>();
        List<Transform> freeTargetPositionsInChildren = new List<Transform>();

        ShowcaseParent showcaseParent;

        private void Start()
        {
            showcaseParent = GetComponentInParent<ShowcaseParent>();

            allTargetPositionsInChildren.Clear();

            if (showcaseParent.gameObject.name.Contains("Sort Hexagons"))
            {
                foreach (OccupiedGrid grid in GetComponentsInChildren<OccupiedGrid>())
                {
                    if (!grid.IsOccupied() && !allTargetPositionsInChildren.Contains(grid.transform))
                    {
                        allTargetPositionsInChildren.Add(grid.transform);
                    }
                }
            }
            else if (showcaseParent.gameObject.name.Contains("Sort Cake"))
            {
                foreach (OccupiedGrid grid in GetComponentsInChildren<OccupiedGrid>())
                {
                    if (!grid.IsOccupied() && !allTargetPositionsInChildren.Contains(grid.transform))
                    {
                        allTargetPositionsInChildren.Add(grid.transform);
                    }
                }
            }
            else if (showcaseParent.gameObject.name.Contains("Bottle Jam"))
            {
                foreach (Transform area in GetComponentInChildren<BoxJumpController>().transform.Find("FirstLine"))
                {
                    if (!allTargetPositionsInChildren.Contains(area))
                    {
                        allTargetPositionsInChildren.Add(area);
                    }
                }
            }

            CheckIfAnyTargetIsFree();
        }

        public void CheckIfAnyTargetIsFree()
        {
            Invoke(nameof(RealCheck), 0.3f);
        }


        void RealCheck()
        {
            if (showcaseParent.gameObject.name.Contains("Sort Hexagons"))
            {
                freeTargetPositionsInChildren.Clear();
                foreach (Transform t in allTargetPositionsInChildren)
                {
                    if (!freeTargetPositionsInChildren.Contains(t) && t.childCount == 0)
                        freeTargetPositionsInChildren.Add(t);
                }

                if (freeTargetPositionsInChildren.Count <= 0)
                {
                    //Try To Delay Re-Checking
                    bool reCheck = false;

                    foreach (ParabolicJump jump in GetComponentsInChildren<ParabolicJump>())
                    {
                        if (jump.IsJumping())
                        {
                            reCheck = true;
                            break;
                        }
                    }

                    if (!reCheck)
                    {
                        foreach (CheckNeighbours checkNeighbours in GetComponentsInChildren<CheckNeighbours>())
                        {
                            if (checkNeighbours.IsMatchingStacks())
                            {
                                reCheck = true;
                                break;
                            }
                        }
                    }

                    if (reCheck)
                    {
                        Invoke(nameof(RealCheck), 0.25f);
                    }
                    else
                    {
                        GetComponentInParent<LevelManager>().ActivateLevelFailedPanel();
                    }
                }
            }
            else if (showcaseParent.gameObject.name.Contains("Sort Cake"))
            {
                freeTargetPositionsInChildren.Clear();
                foreach (Transform t in allTargetPositionsInChildren)
                {
                    if (!freeTargetPositionsInChildren.Contains(t) && t.childCount == 0)
                        freeTargetPositionsInChildren.Add(t);
                }

                if (freeTargetPositionsInChildren.Count <= 0)
                {
                    //Try To Delay Re-Checking
                    bool reCheck = false;

                    foreach (Slice slice in GetComponentsInChildren<Slice>())
                    {
                        if (slice.IsMoving())
                        {
                            reCheck = true;
                            break;
                        }
                    }

                    if (!reCheck)
                    {
                        foreach (CheckNeighbours checkNeighbours in GetComponentsInChildren<CheckNeighbours>())
                        {
                            if (checkNeighbours.isCheckingForSlices && checkNeighbours.IsMatchingStacks())
                            {
                                reCheck = true;
                                break;
                            }
                        }
                    }

                    if (reCheck)
                    {
                        Invoke(nameof(RealCheck), 0.25f);
                    }
                    else
                    {
                        GetComponent<LevelManager>().ActivateLevelFailedPanel();
                    }
                }
            }
            else if (showcaseParent.gameObject.name.Contains("Bottle Jam"))
            {
                freeTargetPositionsInChildren.Clear();
                foreach (Transform t in allTargetPositionsInChildren)
                {
                    if (!freeTargetPositionsInChildren.Contains(t) && t.childCount == 0)
                        freeTargetPositionsInChildren.Add(t);
                }

                if (freeTargetPositionsInChildren.Count <= 0)
                {
                    //Try To Delay Re-Checking
                    bool reCheck = false;

                    foreach (ConveyorBelt convBelt in FindObjectsOfType<ConveyorBelt>())
                    {
                        if (convBelt.isMovingCans)
                        {
                            reCheck = true;
                            break;
                        }
                    }

                    if (!reCheck)
                    {
                        foreach (BoxSpacesForCans box in GetComponentsInChildren<BoxSpacesForCans>())
                        {
                            if (box.isBeingFilled)
                            {
                                reCheck = true;
                                break;
                            }
                        }
                    }

                    if (!reCheck)
                    {
                        if (GetComponentInChildren<BoxJumpController>().IsAnyBoxJumpingToFirstRow())
                        {
                            reCheck = true;
                        }
                    }

                    if (!reCheck)
                    {
                        foreach (CanBottle bottle in GetComponentsInChildren<CanBottle>())
                        {
                            if (bottle.isJumpingToBox)
                            {
                                reCheck = true;
                                break;
                            }
                        }
                    }

                    if (reCheck)
                    {
                        Invoke(nameof(RealCheck), 0.25f);
                    }
                    else
                    {
                        GetComponentInParent<LevelManager>().ActivateLevelFailedPanel();
                    }
                }
            }
        }
    }
}
