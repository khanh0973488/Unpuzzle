using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;
using System;
using static HyperPuzzleEngine.StackColors;
using System.Runtime.CompilerServices;
using System.Linq;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class BoxJumpController : MonoBehaviour
    {
        [Header("Line Holders")]
        public Transform topLineHolder; // Transform holding all top line slots as children
        public Transform middleLineHolder; // Transform holding all middle line slots as children
        public Transform bottomLineHolder; // Transform holding all bottom line slots as children

        [Space(10)]
        [Header("Jump Settings")]
        public float jumpHeight = 2f;
        public float jumpDuration = 1f;

        [Space(10)]
        [Header("Jump Animations")]
        public string jumpToMiddleLineAnimName = "BoxJump_ToMiddle";
        public string jumpToFrontLineAnimName = "BoxJump_ToFront";

        [Space(10)]
        [Header("Box Management")]
        public bool updateBoxes = false;
        public List<LineBox> middleLineBoxPresence;
        public List<LineBox> bottomLineBoxPresence;

        public GameObject boxPrefab; // Prefab for the box to instantiate
        public StackColors stackColors; // Reference to StackColors scriptable object to select colors

        private Transform[] topLineSlots;
        private Transform[] middleLineSlots;
        private Transform[] bottomLineSlots;
        private bool[] reservedTopSlots;

        //private int pendingChecks = 0; // Track the number of requested checks
        //private bool isCheckingConveyorBelts = false; // Flag to indicate if coroutine is running
        private List<ConveyorBelt> sortedConveyorBelts = new List<ConveyorBelt>();

        private void Start()
        {
            InitializeSlots(false);
            InitializeConveyorBelts();
        }

        void Update()
        {
            if (updateBoxes)
            {
                InitializeSlots(false);
                ClearAllBoxes();
                ManageBoxPresence();
                UpdateBoxColors();
                updateBoxes = false;
            }

            if (!Application.isPlaying) return;

            // Check for user input to select a box from the middle line
            if (Application.isPlaying && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Debug.Log("Ray created from screen point: " + Input.mousePosition);
                if (Camera.main == null)
                {
                    Debug.LogWarning("Main camera not found. Make sure there is a camera tagged as MainCamera in the scene.");
                    return;
                }
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log("Raycast successful, hit registered.");
                    Debug.Log("Hit object: " + hit.collider.gameObject.name + " at position: " + hit.point);
                    GameObject selectedBox = hit.collider.gameObject;
                    for (int i = 0; i < middleLineSlots.Length; i++)
                    {
                        Debug.Log("Checking if clicked box is in middle line at index: " + i);
                        if (middleLineSlots[i].childCount > 0 && middleLineSlots[i].GetChild(0).gameObject == selectedBox)
                        {
                            // Set the box's parent to the middle line
                            selectedBox.transform.SetParent(middleLineSlots[i]);
                            // Find the first available slot in the top line
                            for (int j = 0; j < topLineSlots.Length; j++)
                            {
                                if (topLineSlots[j].childCount == 0 && !IsSlotReserved(j))
                                {
                                    Debug.Log("Starting jump for box: " + selectedBox.name + " from middle line to top line");
                                    StartCoroutine(JumpToPosition(selectedBox, topLineSlots[j].position, OnMoved_ToTopLine));
                                    selectedBox.transform.SetParent(topLineSlots[j]);
                                    ReserveSlot(j);
                                    OnMovementStarted_ToTopLine(selectedBox);

                                    // Check if there's a box right behind in the bottom line to move up
                                    if (bottomLineSlots[i].childCount > 0)
                                    {
                                        GameObject bottomBox = bottomLineSlots[i].GetChild(0).gameObject;
                                        Debug.Log("Starting jump for box: " + bottomBox.name + " from bottom line to middle line");
                                        bottomBox.transform.SetParent(middleLineSlots[i]);
                                        StartCoroutine(JumpToPosition(bottomBox, middleLineSlots[i].position, OnMoved_ToMiddleLine));
                                        OnMovementStarted_ToMiddleLine(bottomBox);
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateBoxColors()
        {
            // Update the colors of all boxes based on their specified color in the presence arrays
            for (int i = 0; i < middleLineBoxPresence.Count; i++)
            {
                if (middleLineSlots[i].childCount > 0)
                {
                    GameObject middleBox = middleLineSlots[i].GetChild(0).gameObject;
                    foreach (ColorManager colorManager in middleBox.GetComponentsInChildren<ColorManager>())
                    {
                        if (colorManager != null && stackColors != null)
                        {
                            colorManager.ChangeColorInEditor(GetColorFromEnum(middleLineBoxPresence[i].boxColor));
                        }
                    }
                }
            }

            for (int i = 0; i < bottomLineBoxPresence.Count; i++)
            {
                if (bottomLineSlots[i].childCount > 0)
                {
                    GameObject bottomBox = bottomLineSlots[i].GetChild(0).gameObject;

                    foreach (ColorManager colorManager in bottomBox.GetComponentsInChildren<ColorManager>())
                    {
                        if (colorManager != null && stackColors != null)
                        {
                            colorManager.ChangeColorInEditor(GetColorFromEnum(bottomLineBoxPresence[i].boxColor));
                        }
                    }
                }
            }
        }

        void InitializeSlots(bool didEditBoxesManually = false)
        {
            reservedTopSlots = new bool[topLineHolder.childCount];
            topLineSlots = new Transform[topLineHolder.childCount];
            middleLineSlots = new Transform[middleLineHolder.childCount];
            bottomLineSlots = new Transform[bottomLineHolder.childCount];

            for (int i = 0; i < topLineHolder.childCount; i++)
            {
                topLineSlots[i] = topLineHolder.GetChild(i);
            }

            for (int i = 0; i < middleLineHolder.childCount; i++)
            {
                middleLineSlots[i] = middleLineHolder.GetChild(i);

                if (middleLineSlots[i].childCount > 0)
                {
                    GameObject boxObject = middleLineSlots[i].GetChild(0).gameObject;
                    if (boxObject != null)
                        OpenBoxBySettingItToAnimFirstFrame(boxObject);
                }
            }

            for (int i = 0; i < bottomLineHolder.childCount; i++)
            {
                bottomLineSlots[i] = bottomLineHolder.GetChild(i);
            }

            if (!didEditBoxesManually)
            {
                return;
            }

            // Initialize the presence lists
            middleLineBoxPresence = new List<LineBox>(new LineBox[middleLineSlots.Length]);
            bottomLineBoxPresence = new List<LineBox>(new LineBox[bottomLineSlots.Length]);
        }

        void ClearAllBoxes()
        {
            // Clear all boxes from the middle and bottom lines
            foreach (Transform slot in middleLineSlots)
            {
                if (slot.childCount > 0)
                {
                    DestroyImmediate(slot.GetChild(0).gameObject);
                }
            }

            foreach (Transform slot in bottomLineSlots)
            {
                if (slot.childCount > 0)
                {
                    DestroyImmediate(slot.GetChild(0).gameObject);
                }
            }

            //// Reinitialize the presence lists to match the number of slots
            //middleLineBoxPresence = new List<LineBox>(new LineBox[middleLineSlots.Length]);
            //bottomLineBoxPresence = new List<LineBox>(new LineBox[bottomLineSlots.Length]);
        }

        void ManageBoxPresence()
        {
            for (int i = 0; i < middleLineBoxPresence.Count; i++)
            {
                if (middleLineBoxPresence[i].hasBox && middleLineSlots[i].childCount == 0)
                {
                    GameObject newBox = Instantiate(boxPrefab, middleLineSlots[i].position, Quaternion.identity, middleLineSlots[i]);
                    //ColorManager colorManager = newBox.GetComponent<ColorManager>();

                    foreach (ColorManager colorManager in newBox.GetComponentsInChildren<ColorManager>())
                    {
                        if (colorManager != null && stackColors != null)
                        {
                            colorManager.ChangeColorInEditor(GetColorFromEnum(middleLineBoxPresence[i].boxColor));
                        }
                    }
                }
                else if (!middleLineBoxPresence[i].hasBox && middleLineSlots[i].childCount > 0)
                {
                    DestroyImmediate(middleLineSlots[i].GetChild(0).gameObject);
                }
            }

            for (int i = 0; i < bottomLineBoxPresence.Count; i++)
            {
                if (bottomLineBoxPresence[i].hasBox && bottomLineSlots[i].childCount == 0)
                {
                    GameObject newBox = Instantiate(boxPrefab, bottomLineSlots[i].position, Quaternion.identity, bottomLineSlots[i]);
                    //ColorManager colorManager = newBox.GetComponent<ColorManager>();

                    foreach (ColorManager colorManager in newBox.GetComponentsInChildren<ColorManager>())
                    {
                        if (colorManager != null && stackColors != null)
                        {
                            colorManager.ChangeColorInEditor(GetColorFromEnum(bottomLineBoxPresence[i].boxColor));
                        }
                    }
                }
                else if (!bottomLineBoxPresence[i].hasBox && bottomLineSlots[i].childCount > 0)
                {
                    DestroyImmediate(bottomLineSlots[i].GetChild(0).gameObject);
                }
            }
        }

        void ReserveSlot(int index)
        {
            reservedTopSlots[index] = true;
        }

        public void ReleaseSlot(int index)
        {
            reservedTopSlots[index] = false;
        }

        bool IsSlotReserved(int index)
        {
            return reservedTopSlots[index];
        }

        IEnumerator JumpToPosition(GameObject box, Vector3 targetPosition, Action<GameObject> onComplete = null)
        {
            Vector3 startPosition = box.transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < jumpDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / jumpDuration);
                float height = Mathf.Sin(Mathf.PI * t) * jumpHeight;
                box.transform.position = Vector3.Lerp(startPosition, targetPosition, t) + Vector3.up * height;
                yield return null;
            }

            box.transform.position = targetPosition;

            // Release slot when the box reaches the target position
            for (int i = 0; i < topLineSlots.Length; i++)
            {
                if (topLineSlots[i].position == targetPosition)
                {
                    ReleaseSlot(i);
                    box.transform.SetParent(topLineSlots[i]);
                    break;
                }
            }

            // If an action is provided, invoke it, passing the `box` as a parameter
            onComplete?.Invoke(box);
        }

        void OpenBoxBySettingItToAnimFirstFrame(GameObject box)
        {
            Animation animationComponent = box.GetComponent<Animation>();

            if (animationComponent == null)
            {
                Debug.LogWarning("Animation component is missing.");
                return;
            }

            // Make sure the animation clip exists in the animation component
            if (!animationComponent.IsPlaying(jumpToFrontLineAnimName))
            {
                // Play the animation and immediately set it to the first frame
                animationComponent.Play(jumpToFrontLineAnimName);
                animationComponent[jumpToFrontLineAnimName].time = 0; // Set to the start (first frame)
                animationComponent.Sample(); // Apply the first frame visually
                animationComponent.Stop(); // Stop the animation to keep it at the first frame

                Debug.Log($"Displayed the first frame of animation: {jumpToFrontLineAnimName}");
            }
        }

        #region Movement Actions

        int countOfCurrentlyJumpingBoxesToFirstLine = 0;

        void OnMovementStarted_ToMiddleLine(GameObject box)
        {
            AnimationPlayer animPlayerOfBox = box.GetComponentInChildren<AnimationPlayer>();
            if (animPlayerOfBox != null)
                animPlayerOfBox.PlayAnimation(jumpToMiddleLineAnimName);
        }

        void OnMovementStarted_ToTopLine(GameObject box)
        {
            countOfCurrentlyJumpingBoxesToFirstLine++;

            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_BottleJam_BoxJump();

            AnimationPlayer animPlayerOfBox = box.GetComponentInChildren<AnimationPlayer>();
            if (animPlayerOfBox != null)
                animPlayerOfBox.PlayAnimation(jumpToFrontLineAnimName);
        }

        void OnMoved_ToMiddleLine(GameObject box)
        {

        }

        void OnMoved_ToTopLine(GameObject box)
        {
            countOfCurrentlyJumpingBoxesToFirstLine--;

            CheckForSameColoredCansInFirstRowsBoxes();
            GetComponentInParent<CheckForMoreTargetPositions>().CheckIfAnyTargetIsFree();
        }

        #endregion

        #region Conveyor Belts Checking For Same Colored Cans In First Row

        private int pendingChecks = 0; // Track the number of requested checks
        private bool isCheckingConveyorBelts = false; // Flag to indicate if coroutine is running

        void InitializeConveyorBelts()
        {
            // Sort conveyor belts from left to right by X position
            sortedConveyorBelts = GetComponentInParent<ShowcaseParent>()
                .GetComponentsInChildren<ConveyorBelt>()
                .OrderBy(conveyor => conveyor.transform.position.x)
                .ToList();

            Debug.Log("Conveyor belts sorted from left to right based on X position.");
        }

        public void CheckForSameColoredCansInFirstRowsBoxes()
        {
            // Enqueue a new check by incrementing the pendingChecks counter
            pendingChecks++;

            // Start the coroutine if it's not already running
            if (!isCheckingConveyorBelts)
            {
                StartCoroutine(CheckConveyorBeltsForMatchingCanRoutine());
            }
        }

        IEnumerator CheckConveyorBeltsForMatchingCanRoutine()
        {
            isCheckingConveyorBelts = true;

            while (pendingChecks > 0) // Process all queued checks
            {
                pendingChecks--; // Decrement the counter at the start of each pass

                // Go through each box in topLineSlots from left to right
                for (int j = 0; j < topLineSlots.Length; j++)
                {
                    if (topLineSlots[j].childCount > 0)
                    {
                        GameObject box = topLineSlots[j].GetChild(0).gameObject;
                        Color boxColor = box.GetComponent<ColorManager>().GetColor();
                        Transform emptyCanPos = box.GetComponentInChildren<BoxSpacesForCans>().GetNextEmptyCanPos();

                        if (emptyCanPos == null)
                        {
                            Debug.Log("No more empty positions in the box.");
                            continue;
                        }

                        // Go through each conveyor belt in sortedConveyorBelts, checking only the first can
                        foreach (ConveyorBelt conveyorBelt in sortedConveyorBelts)
                        {
                            // Try to find a matching can and jump it into the box
                            bool foundMatchingCan = conveyorBelt.TryToJumpSameColoredFirstCanIntoBox(boxColor, emptyCanPos);

                            if (foundMatchingCan)
                            {
                                // If a matching can is found, wait 0.2 seconds before moving to the next conveyor belt
                                box.GetComponentInChildren<BoxSpacesForCans>().SetCanPosOccupied(emptyCanPos);

                                emptyCanPos = box.GetComponentInChildren<BoxSpacesForCans>().GetNextEmptyCanPos();

                                if (emptyCanPos == null) break;

                                yield return new WaitForSeconds(0.05f);
                            }

                            // Move to the next conveyor belt immediately, even if more matching cans are present
                            yield return null;
                        }
                    }
                }

                if (topLineHolder.childCount == topLineHolder.GetComponentsInChildren<BoxSpacesForCans>().Length)
                {
                    StopCheckingForLevelFailed();
                    Invoke(nameof(CheckIfLevelIsFailed), 1f);
                }
            }

            if (topLineHolder.childCount == topLineHolder.GetComponentsInChildren<BoxSpacesForCans>().Length)
            {
                StopCheckingForLevelFailed();
                Invoke(nameof(CheckIfLevelIsFailed), 1f);
            }

            isCheckingConveyorBelts = false; // Mark the coroutine as complete when all checks are processed
        }

        #endregion

        public void StopCheckingForLevelFailed()
        {
            if (IsInvoking(nameof(CheckIfLevelIsFailed)))
                CancelInvoke(nameof(CheckIfLevelIsFailed));
        }

        private void CheckIfLevelIsFailed()
        {
            return;

            foreach (ConveyorBelt convBelt in FindObjectsOfType<ConveyorBelt>())
                if (convBelt.isMovingCans) return;
            foreach (CanBottle bottle in FindObjectsOfType<CanBottle>())
                if (bottle.isJumpingToBox) return;

            if (topLineHolder.childCount == topLineHolder.GetComponentsInChildren<BoxSpacesForCans>().Length)
                GetComponentInParent<LevelManager>().ActivateLevelFailedPanel();
        }

        Color GetColorFromEnum(StackColors.StackColor stackColor)
        {
            if (stackColors != null && (int)stackColor < stackColors.colors.Length)
            {
                return stackColors.colors[(int)stackColor];
            }
            return Color.white; // Default fallback color
        }

        public bool IsAnyBoxJumpingToFirstRow()
        {
            return countOfCurrentlyJumpingBoxesToFirstLine > 0;
        }
    }

    [System.Serializable]
    public class LineBox
    {
        public bool hasBox;
        public StackColors.StackColor boxColor;
    }
}