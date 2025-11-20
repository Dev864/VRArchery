using System;
using TMPro;
using UnityEngine;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TMP_Text lastShotScoreText;  // Shows "+100" with flash effect
    [SerializeField] private TMP_Text roundTotalScoreText; // Shows round total
    [SerializeField] private GameObject scorePanel;        // Panel that shows/hides based on safety

    [Header("UI Settings")]
    [SerializeField] private float lastShotDisplayTime = 2f; // How long to show last shot
    [SerializeField] private bool waitForSafetyWarning = true; // Enable/disable safety check

    // Events for other systems to subscribe to
    public event Action<int> OnShotScored;
    public event Action<int> OnRoundScoreChanged;
    public event Action<int> OnLevelCompleted;

    private int currentLevelScore = 0;
    private int lastShotScore = 0;
    private int totalScore = 0;
    private int totalHits = 0;
    private bool safetyAgreed = false;

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
        
        // Hide score panel initially
        if (scorePanel != null)
        {
            scorePanel.SetActive(false);
        }
        
        // Hide last shot text initially
        if (lastShotScoreText != null)
        {
            lastShotScoreText.gameObject.SetActive(false);
        }
        
        // Wait for safety warning if enabled
        if (waitForSafetyWarning)
        {
            StartCoroutine(WaitForSafetyAgreement());
        }
        else
        {
            // No safety warning needed, show immediately
            safetyAgreed = true;
            ShowScorePanel();
        }
    }

    private IEnumerator WaitForSafetyAgreement()
    {
        Debug.Log("[ScoreManager] Waiting for safety agreement...");
        
        // Wait until SafetyWarning.safetyAgreed is true
        yield return new WaitUntil(() => SafetyWarning.safetyAgreed);
        
        safetyAgreed = true;
        ShowScorePanel();
        
        Debug.Log("[ScoreManager] Safety agreed, score panel shown");
    }

    private void ShowScorePanel()
    {
        if (scorePanel != null)
        {
            scorePanel.SetActive(true);
        }
        
        UpdateScoreUI();
    }

    public void AddScore(int points)
    {
        // Don't process scores until safety is agreed (if enabled)
        if (waitForSafetyWarning && !safetyAgreed) 
        {
            Debug.Log("[ScoreManager] Score blocked - waiting for safety agreement");
            return;
        }

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

    public void ResetLevelScore()
    {
        currentLevelScore = 0;
        lastShotScore = 0;
        totalHits = 0;
        
        OnRoundScoreChanged?.Invoke(currentLevelScore);
        UpdateScoreUI();
        
        Debug.Log("[ScoreManager] Level score reset");
    }

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
        // Don't show UI until safety is agreed (if enabled)
        if (waitForSafetyWarning && !safetyAgreed) return;

        // Update last shot score with FLASH effect
        if (lastShotScoreText != null && lastShotScore > 0)
        {
            StopAllCoroutines(); // Stop any existing flash
            StartCoroutine(FlashShotScore(lastShotScore));
        }
        
        // Update round total score
        if (roundTotalScoreText != null)
        {
            roundTotalScoreText.text = $"{currentLevelScore}";
        }
    }

    private IEnumerator FlashShotScore(int points)
    {
        // Show the score
        lastShotScoreText.gameObject.SetActive(true);
        lastShotScoreText.text = $"+{points}";
        
        // Wait for display time
        yield return new WaitForSeconds(lastShotDisplayTime);
        
        // Hide the score
        lastShotScoreText.gameObject.SetActive(false);
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
    public bool IsSafetyAgreed() => safetyAgreed || !waitForSafetyWarning;
}