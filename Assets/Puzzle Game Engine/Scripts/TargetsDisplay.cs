using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEditor;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
public class TargetsDisplay : MonoBehaviour
{
    [System.Serializable]
    public struct TargetData
    {
        public Color color;
        public int targetCount; // The target count that we want to set for each target
        [HideInInspector] public int currentCount; // Counter for the current progress of this target
    }

    [System.Serializable]
    public struct ColorIconMapping
    {
        public Color color;
        public Sprite icon;
    }

    [Header("Controls")]
    public bool updateTargets = false; // Acts as a button to update visuals

    [Header("Target Settings")]
    [Range(1, 5)]
    public int targetCount = 1;

    public List<TargetData> targetsList = new List<TargetData>();

    [Header("Color-Icon Mapping")]
    public List<ColorIconMapping> colorIconMappings = new List<ColorIconMapping>(); // Mapping for color to icon

    [Header("UI Settings")]
    public GameObject targetUIPrefab; // Prefab for each target's UI display
    public Transform targetsHolder;   // Parent transform for spawned targets
    public Transform background1;     // Background 1 for visual scaling
    public Transform background2;     // Background 2 for visual scaling
    public Transform texts;           // Parent transform for text elements

    [Tooltip("Extra scale offset for backgrounds based on target count")]
    public float scaleOffset = 1.0f;

    private List<GameObject> targetUIInstances = new List<GameObject>();

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (updateTargets)
        {
            updateTargets = false;
            UpdateTargetVisuals();
        }

        EditorApplication.delayCall += () =>
        {
            if (this != null) // Check if the script or game object has been destroyed
            {
                UpdateTargets();
            }
        };
#endif
    }

    private void UpdateTargets()
    {
        if (targetUIPrefab == null)
        {
            Debug.LogWarning("Target UI Prefab is not assigned.");
            return;
        }

        if (targetsHolder == null)
        {
            Debug.LogWarning("Targets Holder is not assigned.");
            return;
        }

        // Ensure targetsList size matches targetCount
        AdjustTargetListSize();

        // Adjust UI instances based on target count
        AdjustUIInstances();

        // Update existing UI instances with the latest data
        for (int i = 0; i < targetCount; i++)
        {
            UpdateUIInstance(i);
        }

        // Update the scaling of the backgrounds based on the target count
        UpdateBackgroundScaling();
    }

    private void AdjustTargetListSize()
    {
        // Adjust the size of targetsList based on targetCount
        while (targetsList.Count < targetCount)
            targetsList.Add(new TargetData { currentCount = 0, targetCount = 0 });

        while (targetsList.Count > targetCount)
            targetsList.RemoveAt(targetsList.Count - 1);
    }

    private void AdjustUIInstances()
    {
        // Ensure that `targetUIInstances` list matches `targetCount` in size.

        // Add new UI instances if there are fewer than targetCount
        while (targetUIInstances.Count < targetCount)
        {
            var uiInstance = Instantiate(targetUIPrefab, targetsHolder); // Spawn as child of targetsHolder
            targetUIInstances.Add(uiInstance);
        }

        // Remove excess UI instances if there are more than targetCount
        while (targetUIInstances.Count > targetCount)
        {
            // Remove the last instance and destroy it
            var instanceToRemove = targetUIInstances[targetUIInstances.Count - 1];

            if (instanceToRemove != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(instanceToRemove);
                }
                else
                {
                    Destroy(instanceToRemove);
                }
#else
            Destroy(instanceToRemove);
#endif
            }

            targetUIInstances.RemoveAt(targetUIInstances.Count - 1);
        }

        // Ensure the list only contains valid references by removing any null entries
        targetUIInstances.RemoveAll(item => item == null);
    }


    private void UpdateUIInstance(int index)
    {
        if (index >= targetUIInstances.Count || targetUIInstances[index] == null)
        {
            Debug.LogWarning($"#8Coins2_Target UI instance at index {index} is missing or destroyed.");
            return;
        }

        var targetData = targetsList[index];
        var uiInstance = targetUIInstances[index];

        // Assign color and icon to the UI elements in the prefab
        var imageComponent = uiInstance.GetComponentInChildren<SpriteRenderer>();
        if (imageComponent != null)
        {
            //imageComponent.color = targetData.color;
            imageComponent.sprite = GetIconForColor(targetData.color); // Automatically assign the icon based on color
        }
        else
        {
            Debug.LogWarning($"#8Coins2_Image component is missing in the target UI prefab at index {index}.");
        }

        // Set up target count text (the target goal for this item)
        var textComponent = uiInstance.GetComponentInChildren<TextMeshPro>();
        if (textComponent != null)
        {
            textComponent.text = targetData.targetCount.ToString(); // Display the target count for this target
        }
        else
        {
            Debug.LogWarning($"#8Coins2_Text component is missing in the target UI prefab at index {index}.");
        }
    }

    private Sprite GetIconForColor(Color color)
    {
        // Look up the icon based on the color in colorIconMappings
        foreach (var mapping in colorIconMappings)
        {
            if (mapping.color == color)
            {
                return mapping.icon;
            }
        }
        Debug.LogWarning($"#8Coins2_No icon found for color {color}. Assign icons for all colors in colorIconMappings.");
        return null; // Return null if no matching icon is found
    }

    private void UpdateBackgroundScaling()
    {
        // Scale the backgrounds based on the number of targets and scaleOffset
        float newScaleX = targetCount + scaleOffset;

        if (background1 != null)
        {
            background1.localScale = new Vector3(newScaleX, background1.localScale.y, background1.localScale.z);
        }

        if (background2 != null)
        {
            background2.localScale = new Vector3(newScaleX, background2.localScale.y, background2.localScale.z);
        }
    }

    // Method to update visuals of targets based on color-icon mapping and target counts
    private void UpdateTargetVisuals()
    {
        for (int i = 0; i < targetUIInstances.Count; i++)
        {
            var targetData = targetsList[i];
            var uiInstance = targetUIInstances[i];

            // Update the icon based on color
            SpriteRenderer imageComponent = uiInstance.GetComponentInChildren<SpriteRenderer>();
            if (imageComponent != null)
            {
                //imageComponent.color = targetData.color;
                imageComponent.sprite = GetIconForColor(targetData.color); // Assign icon based on color
            }

            // Update the target count text
            var textComponent = uiInstance.GetComponentInChildren<TextMeshPro>();
            if (textComponent != null)
            {
                textComponent.text = targetData.targetCount.ToString(); // Set target count
            }
        }
    }

    // Public method to decrease current count of a target based on color
    public void DecreaseTargetCounter(Color targetColor)
    {
        for (int i = 0; i < targetsList.Count; i++)
        {
            if (targetsList[i].color == targetColor)
            {
                // Retrieve the struct, modify the current count, and put it back in the list
                TargetData targetData = targetsList[i];
                targetData.currentCount = Mathf.Max(0, targetData.currentCount - 1);
                targetsList[i] = targetData; // Set the modified struct back in the list

                // Update corresponding UI text to reflect current progress
                if (i < targetUIInstances.Count)
                {
                    var textComponent = targetUIInstances[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        textComponent.text = $"{targetData.currentCount}/{targetData.targetCount}";
                    }
                }

                break;
            }
        }
    }
}
}