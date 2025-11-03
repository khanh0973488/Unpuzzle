using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class RewriteCollectedStacksCounter : MonoBehaviour
{
    private void OnEnable()
    {
        string gameParentName = GetComponentInParent<ShowcaseParent>().gameObject.name;
        CollectedStacksCounter counterOfStacksAndPieces = GetComponentInParent<CollectedStacksCounter>();
        int countOfObjectsToCollect = 0;

        if (gameParentName.Contains("Bottle Jam"))
            countOfObjectsToCollect = GetComponentsInChildren<BoxSpacesForCans>().Length;
        else if (gameParentName.Contains("Unscrew Jam"))
            countOfObjectsToCollect = GetComponentInChildren<ContainersManager>().GetComponentsInChildren<ContainerHolesHolder>().Length;
        else if (gameParentName.Contains("Tap Away 3D"))
            countOfObjectsToCollect = GetComponentsInChildren<Block>().Length;
        else if (gameParentName.Contains("Unpuzzle"))
            countOfObjectsToCollect = GetComponentsInChildren<Block>().Length;
        else if (gameParentName.Contains("Sort Nuts"))
            countOfObjectsToCollect = GetComponentsInChildren<SelectableAfterPlaced>().Length / GetComponentInChildren<CheckNeighbours>().countOfStacksNeededToMatch;
        else if (gameParentName.Contains("Sort Cubes"))
            countOfObjectsToCollect = GetComponentsInChildren<RotateAndMoveAround>().Length;
        else if (gameParentName.Contains("Sort Cards 2"))
            countOfObjectsToCollect = GetComponentsInChildren<RotationAnimation>().Length / GetComponentInChildren<CheckNeighbours>().countOfStacksNeededToMatch;

        counterOfStacksAndPieces.SetUpCollectable(transform.GetSiblingIndex(), countOfObjectsToCollect);
    }
}
}