// 12/17/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

public class EndGameScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel; // Reference to the ending panel
    public TMP_Text totalScoreText; // Text element for total score
    public TMP_Text totalArrowsText; // Text element for total arrows
    public TMP_Text totalTimeText; // Text element for total time
    public Button replayButton; // Replay button

    [Header("XR Setup")]
    public Canvas canvas; // Reference to the canvas
    public float distanceFromPlayer = 2f; // Distance from player
    public float heightOffset = 0f; // Height offset for the panel
    public float holdTimeRequired = 2f; // Time required to hold the button
    public Color normalColor = Color.white; // Normal button color
    public Color holdColor = Color.green; // Color when holding the button

    private InputDevice rightHand; // Reference to the right hand controller
    private float holdTimer = 0f; // Timer for button hold
    private Image replayImage; // Image component of the replay button

    void Start()
    {
        SetupCanvas();
        PositionPanel();
        DisablePlayerMovement();

        panel.SetActive(true);

        replayImage = replayButton.GetComponent<Image>();
        if (replayImage != null)
            replayImage.color = normalColor;

        // Display total arrows
        totalArrowsText.text = $"Total Arrows: {PlayerPrefs.GetInt("TotalHits", 0)}";

        // Display total score
        totalScoreText.text = $"Total Score: {PlayerPrefs.GetInt("TotalScore", 0)}";

        // Display total time
        float totalTime = PlayerPrefs.GetFloat("TotalTime", 0f);
        int minutes = Mathf.FloorToInt(totalTime / 60F);
        int seconds = Mathf.FloorToInt(totalTime % 60F);
        totalTimeText.text = $"Total Time: {minutes:00}:{seconds:00}";
        Debug.Log($"Total Time Displayed: {minutes:00}:{seconds:00}");

        InitializeRightHand();
    }

    void Update()
    {
        if (!rightHand.isValid)
        {
            InitializeRightHand();
            return;
        }

        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed))
        {
            if (pressed)
            {
                holdTimer += Time.deltaTime;

                if (replayImage != null)
                {
                    float t = Mathf.Clamp01(holdTimer / holdTimeRequired);
                    replayImage.color = Color.Lerp(normalColor, holdColor, t);
                }

                if (holdTimer >= holdTimeRequired)
                {
                    ReplayTutorial();
                }
            }
            else
            {
                holdTimer = 0f;
                if (replayImage != null)
                    replayImage.color = normalColor;
            }
        }
    }

    void ReplayTutorial()
    {
        // Reset the timer when replaying the game
        if (LevelTimer.Instance != null)
        {
            LevelTimer.Instance.ResetTimer();
        }

        EnablePlayerMovement();
        SceneManager.LoadScene("Tutorial"); // Replace with your tutorial scene name
    }

    void SetupCanvas()
    {
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
        }
    }

    void PositionPanel()
    {
        if (Camera.main != null)
        {
            Transform cameraTransform = Camera.main.transform;
            Vector3 panelPosition = cameraTransform.position + cameraTransform.forward * distanceFromPlayer;
            panelPosition.y += heightOffset;
            panel.transform.position = panelPosition;
            panel.transform.LookAt(cameraTransform);
            panel.transform.Rotate(0, 180, 0); // Rotate to face the player
        }
    }

    void DisablePlayerMovement()
    {
        // Disable player movement logic here
        Debug.Log("Player movement disabled.");
    }

    void EnablePlayerMovement()
    {
        // Enable player movement logic here
        Debug.Log("Player movement enabled.");
    }

    void InitializeRightHand()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, devices);

        if (devices.Count > 0)
        {
            rightHand = devices[0];
            Debug.Log("Right hand controller initialized.");
        }
        else
        {
            Debug.LogWarning("Right hand controller not found.");
        }
    }
}