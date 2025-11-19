using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TMP_Text lastShotScoreText;
    [SerializeField] private TMP_Text roundTotalScoreText; 

    // Events for other systems to subscribe to
    public event Action<int> OnShotScored;
    public event Action<int> OnRoundScoreChanged;
    public event Action<int> OnLevelCompleted;

    private int currentLevelScore = 0;
    private int lastShotScore = 0;
    private int totalScore = 0;
    private int totalHits = 0;

    void Awake()
    {
        // Singleton setup - persists across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        LoadTotalScore();
        UpdateScoreUI();
    }

    /// <summary>
    /// Add points to the current level score
    /// </summary>
    public void AddScore(int points)
    {
        lastShotScore = points;
        currentLevelScore += points;
        totalScore += points;
        totalHits++;
        
        // Trigger events
        OnShotScored?.Invoke(points);
        OnRoundScoreChanged?.Invoke(currentLevelScore);
        
        UpdateScoreUI();
        
        Debug.Log($"[ScoreManager] +{points} (Level Score: {currentLevelScore}, Total: {totalScore})");
    }

    /// <summary>
    /// Reset the score for a new level
    /// </summary>
    public void ResetLevelScore()
    {
        currentLevelScore = 0;
        lastShotScore = 0;
        totalHits = 0;
        
        OnRoundScoreChanged?.Invoke(currentLevelScore);
        UpdateScoreUI();
        
        Debug.Log("[ScoreManager] Level score reset");
    }

    /// <summary>
    /// Save the level completion data
    /// </summary>
    public void CompleteLevel(int levelNumber)
    {
        // Save the level score
        PlayerPrefs.SetInt($"Level_{levelNumber}_Score", currentLevelScore);
        PlayerPrefs.SetInt($"Level_{levelNumber}_Hits", totalHits);
        PlayerPrefs.SetInt("TotalScore", totalScore);
        PlayerPrefs.Save();
        
        // Trigger event
        OnLevelCompleted?.Invoke(levelNumber);
        
        Debug.Log($"[ScoreManager] Level {levelNumber} completed! Score: {currentLevelScore}, Hits: {totalHits}");
    }

    private void LoadTotalScore()
    {
        totalScore = PlayerPrefs.GetInt("TotalScore", 0);
    }

    private void UpdateScoreUI()
    {
        // Update last shot score (shows "+100" for example)
        if (lastShotScoreText != null)
        {
            if (lastShotScore > 0)
            {
                lastShotScoreText.text = $"+{lastShotScore}";
            }
            else
            {
                lastShotScoreText.text = ""; // Clear when round starts
            }
        }
        
        // Update round total score
        if (roundTotalScoreText != null)
        {
            roundTotalScoreText.text = $"{currentLevelScore}";
        }
    }

    // Legacy method for backward compatibility
    public void ResetRound()
    {
        ResetLevelScore();
    }

    // Getters
    public int GetRoundTotal() => currentLevelScore; // Kept for backward compatibility
    public int GetCurrentLevelScore() => currentLevelScore;
    public int GetLastShotScore() => lastShotScore;
    public int GetTotalScore() => totalScore;
    public int GetTotalHits() => totalHits;
    public int GetLevelScore(int levelNumber) => PlayerPrefs.GetInt($"Level_{levelNumber}_Score", 0);
    public int GetLevelHits(int levelNumber) => PlayerPrefs.GetInt($"Level_{levelNumber}_Hits", 0);
}