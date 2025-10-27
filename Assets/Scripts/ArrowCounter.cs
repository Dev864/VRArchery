using TMPro;
using UnityEngine;

public class ArrowCounter : MonoBehaviour
{
    public static ArrowCounter Instance;

    public TMP_Text arrowCountText;
    private int arrowsShot = 0;

    void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public void IncrementArrowCount()
    {
        arrowsShot++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (arrowCountText != null)
            arrowCountText.text = "Arrows: " + arrowsShot;
    }

    public void ResetCounter()
    {
        arrowsShot = 0;
        UpdateUI();
    }
}