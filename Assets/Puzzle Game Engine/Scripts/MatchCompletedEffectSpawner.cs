using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class MatchCompletedEffectSpawner : MonoBehaviour
    {
        public GameObject effectPrefab;
        public Vector3 spawnOffset = Vector3.zero;

        public void SpawnEffect()
        {
            if (GetComponent<Animation>() != null)
                GetComponent<Animation>().Play();

            if (GetComponent<PositionAnimation>() != null)
                GetComponent<PositionAnimation>().PlaySelectedAnimation();
            if (GetComponent<RotationAnimation>() != null)
                GetComponent<RotationAnimation>().PlaySelectedAnimation();
            if (GetComponent<ScaleAnimation>() != null)
                GetComponent<ScaleAnimation>().PlaySelectedAnimation();

            Instantiate(effectPrefab, transform.position + spawnOffset, Quaternion.identity);
        }
    }
}
