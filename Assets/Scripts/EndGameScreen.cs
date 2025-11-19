using TMPro;
using UnityEngine;

public class EndGameScreen : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject summaryPanel;
    
    [Header("Text Elements")]
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text levelScoreText;
    [SerializeField] private TMP_Text totalScoreText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text arrowsUsedText;
    [SerializeField] private TMP_Text targetsHitText;
    
    void Start()
    {
        // Hide panel at start
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Show the end game summary
    /// </summary>
    public void ShowSummary()
    {
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(true);
        }
        
        // Get data from managers
        LevelConfig levelConfig = GameManager.Instance?.GetCurrentLevelConfig();
        int levelScore = ScoreManager.Instance?.GetCurrentLevelScore() ?? 0;
        int totalScore = ScoreManager.Instance?.GetTotalScore() ?? 0;
        int totalHits = ScoreManager.Instance?.GetTotalHits() ?? 0;
        int arrowsShot = ArrowCounter.Instance?.GetArrowsShot() ?? 0;
        int targetsHit = GameManager.Instance?.GetTargetsHit() ?? 0;
        
        // Calculate accuracy
        float accuracy = ArrowCounter.Instance?.GetAccuracy(totalHits) ?? 0f;
        
        // Update UI
        if (levelNameText != null && levelConfig != null)
        {
            levelNameText.text = $"Level {levelConfig.levelNumber}: {levelConfig.levelName}";
        }
        
        if (levelScoreText != null)
        {
            levelScoreText.text = $"Level Score: {levelScore}";
        }
        
        if (totalScoreText != null)
        {
            totalScoreText.text = $"Total Score: {totalScore}";
        }
        
        if (accuracyText != null)
        {
            accuracyText.text = $"Accuracy: {accuracy:F1}%";
        }
        
        if (arrowsUsedText != null)
        {
            int maxArrows = ArrowCounter.Instance?.GetMaxArrows() ?? 0;
            arrowsUsedText.text = $"Arrows Used: {arrowsShot}/{maxArrows}";
        }
        
        if (targetsHitText != null)
        {
            int totalTargets = levelConfig?.targetCount ?? 0;
            targetsHitText.text = $"Targets Hit: {targetsHit}/{totalTargets}";
        }
        
        Debug.Log("[EndGameScreen] Summary displayed");
    }
    
    /// <summary>
    /// Hide the summary panel
    /// </summary>
    public void HideSummary()
    {
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(false);
        }
    }
    
    // Button callbacks
    public void OnRestartButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartLevel();
        }
    }
    
    public void OnNextLevelButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadNextLevel();
        }
    }
    
    public void OnMainMenuButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
}