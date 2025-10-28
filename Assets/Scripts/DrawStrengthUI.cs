using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class DrawStrengthUI : MonoBehaviour
{
    public XRPullInteractable bowPull;  // assign the bow’s XRPullInteractable
    public Slider strengthSlider;       // assign the UI slider
    public TMP_Text strengthLabel;      // assign the strength text label

    [Header("Color Settings")]
    public Image fillImage;
    public Color minColor = Color.green;
    public Color midColor = Color.yellow;
    public Color maxColor = Color.red;

    private void OnEnable()
    {
        // Hide the slider and label at start
        if (strengthSlider)
            strengthSlider.gameObject.SetActive(false);
        if (strengthLabel)
            strengthLabel.gameObject.SetActive(false);

        StartCoroutine(WaitForSafetyAgreement());
    }

    private IEnumerator WaitForSafetyAgreement()
    {
        yield return new WaitUntil(() => SafetyWarning.safetyAgreed);

        if (strengthSlider)
            strengthSlider.gameObject.SetActive(true);
        if (strengthLabel)
            strengthLabel.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!SafetyWarning.safetyAgreed)
            return;

        if (bowPull == null || strengthSlider == null)
            return;

        strengthSlider.value = bowPull.pullAmount;

        if (fillImage != null)
            fillImage.color = EvaluateStrengthColor(bowPull.pullAmount);
    }

    private Color EvaluateStrengthColor(float value)
    {
        // Blend green → yellow → red smoothly
        if (value < 0.5f)
        {
            // from green to yellow
            return Color.Lerp(minColor, midColor, value * 2f);
        }
        else
        {
            // from yellow to red
            return Color.Lerp(midColor, maxColor, (value - 0.5f) * 2f);
        }
    }
}