#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class SetTransformOnEnable : MonoBehaviour
    {
        public bool canExecuteInPlayMode = true;
        public bool executeOnlyOnce = false;
        public bool shouldUnparentAtStart = true;
        public bool canChangeRotation = true;


        public Vector3 rotationAtStart;
        public Vector3 scaleAtStart;

        private bool isExecuted = false;

        bool IsInPrefabMode()
        {
#if UNITY_EDITOR
            return PrefabStageUtility.GetCurrentPrefabStage() != null;
#else
            return false;
#endif
        }

        private void Awake()
        {
            if (executeOnlyOnce && isExecuted) return;

            if (!canExecuteInPlayMode && Application.isPlaying) return;

            Transform parentAtStart = null;

            if (shouldUnparentAtStart && transform.parent != null)
                parentAtStart = transform.parent;

            //Unparent
            if (shouldUnparentAtStart)
                transform.parent = null;

            //Rotate
            if (canChangeRotation)
                transform.Rotate(rotationAtStart);

            //Scale
            if (scaleAtStart.x <= 0f)
                scaleAtStart = new Vector3(1f, scaleAtStart.y, scaleAtStart.z);
            if (scaleAtStart.y <= 0f)
                scaleAtStart = new Vector3(scaleAtStart.x, 1f, scaleAtStart.z);
            if (scaleAtStart.z <= 0f)
                scaleAtStart = new Vector3(scaleAtStart.x, scaleAtStart.y, 1f);
            transform.localScale = scaleAtStart;

            //Set Back Parent
            if (shouldUnparentAtStart && parentAtStart != null)
                transform.parent = parentAtStart;


            if (IsInPrefabMode())
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }

            isExecuted = true;
        }
    }
}