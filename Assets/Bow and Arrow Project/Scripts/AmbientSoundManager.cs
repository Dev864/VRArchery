using UnityEngine;

public class AmbientSoundManager : MonoBehaviour
{
    public AudioClip ambientMusic;
    public AudioClip forestSounds; // or whatever fits your scene
    private AudioSource musicSource;
    private AudioSource ambientSource;

    void Start()
    {
        // Create audio sources
        musicSource = gameObject.AddComponent<AudioSource>();
        ambientSource = gameObject.AddComponent<AudioSource>();

        // Set up music
        musicSource.clip = ambientMusic;
        musicSource.loop = true;
        musicSource.volume = 0.3f;
        musicSource.Play();

        // Set up ambient sounds
        ambientSource.clip = forestSounds;
        ambientSource.loop = true;
        ambientSource.volume = 0.2f;
        ambientSource.Play();
    }

    // Optional: Control methods
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetAmbientVolume(float volume)
    {
        ambientSource.volume = volume;
    }
}