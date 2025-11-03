using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class CollectedEffects : MonoBehaviour
    {
        int tempEffectIndex = 0;

        public void PlayCollectedEffect()
        {
            transform.GetChild(tempEffectIndex).gameObject.SetActive(true);
            tempEffectIndex++;

            CancelInvoke("ResetEffects");
            Invoke("ResetEffects", 0.7f);

            if (tempEffectIndex >= transform.childCount)
                ResetEffects();
        }

        private void ResetEffects()
        {
            for (int i = 0; i < tempEffectIndex; i++)
                transform.GetChild(i).gameObject.SetActive(false);

            tempEffectIndex = 0;
        }
    }
}
