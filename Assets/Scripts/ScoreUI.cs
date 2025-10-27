using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public TMP_Text shotScoreText;
    public TMP_Text totalScoreText;
    public GameObject scorePanel;
    
    void OnEnable()
    {
        if (scorePanel)
        {
            scorePanel.SetActive(false);
        }
        
        StartCoroutine(WaitForScoreManager());
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnShotScored -= OnShotScored;
            ScoreManager.Instance.OnRoundScoreChanged -= OnRoundScoreChanged;
        }
    }

    private System.Collections.IEnumerator WaitForScoreManager()
    {
        yield return new WaitUntil(() => ScoreManager.Instance != null);

        ScoreManager.Instance.OnShotScored += OnShotScored;
        ScoreManager.Instance.OnRoundScoreChanged += OnRoundScoreChanged;

        yield return new WaitUntil(() => SafetyWarning.safetyAgreed);

        if (scorePanel)
        {
            scorePanel.SetActive(true);
        }
    }

    void OnShotScored(int points)
    {
        if (!SafetyWarning.safetyAgreed) return;
        
        if (shotScoreText)
        {
            shotScoreText.text = $"+{points}";
            StartCoroutine(FlashShotText());
        }
    }

    void OnRoundScoreChanged(int total)
    {
        if (!SafetyWarning.safetyAgreed) return;

        if (totalScoreText)
            totalScoreText.text = "Total: " + total.ToString();
    }

    System.Collections.IEnumerator FlashShotText()
    {
        shotScoreText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        shotScoreText.gameObject.SetActive(false);
    }
}
