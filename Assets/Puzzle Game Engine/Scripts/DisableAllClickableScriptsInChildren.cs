using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class DisableAllClickableScriptsInChildren : MonoBehaviour
    {
        private void Awake()
        {
            foreach (Clickable clickable in GetComponentsInChildren<Clickable>(true))
                Destroy(clickable);
        }
    }
}