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

    private void OnEnable()
    {
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
    }
}
