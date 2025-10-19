using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ArrowSpawner : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private GameObject _arrowPrefab;
    [SerializeField] private float _spawnDelay = 0.1f;

    [Header("References")]
    [SerializeField] private Transform _notchPoint;
    [SerializeField] private XRPullInteractable _pullInteractable;

    private GameObject _currentArrow;
    private bool _isSpawning = false;
    private bool _hasArrow = false;

    private void Awake()
    {
        // Try to get the pull interactable if not assigned
        if (_pullInteractable == null)
        {
            _pullInteractable = GetComponent<XRPullInteractable>();
        }

        // Try to find notch point if not assigned
        if (_notchPoint == null)
        {
            // Look for a child object with "notch" in the name
            foreach (Transform child in transform)
            {
                if (child.name.ToLower().Contains("notch"))
                {
                    _notchPoint = child;
                    break;
                }
            }
        }

        if (_pullInteractable != null)
        {
            _pullInteractable.PullStarted += HandlePullStarted;
            _pullInteractable.PullActionReleased += HandlePullReleased;
            _pullInteractable.PullEnded += HandlePullEnded;
        }
        else
        {
            Debug.LogError("XRPullInteractable not found on ArrowSpawner!");
        }
    }

    private void OnDestroy()
    {
        if (_pullInteractable != null)
        {
            _pullInteractable.PullStarted -= HandlePullStarted;
            _pullInteractable.PullActionReleased -= HandlePullReleased;
            _pullInteractable.PullEnded -= HandlePullEnded;
        }
    }

    private void HandlePullStarted()
    {
        // Spawn arrow when user starts pulling the string
        if (!_hasArrow && !_isSpawning)
        {
            StartCoroutine(SpawnArrowAfterDelay());
        }
    }

    private void HandlePullReleased(float pullAmount)
    {
        // Launch the arrow if we have one
        if (_currentArrow != null)
        {
            var arrowLauncher = _currentArrow.GetComponent<ArrowLauncher>();
            if (arrowLauncher != null)
            {
                arrowLauncher.LaunchArrow(pullAmount);
            }
        }

        // Clear references
        _currentArrow = null;
        _hasArrow = false;
    }

    private void HandlePullEnded()
    {
        // If pull ended without an arrow (cancelled grab), ensure we can spawn again
        if (_currentArrow == null && !_isSpawning)
        {
            _hasArrow = false;
        }
    }

    private IEnumerator SpawnArrowAfterDelay()
    {
        _isSpawning = true;
        yield return new WaitForSeconds(_spawnDelay);
        SpawnArrow();
        _isSpawning = false;
        _hasArrow = true;
    }

    private void SpawnArrow()
    {
        if (_arrowPrefab == null || _notchPoint == null)
        {
            Debug.LogWarning("Arrow prefab or notch point not set in ArrowSpawner");
            return;
        }

        if (_currentArrow != null)
        {
            Destroy(_currentArrow);
        }

        // Spawn arrow with NO ROTATION from prefab
        _currentArrow = Instantiate(_arrowPrefab, _notchPoint.position, Quaternion.identity);
        _currentArrow.transform.SetParent(_notchPoint);

        // Completely reset and force the rotation to match the bow's forward direction
        _currentArrow.transform.rotation = Quaternion.LookRotation(_notchPoint.forward, Vector3.up);

        // Reset arrow state
        var arrowLauncher = _currentArrow.GetComponent<ArrowLauncher>();
        if (arrowLauncher != null)
        {
            arrowLauncher.ResetArrow();
        }
    }

    // Public method to manually spawn arrow if needed
    public void ForceSpawnArrow()
    {
        if (!_isSpawning)
        {
            StopAllCoroutines();
            SpawnArrow();
            _hasArrow = true;
        }
    }

    // Getters for external access
    public GameObject GetCurrentArrow() => _currentArrow;
    public bool IsArrowReady() => _currentArrow != null;
    public bool HasArrow() => _hasArrow;
}