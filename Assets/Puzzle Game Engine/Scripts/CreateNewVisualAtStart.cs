using UnityEngine;

using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteInEditMode]
    public class CreateNewVisualAtStart : MonoBehaviour
    {
        public GameObject newVisualPrefab;
        public Vector3 localScaleOfObject;

        public bool spawn = false;

        private void Update()
        {
            if (spawn)
            {
                spawn = false;

                GameObject newVisualObj = Instantiate(newVisualPrefab, transform.GetComponentInParent<ShowcaseParent>().transform.position, Quaternion.identity);
                newVisualObj.transform.parent = transform.GetComponentInParent<ShowcaseParent>().transform;
                newVisualObj.GetComponent<FollowObject>().objectToFollow = transform;
                newVisualObj.transform.localScale = localScaleOfObject;
            }
        }
    }
}
