using UnityEngine;

/// <summary>
/// Manages VR play area confined to the archery canopy bounds
/// Allows free movement within canopy but shows warnings at edges
/// </summary>
public class CanopyBoundarySystem : MonoBehaviour
{
    [Header("Canopy Bounds Settings")]
    [SerializeField] private Vector3 canopyCenter = Vector3.zero;
    [SerializeField] private float canopyWidth = 2.75f; // Length along Z-axis (depth)
    [SerializeField] private float canopyLength = 18.5f;   // Width along X-axis
    [SerializeField] private float canopyHeight = 3f;  // Height of canopy
    
    [Header("Boundary Warning Settings")]
    [SerializeField] private float warningDistance = 0.5f; // Distance from edge to show warning
    [SerializeField] private Color boundaryColor = new Color(0, 1, 1, 0.3f); // Cyan
    [SerializeField] private Color warningColor = new Color(1, 0, 0, 0.5f); // Red
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float lineHeight = 0.01f; // Height above ground
    
    [Header("References")]
    [SerializeField] private Transform xrOrigin; // The XR Origin that player controls
    [SerializeField] private Transform playerHead; // Main Camera
    
    [Header("Soft Boundary Options")]
    [SerializeField] private bool enableSoftBoundary = true; // Gently push player back
    [SerializeField] private float pushbackStrength = 2f;
    
    private LineRenderer[] boundaryLines = new LineRenderer[4]; // 4 walls
    private GameObject boundaryContainer;
    private bool[] wallWarningStates = new bool[4]; // Track warning state per wall

    void Start()
    {
        if (playerHead == null)
        {
            playerHead = Camera.main?.transform;
        }
        
        if (xrOrigin == null)
        {
            // Try to find XR Origin
            xrOrigin = GameObject.Find("XR Origin")?.transform;
            if (xrOrigin == null)
            {
                xrOrigin = GameObject.Find("XR Origin (XR Rig)")?.transform;
            }
        }
        
        SetupBoundaryVisuals();
    }

    void Update()
    {
        CheckBoundaryProximity();
        
        if (enableSoftBoundary)
        {
            EnforceBoundary();
        }
    }

    /// <summary>
    /// Creates visual boundary lines for the rectangular canopy
    /// </summary>
    void SetupBoundaryVisuals()
    {
        boundaryContainer = new GameObject("CanopyBoundaryVisuals");
        boundaryContainer.transform.parent = transform;
        boundaryContainer.transform.localPosition = canopyCenter;

        // Create 4 lines for rectangular boundary (Front, Back, Left, Right)
        string[] wallNames = { "Front Wall", "Back Wall", "Left Wall", "Right Wall" };
        
        for (int i = 0; i < 4; i++)
        {
            GameObject lineObj = new GameObject(wallNames[i]);
            lineObj.transform.parent = boundaryContainer.transform;
            lineObj.transform.localPosition = Vector3.zero;
            
            boundaryLines[i] = lineObj.AddComponent<LineRenderer>();
            ConfigureLineRenderer(boundaryLines[i]);
        }
        
        DrawBoundaryRectangle();
    }

    /// <summary>
    /// Configure a line renderer for boundary visualization
    /// </summary>
    void ConfigureLineRenderer(LineRenderer line)
    {
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = boundaryColor;
        line.endColor = boundaryColor;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = 2;
        line.useWorldSpace = false;
    }

    /// <summary>
    /// Draw the rectangular boundary lines
    /// </summary>
    void DrawBoundaryRectangle()
    {
        float halfLength = canopyLength / 2f;
        float halfWidth = canopyWidth / 2f;

        // Front wall (positive Z)
        boundaryLines[0].SetPosition(0, new Vector3(-halfWidth, lineHeight, halfLength));
        boundaryLines[0].SetPosition(1, new Vector3(halfWidth, lineHeight, halfLength));

        // Back wall (negative Z)
        boundaryLines[1].SetPosition(0, new Vector3(-halfWidth, lineHeight, -halfLength));
        boundaryLines[1].SetPosition(1, new Vector3(halfWidth, lineHeight, -halfLength));

        // Left wall (negative X)
        boundaryLines[2].SetPosition(0, new Vector3(-halfWidth, lineHeight, -halfLength));
        boundaryLines[2].SetPosition(1, new Vector3(-halfWidth, lineHeight, halfLength));

        // Right wall (positive X)
        boundaryLines[3].SetPosition(0, new Vector3(halfWidth, lineHeight, -halfLength));
        boundaryLines[3].SetPosition(1, new Vector3(halfWidth, lineHeight, halfLength));
    }

    /// <summary>
    /// Check if player is near any boundary wall and show warnings
    /// </summary>
    void CheckBoundaryProximity()
    {
        if (playerHead == null || boundaryLines[0] == null) return;

        Vector3 playerPos = playerHead.position;
        Vector3 centerPos = transform.position + canopyCenter;
        
        float halfLength = canopyLength / 2f;
        float halfWidth = canopyWidth / 2f;

        // Calculate distances to each wall
        float distToFront = (centerPos.z + halfLength) - playerPos.z;  // Front (positive Z)
        float distToBack = playerPos.z - (centerPos.z - halfLength);   // Back (negative Z)
        float distToLeft = playerPos.x - (centerPos.x - halfWidth);    // Left (negative X)
        float distToRight = (centerPos.x + halfWidth) - playerPos.x;   // Right (positive X)

        // Check each wall
        UpdateWallWarning(0, distToFront);  // Front
        UpdateWallWarning(1, distToBack);   // Back
        UpdateWallWarning(2, distToLeft);   // Left
        UpdateWallWarning(3, distToRight);  // Right
    }

    /// <summary>
    /// Update warning state for a specific wall
    /// </summary>
    void UpdateWallWarning(int wallIndex, float distance)
    {
        bool shouldWarn = distance < warningDistance;

        if (shouldWarn && !wallWarningStates[wallIndex])
        {
            // Enter warning state
            wallWarningStates[wallIndex] = true;
            boundaryLines[wallIndex].startColor = warningColor;
            boundaryLines[wallIndex].endColor = warningColor;
            boundaryLines[wallIndex].startWidth = lineWidth * 2f;
            boundaryLines[wallIndex].endWidth = lineWidth * 2f;
        }
        else if (!shouldWarn && wallWarningStates[wallIndex])
        {
            // Exit warning state
            wallWarningStates[wallIndex] = false;
            boundaryLines[wallIndex].startColor = boundaryColor;
            boundaryLines[wallIndex].endColor = boundaryColor;
            boundaryLines[wallIndex].startWidth = lineWidth;
            boundaryLines[wallIndex].endWidth = lineWidth;
        }
    }

    /// <summary>
    /// Gently prevent player from moving outside canopy bounds
    /// </summary>
    void EnforceBoundary()
    {
        if (xrOrigin == null || playerHead == null) return;

        Vector3 playerPos = playerHead.position;
        Vector3 centerPos = transform.position + canopyCenter;
        Vector3 xrOriginPos = xrOrigin.position;
        
        float halfLength = canopyLength / 2f;
        float halfWidth = canopyWidth / 2f;

        // Calculate how much player is outside bounds
        Vector3 pushback = Vector3.zero;

        // Check X bounds (width)
        if (playerPos.x < centerPos.x - halfWidth)
        {
            pushback.x = (centerPos.x - halfWidth - playerPos.x) * pushbackStrength * Time.deltaTime;
        }
        else if (playerPos.x > centerPos.x + halfWidth)
        {
            pushback.x = (centerPos.x + halfWidth - playerPos.x) * pushbackStrength * Time.deltaTime;
        }

        // Check Z bounds (length)
        if (playerPos.z < centerPos.z - halfLength)
        {
            pushback.z = (centerPos.z - halfLength - playerPos.z) * pushbackStrength * Time.deltaTime;
        }
        else if (playerPos.z > centerPos.z + halfLength)
        {
            pushback.z = (centerPos.z + halfLength - playerPos.z) * pushbackStrength * Time.deltaTime;
        }

        // Apply pushback to XR Origin
        if (pushback != Vector3.zero)
        {
            xrOrigin.position += pushback;
        }
    }

    /// <summary>
    /// Toggle boundary visibility
    /// </summary>
    public void ToggleBoundaryVisibility(bool visible)
    {
        if (boundaryContainer != null)
        {
            boundaryContainer.SetActive(visible);
        }
    }

    /// <summary>
    /// Update canopy dimensions (useful if canopy is dynamic)
    /// </summary>
    public void SetCanopyDimensions(float length, float width, float height)
    {
        canopyLength = length;
        canopyWidth = width;
        canopyHeight = height;
        DrawBoundaryRectangle();
    }

    /// <summary>
    /// Manually set canopy center position
    /// </summary>
    public void SetCanopyCenter(Vector3 center)
    {
        canopyCenter = center;
        if (boundaryContainer != null)
        {
            boundaryContainer.transform.localPosition = canopyCenter;
        }
    }

    // Visualize in editor
    void OnDrawGizmos()
    {
        Vector3 center = transform.position + canopyCenter;
        float halfLength = canopyLength / 2f;
        float halfWidth = canopyWidth / 2f;

        // Draw canopy bounds (cyan)
        Gizmos.color = Color.cyan;
        Vector3[] corners = new Vector3[4]
        {
            center + new Vector3(-halfWidth, 0, -halfLength),
            center + new Vector3(halfWidth, 0, -halfLength),
            center + new Vector3(halfWidth, 0, halfLength),
            center + new Vector3(-halfWidth, 0, halfLength)
        };

        // Draw ground rectangle
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }

        // Draw warning zone (yellow)
        Gizmos.color = Color.yellow;
        float warnLength = canopyLength - (warningDistance * 2);
        float warnWidth = canopyWidth - (warningDistance * 2);
        float halfWarnLength = warnLength / 2f;
        float halfWarnWidth = warnWidth / 2f;

        Vector3[] warnCorners = new Vector3[4]
        {
            center + new Vector3(-halfWarnWidth, 0, -halfWarnLength),
            center + new Vector3(halfWarnWidth, 0, -halfWarnLength),
            center + new Vector3(halfWarnWidth, 0, halfWarnLength),
            center + new Vector3(-halfWarnWidth, 0, halfWarnLength)
        };

        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(warnCorners[i], warnCorners[(i + 1) % 4]);
        }

        // Draw vertical lines to show height
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        foreach (Vector3 corner in corners)
        {
            Gizmos.DrawLine(corner, corner + Vector3.up * canopyHeight);
        }
    }
}