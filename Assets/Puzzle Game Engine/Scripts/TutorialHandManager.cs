using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class TutorialHandManager : MonoBehaviour
    {
        public bool showTutorialOnlyOnce = true;
        public GameObject tutorialHolder;

        public enum TutorialType
        {
            MoveFromAtoB,
            ClickOnAthenB
        }
        public TutorialType tutorialType;

        [Space]
        [Header("---- Tutorial Type - Move From A to B ----")]
        public Transform startTarget;
        public Transform endTarget;

        public UnityEvent OnReachedEndTarget;

        [Space]
        public float speed = 10f;

        private RectTransform tutorialRectTransform;
        private Camera cam;

        [Space]
        [Header("Tutorial Stop Condition")]
        public Transform[] conditionObjects;
        public bool childCountNeedToDecrease;

        [Space]
        [Header("---- Tutorial Type - Click On A then B ----")]
        public bool onlyShowTempClickableTargetObject = true;
        public Transform[] targetsToClickOn;
        int tempTargetToClickOnIndex = 0;
        Transform tempTargetToShow;
        public Transform[] conditionObjectsToCheckMovement;

        [HideInInspector] public bool canShowTutorial = true;
        [HideInInspector] public bool tutorialIsCompleted = false;

        private void OnEnable()
        {
            if (showTutorialOnlyOnce)
                canShowTutorial = false;

            tutorialRectTransform = GetComponent<RectTransform>();
            cam = Camera.main; // Ensure this is the correct camera that views the world objects

            switch (tutorialType)
            {
                case TutorialType.MoveFromAtoB:
                    StartCoroutine(Move());
                    StartCoroutine(CheckIfShouldHideTutorial_Move());
                    break;

                case TutorialType.ClickOnAthenB:
                    tempTargetToShow = targetsToClickOn[tempTargetToClickOnIndex];
                    TryToShowOnlyClickableTarget();
                    MoveToCanvasPosition(tempTargetToShow.position);
                    StartCoroutine(CheckIfShouldHideTutorial_Click());
                    break;
            }
        }

        private void Update()
        {
            if (IsUserPressingAnyOfTheRequiredKeys())
            {
                StopAllCoroutines();
                HideTutorial();
            }
        }

        private bool IsUserPressingAnyOfTheRequiredKeys()
        {
            if (Input.GetKeyDown(KeyCode.A) ||
                         Input.GetKeyDown(KeyCode.W) ||
                         Input.GetKeyDown(KeyCode.S) ||
                         Input.GetKeyDown(KeyCode.D) ||
                         Input.GetKeyDown(KeyCode.UpArrow) ||
                         Input.GetKeyDown(KeyCode.DownArrow) ||
                         Input.GetKeyDown(KeyCode.RightArrow) ||
                         Input.GetKeyDown(KeyCode.LeftArrow))
                return true;

            return false;
        }

        private void TryToShowOnlyClickableTarget()
        {
            if (onlyShowTempClickableTargetObject)
            {
                for (int i = 0; i < targetsToClickOn.Length; i++)
                {
                    targetsToClickOn[i].gameObject.SetActive(tempTargetToClickOnIndex == i);
                }
            }
        }

        private IEnumerator Move(float startDelay = 0f)
        {
            yield return new WaitForSeconds(startDelay);

            while (true)
            {
                float elapsed = 0;
                float journeyLength = Vector3.Distance(startTarget.position, endTarget.position);
                while (elapsed < journeyLength / speed)
                {
                    elapsed += Time.deltaTime;
                    float fracComplete = elapsed / (journeyLength / speed);
                    Vector3 newPos = Vector3.Lerp(startTarget.position, endTarget.position, fracComplete);
                    MoveToCanvasPosition(newPos);

                    yield return null;
                }

                OnReachedEndTarget.Invoke();


                while (true)
                {
                    elapsed += Time.deltaTime;
                    float fracComplete = elapsed / (journeyLength / speed);
                    Vector3 newPos = Vector3.Lerp(startTarget.position, endTarget.position, fracComplete);
                    MoveToCanvasPosition(newPos);

                    yield return null;
                }
            }
        }

        IEnumerator CheckIfShouldHideTutorial_Move()
        {
            #region Create Helper List To Check Conditions

            List<int> countOfConditionObjectsChildrenAtStart = new List<int>();

            for (int i = 0; i < conditionObjects.Length; i++)
                countOfConditionObjectsChildrenAtStart.Add(conditionObjects[i].childCount);

            #endregion

            while (true)
            {
                yield return new WaitForSeconds(0.2f);

                if (childCountNeedToDecrease)
                {
                    for (int i = 0; i < conditionObjects.Length; i++)
                    {
                        if (conditionObjects[i].childCount < countOfConditionObjectsChildrenAtStart[i])
                        {
                            HideTutorial(true);
                            StopAllCoroutines();
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < conditionObjects.Length; i++)
                    {
                        if (conditionObjects[i].childCount > countOfConditionObjectsChildrenAtStart[i])
                        {
                            HideTutorial(true);
                            StopAllCoroutines();
                        }
                    }
                }
            }
        }

        IEnumerator CheckIfShouldHideTutorial_Click()
        {
            #region Create Helper Lists To Check Conditions

            List<int> countOfConditionObjectsChildrenAtStart = new List<int>();
            for (int i = 0; i < conditionObjects.Length; i++)
                countOfConditionObjectsChildrenAtStart.Add(conditionObjects[i].childCount);

            List<Vector3> conditionObjectsStartPositions = new List<Vector3>();
            for (int i = 0; i < conditionObjectsToCheckMovement.Length; i++)
                conditionObjectsStartPositions.Add(conditionObjectsToCheckMovement[i].transform.position);

            #endregion

            while (true)
            {
                yield return new WaitForSeconds(0.2f);

                //Check Grid Child changes
                if (childCountNeedToDecrease)
                {
                    for (int i = 0; i < conditionObjects.Length; i++)
                    {
                        if (conditionObjects[i].childCount < countOfConditionObjectsChildrenAtStart[i])
                        {
                            //Grid Lost Child

                            conditionObjects[i] = null;

                            #region Update Helper Lists And Condition Arrays

                            countOfConditionObjectsChildrenAtStart = new List<int>();
                            List<Transform> list = new List<Transform>();
                            for (int j = 0; j < conditionObjects.Length; j++)
                            {
                                if (conditionObjects[j] != null)
                                    list.Add(conditionObjects[j]);
                            }
                            conditionObjects = list.ToArray();
                            for (int j = 0; j < conditionObjects.Length; j++)
                                countOfConditionObjectsChildrenAtStart.Add(conditionObjects[j].childCount);

                            conditionObjectsStartPositions = new List<Vector3>();
                            list = new List<Transform>();
                            for (int j = 0; j < conditionObjectsToCheckMovement.Length; j++)
                            {
                                if (conditionObjectsToCheckMovement[j] != null)
                                    list.Add(conditionObjectsToCheckMovement[j]);
                            }
                            conditionObjectsToCheckMovement = list.ToArray();
                            for (int j = 0; j < conditionObjectsToCheckMovement.Length; j++)
                                conditionObjectsStartPositions.Add(conditionObjectsToCheckMovement[j].transform.position);

                            #endregion

                            TryToShowNextTarget();
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < conditionObjects.Length; i++)
                    {
                        if (conditionObjects[i].childCount > countOfConditionObjectsChildrenAtStart[i])
                        {
                            //Grid Got New Child

                            conditionObjects[i] = null;

                            #region Update Helper Lists And Condition Arrays

                            countOfConditionObjectsChildrenAtStart = new List<int>();
                            List<Transform> list = new List<Transform>();
                            for (int j = 0; j < conditionObjects.Length; j++)
                            {
                                if (conditionObjects[j] != null)
                                    list.Add(conditionObjects[j]);
                            }
                            conditionObjects = list.ToArray();
                            for (int j = 0; j < conditionObjects.Length; j++)
                                countOfConditionObjectsChildrenAtStart.Add(conditionObjects[j].childCount);

                            conditionObjectsStartPositions = new List<Vector3>();
                            list = new List<Transform>();
                            for (int j = 0; j < conditionObjectsToCheckMovement.Length; j++)
                            {
                                if (conditionObjectsToCheckMovement[j] != null)
                                    list.Add(conditionObjectsToCheckMovement[j]);
                            }
                            conditionObjectsToCheckMovement = list.ToArray();
                            for (int j = 0; j < conditionObjectsToCheckMovement.Length; j++)
                                conditionObjectsStartPositions.Add(conditionObjectsToCheckMovement[j].transform.position);

                            #endregion

                            TryToShowNextTarget();
                        }
                    }
                }

                //Check Movement Changes
                for (int i = 0; i < conditionObjectsToCheckMovement.Length; i++)
                {
                    if (conditionObjectsToCheckMovement[i].transform.position != conditionObjectsStartPositions[i])
                    {
                        //Position Changed

                        conditionObjectsToCheckMovement[i] = null;

                        #region Update Helper Lists And Condition Arrays

                        countOfConditionObjectsChildrenAtStart = new List<int>();
                        List<Transform> list = new List<Transform>();
                        for (int j = 0; j < conditionObjects.Length; j++)
                        {
                            if (conditionObjects[j] != null)
                                list.Add(conditionObjects[j]);
                        }
                        conditionObjects = list.ToArray();
                        for (int j = 0; j < conditionObjects.Length; j++)
                            countOfConditionObjectsChildrenAtStart.Add(conditionObjects[j].childCount);

                        conditionObjectsStartPositions = new List<Vector3>();
                        list = new List<Transform>();
                        for (int j = 0; j < conditionObjectsToCheckMovement.Length; j++)
                        {
                            if (conditionObjectsToCheckMovement[j] != null)
                                list.Add(conditionObjectsToCheckMovement[j]);
                        }
                        conditionObjectsToCheckMovement = list.ToArray();
                        for (int j = 0; j < conditionObjectsToCheckMovement.Length; j++)
                            conditionObjectsStartPositions.Add(conditionObjectsToCheckMovement[j].transform.position);

                        #endregion

                        TryToShowNextTarget();
                    }
                }
            }
        }

        private void TryToShowNextTarget()
        {
            tempTargetToClickOnIndex++;
            if (tempTargetToClickOnIndex >= targetsToClickOn.Length)
            {
                HideTutorial(true);
                StopAllCoroutines();
            }
            else
            {
                tempTargetToShow = targetsToClickOn[tempTargetToClickOnIndex];
                MoveToCanvasPosition(tempTargetToShow.position);
                TryToShowOnlyClickableTarget();
            }
        }

        public void HideTutorial(bool isHidingBecauseCompleted = false)
        {
            Debug.Log("HIDING TUTORIAL");

            if (isHidingBecauseCompleted) tutorialIsCompleted = true;
            tutorialHolder.gameObject.SetActive(false);
        }

        private void MoveToCanvasPosition(Vector3 worldPos)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
            Vector2 movePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(tutorialRectTransform.parent as RectTransform, screenPoint, cam, out movePos);
            tutorialRectTransform.anchoredPosition = movePos;
        }

        public void MoveBackAfterSeconds(float delay)
        {
            SwitchTargets();

            StartCoroutine(Move(delay));
        }

        public void RestartMovementAfterSeconds(float delay)
        {
            StartCoroutine(Move(delay));
        }

        void SwitchTargets()
        {
            Transform temp = startTarget;
            startTarget = endTarget;
            endTarget = temp;
        }

        private void LateUpdate()
        {
            if (tutorialType == TutorialType.ClickOnAthenB)
                MoveToCanvasPosition(tempTargetToShow.position);
        }
    }
}