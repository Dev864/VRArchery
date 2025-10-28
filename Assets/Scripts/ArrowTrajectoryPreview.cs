using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(LineRenderer))]
public class ArrowRayPreview : MonoBehaviour
{
    [Header("References")]
    public XRPullInteractable bowPull;  // XR bow pull script
    public Transform arrowTip;          // the actual tip of the arrow

    [Header("Ray Settings")]
    public float maxDistance = 20f;     // how far the ray points
    public float rayWidth = 0.01f;      // width of the line

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.useWorldSpace = true; // ensure it follows world coordinates
        lineRenderer.enabled = false;
    }

    private void OnEnable()
    {
        if (bowPull != null)
        {
            bowPull.PullStarted += OnPullStarted;
            bowPull.PullUpdated += OnPullUpdated;
            bowPull.PullEnded += OnPullEnded;
        }
    }

    private void OnDisable()
    {
        if (bowPull != null)
        {
            bowPull.PullStarted -= OnPullStarted;
            bowPull.PullUpdated -= OnPullUpdated;
            bowPull.PullEnded -= OnPullEnded;
        }
    }

    private void OnPullStarted()
    {
        if (arrowTip != null)
            lineRenderer.enabled = true;
    }

    private void OnPullUpdated(float pullAmount)
    {
        if (!lineRenderer.enabled || arrowTip == null) return;

        // Use the tip position as start
        Vector3 start = arrowTip.position;

        // Use the tip's world forward as direction
        Vector3 direction = arrowTip.forward;

        // Optional: scale with pull amount
        float distance = pullAmount * maxDistance;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, start + direction * distance);
    }

    private void OnPullEnded()
    {
        lineRenderer.enabled = false;
    }
}
