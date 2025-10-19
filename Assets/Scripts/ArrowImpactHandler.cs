using UnityEngine;

public class ArrowImpactHandler : MonoBehaviour
{
    [Header("Impact Settings")]
    [SerializeField] private float _stickDuration = 10f;
    [SerializeField] private float _impactDepth = 0.1f;
    [SerializeField] private LayerMask _collisionLayers = ~0;

    [Header("Effects")]
    [SerializeField] private GameObject _impactEffect;
    [SerializeField] private bool _enableExplosion = false;
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private float _explosionForce = 10f;

    private bool _hasImpacted = false;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasImpacted) return;

        // Check if the collision is on the allowed layers
        if (((1 << collision.gameObject.layer) & _collisionLayers) != 0)
        {
            HandleImpact(collision);
        }
    }

    private void HandleImpact(Collision collision)
    {
        _hasImpacted = true;

        // Stick the arrow to the surface
        if (_rb != null)
        {
            _rb.isKinematic = true;
        }

        // Parent the arrow to what it hit
        transform.SetParent(collision.transform);

        // Push the arrow slightly into the surface for better visual
        transform.position += transform.forward * -_impactDepth;

        // Spawn impact effect
        if (_impactEffect != null)
        {
            Instantiate(_impactEffect, collision.contacts[0].point, Quaternion.identity);
        }

        // Handle explosion if enabled
        if (_enableExplosion)
        {
            HandleExplosion(collision.contacts[0].point);
        }

        // Schedule destruction after stick duration
        if (_stickDuration > 0)
        {
            Destroy(gameObject, _stickDuration);
        }
    }

    private void HandleExplosion(Vector3 explosionPoint)
    {
        Collider[] colliders = Physics.OverlapSphere(explosionPoint, _explosionRadius);

        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(_explosionForce, explosionPoint, _explosionRadius);
            }
        }
    }

    public bool HasImpacted() => _hasImpacted;
}