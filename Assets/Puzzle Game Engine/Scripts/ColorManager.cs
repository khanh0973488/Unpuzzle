using TMPro;
using UnityEngine;
using HyperPuzzleEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HyperPuzzleEngine
{
    public class ColorManager : MonoBehaviour
    {
        public bool canUseThisMaterialIfMaterialIsNull = false;
        //public bool cannotRewriteSavedMaterialWhilePlayMode = true;
        public bool destroyComponentAfterChooseMaterialAtAwake = false;
        public bool visibleOnlyAfterSelectedColor = false;
        public bool canSetUpNumberTextIfFound = true;
        public bool chooseColorRandomlyAtStartFromStackColors = false;
        public Material thisMaterial;
        [Header("Only Add If There Are Multiple Materials")]
        public Material[] thisMaterialMultiple;
        public StackColors stackColors;

        private MeshRenderer thisRenderer;
        private Color defaultColor = Color.white;

        [Space]
        [Header("Saving Color")]
        public bool loadColorAtStart = false;
        [SerializeField] int savedColorIndex = 0;
        [SerializeField] int materialIndexToSwitch = 0;

        private void Awake()
        {
            if (thisMaterialMultiple.Length == 0)
            {
                thisMaterialMultiple = new Material[1];
                thisMaterialMultiple[0] = thisMaterial;
            }

            thisRenderer = GetComponent<MeshRenderer>();

            if (DoesMaterialEmptyOrDefault())
            {
                //Debug.Log("TEST_ADDING MATERIAL TO: " + gameObject.name);
                if (GetComponent<SetMaterialAtStart>() != null)
                    thisMaterial = GetComponent<SetMaterialAtStart>().GetCurrentMaterial();

                thisRenderer.SetMaterials(thisMaterialMultiple.ToList());
            }
            //else
            //    thisMaterial = thisRenderer.material;

            if (visibleOnlyAfterSelectedColor)
                SetVisibility(false);

            if (destroyComponentAfterChooseMaterialAtAwake && GetComponentInParent<ShowcaseParent>().IsInGameMode())
                Destroy(this);

            Debug.Log("- Object name: " + gameObject.name);

            if (loadColorAtStart)
                ChangeColor(stackColors.colors[savedColorIndex]);

            Debug.Log("- Object nameV2: " + gameObject.name);
        }

        private bool DoesMaterialEmptyOrDefault()
        {
            if (thisRenderer == null) return false;

            if (thisRenderer.materials.Length < 2)
                return thisRenderer.materials[materialIndexToSwitch] == null ||
                       thisRenderer.materials[materialIndexToSwitch].name.ToLower().Contains("default") ||
                       thisRenderer.materials[materialIndexToSwitch].color == Color.white ||
                        thisMaterial != null;
            else
            {
                bool hasEmptyMaterial = false;

                for (int i = 0; i < thisRenderer.materials.Length; i++)
                {
                    hasEmptyMaterial = thisRenderer.materials[i] == null ||
                       thisRenderer.materials[i].name.ToLower().Contains("default") ||
                       thisRenderer.materials[i].color == Color.white ||
                        thisMaterial != null;

                    if (hasEmptyMaterial) return true;
                }
            }

            return false;
        }

        private void Start()
        {
            if (thisRenderer == null)
                thisRenderer = GetComponent<MeshRenderer>();

            if (thisRenderer != null)
                defaultColor = thisRenderer.materials[materialIndexToSwitch].color;
            else
                defaultColor = Color.white;

            if (chooseColorRandomlyAtStartFromStackColors)
                ChangeColor(stackColors.colors[Random.Range(0, stackColors.colors.Length)]);

            Debug.Log("- Object nameV3: " + gameObject.name);

            if (loadColorAtStart)
                ChangeColor(stackColors.colors[savedColorIndex]);

            Debug.Log("- Object nameV4: " + gameObject.name);
        }

        private void SetVisibility(bool isVisible)
        {
            thisRenderer.enabled = isVisible;
            foreach (TextMeshPro text in GetComponentsInChildren<TextMeshPro>())
                text.enabled = isVisible;
            foreach (MeshRenderer rend in GetComponentsInChildren<MeshRenderer>())
                rend.enabled = isVisible;
            foreach (MeshRenderer rend in GetComponentsInParent<MeshRenderer>())
            {
                if (rend.GetComponent<CheckNeighbours>() == null)
                    rend.enabled = isVisible;
            }
        }

        public void ChangeColor(Color newColor)
        {
            if (!Application.isPlaying)
                return;

            if (thisRenderer.materials[materialIndexToSwitch].color == newColor) return;

            float currentTransparency = GetColor().a;

            if (thisRenderer == null) thisRenderer = GetComponent<MeshRenderer>();

            newColor.a = currentTransparency;

            thisRenderer.materials[materialIndexToSwitch].color = newColor;

            if (GetComponent<MatchSymbolWithColor>() != null)
                GetComponent<MatchSymbolWithColor>().SetUpSymbolBasedOnColor(newColor);

            if (visibleOnlyAfterSelectedColor)
                SetVisibility(true);

            Debug.Log("Changed Color To: " + newColor + " - Object name: " + gameObject.name);
        }

        public void ChangeColor(StackColors.StackColor newColorInput)
        {
            if (!Application.isPlaying)
                return;

            float currentTransparency = GetColor().a;

            if (thisRenderer == null) thisRenderer = GetComponent<MeshRenderer>();

            Color newColor = stackColors.colors[(int)newColorInput];

            newColor.a = currentTransparency;

            thisRenderer.materials[materialIndexToSwitch].color = newColor;

            if (GetComponent<MatchSymbolWithColor>() != null)
                GetComponent<MatchSymbolWithColor>().SetUpSymbolBasedOnColor(newColor);

            if (visibleOnlyAfterSelectedColor)
                SetVisibility(true);

            Debug.Log("Changed Color To: " + newColor + " - Object name: " + gameObject.name);
        }


        public void ChangeColorInEditor(Color newColor)
        {
            // Ensure this only runs in the editor
            if (Application.isPlaying)
                return;

            // Ensure the MeshRenderer is initialized
            if (thisRenderer == null) thisRenderer = GetComponent<MeshRenderer>();
            if (thisRenderer == null || thisRenderer.sharedMaterials.Length <= materialIndexToSwitch) return;

            // Get current transparency level and apply it to the new color
            float currentTransparency = GetColor().a;
            newColor.a = currentTransparency;

#if UNITY_EDITOR
            // Check if this object already has a unique material instance at the specified index
            Material currentMaterial = thisRenderer.sharedMaterials[materialIndexToSwitch];
            if (currentMaterial == thisMaterial)
            {
                // If using the shared material, create a unique instance for this object
                Material uniqueMaterial = Instantiate(thisMaterial);
                uniqueMaterial.color = newColor;

                // Assign the unique material to this object's renderer at the specified index
                Material[] materials = thisRenderer.sharedMaterials;
                materials[materialIndexToSwitch] = uniqueMaterial;
                thisRenderer.sharedMaterials = materials;

                // Mark the renderer as modified so Unity saves this change in the editor without runtime leakage
                EditorUtility.SetDirty(thisRenderer);
            }
            else
            {
                // If already a unique material, just set the color
                if (newColor == null) newColor = Color.white;

                if (currentMaterial != null)
                    currentMaterial.color = newColor;
            }
#endif

            // Additional color setup
            if (GetComponent<MatchSymbolWithColor>() != null)
                GetComponent<MatchSymbolWithColor>().SetUpSymbolBasedOnColor(newColor);

            if (visibleOnlyAfterSelectedColor)
                SetVisibility(true);

            // Save the color and mark as default
            loadColorAtStart = true;
            defaultColor = newColor;
            SaveColorByIndex();
        }

        public Color GetColor()
        {
#if UNITY_EDITOR
            // Check if this object is part of a prefab
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                Debug.LogWarning($"Object {gameObject.name} is part of a prefab. Cannot get color.");
                return defaultColor; // Return default if part of a prefab
            }
#endif

            // Ensure `thisRenderer` is initialized
            if (thisRenderer == null)
            {
                thisRenderer = GetComponent<MeshRenderer>();
                if (thisRenderer == null)
                {
                    Debug.LogWarning($"MeshRenderer component is missing on object: {gameObject.name}");
                    return defaultColor; // Return default if MeshRenderer is not found
                }
            }

            //#if UNITY_EDITOR
            //if (thisRenderer.material == null && canUseThisMaterialIfMaterialIsNull)
            //    thisRenderer.material = thisMaterial;
            //#else
            //if (thisRenderer.sharedMaterial == null && canUseThisMaterialIfMaterialIsNull)
            //thisRenderer.sharedMaterial = thisMaterial;
            //#endif

            //If currently building for WebGL, Enable This
            //if (thisRenderer.sharedMaterial == null && canUseThisMaterialIfMaterialIsNull)
            //    thisRenderer.sharedMaterial = thisMaterial;

            //Else, Enable This
            if (thisRenderer.material == null && canUseThisMaterialIfMaterialIsNull)
                thisRenderer.material = thisMaterial;


            // Check that `materialIndexToSwitch` is within bounds of `sharedMaterials`
            if (thisRenderer.sharedMaterials == null || thisRenderer.sharedMaterials.Length <= materialIndexToSwitch)
            {
                Debug.LogWarning($"Material at index {materialIndexToSwitch} is out of bounds or missing on object: {gameObject.name}");
                return defaultColor; // Return default if materials array is invalid or out of bounds
            }

            // Check if the material at `materialIndexToSwitch` is not null
            Material targetMaterial = thisRenderer.sharedMaterials[materialIndexToSwitch];
            if (targetMaterial == null)
            {
                Debug.LogWarning($"Material at index {materialIndexToSwitch} is null on object: {gameObject.name}");
                return defaultColor; // Return default if material itself is null
            }

            // If everything is valid, return the color
            return targetMaterial.color;
        }


        public void ResetColorToDefault()
        {
            ChangeColor(defaultColor);
        }

        public void SetUpNumberInStackBasedOnColor(Color color)
        {
            if (!canSetUpNumberTextIfFound) return;
            if (GetComponentInChildren<TextMeshPro>(false) == null) return;

            int numberOfChoosenColor = stackColors.GetIndexOfColor(color);
            numberOfChoosenColor++;
            foreach (TextMeshPro text in GetComponentsInChildren<TextMeshPro>())
                text.text = numberOfChoosenColor.ToString();
        }

        public void SetCurrentColorAsDefault()
        {
            defaultColor = GetColor();
        }

        public void ColorizeBasedOnDropdownColor()
        {
            defaultColor = GetComponentInParent<UnlockedColors>().GetSelectedDropdownColor();
            ChangeColor(defaultColor);
            SetUpNumberInStackBasedOnColor(defaultColor);
            if (GetComponent<MatchSymbolWithColor>() != null)
                GetComponent<MatchSymbolWithColor>().SetUpSymbolBasedOnColor(defaultColor);

            SaveColorByIndex(stackColors.GetIndexOfColor(defaultColor));
        }

        public void ColorizeBasedOnSliderColor()
        {
            defaultColor = GetComponentInParent<UnlockedColors>().GetSelectedSliderColor();
            ChangeColor(defaultColor);

            SaveColorByIndex(stackColors.GetIndexOfColor(defaultColor));
            SetUpNumberInStackBasedOnColor(defaultColor);
            if (GetComponent<MatchSymbolWithColor>() != null)
                GetComponent<MatchSymbolWithColor>().SetUpSymbolBasedOnColor(defaultColor);
        }

        public void SaveColorByIndex(int newColorIndex = -1)
        {
            //if (cannotRewriteSavedMaterialWhilePlayMode && Application.isPlaying) return;

            if (newColorIndex < 0)
            {
                if (stackColors != null)
                    newColorIndex = stackColors.GetIndexOfColor(defaultColor);
            }

            savedColorIndex = newColorIndex;
            Debug.Log("SAVED COLOR INDEX: " + newColorIndex);
        }

        public int GetColorIndex()
        {
            return savedColorIndex;
        }
    }
}
