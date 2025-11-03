using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class ShowcaseParent : MonoBehaviour
    {
        public bool ifFailedThenSelectOtherStack = false;

        private bool isInGameMode = true;

        public void SetGameMode(bool isGameMode)
        {
            isInGameMode = isGameMode;
        }

        public bool IsInGameMode()
        {
            return isInGameMode;
        }

        #region Levitating Children Functions

        public void SetLevitatingToFalseInAllChildren()
        {
            foreach (SelectableAfterPlaced selectable in GetComponentsInChildren<SelectableAfterPlaced>(true))
            {
                selectable.SetLevitating(false);
            }
        }

        public bool IsSelectableLevitating()
        {
            foreach (SelectableAfterPlaced selectable in GetComponentsInChildren<SelectableAfterPlaced>())
            {
                if (selectable.IsLevitating())
                    return true;
            }

            return false;
        }

        public Color GetLevitatingColor()
        {
            foreach (SelectableAfterPlaced selectable in GetComponentsInChildren<SelectableAfterPlaced>())
            {
                if (selectable.IsLevitating())
                    return selectable.GetComponent<ColorManager>().GetColor();
            }

            return Color.white;
        }

        #endregion

        #region Getters

        public GridChildrenAreSelectable GetTempSelectedGridStack()
        {
            foreach (SelectableAfterPlaced selectable in GetComponentsInChildren<SelectableAfterPlaced>())
            {
                if (selectable.IsLevitating())
                    return selectable.GetComponentInParent<GridChildrenAreSelectable>();
            }

            return null;
        }

        public GridChildrenAreSelectable[] GetActiveGrids()
        {
            List<GridChildrenAreSelectable> activeGrids = new List<GridChildrenAreSelectable>();

            foreach (OccupiedGrid occupied in GetComponentsInChildren<OccupiedGrid>())
            {
                if (!occupied.IsOccupied())
                    activeGrids.Add(occupied.GetComponent<GridChildrenAreSelectable>());
            }

            return activeGrids.ToArray();
        }

        public GridChildrenAreSelectable[] GetActiveGridsWhichCanBeDealtCardToo()
        {
            List<GridChildrenAreSelectable> activeGrids = new List<GridChildrenAreSelectable>();

            foreach (OccupiedGrid occupied in GetComponentsInChildren<OccupiedGrid>())
            {
                if (!occupied.IsOccupied() && occupied.CanDealCard())
                    activeGrids.Add(occupied.GetComponent<GridChildrenAreSelectable>());
            }

            return activeGrids.ToArray();
        }

        #endregion

        #region Returning Stack States

        public bool IsAnyStackJumping()
        {
            foreach (ParabolicJump jump in GetComponentsInChildren<ParabolicJump>())
            {
                if (jump.IsJumping())
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAnyStackRotating()
        {
            foreach (RotateConstantly rotatingObject in GetComponentsInChildren<RotateConstantly>())
            {
                if (rotatingObject.IsRotating())
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
