using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class SpawnerOfStackContainer : MonoBehaviour
    {
        public bool spawn = false;

        public bool isSpawningRandomContainer = true;

        [Space]
        [Header("Prefabs")]
        public GameObject stackContainerManual;
        public GameObject stackContainerRandom;

        [Space]
        [Header("Animation")]
        public string spawnAnimationName;
        public AnimationClip[] chooseableAnimations;


        private void Update()
        {
            if (spawn)
            {
                spawn = false;

                DeleteCurrentContainer();

                SpawnContainer();
            }
        }

        public void SpawnContainer(int countOfFixedSpawns = 0)
        {
            GameObject tempContainer = null;

            if (isSpawningRandomContainer)
            {
                tempContainer = Instantiate(stackContainerRandom, transform);

                if (tempContainer.GetComponent<StackContainerRandomSpawn>() != null)
                {
                    if (countOfFixedSpawns != 0)
                    {
                        tempContainer.GetComponent<StackContainerRandomSpawn>().SetFixedSpawnCountOnce(countOfFixedSpawns);
                    }

                    tempContainer.GetComponent<StackContainerRandomSpawn>().spawn = true;
                }
                else if (tempContainer.GetComponentInChildren<StackContainerRandomSpawnSlices>() != null)
                {
                    if (countOfFixedSpawns != 0)
                    {
                        tempContainer.GetComponent<StackContainerRandomSpawnSlices>().SetFixedSpawnCountOnce(countOfFixedSpawns);
                    }

                    tempContainer.GetComponentInChildren<StackContainerRandomSpawnSlices>().spawn = true;
                }
            }
            else
            {
                tempContainer = Instantiate(stackContainerManual, transform);

                if (countOfFixedSpawns != 0)
                {
                    tempContainer.GetComponent<StackContainer>().SetFixedSpawnCountOnce(countOfFixedSpawns);
                }

                tempContainer.GetComponent<StackContainer>().spawn = true;
            }

        }

        public void SpawnContainer(int countOfFixedSpawns, Color chooseColor)
        {
            GameObject tempContainer = null;

            if (isSpawningRandomContainer)
            {
                tempContainer = Instantiate(stackContainerRandom, transform);

                if (tempContainer.GetComponent<StackContainerRandomSpawn>() != null)
                {
                    if (countOfFixedSpawns != 0)
                    {
                        tempContainer.GetComponent<StackContainerRandomSpawn>().SetFixedSpawnCountOnce(countOfFixedSpawns);
                        tempContainer.GetComponent<StackContainerRandomSpawn>().SetFixedSpawnColorOnce(chooseColor);
                    }

                    tempContainer.GetComponent<StackContainerRandomSpawn>().spawn = true;
                }
                else if (tempContainer.GetComponentInChildren<StackContainerRandomSpawnSlices>() != null)
                {
                    if (countOfFixedSpawns != 0)
                    {
                        tempContainer.GetComponent<StackContainerRandomSpawnSlices>().SetFixedSpawnCountOnce(countOfFixedSpawns);
                    }

                    tempContainer.GetComponentInChildren<StackContainerRandomSpawnSlices>().spawn = true;
                }
            }
            else
            {
                tempContainer = Instantiate(stackContainerManual, transform);

                if (countOfFixedSpawns != 0)
                {
                    tempContainer.GetComponent<StackContainer>().SetFixedSpawnCountOnce(countOfFixedSpawns);
                }

                tempContainer.GetComponent<StackContainer>().spawn = true;
            }
        }

        public void AddSpawnAnim(GameObject tempContainer)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<RotationAnimation>() != null)
                    transform.GetChild(i).GetComponent<RotationAnimation>().PlaySelectedAnimation();
                if (transform.GetChild(i).GetComponent<PositionAnimation>() != null)
                    transform.GetChild(i).GetComponent<PositionAnimation>().PlaySelectedAnimation();

                return;
            }

            Debug.Log("Trying To Add Animation at Spawn");
            bool hasFoundSelectedAnimation = false;
            int indexOfSelectedAnim = 0;

            for (int i = 0; i < chooseableAnimations.Length; i++)
            {
                if (chooseableAnimations[i].name == spawnAnimationName)
                {
                    hasFoundSelectedAnimation = true;
                    indexOfSelectedAnim = i;
                    break;
                }
            }

            if (!hasFoundSelectedAnimation)
            {
                Debug.Log("No Spawn Animation Has Been Found.");
                return;
            }
            else
            {
                Animation tempContainerAnim = tempContainer.GetComponent<Animation>();
                if (tempContainerAnim == null)
                    tempContainerAnim = tempContainer.AddComponent<Animation>();
                tempContainerAnim.playAutomatically = false;
                tempContainerAnim.clip = chooseableAnimations[indexOfSelectedAnim];
                tempContainerAnim.AddClip(chooseableAnimations[indexOfSelectedAnim], spawnAnimationName);
                tempContainerAnim.Play(spawnAnimationName);
            }
        }

        public void DeleteCurrentContainer()
        {
            if (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);
        }

        #region Overwrite Values By UI

        [Space]
        [Header("Overwrite Values")]
        public bool canOverwriteValuesByUI = true;
        public UnityEvent OnOverwriteValue;
        public GameObject[] stackTypes;

        public void SpawnStacks()
        {
            //spawn = true;
        }

        public void SpawnStackPieces()
        {
            GetComponentInChildren<StackContainerRandomSpawn>().spawn = true;
        }

        public void OverwriteValue_HorizontalDistance(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            GetComponentInParent<SpaceChildrenEvenly>().distance = new Vector3(slider.value, 0f, 0f);
            GetComponentInParent<SpaceChildrenEvenly>().OrderChildren();

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_PiecesDistance(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            GetComponentInChildren<StackContainerRandomSpawn>().distanceOfStackedObjects = slider.value;

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_SpawnDirection_X(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            GetComponentInChildren<StackContainerRandomSpawn>().spawnDirection = new Vector3(
                slider.value,
                GetComponentInChildren<StackContainerRandomSpawn>().spawnDirection.y,
                GetComponentInChildren<StackContainerRandomSpawn>().spawnDirection.z);

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_SpawnDirection_Y(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            GetComponentInChildren<StackContainerRandomSpawn>().spawnDirection = new Vector3(
                GetComponentInChildren<StackContainerRandomSpawn>().spawnDirection.x,
                slider.value,
                GetComponentInChildren<StackContainerRandomSpawn>().spawnDirection.z);

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_SpawnDirection_Z(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            GetComponentInChildren<StackContainerRandomSpawn>().spawnDirection = new Vector3(
                GetComponentInChildren<StackContainerRandomSpawn>().spawnDirection.x,
                GetComponentInChildren<StackContainerRandomSpawn>().spawnDirection.y,
                slider.value);

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_CountOfPieces_Min(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            GetComponentInChildren<StackContainerRandomSpawn>().countOfStackToSpawnMin = Mathf.RoundToInt(slider.value);

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_CountOfPieces_Max(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            GetComponentInChildren<StackContainerRandomSpawn>().countOfStackToSpawnMax = Mathf.RoundToInt(slider.value);

            OnOverwriteValue.Invoke();
        }

        public void OverwriteValue_MaxDifferentColorsInStack(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            GetComponentInChildren<StackContainerRandomSpawn>().maxDifferentColorsInStack = Mathf.RoundToInt(slider.value);
            GetComponentInChildren<StackContainerRandomSpawn>().ColorizeStack();
        }

        public void OverwriteValue_MinColorsNextToEachOther(Slider slider)
        {
            if (!canOverwriteValuesByUI) return;

            GetComponentInChildren<StackContainerRandomSpawn>().minConsequentColorsAboveEachOther = Mathf.RoundToInt(slider.value);
            GetComponentInChildren<StackContainerRandomSpawn>().ColorizeStack();
        }

        public void ColorizeChildrenStack()
        {
            GetComponentInChildren<StackContainerRandomSpawn>().ColorizeStack();
        }

        public void OverwriteValue_StackType(TMP_Dropdown dropdown)
        {
            if (!canOverwriteValuesByUI) return;

            int stackTypeInt = (int)dropdown.value;
            stackContainerRandom = stackTypes[stackTypeInt];

            spawn = true;
        }

        #endregion
    }
}
