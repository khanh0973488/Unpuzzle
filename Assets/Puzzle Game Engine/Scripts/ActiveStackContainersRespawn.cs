using System.Collections;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class ActiveStackContainersRespawn : MonoBehaviour
    {
        public bool respawnOnlyIfAllContainersAreEmpty = true;

        [Range(0f, 1f)]
        public float timeBetweenRespawns = 0f;

        public void TryToRespawn()
        {
            if (respawnOnlyIfAllContainersAreEmpty)
            {
                if (GetComponentInChildren<Draggable>(false) == null)
                    StartCoroutine(Respawn());
            }
            else
                StartCoroutine(Respawn());
        }

        IEnumerator Respawn()
        {
            foreach (SpawnerOfStackContainer containerSpawner in GetComponentsInChildren<SpawnerOfStackContainer>(false))
            {
                if (containerSpawner.GetComponentInChildren<Draggable>(false) == null)
                {
                    containerSpawner.SpawnContainer();

                    if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                        GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Appear();

                    yield return new WaitForSeconds(timeBetweenRespawns);
                }
            }
        }
    }
}
