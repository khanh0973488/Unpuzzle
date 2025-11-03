using UnityEngine;
using HyperPuzzleEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HyperPuzzleEngine
{
    public class RuntimePrefabCreator : MonoBehaviour
{
    public GameObject objectToCreatePrefabFrom;
    public string path;
    public bool addCheckForMoreTargetPositions = false;

    private string prefabName;
    private string defaultPrefabName = "#1_NewLevelPrefab";

#if UNITY_EDITOR
    // Call this method from your game logic wherever appropriate
    public void CreateAndSavePrefab(TMP_InputField inputFieldOfPrefabName)
    {
        if (!Application.isEditor)
        {
            Debug.LogError("Prefab creation is only allowed in the Unity Editor.");
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/" + path))
        {
            AssetDatabase.CreateFolder("Assets", path);
        }

        if (objectToCreatePrefabFrom.GetComponentInChildren<GridGenerator>() != null)
            objectToCreatePrefabFrom.GetComponentInChildren<GridGenerator>().spawnGridAtStart = false;

        if (addCheckForMoreTargetPositions && objectToCreatePrefabFrom.GetComponent<CheckForMoreTargetPositions>() == null)
            objectToCreatePrefabFrom.AddComponent<CheckForMoreTargetPositions>();

        foreach (SetTransformOnEnable setTransform in objectToCreatePrefabFrom.GetComponentsInChildren<SetTransformOnEnable>(true))
            Destroy(setTransform);

        prefabName = inputFieldOfPrefabName.text;
        if (string.IsNullOrEmpty(prefabName))
            prefabName = defaultPrefabName;

        Invoke(nameof(CreatePrefab), 0.1f);
    }

    private void CreatePrefab()
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(objectToCreatePrefabFrom, $"Assets/{path}/{prefabName}.prefab");
        if (objectToCreatePrefabFrom.GetComponentInChildren<GridGenerator>() != null)
            objectToCreatePrefabFrom.GetComponentInChildren<GridGenerator>().spawnGridAtStart = true;

        if (prefab != null)
        {
            Debug.Log($"Prefab saved: {prefab.name}");
        }
        else
        {
            Debug.LogError("Failed to create prefab.");
        }
    }
#endif
}
}