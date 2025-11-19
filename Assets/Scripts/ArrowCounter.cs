using TMPro;
using UnityEngine;

public class ArrowCounter : MonoBehaviour
{
    public static ArrowCounter Instance;

    [Header("UI")]
    public TMP_Text arrowCountText;
    
    [Header("Arrow Limits")]
    private int maxArrows;
    private int arrowsRemaining;
    private int arrowsShot = 0;

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

    // Call this from GameManager when level starts
    public void InitializeArrows(int amount)
    {
        maxArrows = amount;
        arrowsRemaining = maxArrows;
        arrowsShot = 0;
        UpdateUI();
    }

    // Check if player can shoot
    public bool CanShootArrow()
    {
        return arrowsRemaining > 0;
    }

    // Call this when arrow is shot
    public void IncrementArrowCount()
    {
        if (arrowsRemaining > 0)
        {
            arrowsShot++;
            arrowsRemaining--;
            UpdateUI();
            
            if (arrowsRemaining == 0)
            {
                OnOutOfArrows();
            }
        }
    }

    private void OnOutOfArrows()
    {
        Debug.Log("Out of arrows!");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnOutOfArrows();
        }
    }

    private void UpdateUI()
    {
        if (arrowCountText != null)
        {
            arrowCountText.text = $"Arrows: {arrowsRemaining}/{maxArrows}";
        }
    }

    public void ResetCounter()
    {
        arrowsShot = 0;
        arrowsRemaining = maxArrows;
        UpdateUI();
    }

    // Getters
    public int GetArrowsRemaining() => arrowsRemaining;
    public int GetMaxArrows() => maxArrows;
    public int GetArrowsShot() => arrowsShot;
    public float GetAccuracy(int hits)
    {
        return arrowsShot > 0 ? (hits / (float)arrowsShot) * 100f : 0f;
    }
}