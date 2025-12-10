using System;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI References (use tags)")]
    [SerializeField] private TMP_Text lastShotScoreText;  // Shows "+100"
    [SerializeField] private TMP_Text roundTotalScoreText; // Shows round total
    [SerializeField] private GameObject scorePanel;        // Panel that shows/hides based on safety

    [Header("UI Settings")]
    [SerializeField] private float lastShotDisplayTime = 2f; // How long to show last shot
    [SerializeField] private bool waitForSafetyWarning = true; // Enable/disable safety check

    // Events
    public event Action<int> OnShotScored;
    public event Action<int> OnRoundScoreChanged;
    public event Action<int> OnLevelCompleted;

    private int currentLevelScore = 0;
    private int currentLevelHits = 0; // added this - didnt break
    private int lastShotScore = 0;
    private int totalScore = 0;
    private int totalHits = 0;
    private bool safetyAgreed = false;

    private Coroutine lastShotCoroutine;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe to scene load
            totalScore = 0;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        
        // If safety check disabled, show score panel immediately
        if (!waitForSafetyWarning)
        {
            safetyAgreed = true;
            UpdateScoreUI();
            if (scorePanel != null)
                scorePanel.SetActive(true);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find UI elements in the new scene by tags
        lastShotScoreText = GameObject.FindWithTag("LastShotScore")?.GetComponent<TMP_Text>();
        roundTotalScoreText = GameObject.FindWithTag("RoundTotalScore")?.GetComponent<TMP_Text>();
        scorePanel = GameObject.FindWithTag("ScorePanel");

        if (lastShotScoreText != null)
            lastShotScoreText.gameObject.SetActive(false);

        // Show score panel if safety is already agreed
        if (scorePanel != null)
            scorePanel.SetActive(safetyAgreed || !waitForSafetyWarning);

        // Reset per-level score
        ResetLevelScore();

        // Wait for safety warning if enabled
        if (waitForSafetyWarning && !SafetyWarning.safetyAgreed)
        {
            StartCoroutine(WaitForSafetyAgreement());
        }

        UpdateScoreUI();
    }


    private IEnumerator WaitForSafetyAgreement()
    {
        Debug.Log("[ScoreManager] Waiting for safety agreement...");
        yield return new WaitUntil(() => SafetyWarning.safetyAgreed);

        safetyAgreed = true;
        if (scorePanel != null)
            scorePanel.SetActive(true);

        UpdateScoreUI();

        Debug.Log("[ScoreManager] Safety agreed, score panel shown");
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
        currentLevelHits++; // added this - didnt break
        totalScore += points;
        Debug.Log($"{totalScore} on hit {totalHits}");
        totalHits++;

        OnShotScored?.Invoke(points);
        OnRoundScoreChanged?.Invoke(currentLevelScore);

        UpdateScoreUI();

        Debug.Log($"[ScoreManager] +{points} (Level Score: {currentLevelScore}, Total: {totalScore})");
    }

    public void ResetLevelScore()
    {
        currentLevelScore = 0;
        currentLevelHits = 0; // added this - didnt break
        lastShotScore = 0;

        OnRoundScoreChanged?.Invoke(currentLevelScore);
        UpdateScoreUI();

        Debug.Log("[ScoreManager] Level score reset");
    }

    public void CompleteLevel(int levelNumber)
    {
        // Save scores
        PlayerPrefs.SetInt($"Level_{levelNumber}_Score", currentLevelScore);
        PlayerPrefs.SetInt($"Level_{levelNumber}_Hits", currentLevelHits); // added this - didnt break
        PlayerPrefs.SetInt($"TotalHits", totalHits);
        PlayerPrefs.SetInt("TotalScore", totalScore);
        PlayerPrefs.Save();

        OnLevelCompleted?.Invoke(levelNumber);

        Debug.Log($"[ScoreManager] Level {levelNumber} completed! Score: {currentLevelScore}, Hits: {totalHits}");
    }

    private void LoadTotalScore()
    {
        totalScore = PlayerPrefs.GetInt("TotalScore", 0);
    }

    private void UpdateScoreUI()
    {
        if (waitForSafetyWarning && !safetyAgreed) return;

        // Ensure the panel is active
        if (scorePanel != null && !scorePanel.activeSelf)
            scorePanel.SetActive(true);

        // Last shot flash
        if (lastShotScoreText != null && lastShotScore > 0)
        {
            if (lastShotCoroutine != null)
                StopCoroutine(lastShotCoroutine);

            lastShotCoroutine = StartCoroutine(FlashShotScore(lastShotScore));
        }

        // Round total
        if (roundTotalScoreText != null)
            roundTotalScoreText.text = $"{currentLevelScore}";
    }


    private IEnumerator FlashShotScore(int points)
    {
        lastShotScoreText.gameObject.SetActive(true);
        lastShotScoreText.text = $"+{points}";

        yield return new WaitForSeconds(lastShotDisplayTime);

        lastShotScoreText.gameObject.SetActive(false);
        lastShotCoroutine = null;
    }

    // Getters
    public int GetRoundTotal() => currentLevelScore;
    public int GetCurrentLevelScore() => currentLevelScore;
    public int GetLastShotScore() => lastShotScore;
    public int GetTotalScore() => totalScore;
    public int GetTotalHits() => totalHits;
    public int GetLevelScore(int levelNumber) => PlayerPrefs.GetInt($"Level_{levelNumber}_Score", 0);
    public int GetLevelHits(int levelNumber) => PlayerPrefs.GetInt($"Level_{levelNumber}_Hits", 0);
    public bool IsSafetyAgreed() => safetyAgreed || !waitForSafetyWarning;
}