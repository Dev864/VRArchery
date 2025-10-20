using UnityEngine;

public class RandomPitch : MonoBehaviour
{
    public AudioSource audioSource;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        audioSource.pitch = Random.Range(minPitch, maxPitch);
    }
}