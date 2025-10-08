using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Manages VR rig setup with controller support
/// </summary>
public class VRRigSetup : MonoBehaviour
{
    [Header("Controller References")]
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;
    
    [Header("Controller Settings")]
    [SerializeField] private bool enableControllersOnStart = true;

    void Start()
    {
        // Initialize with controllers active
        if (enableControllersOnStart)
        {
            EnableControllers();
        }
    }

    /// <summary>
    /// Enable both controllers
    /// </summary>
    public void EnableControllers()
    {
        if (leftController != null)
        {
            leftController.SetActive(true);
        }
        
        if (rightController != null)
        {
            rightController.SetActive(true);
        }
    }

    /// <summary>
    /// Disable both controllers (useful for cutscenes or special states)
    /// </summary>
    public void DisableControllers()
    {
        if (leftController != null)
        {
            leftController.SetActive(false);
        }
        
        if (rightController != null)
        {
            rightController.SetActive(false);
        }
    }

    /// <summary>
    /// Toggle controllers on/off
    /// </summary>
    public void ToggleControllers()
    {
        bool currentState = leftController != null && leftController.activeSelf;
        
        if (currentState)
        {
            DisableControllers();
        }
        else
        {
            EnableControllers();
        }
    }

    /// <summary>
    /// Check if controllers are currently active
    /// </summary>
    public bool AreControllersActive()
    {
        return (leftController != null && leftController.activeSelf) ||
               (rightController != null && rightController.activeSelf);
    }

    // Show helpful debug info in inspector
    void OnValidate()
    {
        if (leftController == null || rightController == null)
        {
            Debug.LogWarning("VRRigSetup: Please assign Left and Right Controller references!");
        }
    }
}