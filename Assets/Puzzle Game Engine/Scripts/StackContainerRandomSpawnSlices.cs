using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class StackContainerRandomSpawnSlices : MonoBehaviour
    {
        public bool spawn = false;
        public bool spawnAtStart = false;
        [Space]
        public bool rotateRandomlyAfterSpawned = true;
        public bool setStackToDefaultLayer = true;

        [Space]
        [Range(3, 6)]
        public int pieSlicesCount;
        public GameObject[] piePrefabs;

        [Space]
        [Header("Deciding Random Spawn Count")]
        [Range(1, 6)]
        public int countOfStackToSpawnMin;
        [Range(1, 6)]
        public int countOfStackToSpawnMax;

        private int countOfStackToSpawn;

        [Space]
        [Range(0f, 1f)]
        public float distanceOfStackedObjects;

        [Space]
        [Header("Spawned Object Local Transform")]
        public Vector3 rotateAfterSpawn;
        public Vector3 localPosAfterSpawn;
        public Vector3 localScaleAfterSpawn;

        [Space]
        [Header("Coloring Stacks")]
        public StackColors stackColorsScriptableObject;
        [Header("Deciding Random Colors")]
        public int maxDifferentColorsInStack;
        public int minConsequentColorsAboveEachOther;

        private void Start()
        {
            if (spawnAtStart)
                spawn = true;
        }

        private void Update()
        {
            if (spawn)
            {
                spawn = false;

                DestroyAllChildren();

                SpawnChildren();

                TryToAddAnimation();

                ColorizeStack();
            }
        }

        private void TryToAddAnimation()
        {
            if (Application.isPlaying)
            {
                if (GetComponentInParent<SpawnerOfStackContainer>() != null)
                    GetComponentInParent<SpawnerOfStackContainer>().AddSpawnAnim(GetComponentInParent<SpawnerOfStackContainer>().transform.GetChild(0).gameObject);
            }
        }

        private void SpawnChildren()
        {
            countOfStackToSpawn = Random.Range(countOfStackToSpawnMin, countOfStackToSpawnMax + 1);

            //Making sure that we cannot have empty or full pie
            if (countOfStackToSpawn == 0)
                countOfStackToSpawn = 1;
            if (countOfStackToSpawn == pieSlicesCount)
                countOfStackToSpawn = pieSlicesCount - 1;
            //-------------

            int slicesToRemove = pieSlicesCount - countOfStackToSpawn;

            GameObject tempSpawnedObj = null;
            Vector3 targetPosition;

            tempSpawnedObj = Instantiate(piePrefabs[pieSlicesCount], transform);
            targetPosition = transform.position;
            tempSpawnedObj.transform.position = targetPosition;

            if (setStackToDefaultLayer)
                tempSpawnedObj.layer = 0;

            for (int i = 0; i < slicesToRemove; i++)
            {
                Debug.Log("Disabling " + slicesToRemove + "; because " + pieSlicesCount + " is max count and we want to have enabled count:" + countOfStackToSpawn);
                tempSpawnedObj.transform.GetChild(i).gameObject.SetActive(false);
            }

            if (GetComponent<Animation>() != null)
                GetComponent<Animation>().Play();

            tempSpawnedObj.transform.Rotate(rotateAfterSpawn);
            tempSpawnedObj.transform.localPosition = localPosAfterSpawn;
            tempSpawnedObj.transform.localScale = localScaleAfterSpawn;

            if (rotateRandomlyAfterSpawned)
                tempSpawnedObj.transform.Rotate(Vector3.up, Random.Range(0, 6) * 360f / pieSlicesCount);

            for (int i = 0; i < tempSpawnedObj.transform.childCount; i++)
            {
                tempSpawnedObj.transform.GetChild(i).transform.GetChild(0).localPosition -= Vector3.right * distanceOfStackedObjects;
            }
        }

        private void DestroyAllChildren()
        {
            for (int i = (transform.childCount) - (1); i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }



        private void ColorizeStack()
        {
            Color selectedRandomColor = Color.white;
            if (Application.isPlaying)
            {
                if (GetComponentInParent<UnlockedColors>() != null)
                    selectedRandomColor = stackColorsScriptableObject.GetRandomColor(GetComponentInParent<UnlockedColors>().unlockedColorsAtStart);
                else
                    selectedRandomColor = stackColorsScriptableObject.GetRandomColor();
            }
            else
                selectedRandomColor = stackColorsScriptableObject.GetRandomColor();

            Color newColor = Color.white;

            int i = 0;
            int coloredCount = 0;
            foreach (ColorManager colorManager in GetComponentsInChildren<ColorManager>())
            {
                if ((Random.Range(0, 3) == 0) && coloredCount >= minConsequentColorsAboveEachOther && (i + minConsequentColorsAboveEachOther) <= transform.GetComponentsInChildren<Slice>(false).Length)
                {
                    Debug.Log("Changing color to selected random color: " + selectedRandomColor);

                    do
                    {
                        if (Application.isPlaying)
                        {
                            if (GetComponentInParent<UnlockedColors>() != null)
                            {
                                newColor = stackColorsScriptableObject.GetRandomColor(GetComponentInParent<UnlockedColors>().unlockedColorsAtStart);

                                if (GetComponentInParent<UnlockedColors>().unlockedColorsAtStart <= 1)
                                    break;
                            }
                            else
                            {
                                if (stackColorsScriptableObject.colors.Length <= 1)
                                    break;

                                newColor = stackColorsScriptableObject.GetRandomColor();
                            }
                        }
                        else
                        {
                            if (stackColorsScriptableObject.colors.Length <= 1)
                                break;

                            newColor = stackColorsScriptableObject.GetRandomColor();
                        }
                    } while (newColor == selectedRandomColor);

                    selectedRandomColor = newColor;
                    coloredCount = 0;
                }

                colorManager.ChangeColor(selectedRandomColor);

                i++;
                coloredCount++;
            }
        }


        int minSpawnAtStart;
        int maxSpawnAtStart;

        public void SetFixedSpawnCountOnce(int spawnCount)
        {
            minSpawnAtStart = countOfStackToSpawnMin;
            maxSpawnAtStart = countOfStackToSpawnMax;

            Invoke(nameof(SetBackSpawnCountToDefault), 2f);

            countOfStackToSpawnMin = countOfStackToSpawnMax = spawnCount;
        }

        private void SetBackSpawnCountToDefault()
        {
            countOfStackToSpawnMin = minSpawnAtStart;
            countOfStackToSpawnMax = maxSpawnAtStart;
        }
    }
}