using UnityEngine;
using UnityEngine.Events;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class DestroyGameobject : MonoBehaviour
    {
        [Header("Different Events Which Are Called On Destroy")]
        public GameObject[] onDestroyEnable;
        public GameObject[] onDestroyDisable;

        [Space]
        public UnityEvent OnDestroyDo;

        public void DestroyObject(float delay = 0f)
        {
            transform.parent = null;
            Destroy(gameObject, delay);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < onDestroyEnable.Length; i++)
                onDestroyEnable[i].SetActive(true);

            for (int i = 0; i < onDestroyDisable.Length; i++)
                onDestroyDisable[i].SetActive(false);

            OnDestroyDo.Invoke();
        }
    }
}
