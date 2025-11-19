using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Level Configuration")]
    [SerializeField] private LevelConfig currentLevelConfig;
    
    [Header("Managers")]
    [SerializeField] private ArrowCounter arrowCounter;
    [SerializeField] private ScoreManager scoreManager;
    
    [Header("End Game Screen")]
    [SerializeField] private EndGameScreen endGameScreen;
    
    private bool levelComplete = false;
    private int targetsHit = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (currentLevelConfig != null)
        {
            LoadLevel(currentLevelConfig);
        }
        else
        {
            Debug.LogError("[GameManager] No LevelConfig assigned!");
        }
    }
    
    void LoadLevel(LevelConfig config)
    {
        Debug.Log($"[GameManager] Loading Level {config.levelNumber}: {config.levelName}");
        
        // Initialize arrow system
        if (arrowCounter != null)
        {
            arrowCounter.InitializeArrows(config.maxArrows);
        }
        else
        {
            Debug.LogError("[GameManager] ArrowCounter not assigned!");
        }
        
        // Reset score for new level
        if (scoreManager != null)
        {
            scoreManager.ResetLevelScore();
        }
        else
        {
            Debug.LogError("[GameManager] ScoreManager not assigned!");
        }
        
        levelComplete = false;
        targetsHit = 0;
    }
    
    /// <summary>
    /// Call this when a target is hit
    /// </summary>
    public void OnTargetHit(int points)
    {
        if (levelComplete) return;
        
        targetsHit++;
        
        if (scoreManager != null)
        {
            scoreManager.AddScore(points);
        }
        
        Debug.Log($"[GameManager] Target hit! Points: {points}, Total targets hit: {targetsHit}");
        
        // Check if all targets are hit
        if (currentLevelConfig != null && targetsHit >= currentLevelConfig.targetCount)
        {
            OnAllTargetsHit();
        }
    }
    
    /// <summary>
    /// Called when player runs out of arrows
    /// </summary>
    public void OnOutOfArrows()
    {
        if (!levelComplete)
        {
            Debug.Log("[GameManager] Level ended - out of arrows");
            CompleteLevel();
        }
    }
    
    /// <summary>
    /// Called when all targets are destroyed
    /// </summary>
    public void OnAllTargetsHit()
    {
        if (!levelComplete)
        {
            Debug.Log("[GameManager] Level ended - all targets hit!");
            CompleteLevel();
        }
    }
    
    void CompleteLevel()
    {
        levelComplete = true;
        
        if (scoreManager != null && currentLevelConfig != null)
        {
            scoreManager.CompleteLevel(currentLevelConfig.levelNumber);
        }
        
        // Show end screen after a short delay
        Invoke("ShowEndScreen", 2f);
    }
    
    void ShowEndScreen()
    {
        if (endGameScreen != null)
        {
            endGameScreen.ShowSummary();
        }
        else
        {
            Debug.LogWarning("[GameManager] End game screen not assigned. Level complete!");
        }
    }
    
    /// <summary>
    /// Restart the current level
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f; // Resume time in case it was paused
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Load the next level
    /// </summary>
    public void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("[GameManager] No more levels! Game complete!");
            // TODO: Go to main menu or credits
        }
    }
    
    /// <summary>
    /// Load main menu
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Assumes main menu is scene 0
    }
    
    // Getters
    public LevelConfig GetCurrentLevelConfig() => currentLevelConfig;
    public int GetTargetsHit() => targetsHit;
}