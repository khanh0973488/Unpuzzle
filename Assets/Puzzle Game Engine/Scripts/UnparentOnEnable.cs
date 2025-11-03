using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class UnparentOnEnable : MonoBehaviour
    {
        private void OnEnable()
        {
            transform.parent = null;
        }
    }
}