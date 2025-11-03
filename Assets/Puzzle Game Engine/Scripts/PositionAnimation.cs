using System.Collections;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
public class PositionAnimation : MonoBehaviour
{
    public Vector3 direction = Vector3.forward;  // Direction to move along, can be set in the inspector
    public float duration = 0.4f;  // Total duration of the move
    public float magnitude = 3f;  // Maximum position offset

    public bool isSnappingToFirstMultiplier = false;
    [Tooltip("Define multipliers for the move animation curve. Each element represents a fraction of the maximum position offset applied at evenly spaced intervals during the move duration. The last multiplier should typically be 0 to return to the starting position.")]
    public float[] multipliers = new float[] { 1f, -0.8f, 0.5f, 0f };

    public enum RotationAnimationType
    {
        Move
    }

    public RotationAnimationType animationType;

    public void PlaySelectedAnimation()
    {
        switch (animationType)
        {
            case RotationAnimationType.Move:
                Move();
                break;
            default:
                break;
        }
    }

    public void Move()
    {
        StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        Vector3 startPos = transform.position;
        float elapsed = 0.0f;

        Vector3[] targetPositions = new Vector3[multipliers.Length];

        // Pre-calculate target positions based on the multipliers
        for (int i = 0; i < multipliers.Length; i++)
        {
            float offset = magnitude * multipliers[i];
            targetPositions[i] = startPos + direction.normalized * offset;
        }

        Vector3 currentPos = startPos;  // Start from the original position
        int nextTarget = 0;

        if (isSnappingToFirstMultiplier)
        {
            currentPos = transform.position = targetPositions[0];
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float relativeTime = elapsed / duration;
            float segmentDuration = duration / multipliers.Length;
            float segmentElapsed = elapsed % segmentDuration;
            float lerpFactor = segmentElapsed / segmentDuration;

            // Update the next target index based on the relative time
            int prevTarget = nextTarget;
            nextTarget = Mathf.FloorToInt(relativeTime * multipliers.Length);

            // Ensure we do not go out of bounds
            if (nextTarget >= targetPositions.Length)
                break;

            // When the target changes, start interpolating from the last current position
            if (prevTarget != nextTarget)
            {
                currentPos = transform.position;
            }

            // Smoothly interpolate from the current position to the next target position
            transform.position = Vector3.Lerp(currentPos, targetPositions[nextTarget], lerpFactor);

            yield return null;
        }

        // Optionally, reset position to the start to prevent drift
        transform.position = startPos;
    }
}
}
