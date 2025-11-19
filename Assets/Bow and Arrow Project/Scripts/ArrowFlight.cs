using UnityEngine;

public class ArrowFlight : MonoBehaviour
{
    private AudioSource whistleSource;
    private Rigidbody rb;
    private bool isFlying = false;
    private bool hasImpact = false;

    void Start()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();

        if (audioSources.Length >= 2)
        {
            whistleSource = audioSources[0];
            // KEEP the clip assigned in Inspector - don't set it to null
            whistleSource.loop = false;
            whistleSource.volume = 0f; // Start at zero
            whistleSource.Stop();
        }

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rb != null && !isFlying && !hasImpact && rb.linearVelocity.magnitude > 2f)
        {
            isFlying = true;
            if (whistleSource != null && !whistleSource.isPlaying)
            {
                whistleSource.Play();
            }
        }

        if (isFlying && !hasImpact && whistleSource != null && rb != null)
        {
            float speed = rb.linearVelocity.magnitude;
            whistleSource.volume = Mathf.Clamp(speed / 25f, 0.1f, 0.7f);
            whistleSource.pitch = 0.8f + (speed / 40f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasImpact)
        {
            hasImpact = true;
            isFlying = false;
            if (whistleSource != null && whistleSource.isPlaying)
            {
                whistleSource.Stop();
                whistleSource.volume = 0f; // Reset volume for next shot
            }
        }
    }
}