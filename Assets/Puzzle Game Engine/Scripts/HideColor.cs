using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class HideColor : MonoBehaviour
    {
        public GameObject hideColorObject;

        public bool isHidingColor = false;

        private void Start()
        {
            hideColorObject.SetActive(isHidingColor);
        }

        private void OnValidate()
        {
            hideColorObject.SetActive(isHidingColor);
        }

        public void ShowColor()
        {
            if (!isHidingColor) return;

            isHidingColor = false;

            hideColorObject.GetComponent<Animation>().Play();
        }

        public void UpdateColorHider()
        {
            hideColorObject.SetActive(isHidingColor);
        }
    }
}
