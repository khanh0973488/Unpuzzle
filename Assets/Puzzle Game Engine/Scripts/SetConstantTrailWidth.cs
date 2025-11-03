using UnityEngine;

namespace HyperPuzzleEngine
{
    [RequireComponent(typeof(TrailRenderer))]
public class SetConstantTrailWidth : MonoBehaviour
{
    public float defaultTrailWidth = 0.8f;  // Default trail width
    public float defaultParentXScale = 0.3946016f;  // Default X scale of the parent
    private TrailRenderer trailRenderer;

    private void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void Start()
    {
        if (transform.parent == null) return;  // Ensure there's a parent object

        // Calculate the relative scale factor
        float currentParentXScale = transform.parent.localScale.x;
        float scaleFactor = currentParentXScale / defaultParentXScale;

        // Adjust the trail width based on the scale factor
        trailRenderer.startWidth = defaultTrailWidth * scaleFactor;
        trailRenderer.endWidth = 0;  // Set to zero for a tapered trail (as shown in the image)
    }
}
}