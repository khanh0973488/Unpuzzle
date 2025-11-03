using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
public class GridChildrenAreSelectable : MonoBehaviour
{
    public bool onlyOneCanBeSelectedAtOnce = false;

    public Vector3 axisForStackingNextPos = Vector3.up;

    public int leaveEmptyStack = 0;
    public float distanceOfStackedObjects = 0.104f;

    public string paymentFailedAnimName = "NoMatchGridRotate";

    bool ifFailedThenSelectOtherStack;

    private void Start()
    {
        ifFailedThenSelectOtherStack = GetComponentInParent<ShowcaseParent>().ifFailedThenSelectOtherStack;
    }

    public void LevitateChildren()
    {
        if (transform.childCount <= 0) return;

        Color topColor = GetTopChildColor();
        Color currentColor = topColor;

        for (int i = (transform.childCount) - (1); i >= 0; i--)
        {
            if (transform.GetChild(i).GetComponentInChildren<ColorManager>() != null)
                currentColor = transform.GetChild(i).GetComponentInChildren<ColorManager>().GetColor();
            else
                return;

            if (currentColor == topColor)
            {
                transform.GetChild(i).GetComponentInChildren<SelectableAfterPlaced>().ChangeLevitation(Vector3.one * 444f);

                //ONLY ONE CAN LEVITATE AT ONCE
                if (onlyOneCanBeSelectedAtOnce)
                    break;
            }
            else
                break;
        }
    }

    public bool CanGetMoreStack(bool areThereLockedSlots = false)
    {
        if (!areThereLockedSlots)
            return transform.childCount < GetComponent<CheckNeighbours>().maxCountOfStackChildren;
        else
            return transform.childCount < GetComponent<CheckNeighbours>().maxCountOfStackChildrenIfHasLockedSlots;
    }

    public void TryToJump()
    {
        Color levitatingColor = GetComponentInParent<ShowcaseParent>().GetLevitatingColor();

        GridChildrenAreSelectable tempSelectedGridStack = GetComponentInParent<ShowcaseParent>().GetTempSelectedGridStack();

        if (tempSelectedGridStack == null) return;

        if (transform.childCount <= 0)
        {
            //Jump To Empty
            tempSelectedGridStack.StartToJump(this);

            return;
        }

        if (levitatingColor == GetTopChildColor())
        {
            //Matching Colors - Can Jump
            tempSelectedGridStack.StartToJump(this);

            return;
        }
        else
        {
            //If This Has Locked Slots
            if (GetComponent<LockedSlotsWithKey>() != null)
            {
                int countOfColorManagersInChildren = 0;
                if (GetComponentInChildren<ColorManager>() != null)
                    countOfColorManagersInChildren = GetComponentsInChildren<ColorManager>().Length;

                //There Are Free Slots
                if (GetComponent<CheckNeighbours>().maxCountOfStackChildren > countOfColorManagersInChildren)
                {
                    Color topColor = GetTopChildColor();

                    //There Is No Top Color (No Stack Child) - Can Jump
                    if (topColor == Color.white)
                    {
                        //Jump To Empty
                        tempSelectedGridStack.StartToJump(this);

                        return;
                    }

                    //Top Colors Match
                    if (levitatingColor == topColor)
                    {
                        //Jump To Empty
                        tempSelectedGridStack.StartToJump(this);

                        return;
                    }
                    else
                    {
                        //Top Colors Don't Match - Cannot Jump
                        if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Clickable_FailedMatch();

                        tempSelectedGridStack.LevitateChildren();

                        if (ifFailedThenSelectOtherStack)
                            LevitateChildren();
                        else
                        {
                            GetComponent<Animation>().Play(paymentFailedAnimName);

                            if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Clickable_FailedMatch();
                        }
                    }
                }
                else
                {
                    //Not Enough Free Slots - Cannot Jump
                    tempSelectedGridStack.LevitateChildren();

                    if (ifFailedThenSelectOtherStack)
                        LevitateChildren();
                    else
                        GetComponent<Animation>().Play(paymentFailedAnimName);
                }
            }
            else
            {
                //Colors Don't Match - Cannot Jump

                if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                    GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Clickable_FailedMatch();

                tempSelectedGridStack.LevitateChildren();

                if (ifFailedThenSelectOtherStack)
                    LevitateChildren();
                else
                    GetComponent<Animation>().Play(paymentFailedAnimName);
            }
        }
    }

    public void StartToJump(GridChildrenAreSelectable targetGrid)
    {
        StopAllCoroutines();
        StartCoroutine(Jump(targetGrid));
    }

    IEnumerator Jump(GridChildrenAreSelectable targetGrid)
    {
        Color topChildColor = GetTopChildColor();
        List<ParabolicJump> levitatingParabolicJumps = new List<ParabolicJump>();

        for (int i = (transform.childCount) - (1); i >= 0; i--)
        {
            if (transform.GetChild(i).GetComponentInChildren<ColorManager>() != null &&
                (transform.GetChild(i).GetComponentInChildren<ColorManager>().GetColor() == topChildColor))
                levitatingParabolicJumps.Add(transform.GetChild(i).GetComponentInChildren<ParabolicJump>());
            else
                break;
        }

        for (int i = 0; i < levitatingParabolicJumps.Count; i++)
        {
            if (targetGrid.CanGetMoreStack(targetGrid.GetComponent<LockedSlotsWithKey>() != null))
            {
                Transform newChild = levitatingParabolicJumps[i].transform;
                newChild.GetComponent<ParabolicJump>().parentAtStartOfJump = newChild.parent;
                newChild.parent = targetGrid.transform;

                levitatingParabolicJumps[i].GetComponentInChildren<SelectableAfterPlaced>().SetLevitating(false);
                levitatingParabolicJumps[i].SimpleJump(targetGrid.GetNextEmptyPos(), false);

                if (levitatingParabolicJumps.Count > i + 1)
                {
                    if (levitatingParabolicJumps[i + 1].GetComponent<HideColor>() != null)
                    {
                        bool wasHidingColor = levitatingParabolicJumps[i + 1].GetComponent<HideColor>().isHidingColor;
                        levitatingParabolicJumps[i + 1].GetComponent<HideColor>().ShowColor();

                        if (wasHidingColor)
                            break;
                    }
                }
                else
                {
                    if (transform.childCount > 0 && transform.GetChild(transform.childCount - 1).GetComponent<HideColor>() != null)
                    {
                        transform.GetChild(transform.childCount - 1).GetComponent<HideColor>().ShowColor();
                    }
                }

                yield return new WaitForSeconds(0.05f);
            }
            else
            {
                //Fly Back
                Debug.Log("DOES NOT HAVE EMPTY SLOTS");
                levitatingParabolicJumps[i].GetComponent<SelectableAfterPlaced>().ChangeLevitation(Vector3.one * 444f);
            }
        }
    }

    public Vector3 GetNextEmptyPos()
    {
        return transform.position/*localPosition*/ + (axisForStackingNextPos * distanceOfStackedObjects * (transform.childCount + leaveEmptyStack  /*- 1*/));
    }

    public Vector3 GetPos(int posIndex)
    {
        return transform.position/*localPosition*/ + (axisForStackingNextPos * distanceOfStackedObjects * posIndex);
    }

    private Color GetTopChildColor()
    {
        if (transform.GetChild(transform.childCount - 1).GetComponentInChildren<ColorManager>() == null)
            return Color.white;
        return transform.GetChild(transform.childCount - 1).GetComponentInChildren<ColorManager>().GetColor();
    }

    public bool IsLevitating()
    {
        foreach (SelectableAfterPlaced selectable in GetComponentsInChildren<SelectableAfterPlaced>())
        {
            if (selectable.IsLevitating())
                return true;
        }

        return false;
    }

    private void OnMouseDown()
    {
        Debug.Log("ClickedOn_v2");

        ShowcaseParent showcaseParent = GetComponentInParent<ShowcaseParent>();
        GridChildrenAreSelectable gridChildrenAreSelectable = GetComponentInParent<GridChildrenAreSelectable>();

        if (showcaseParent.GetComponentInChildren<DealStackButton>() != null &&
            showcaseParent.GetComponentInChildren<DealStackButton>().IsDealingCards())
            return;

        if (showcaseParent.IsAnyStackJumping())
            return;

        if (showcaseParent.IsAnyStackRotating())
            return;

        if (showcaseParent.IsSelectableLevitating())
        {
            //This stack is already levitating - Clicked on same stack
            if (gridChildrenAreSelectable.IsLevitating())
            {
                gridChildrenAreSelectable.LevitateChildren();
            }
            else
            {
                //Try to jump - Clicked on other stack
                gridChildrenAreSelectable.TryToJump();
            }
        }
        else
        {
            gridChildrenAreSelectable.LevitateChildren();
        }
    }
}
}
