using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class MainCameraController : MonoBehaviour
    {
        #region Create Instance

        public static MainCameraController Instance;

        private void Awake()
        {
            Instance = this;
        }

        #endregion

        public Button[] focusButtons;
        public bool isDeactivatingNonFocusedTemplates = false;
        [Header("Zoom")]
        public float minCamSize = 5f;
        public float maxCamSize = 12f;

        [Space]
        [Header("Speed")]
        public float movementSpeed = 10f;
        public float movementSpeedFast = 15f;

        private float tempSpeed = 10f;
        private Camera mainCam;
        bool isFocusingOnObject = false;
        bool isView2D = true;

        [Space]
        public float transitionDuration = 2.0f; // Duration of the transition in seconds
        float elapsedTime = 0.0f;

        private GameObject tutorialMovement = null;

        [Space]
        public Transform[] gamesCamHolders;

        [Space]
        [Header("Template View Info")]
        public TextMeshProUGUI templateCountText;
        public TextMeshProUGUI templateNameText;

        private bool canListenPlayerInput = true;

        private void Start()
        {
            if (FindObjectOfType<TutorialMovement>(true) != null)
                tutorialMovement = FindObjectOfType<TutorialMovement>(true).gameObject;

            mainCam = Camera.main;
        }

        public void SetInputListener(bool canListenForUserInput)
        {
            canListenPlayerInput = canListenForUserInput;
        }

        private void Update()
        {
            //Input Handling
            if (!canListenPlayerInput) return;

            if (Input.GetKeyUp(KeyCode.C))
                SwitchCameraPerspective();

            #region Movements With Arrow Keys

            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                FocusOnObject(GetComponentInParent<FocusableObject>().GetNextFocusableObject());
                return;
            }
            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                FocusOnObject(GetComponentInParent<FocusableObject>().GetPreviousFocusableObject());
                return;
            }
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                int countOfButtonsInFocusButtonHolder = focusButtons.Length;
                int previousIndex = GetComponentInParent<FocusableObject>().categoryIndex - 1;

                if (previousIndex >= 0)
                    focusButtons[previousIndex].onClick.Invoke();

                return;
            }
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                int countOfButtonsInFocusButtonHolder = focusButtons.Length;
                int nextIndex = GetComponentInParent<FocusableObject>().categoryIndex + 1;

                if (nextIndex < countOfButtonsInFocusButtonHolder)
                    focusButtons[nextIndex].onClick.Invoke();

                return;
            }

            #endregion

            #region Focusing On Template

            if (isFocusingOnObject)
            {
                if (elapsedTime < transitionDuration)
                {
                    // Calculate the fraction of the total transition time that has passed
                    float fraction = elapsedTime / transitionDuration;

                    // Move the position towards Vector3.zero
                    transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, fraction);

                    if (tutorialMovement == null || !tutorialMovement.activeInHierarchy)
                    {
                        // Rotate towards Quaternion.identity
                        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, fraction);
                    }

                    // Adjust the camera's orthographic size
                    mainCam.orthographicSize = Mathf.Lerp(mainCam.orthographicSize, minCamSize, fraction);

                    if (tutorialMovement == null || !tutorialMovement.activeInHierarchy)
                    {
                        if (transform.localPosition == Vector3.zero && transform.localRotation == Quaternion.identity && mainCam.orthographicSize == minCamSize)
                            elapsedTime = transitionDuration;
                    }
                    else
                    {
                        if (transform.localPosition == Vector3.zero && mainCam.orthographicSize == minCamSize)
                            elapsedTime = transitionDuration;
                    }

                    // Increment the elapsed time by the time since the last frame
                    elapsedTime += Time.deltaTime;
                }
                else
                {
                    // Ensure the final values are set once the duration has passed
                    transform.localPosition = Vector3.zero;
                    if (tutorialMovement == null || !tutorialMovement.activeInHierarchy)
                        transform.localRotation = Quaternion.identity;
                    mainCam.orthographicSize = minCamSize;
                    isFocusingOnObject = false;

                    //At end of focus try to enable tutorial for the template
                    Transform showcaseParent = null;

                    if (GetComponentInParent<ShowcaseParent>() != null)
                        showcaseParent = GetComponentInParent<ShowcaseParent>().transform;

                    if (showcaseParent != null)
                    {
                        if (showcaseParent.GetComponentInChildren<TutorialUI>(true) != null)
                        {
                            showcaseParent.GetComponentInChildren<TutorialUI>(true).gameObject.SetActive(true);
                        }
                        if (showcaseParent.GetComponentInChildren<TutorialHandManager>(true) != null)
                        {
                            if (showcaseParent.GetComponentInChildren<TutorialHandManager>(true).canShowTutorial &&
                                !showcaseParent.GetComponentInChildren<TutorialHandManager>(true).tutorialIsCompleted)
                                showcaseParent.GetComponentInChildren<TutorialHandManager>(true).tutorialHolder.gameObject.SetActive(true);
                        }
                    }
                }

                return; // Early exit from update if focusing
            }

            #endregion

            #region Faster Movement

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                tempSpeed = movementSpeedFast;
            else
                tempSpeed = movementSpeed;

            #endregion

            #region Movements With W-A-S-D

            if (Input.GetKey(KeyCode.W))
                transform.Translate(Vector3.forward * tempSpeed * Time.deltaTime, Space.World);
            if (Input.GetKey(KeyCode.S))
                transform.Translate(-Vector3.forward * tempSpeed * Time.deltaTime, Space.World);
            if (Input.GetKey(KeyCode.A))
                transform.Translate(-Vector3.right * tempSpeed * Time.deltaTime, Space.World);
            if (Input.GetKey(KeyCode.D))
                transform.Translate(Vector3.right * tempSpeed * Time.deltaTime, Space.World);

            //if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.W) &&
            //    !Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Q))
            if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.W) ||
    Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Q) &&
    (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.W) &&
                !Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Q))
    )
                Focus();

            #endregion

            #region Zooming In and Out

            if (Input.GetKey(KeyCode.E))
                mainCam.orthographicSize = Mathf.Clamp(mainCam.orthographicSize + tempSpeed * Time.deltaTime, minCamSize, maxCamSize);
            if (Input.GetKey(KeyCode.Q))
                mainCam.orthographicSize = Mathf.Clamp(mainCam.orthographicSize - tempSpeed * Time.deltaTime, minCamSize, maxCamSize);

            #endregion

            if (Input.GetKey(KeyCode.F))
                Focus();
        }

        private void SwitchCameraPerspective()
        {
            isView2D = !isView2D;

            mainCam.orthographic = isView2D;
        }

        #region Start To Focus On Template

        public void FocusOnObject(Transform camHolder)
        {
            if (camHolder != null)
            {
                elapsedTime = 0.0f; // Reset the elapsed time at the start of focus
                isFocusingOnObject = true;

                transform.parent = camHolder;

                DisableNonFocusedTutorials();

                DisableNonFocusedMoneyUIs();

                DisableNonFocusedTemplates();

                UpdateViewInfoTexts();
            }
        }

        public void FocusOnOGameByIndexOfObject(Transform transformOfObject)
        {
            if (transformOfObject != null)
            {
                elapsedTime = 0.0f; // Reset the elapsed time at the start of focus
                isFocusingOnObject = true;

                transform.parent = gamesCamHolders[transformOfObject.GetSiblingIndex()];

                DisableNonFocusedTutorials();

                DisableNonFocusedMoneyUIs();

                DisableNonFocusedTemplates();

                UpdateViewInfoTexts();
            }
        }

        private void Focus()
        {
            Debug.Log("CALLED FOCUS OF CAMERA");

            List<FocusableObject> focusableObjects = new List<FocusableObject>();

            float currentClosestDistance = Mathf.Infinity;
            FocusableObject currentClosestFocusableObject = null;

            foreach (FocusableObject obj in FindObjectsOfType<FocusableObject>())
            {
                float tempDistance = Vector3.Distance(transform.position, obj.transform.position);

                if (tempDistance <= currentClosestDistance)
                {
                    currentClosestDistance = tempDistance;
                    currentClosestFocusableObject = obj;
                }
            }

            if (currentClosestFocusableObject != null)
                FocusOnObject(currentClosestFocusableObject.GetCurrentCamHolder());
        }

        #endregion

        #region Disabling Non Focused Elements

        private void DisableNonFocusedTutorials()
        {
            ShowcaseParent showcaseParent = GetComponentInParent<ShowcaseParent>(true);
            if (showcaseParent == null)
                return;

            //Disable all tutorials except for this templates'
            foreach (TutorialHandManager tutorial in FindObjectsOfType<TutorialHandManager>())
            {
                if (tutorial.GetComponentInParent<ShowcaseParent>(true).gameObject.name != GetComponentInParent<ShowcaseParent>(true).gameObject.name)
                    tutorial.HideTutorial(false);
            }

            showcaseParent.gameObject.SetActive(true);
        }

        private void DisableNonFocusedMoneyUIs()
        {
            ShowcaseParent showcaseParent = GetComponentInParent<ShowcaseParent>(true);
            if (showcaseParent == null)
                return;

            //Disable all money UIs except for this templates'
            foreach (MoneyChooserUI moneyUI in FindObjectsOfType<MoneyChooserUI>())
            {
                if (moneyUI.GetComponentInParent<ShowcaseParent>(true).gameObject.name != showcaseParent.gameObject.name)
                    moneyUI.gameObject.SetActive(false);
            }
            if (showcaseParent.GetComponentInChildren<MoneyChooserUI>(true) != null)
                showcaseParent.GetComponentInChildren<MoneyChooserUI>(true).gameObject.SetActive(true);
        }

        private void DisableNonFocusedTemplates()
        {
            ShowcaseParent showcaseParent = GetComponentInParent<ShowcaseParent>(true);
            if (showcaseParent == null)
                return;

            //Disable Other Templates
            if (isDeactivatingNonFocusedTemplates)
            {
                foreach (ShowcaseParent template in FindObjectsOfType<ShowcaseParent>())
                {
                    if (template != showcaseParent)
                        template.gameObject.SetActive(false);
                }
            }

            foreach (CheckNeighboursQueue neighboursChecking in FindObjectsOfType<CheckNeighboursQueue>())
            {
                if (neighboursChecking.GetComponentInParent<ShowcaseParent>() != showcaseParent)
                    neighboursChecking.enabled = false;
                else
                    neighboursChecking.enabled = true;
            }
        }

        #endregion

        private void UpdateViewInfoTexts()
        {
            if (templateCountText == null || templateNameText == null) return;

            ShowcaseParent showcaseParent = GetComponentInParent<ShowcaseParent>();

            if (showcaseParent == null)
            {
                templateNameText.text = "Tutorial";
                templateCountText.text = "";
            }
            else
            {
                templateNameText.text = showcaseParent.transform.parent.gameObject.name;
                templateCountText.text = (showcaseParent.transform.GetSiblingIndex() + 1).ToString() + "/" + showcaseParent.transform.parent.childCount.ToString();
            }
        }

        public bool IsFocusingOnTemplate(ShowcaseParent templateToCheck)
        {
            if (GetComponentInParent<ShowcaseParent>() == null)
                return false;

            Debug.Log(GetComponentInParent<ShowcaseParent>().gameObject.name + "____" + templateToCheck.gameObject.name);

            return (GetComponentInParent<ShowcaseParent>().gameObject.name == templateToCheck.gameObject.name);
        }
    }
}