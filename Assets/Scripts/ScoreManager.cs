using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public event Action<int> OnShotScored;
    public event Action<int> OnRoundScoreChanged;

    private int roundTotal;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(int points)
    {
        roundTotal += points;
        OnShotScored?.Invoke(points);
        OnRoundScoreChanged?.Invoke(roundTotal);
        Debug.Log($"[ScoreManager] +{points} (Round Total: {roundTotal})");
    }

    public int GetRoundTotal() => roundTotal;

    public void ResetRound()
    {
        roundTotal = 0;
        OnRoundScoreChanged?.Invoke(roundTotal);
    }
}
