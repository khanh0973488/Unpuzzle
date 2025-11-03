using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
public class ConveyorBelt : MonoBehaviour
{
    public MeshRenderer conveyorBeltMeshRenderer;

    [Space]
    [Header("Can Management")]
    public GameObject canBottlePrefab;
    [Space(10)]
    public bool fillUpLineWithCans = false;
    public bool updateCanColors = false;
    [Space(10)]
    [Range(0, 50)]
    public int totalCans = 20;

    [Space(15)]
    [Header("Color Settings")]
    public StackColors stackColors;
    public List<StackColors.StackColor> selectedCanColors = new List<StackColors.StackColor>();

    private Transform[] canPositions;
    private int currentCanCount;
    private int cansAtStart;

    private List<StackColors.StackColor> pendingCanColors = new List<StackColors.StackColor>();

    void Start()
    {
        InitializeCanPositions();
        cansAtStart = GetComponentsInChildren<CanBottle>().Length;

        // Initialize pendingCanColors based on selectedCanColors
        InitializePendingCanColors();
    }

    void InitializePendingCanColors()
    {
        pendingCanColors.Clear();

        // Calculate the number of visible cans
        int visibleCans = GetComponentsInChildren<CanBottle>().Length;

        // Populate the pending list with the remaining colors
        for (int i = visibleCans; i < totalCans; i++)
        {
            if (i < selectedCanColors.Count)
            {
                pendingCanColors.Add(selectedCanColors[i]);
            }
            else
            {
                // Default to blue if there aren't enough colors defined
                pendingCanColors.Add(StackColors.StackColor.Blue);
            }
        }
    }

    void Update()
    {
        if (fillUpLineWithCans)
        {
            RemoveAllCans();
            EnsureColorListMatchesTotalCans();
            FillUpLineWithCans();
            fillUpLineWithCans = false;
        }

        if (updateCanColors)
        {
            UpdateCanColors();
            updateCanColors = false;
        }
    }

    void InitializeCanPositions()
    {
        canPositions = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            canPositions[i] = transform.GetChild(i);
        }
    }

    void RemoveAllCans()
    {
        InitializeCanPositions();

        foreach (Transform position in canPositions)
        {
            if (position.childCount > 0)
            {
                DestroyImmediate(position.GetChild(0).gameObject);
            }
        }
        currentCanCount = 0;
    }

    void FillUpLineWithCans()
    {
        InitializeCanPositions();
        currentCanCount = 0;

        for (int i = 0; i < canPositions.Length && currentCanCount < totalCans; i++)
        {
            if (canPositions[i].childCount == 0)
            {
                GameObject newCan = Instantiate(canBottlePrefab, canPositions[i].position, Quaternion.identity, canPositions[i]);
                SetCanColor(newCan, i);
                currentCanCount++;
            }
        }
    }

    public void UpdateCanColors()
    {
        for (int i = 0; i < canPositions.Length; i++)
        {
            if (canPositions[i].childCount > 0)
            {
                GameObject can = canPositions[i].GetChild(0).gameObject;
                SetCanColor(can, i);
            }
        }
    }

    void EnsureColorListMatchesTotalCans()
    {
        while (selectedCanColors.Count < totalCans)
        {
            selectedCanColors.Add(StackColors.StackColor.Blue); // Add a default color if needed
        }
        while (selectedCanColors.Count > totalCans)
        {
            selectedCanColors.RemoveAt(selectedCanColors.Count - 1);
        }
    }

    Color GetColorFromEnum(StackColors.StackColor stackColor)
    {
        if (stackColors != null && (int)stackColor < stackColors.colors.Length)
        {
            return stackColors.colors[(int)stackColor];
        }
        return Color.white;
    }

    void SetCanColor(GameObject can, int index, bool isChangingInEditor = true)
    {
        ColorManager colorManager = can.GetComponentInChildren<ColorManager>();
        if (colorManager != null && index < selectedCanColors.Count)
        {
            if (isChangingInEditor)
                colorManager.ChangeColorInEditor(GetColorFromEnum(selectedCanColors[index]));
            else
                colorManager.ChangeColor(GetColorFromEnum(selectedCanColors[index]));
            //Debug.Log($"Set color of can at index {index} to {selectedCanColors[index]}");
        }
    }

    void SetCanColor(GameObject can, StackColors.StackColor color, bool isChangingInEditor = true)
    {
        ColorManager colorManager = can.GetComponentInChildren<ColorManager>();
        if (colorManager != null)
        {
            if (isChangingInEditor)
                colorManager.ChangeColorInEditor(GetColorFromEnum(color));
            else
                colorManager.ChangeColor(GetColorFromEnum(color));
        }
    }


    private void LateUpdate()
    {
        if (Input.GetKeyUp(KeyCode.J))
        {
            StartConveyorBeltMovement();
        }
    }

    public void StartConveyorBeltMovement()
    {
        StartCoroutine(MoveCansBackward());
    }

    [HideInInspector] public bool isMovingCans = false;

    IEnumerator MoveCansBackward()
    {
        InitializeCanPositions();
        float distanceBetweenPositions = Vector3.Distance(canPositions[0].position, canPositions[1].position);

        isMovingCans = true;

        // Unparent the can object in the first position
        if (canPositions[0].childCount > 0)
        {
            Transform firstCan = canPositions[0].GetChild(0);
            firstCan.SetParent(null);
            Debug.Log("Unparented first can.");
        }
        else
        {
            Debug.LogWarning("No can to unparent in the first position.");
        }

        // Shift each can one position backward
        for (int i = 1; i < canPositions.Length; i++)
        {
            // Move the can at position i to position i - 1 if possible
            if (canPositions[i].childCount > 0 && canPositions[i - 1].childCount == 0)
            {
                Transform canToMove = canPositions[i].GetChild(0);
                canToMove.SetParent(canPositions[i - 1]);
                StartCoroutine(MoveCanToPosition(canToMove, canPositions[i - 1].position));
                Debug.Log($"Moved can from position {i} to position {i - 1}");
            }
            //yield return null; // Ensure each move completes before proceeding to the next
        }

        // Spawn a new can if needed and move it to the last position
        //if ((currentCanCount + cansAtStart) < totalCans)
        //{
        //    // Adjusted spawn position to be behind the last position
        //    Vector3 spawnPosition = canPositions[canPositions.Length - 1].position + new Vector3(0, 0, distanceBetweenPositions);
        //    GameObject newCan = Instantiate(canBottlePrefab, spawnPosition, Quaternion.identity);
        //    SetCanColor(newCan, currentCanCount + cansAtStart, false);
        //    newCan.transform.SetParent(canPositions[canPositions.Length - 1]);
        //    yield return StartCoroutine(MoveCanToPosition(newCan.transform, canPositions[canPositions.Length - 1].position));
        //    currentCanCount++;
        //    Debug.Log("Spawned and moved new can to last position.");
        //}

        if (pendingCanColors.Count > 0)
        {
            Vector3 spawnPosition = canPositions[canPositions.Length - 1].position + new Vector3(0, 0, distanceBetweenPositions);
            GameObject newCan = Instantiate(canBottlePrefab, spawnPosition, Quaternion.identity);

            // Get the color from pendingCanColors and remove it
            StackColors.StackColor colorToAssign = pendingCanColors[0];
            pendingCanColors.RemoveAt(0);

            SetCanColor(newCan, colorToAssign, false);
            newCan.transform.SetParent(canPositions[canPositions.Length - 1]);
            yield return StartCoroutine(MoveCanToPosition(newCan.transform, canPositions[canPositions.Length - 1].position));
            currentCanCount++;
            Debug.Log($"Spawned and moved new can to last position with color {colorToAssign}.");
        }


        GetComponentInParent<ShowcaseParent>().GetComponentInChildren<BoxJumpController>().CheckForSameColoredCansInFirstRowsBoxes();

        isMovingCans = false;
    }

    IEnumerator MoveCanToPosition(Transform can, Vector3 targetPosition)
    {
        Vector3 startPosition = can.position;
        float elapsedTime = 0f;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            can.position = Vector3.Lerp(startPosition, targetPosition, t);

            conveyorBeltMeshRenderer.material.mainTextureOffset += Vector2.left * 1.2f * Time.deltaTime;

            yield return null;
        }

        can.position = targetPosition;
    }

    public bool TryToJumpSameColoredFirstCanIntoBox(Color boxColor, Transform emptyPositionForCan)
    {
        if (emptyPositionForCan.childCount > 0) return false;

        // Check if there is a can in the first position
        if (canPositions[0].childCount > 0)
        {
            GameObject firstCan = canPositions[0].GetChild(0).gameObject;
            ColorManager colorManager = firstCan.GetComponentInChildren<ColorManager>();

            if (colorManager != null)
            {
                // Compare the color of the first can with the provided boxColor
                Color firstCanColor = colorManager.GetColor();
                if (firstCanColor == boxColor)
                {
                    // If the colors match, jump the can to the empty position
                    CanBottle canBottle = firstCan.GetComponent<CanBottle>();
                    if (canBottle != null)
                    {
                        canBottle.JumpToTarget(emptyPositionForCan);

                        // Start the conveyor belt movement after jumping the can
                        StartConveyorBeltMovement();

                        Debug.Log("First can jumped into the box. Conveyor belt started.");
                        return true; // Indicate success
                    }
                }
                else
                {
                    Debug.Log("First can color does not match the box color.");
                }
            }
        }

        Debug.LogWarning("No can in the first position or color manager not found.");
        return false; // No matching color or first can is missing
    }
}
}