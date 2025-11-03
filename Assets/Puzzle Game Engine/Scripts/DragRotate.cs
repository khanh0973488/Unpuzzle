using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

namespace HyperPuzzleEngine
{
    public class DragRotate : MonoBehaviour
{
    public string includeObjectsWithNameIntoPivotCalculation = "BigCube";
    public float pivotAdjustingDuration = 0.65f;

    private Vector3 lastMousePosition;
    private float rotationSpeed = 9.0f;
    private bool isDragging = false;
    private Vector3 startPosition;

    // Latest calculated central pivot point
    private Vector3 latestCentralPivot;

    void Start()
    {
        startPosition = transform.position;
        Invoke(nameof(RepositionAtStart), 0.2f);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            rotationSpeed = 28f;
        else if (Application.platform == RuntimePlatform.Android)
            rotationSpeed = 5.5f;
    }

    void Update()
    {
        HandleMouseInput();
    }

    private void RepositionAtStart()
    {
        if(gameObject.activeInHierarchy)
        RepositionCentralPivot(null);
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            RotateObject(delta);
            lastMousePosition = Input.mousePosition;
        }
    }

    void RotateObject(Vector3 delta)
    {
        float rotationX = delta.y * rotationSpeed * Time.deltaTime;
        float rotationY = -delta.x * rotationSpeed * Time.deltaTime;

        transform.Rotate(Vector3.right, rotationX, Space.World);
        transform.Rotate(Vector3.up, rotationY, Space.World);
    }

    public void RepositionCentralPivot(Transform doNotInclude = null)
    {
        if (doNotInclude != null && doNotInclude.parent == transform) doNotInclude.parent = transform.parent;

        // Step 1: Calculate the centroid of relevant children
        Vector3 centroid = Vector3.zero;
        int childCount = 0;

        foreach (Transform child in transform)
        {
            // Only include children with names matching the specified criteria
            if (!child.name.Contains(includeObjectsWithNameIntoPivotCalculation)) continue;
            if (child.GetComponent<Block>() != null && child.GetComponent<Block>().IsMoving()) continue;
            if (doNotInclude != null && doNotInclude == child) continue;

            centroid += child.position;
            childCount++;
        }

        // Check if there are any valid children to avoid division by zero
        if (childCount == 0)
        {
            Debug.LogWarning("No valid children found for pivot calculation.");
            return;
        }

        // Calculate the final central pivot by averaging positions
        latestCentralPivot = centroid / childCount;
        //Debug.Log($"Calculated Central Pivot (Centroid): {latestCentralPivot}" + "___COUNT OF CHILD:" + coun);

        // Step 2: Move the parent object to the new central pivot
        Vector3 previousPosition = transform.position; // Store current position for smooth transition
        transform.position = latestCentralPivot;

        // Step 3: Offset children to maintain their world positions relative to the new pivot
        foreach (Transform child in transform)
        {
            // Adjust each child's position relative to the parent's new position
            child.position += previousPosition - latestCentralPivot;
        }

        // Step 4: Smoothly move the parent object back to the original start position
        StartCoroutine(SmoothMoveToStartPosition(previousPosition));
    }

    // Coroutine to smoothly transition back to the start position without disturbing children
    private IEnumerator SmoothMoveToStartPosition(Vector3 previousPosition)
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = transform.position;

        while (elapsedTime < pivotAdjustingDuration)
        {
            transform.position = Vector3.Lerp(initialPosition, startPosition, elapsedTime / pivotAdjustingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = startPosition; // Ensure final position is the start position
    }

    #region Zooming In and Out

    [Space]
    [Header("Zooming In and Out")]
    public int minZoomLevel = -3;
    public int maxZoomLevel = 3;
    public float zoomStep = 1.0f;

    private float zoomLevel = 0f;

    public void ZoomIn()
    {
        if (zoomLevel < maxZoomLevel)
        {
            zoomLevel += zoomStep;
            UpdateScale();
        }
    }

    public void ZoomOut()
    {
        if (zoomLevel > minZoomLevel)
        {
            zoomLevel -= zoomStep;
            UpdateScale();
        }
    }

    private void UpdateScale()
    {
        Vector3 targetScale = Vector3.one * (1 + (zoomLevel * 0.1f));
        StartCoroutine(SmoothScaleToTarget(targetScale));
    }

    private IEnumerator SmoothScaleToTarget(Vector3 targetScale)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;
        Vector3 initialScale = transform.localScale;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    #endregion
}
}