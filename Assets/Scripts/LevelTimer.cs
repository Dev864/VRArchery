using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    private float timer;
    private bool isTiming;

    public static LevelTimer Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        timer = 0f;
        isTiming = true;
    }

    void Update()
    {
        if (isTiming)
        {
            timer += Time.deltaTime;
        }
    }

    public void StartTimer()
    {
        isTiming = true;
    }

    public void StopTimer()
    {
        isTiming = false;
    }

    public void SaveTotalTime()
    {
        PlayerPrefs.SetFloat("TotalTime", timer);
        PlayerPrefs.Save();
        Debug.Log($"Total time saved: {timer} seconds");
    }

    public void ResetTimer()
    {
        timer = 0f;
        isTiming = true;
    }

    public float GetTotalTime()
    {
        return timer;
    }
}