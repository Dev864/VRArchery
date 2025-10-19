using UnityEngine;
using System.Collections;

public class ArrowLauncher : MonoBehaviour
{
    [Header("Launch Settings")]
    [SerializeField] private float _baseForce = 20f;
    [SerializeField] private float _maxForceMultiplier = 2f;
    [SerializeField] private float _minForceMultiplier = 0.5f;
    [SerializeField] private float _forwardOffset = 0.3f;

    [Header("Visual Fix")]
    [SerializeField] private Transform _arrowModel; // Drag the visual model here

    private Rigidbody _rb;
    private bool _isLaunched = false;
    private Collider _arrowCollider;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _arrowCollider = GetComponent<Collider>();

        // Start with physics disabled
        if (_rb != null)
        {
            _rb.isKinematic = true;
        }

        // Disable collider until launched
        if (_arrowCollider != null)
        {
            _arrowCollider.enabled = false;
        }
    }

    public void LaunchArrow(float pullAmount)
    {
        if (_isLaunched) return;

        _isLaunched = true;

        // Move arrow forward to clear the bow before enabling physics
        transform.position += transform.forward * _forwardOffset;

        // Enable collider
        if (_arrowCollider != null)
        {
            _arrowCollider.enabled = true;
        }

        // Calculate force based on pull amount
        float forceMultiplier = Mathf.Lerp(_minForceMultiplier, _maxForceMultiplier, pullAmount);
        float launchForce = _baseForce * forceMultiplier;

        Debug.Log($"Launching arrow with force: {launchForce} (pull: {pullAmount})");

        // Apply force forward
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.AddForce(transform.forward * launchForce, ForceMode.VelocityChange);
        }

        // Detach from parent (bow notch)
        transform.SetParent(null);
    }

    public void FixArrowVisual()
    {
        // If the arrow model is pointing wrong, rotate it
        if (_arrowModel != null)
        {
            _arrowModel.localRotation = Quaternion.Euler(-90, 0, 0); // Adjust as needed
        }
    }

    public void ResetArrow()
    {
        _isLaunched = false;

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        if (_arrowCollider != null)
        {
            _arrowCollider.enabled = false;
        }

        FixArrowVisual(); // Apply visual fix
    }

    public bool IsLaunched() => _isLaunched;
}