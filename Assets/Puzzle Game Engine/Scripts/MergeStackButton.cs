using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class MergeStackButton : MonoBehaviour
    {
        ShowcaseParent showcaseParent;

        private void Start()
        {
            showcaseParent = GetComponentInParent<ShowcaseParent>();
        }

        public void MergeStack()
        {
            if (showcaseParent.GetComponentInChildren<DealStackButton>().IsDealingCards()) return;

            foreach (CheckNeighbours grid in showcaseParent.GetComponentsInChildren<CheckNeighbours>())
            {
                grid.StartCheckMatchingColorsInStack(true);
            }
        }
    }
}
