using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class HammerSkill : MonoBehaviour
{
    public GameObject effectToSpawnOnImpact;

    public void Impact()
    {
        Destroy(Instantiate(effectToSpawnOnImpact, SkillButton.currentlyTappedPiece.transform.position, Quaternion.identity), 1f);

        if (SkillButton.currentlyTappedPiece.transform.GetComponentInParent<SoundsManagerForTemplate>() != null)
            SkillButton.currentlyTappedPiece.transform.GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Skill_Hammer_Smash();

        SkillButton.currentlyTappedPiece.GetComponentInParent<CheckNeighbours>().StartRemovingAllPiecesInStack();

        SkillButton.currentlyTappedPiece = null;
    }
}
}