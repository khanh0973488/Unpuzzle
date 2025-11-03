using HyperPuzzleEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class ClickableScrewManager : MonoBehaviour
    {
        //public LayerMask holeLayer;

        private Screw currentActiveScrew = null;

        private ShowcaseParent showcaseParent;
        private PagesSystem pagesSystem;

        private void Start()
        {
            showcaseParent = GetComponentInParent<ShowcaseParent>();
            pagesSystem = showcaseParent.GetComponentInChildren<PagesSystem>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (!MainCameraController.Instance.IsFocusingOnTemplate(showcaseParent)) return;
                if (!showcaseParent.IsInGameMode() && pagesSystem.GetCurrentPageIndex() == 0) return;

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100f/*, holeLayer, QueryTriggerInteraction.Collide*/))
                {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject.name.ToLower().Contains("holeprefab"))
                    {
                        //Has Active Screw and Can Screw It In Hole
                        if (currentActiveScrew != null)
                        {
                            currentActiveScrew.MoveAboveHole(hitObject.transform);
                            currentActiveScrew = null;
                        }
                    }
                    else if (hitObject.GetComponent<Screw>() != null)
                    {
                        //Clicked On The Same Active Screw
                        if (currentActiveScrew == hitObject.GetComponent<Screw>())
                            ChangeLevitationOfCurrentScrew(true);
                        //Clicked On Non-Active Screw
                        else
                        {
                            ChangeLevitationOfCurrentScrew(/*true*/);

                            currentActiveScrew = hitObject.GetComponent<Screw>();
                            ChangeLevitationOfCurrentScrew();
                        }
                    }
                    else
                    {
                        //Clicked on Nothing
                        ChangeLevitationOfCurrentScrew(true);
                    }
                }
            }
        }

        private void ChangeLevitationOfCurrentScrew(bool isReturningToSamePosition = false)
        {
            if (currentActiveScrew != null)
            {
                if (currentActiveScrew.ChangeLevitation(isReturningToSamePosition))
                    currentActiveScrew = null;
            }
        }
    }
}