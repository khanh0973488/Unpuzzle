using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class HighlightMatchingStack : MonoBehaviour
    {
        public GameObject highlightPrefab;
        public Vector3 positionOffset;

        private GameObject currentHighlightObject;

        private void Start()
        {
            SpawnHighlightPrefab();
            Highlight(false);
        }

        private void SpawnHighlightPrefab()
        {
            currentHighlightObject = Instantiate(highlightPrefab, null);
            currentHighlightObject.transform.position = transform.position + positionOffset;
            currentHighlightObject.transform.parent = GetComponentInParent<ShowcaseParent>().transform;
        }

        public void Highlight(bool setOn)
        {
            currentHighlightObject.SetActive(setOn);
        }
    }
}
