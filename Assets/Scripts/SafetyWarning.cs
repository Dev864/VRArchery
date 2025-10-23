using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SafetyWarning : MonoBehaviour
{
    public GameObject panel;
    public Toggle agreeToggle;
    public Button confirmButton;
    public string agreedKey = "SafetyAgreed";

    void Start()
    {
        // bool previouslyAgreed = PlayerPrefs.GetInt(agreedKey, 0) == 1;
        // if (previouslyAgreed) { panel.SetActive(false); enabled = false; return; }
        panel.SetActive(true);
        confirmButton.interactable = agreeToggle.isOn;
        agreeToggle.onValueChanged.AddListener(OnToggleChanged);
        confirmButton.onClick.AddListener(OnConfirm);
        Time.timeScale = 0f;
    }

    void OnToggleChanged(bool v) => confirmButton.interactable = v;

    void OnConfirm()
    {
        PlayerPrefs.SetInt(agreedKey, 1);
        PlayerPrefs.Save();
        panel.SetActive(false);
        Time.timeScale = 1f;
        enabled = false;
    }
}
