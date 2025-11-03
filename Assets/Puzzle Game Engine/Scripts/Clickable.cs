using UnityEngine;
using UnityEngine.Events;
using HyperPuzzleEngine;
using System.Collections.Generic;

namespace HyperPuzzleEngine
{
    public class Clickable : MonoBehaviour
    {
        public enum MatchingColorTypes
        {
            Color,
            Points
        }

        [Header("Checking By ColorManager or DominoPointsManager")]
        public MatchingColorTypes matchingByColorType = MatchingColorTypes.Color;

        [Space]
        [Header("Conditions")]
        public bool canSelectOnlyIfTopChild = true;
        public bool canJumpToEmptyGrid = true;

        public enum RotationAxis { X, Y, Z }
        [Space]
        [Header("Conditions Regarding Flipped")]
        public bool canJumpOnlyIfFlipped = false;
        public RotationAxis rotationAxisLocally = RotationAxis.Y;
        public float requiredRotationValueLocally = 0.0f;
        float flipRotationTolerance = 0.1f;

        [Space]
        [Header("Events")]
        public UnityEvent OnStartedToJump;
        public UnityEvent OnFailedMatch;

        private bool canClick = true;

        bool IsFlipped()
        {
            if (canJumpOnlyIfFlipped)
            {
                float currentRotationValue = 0f;
                switch (rotationAxisLocally)
                {
                    case RotationAxis.X:
                        currentRotationValue = transform.localRotation.eulerAngles.x;
                        break;
                    case RotationAxis.Y:
                        currentRotationValue = transform.localRotation.eulerAngles.y;
                        break;
                    case RotationAxis.Z:
                        currentRotationValue = transform.localRotation.eulerAngles.z;
                        break;
                }

                if (Mathf.Abs(currentRotationValue - requiredRotationValueLocally) > flipRotationTolerance)
                {
                    Debug.Log("Clicked But Not Flipped.. Val1: " + currentRotationValue + "_Val2: " + requiredRotationValueLocally);
                    return false;
                }
            }

            Debug.Log("Flipped condition met or flipping not required.");
            return true;
        }

        private void OnMouseUp()
        {
            Debug.Log("Clicked..");

            if (canJumpOnlyIfFlipped)
            {
                if (!IsFlipped()) return;
            }

            if (!canClick) return;

            if (canSelectOnlyIfTopChild)
            {
                //TOP CHILD
                if (transform.GetSiblingIndex() == transform.parent.childCount - 1)
                {
                    switch (matchingByColorType)
                    {
                        case MatchingColorTypes.Color:
                            #region Try To Match Top Color Of Grid

                            foreach (CheckNeighbours grid in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<CheckNeighbours>())
                            {
                                Color gridColor = grid.GetTopChildColor(grid.transform);

                                if (gridColor == GetComponent<ColorManager>().GetColor())
                                {
                                    //Match
                                    Debug.Log("Matched..");

                                    transform.parent = grid.transform;
                                    GetComponent<ParabolicJump>().SimpleJump(grid.GetNextPosition(), false);

                                    OnStartedToJump.Invoke();

                                    return;
                                }
                            }

                            #endregion
                            break;

                        case MatchingColorTypes.Points:
                            #region Try To Match Top Points Of Grid

                            foreach (CheckNeighbours grid in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<CheckNeighbours>())
                            {
                                List<int> pointsCounts = grid.GetTopChildPoints(grid.transform);

                                foreach (int pointsCount in pointsCounts)
                                {
                                    if (GetComponent<DominoPointsManager>().GetAllMatchablePoints().Contains(pointsCount))
                                    {
                                        //Match
                                        Debug.Log("Matched.. By Number Of Points");

                                        transform.parent = grid.transform;
                                        GetComponent<ParabolicJump>().SimpleJump(grid.GetNextPosition(), false);

                                        OnStartedToJump.Invoke();

                                        return;
                                    }
                                }
                            }

                            #endregion
                            break;
                    }

                    #region Try To Jump To Empty Grid

                    if (canJumpToEmptyGrid)
                    {
                        foreach (CheckNeighbours grid in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<CheckNeighbours>())
                        {
                            if (grid.transform.childCount <= 0)
                            {
                                transform.parent = grid.transform;

                                GetComponent<ParabolicJump>().SimpleJump(grid.GetNextPosition(), false);

                                OnStartedToJump.Invoke();

                                return;
                            }
                        }
                    }

                    #endregion

                    if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                        GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Clickable_FailedMatch();

                    OnFailedMatch.Invoke();
                }
                else
                {
                    if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                        GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Clickable_FailedMatch();

                    OnFailedMatch.Invoke();
                }
            }
            else
            {
                switch (matchingByColorType)
                {
                    case MatchingColorTypes.Color:
                        #region Try To Match Top Color Of Grid

                        foreach (CheckNeighbours grid in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<CheckNeighbours>())
                        {
                            Color gridColor = grid.GetTopChildColor(grid.transform);

                            if (gridColor == GetComponent<ColorManager>().GetColor())
                            {
                                //Match
                                Debug.Log("Matched..");

                                transform.parent = grid.transform;
                                GetComponent<ParabolicJump>().SimpleJump(grid.GetNextPosition(), false);

                                OnStartedToJump.Invoke();

                                return;
                            }
                        }

                        #endregion
                        break;

                    case MatchingColorTypes.Points:
                        #region Try To Match Top Points Of Grid

                        foreach (CheckNeighbours grid in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<CheckNeighbours>())
                        {
                            List<int> pointsCounts = grid.GetTopChildPoints(grid.transform);

                            foreach (int pointsCount in pointsCounts)
                            {
                                if (GetComponent<DominoPointsManager>().GetAllMatchablePoints().Contains(pointsCount)
                                    || GetComponent<DominoPointsManager>().canJumpWithoutMatchingPoints)
                                {
                                    //Match
                                    Debug.Log("Matched.. By Number Of Points");

                                    transform.parent = grid.transform;
                                    GetComponent<ParabolicJump>().SimpleJump(grid.GetNextPosition(), false);

                                    GetComponent<DominoPointsManager>().MarkSideWhichMatchedByPoints(pointsCount);

                                    OnStartedToJump.Invoke();

                                    return;
                                }
                            }
                        }

                        #endregion
                        break;
                }

                #region Try To Jump To Empty Grid

                if (canJumpToEmptyGrid)
                {
                    foreach (CheckNeighbours grid in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<CheckNeighbours>())
                    {
                        if (grid.transform.childCount <= 0)
                        {
                            transform.parent = grid.transform;

                            GetComponent<ParabolicJump>().SimpleJump(grid.GetNextPosition(), false);

                            OnStartedToJump.Invoke();

                            return;
                        }
                    }
                }

                #endregion

                if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                    GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Clickable_FailedMatch();

                OnFailedMatch.Invoke();
            }
        }

        public void SetUnclickable()
        {
            canClick = false;
        }
    }
}