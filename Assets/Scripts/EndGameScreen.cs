using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

using UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class EndingScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TMP_Text totalScoreText;
    public TMP_Text totalArrowsText;
    public Button replayButton;

    [Header("XR Setup")]
    public Canvas canvas;
    public float distanceFromPlayer = 2f;
    public float heightOffset = 0f;
    public float holdTimeRequired = 2f;
    public Color normalColor = Color.white;
    public Color holdColor = Color.green;

    private InputDevice rightHand;
    private float holdTimer = 0f;
    private Image replayImage;

    // Movement references
    private UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;
    private ContinuousMoveProvider continuousMove;
    private ContinuousTurnProvider continuousTurn;
    private SnapTurnProvider snapTurn;
    private List<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor> teleportAnchors = new List<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor>();
    private List<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea> teleportAreas = new List<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea>();
    private List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor> rayInteractors = new List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
    private List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor> directInteractors = new List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();
    private Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor, LayerMask> originalRaycastMasks = new Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor, LayerMask>();

    void Start()
    {
        SetupCanvas();
        PositionPanel();
        DisablePlayerMovement();

        panel.SetActive(true);

        replayImage = replayButton.GetComponent<Image>();
        if (replayImage != null)
            replayImage.color = normalColor;

        
        // totalArrowsText.text = $"Total Arrows: {ScoreManager.Instance.GetTotalHits()}";
        // totalScoreText.text = $"Total Score: {ScoreManager.Instance.GetTotalScore()}";

        totalArrowsText.text = $"Total Arrows: {PlayerPrefs.GetInt("TotalHits", 0)}";
        totalScoreText.text = $"Total Score: {PlayerPrefs.GetInt("TotalScore", 0)}";

        InitializeRightHand();
    }

    void InitializeRightHand()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        if (devices.Count > 0)
            rightHand = devices[0];
        else
            Debug.LogWarning("Right hand controller not found.");
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

    void SetupCanvas()
    {
        if (canvas == null)
            canvas = panel.GetComponentInParent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.transform.localScale = Vector3.one * 0.002f;
    }

    void PositionPanel()
    {
        Camera xrCamera = Camera.main;
        if (xrCamera != null)
        {
            Vector3 forward = xrCamera.transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 position = xrCamera.transform.position + forward * distanceFromPlayer;
            position.y = xrCamera.transform.position.y + heightOffset;

            canvas.transform.position = position;
            canvas.transform.LookAt(xrCamera.transform);
            canvas.transform.Rotate(0, 180, 0);
        }
    }

    // --- MOVEMENT DISABLING COPIED FROM SafetyWarning ---
    void DisablePlayerMovement()
    {
        GameObject xrOrigin = GameObject.Find("XR Origin") ?? GameObject.Find("XR Origin (XR Rig)") ?? GameObject.Find("XROrigin");

        if (xrOrigin != null)
        {
            teleportationProvider = xrOrigin.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>();
            if (teleportationProvider) teleportationProvider.enabled = false;

            continuousMove = xrOrigin.GetComponentInChildren<ContinuousMoveProvider>();
            if (continuousMove) continuousMove.enabled = false;

            continuousTurn = xrOrigin.GetComponentInChildren<ContinuousTurnProvider>();
            if (continuousTurn) continuousTurn.enabled = false;

            snapTurn = xrOrigin.GetComponentInChildren<SnapTurnProvider>();
            if (snapTurn) snapTurn.enabled = false;
        }

        teleportAnchors.AddRange(FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor>());
        teleportAnchors.ForEach(a => a.enabled = false);

        teleportAreas.AddRange(FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea>());
        teleportAreas.ForEach(a => a.enabled = false);

        rayInteractors.AddRange(FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>());
        foreach (var ray in rayInteractors)
        {
            originalRaycastMasks[ray] = ray.raycastMask;
            ray.enableUIInteraction = false;
        }

        directInteractors.AddRange(FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>());
        directInteractors.ForEach(d => d.enabled = false);
    }

    void EnablePlayerMovement()
    {
        if (teleportationProvider) teleportationProvider.enabled = true;
        if (continuousMove) continuousMove.enabled = true;
        if (continuousTurn) continuousTurn.enabled = true;
        if (snapTurn) snapTurn.enabled = true;

        teleportAnchors.ForEach(a => { if (a) a.enabled = true; });
        teleportAreas.ForEach(a => { if (a) a.enabled = true; });
        rayInteractors.ForEach(r => { if (r) r.enableUIInteraction = true; });
        directInteractors.ForEach(d => { if (d) d.enabled = true; });

        Debug.Log("Movement systems re-enabled");
    }

    void ReplayTutorial()
    {
        ScoreManager.Instance.ResetLevelScore();
        EnablePlayerMovement();
        SceneManager.LoadScene("Tutorial"); // change to your tutorial scene name
    }
}
