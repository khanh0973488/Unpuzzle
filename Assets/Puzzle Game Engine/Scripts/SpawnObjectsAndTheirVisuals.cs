#if UNITY_EDITOR
using HyperPuzzleEngine;
using UnityEditor;
#endif
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class SpawnObjectsAndTheirVisuals : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject objectPrefab;
    public GameObject visualPrefab;

    [Space]
    [Header("Parents")]
    public Transform objectParent;
    public Transform visualParent;

    [Space]
    [Header("Settings")]
    [Range(0, 7)]
    public int numberOfObjects;
    public Vector3 visualFollowOffset;

    public enum TypeOfSpawner
    {
        ConveyorBelt,
        FirstLineArea
    }

    public TypeOfSpawner spawnerType;

    private void OnValidate()
    {
        if (visualPrefab == null || objectParent == null || visualParent == null)
        {
            Debug.LogWarning("Visual prefab or parent transforms are not assigned.");
            return;
        }

        // Defer adjustment operations
#if UNITY_EDITOR
        EditorApplication.delayCall += AdjustChildCount;
#endif
    }

    private void AdjustChildCount()
    {
        // Ensure the object is still valid (could be destroyed between calls)
        if (this == null) return;

        // Calculate current children counts
        int objectCount = objectParent.childCount;
        int visualCount = visualParent.childCount;

        // Remove excess objects
        if (objectCount > numberOfObjects)
            DeferRemoveExcessChildren(objectParent, objectCount - numberOfObjects);

        if (visualCount > numberOfObjects)
            DeferRemoveExcessChildren(visualParent, visualCount - numberOfObjects);

        // Add missing objects
        if (objectCount < numberOfObjects)
            AddMissingChildren(numberOfObjects - objectCount);

        switch (spawnerType)
        {
            case TypeOfSpawner.ConveyorBelt:
                for (int i = 0; i < objectParent.childCount; i++)
                {
                    ConveyorBelt belt = objectParent.GetChild(i).GetComponent<ConveyorBelt>();
                    if (belt != null)
                        belt.conveyorBeltMeshRenderer = visualParent.GetChild(i).GetChild(0).Find("ConveyorBelt").GetComponent<MeshRenderer>();
                }
                break;

            case TypeOfSpawner.FirstLineArea:

                break;
        }
    }

    private void DeferRemoveExcessChildren(Transform parent, int excessCount)
    {
        for (int i = 0; i < excessCount; i++)
        {
            Transform childToRemove = parent.GetChild(parent.childCount - 1);
#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                if (childToRemove != null)
                    DestroyImmediate(childToRemove.gameObject);
            };
#else
            Destroy(childToRemove.gameObject);
#endif
        }
    }

    private void AddMissingChildren(int missingCount)
    {
        for (int i = 0; i < missingCount; i++)
        {
            GameObject newObject;

            if (objectPrefab == null)
            {
                // Create a primitive cube and remove its components
                newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                RemoveAllComponents(newObject);
                newObject.transform.SetParent(objectParent);
            }
            else
            {
                // Spawn the objectPrefab
                newObject = Instantiate(objectPrefab, objectParent);
            }

            // Spawn visual
            GameObject newVisual = Instantiate(visualPrefab, visualParent);

            // Assign follow offset
            if (newVisual == null) return;

            if (newVisual.TryGetComponent<HyperPuzzleEngine.FollowObject>(out HyperPuzzleEngine.FollowObject followObject))
            {
                followObject.objectToFollow = newObject.transform;
                followObject.followOffset = visualFollowOffset;
            }
        }
    }

    private void RemoveAllComponents(GameObject obj)
    {
        // Remove all components from the object
        foreach (var component in obj.GetComponents<Component>())
        {
            if (!(component is Transform))
            {
#if UNITY_EDITOR
                DestroyImmediate(component);
#else
                Destroy(component);
#endif
            }
        }
    }
}
}