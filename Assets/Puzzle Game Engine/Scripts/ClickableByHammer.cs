using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class ClickableByHammer : MonoBehaviour
    {
        private void OnMouseUp()
        {
            SkillButton.currentlyUsingSkill.SpawnHammer(transform);
        }
    }
}