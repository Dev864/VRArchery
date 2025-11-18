using UnityEngine;

public class ArrowImpact : MonoBehaviour
{
    private AudioSource impactSource;
    private bool hasPlayedImpact = false;

    void Start()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();

        if (audioSources.Length >= 2)
        {
            impactSource = audioSources[1];
            impactSource.playOnAwake = false;
            impactSource.loop = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasPlayedImpact && impactSource != null)
        {
            impactSource.Play();
            hasPlayedImpact = true;

            GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}