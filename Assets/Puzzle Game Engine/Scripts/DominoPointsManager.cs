using HyperPuzzleEngine;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class DominoPointsManager : MonoBehaviour
    {
        public bool canJumpWithoutMatchingPoints = false;

        [Space]
        [Header("Point Colors")]
        public Color[] colorsBasedOnPointsCount;

        [Space]
        [Header("Sides Transforms")]
        public Transform side1;
        public Transform side2;

        [Space]
        [Header("Sides Points")]
        public bool canSide1Match = true;
        [Range(0, 6)]
        public int side1Points;
        [Space]
        public bool canSide2Match = true;
        [Range(0, 6)]
        public int side2Points;

        [Space]
        public bool randomizePoints = false;

        private void OnValidate()
        {
            SetPoints();
        }

        private void SetPoints()
        {
            for (int i = 0; i < side1.childCount; i++)
                side1.GetChild(i).gameObject.SetActive(side1Points == (i + 1));

            for (int i = 0; i < side2.childCount; i++)
                side2.GetChild(i).gameObject.SetActive(side2Points == (i + 1));

            ColorizePoints();
        }

        private void ColorizePoints()
        {
            foreach (SpriteRenderer rend in side1.GetComponentsInChildren<SpriteRenderer>())
                rend.color = colorsBasedOnPointsCount[side1Points];

            foreach (SpriteRenderer rend in side2.GetComponentsInChildren<SpriteRenderer>())
                rend.color = colorsBasedOnPointsCount[side2Points];
        }

        public int GetMatchablePoint()
        {
            if (canSide1Match)
                return side1Points;
            if (canSide2Match)
                return side2Points;

            return -1;
        }

        public List<int> GetAllMatchablePoints()
        {
            List<int> result = new List<int>();

            if (canJumpWithoutMatchingPoints)
            {
                //for (int i = 0; i < 7; i++)
                //{
                //    result.Add(i);
                //}

                result.Add(side1Points); result.Add(side2Points);

                return result;
            }

            if (canSide1Match)
                result.Add(side1Points);
            if (canSide2Match)
                result.Add(side2Points);

            foreach (int point in result)
            {
                Debug.Log("Can Match With These Points: " + point);
            }

            return result;
        }

        private void Update()
        {
            if (randomizePoints)
            {
                randomizePoints = false;

                side1Points = Random.Range(0, 7);
                side2Points = Random.Range(0, 7);

                SetPoints();
            }
        }

        public void MarkSideWhichMatchedByPoints(int matchedPoints)
        {
            Debug.Log("Domino_Match_ Dominos Matched With Points " + matchedPoints);

            //Both sides have the same points
            if ((side1Points == side2Points) && (side2Points == matchedPoints))
            {

            }
            //Sides have different count of points
            else
            {
                if (matchedPoints == side1Points)
                {
                    Debug.Log("Domino_Match_ Next Domino Can Match With Points " + side2Points);

                    canSide1Match = false;
                }
                else if (matchedPoints == side2Points)
                {
                    Debug.Log("Domino_Match_ Next Domino Can Match With Points " + side1Points);

                    canSide2Match = false;
                }

                OnDominoMatched();
            }
        }

        public void OnDominoMatched()
        {
            //Only if not helper domino
            if (canSide1Match && canSide2Match)
            {
            }
            else
                GetComponentInParent<CollectedStacksCounter>().IncreaseCollectedPieces();
        }

        public void RotateBasedOnMatchablePointsSide()
        {
            if (canJumpWithoutMatchingPoints)//If HELPER DOMINO
            {
                transform.Rotate(Vector3.up, 90f, Space.World);
                transform.position += Vector3.forward * 0.35f;
            }
            else if (canSide1Match)
                transform.Rotate(Vector3.up, 180f, Space.World);
        }
    }
}