using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class ActiveStackContainersRandomManualRespawn : MonoBehaviour
    {
        [Header("Values Only For 1st Spawn")]
        public bool spawnAtStart = true;
        public int countToSpawnAtStart = 5;

        [HideInInspector] public bool canRespawn = true;

        [Space(25)]
        public int minimumEmptyGridsNeededToSpawn = 15;

        [Range(1, 30)]
        public int minCountOfRespawns = 1;
        [Range(1, 30)]
        public int maxCountOfRespawns = 2;

        [Space]
        public float timeBetweenRespawnOfPieces = 0.1f;

        private void Start()
        {
            if (spawnAtStart)
                TryToRespawn(countToSpawnAtStart, countToSpawnAtStart);
        }

        public void TryToRespawn(int newMinSpawn = 0, int newMaxSpawn = 0)
        {
            if (canRespawn)
            {
                if (newMinSpawn > 0)
                    minCountOfRespawns = newMinSpawn;
                if (newMaxSpawn > 0)
                    maxCountOfRespawns = newMaxSpawn;

                StartCoroutine(Respawn());
            }
        }

        IEnumerator Respawn()
        {
            int countToSpawn = Random.Range(minCountOfRespawns, maxCountOfRespawns);

            List<SpawnerOfStackContainer> emptyContainers = new List<SpawnerOfStackContainer>();

            foreach (SpawnerOfStackContainer containerSpawner in GetComponentsInChildren<SpawnerOfStackContainer>(true))
            {
                if (containerSpawner.GetComponentInChildren<ColorManager>(true) == null)
                {
                    emptyContainers.Add(containerSpawner);
                }
            }

            for (int i = 0; i < emptyContainers.Count; i++)
            {
                if (emptyContainers[i].GetComponent<FollowObject>().objectToFollow.childCount > 0)
                {
                    emptyContainers.RemoveAt(i);
                    i--;
                }
            }

            if (emptyContainers.Count >= minimumEmptyGridsNeededToSpawn)
            {
                for (int i = 0; i < countToSpawn; i++)
                {
                    int randomContainerIndex = Random.Range(0, emptyContainers.Count);
                    emptyContainers[randomContainerIndex].SpawnContainer();

                    emptyContainers.RemoveAt(randomContainerIndex);

                    yield return new WaitForSeconds(timeBetweenRespawnOfPieces);
                }
            }
        }

        public void RespawnAtPosition(Transform targetGrid, int countOfStacksToSpawn, Color colorToChoose)
        {
            foreach (SpawnerOfStackContainer containerSpawner in GetComponentsInChildren<SpawnerOfStackContainer>(true))
            {
                if (containerSpawner.GetComponent<FollowObject>().objectToFollow == targetGrid)
                {
                    containerSpawner.SpawnContainer(countOfStacksToSpawn, colorToChoose);
                }
            }
        }
    }
}
