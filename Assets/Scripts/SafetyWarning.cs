using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class SafetyWarning : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public Button confirmButton;
    public string agreedKey = "SafetyAgreed";

    [Header("XR Setup")]
    public Canvas safetyCanvas;
    public float distanceFromPlayer = 2f;
    public float heightOffset = 0f;
    public float holdTimeRequired = 2f;
    public Color normalColor = Color.white;
    public Color holdColor = Color.green;

    private bool isHoldingTrigger = false;
    private float holdTimer = 0f;
    private Image confirmImage;
    private InputDevice rightHand;

    private TeleportationProvider teleportationProvider;
    private List<TeleportationAnchor> teleportAnchors = new List<TeleportationAnchor>();
    private List<TeleportationArea> teleportAreas = new List<TeleportationArea>();
    private List<XRRayInteractor> rayInteractors = new List<XRRayInteractor>();
    private List<XRDirectInteractor> directInteractors = new List<XRDirectInteractor>();
    private ContinuousMoveProvider continuousMove;
    private ContinuousTurnProvider continuousTurn;
    private SnapTurnProvider snapTurn;
    private Dictionary<XRRayInteractor, LayerMask> originalRaycastMasks = new Dictionary<XRRayInteractor, LayerMask>();

    void Start()
    {
        SetupCanvasForXR();
        PositionPanelInFrontOfPlayer();
        DisablePlayerMovement();

        panel.SetActive(true);
        confirmButton.interactable = true;
        confirmImage = confirmButton.GetComponent<Image>();
        if (confirmImage != null)
            confirmImage.color = normalColor;

        InitializeRightHandDevice();
    }

    void InitializeRightHandDevice()
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
            InitializeRightHandDevice();
            return;
        }

        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed))
        {
            if (triggerPressed)
            {
                if (!isHoldingTrigger)
                {
                    isHoldingTrigger = true;
                    holdTimer = 0f;
                }

                holdTimer += Time.deltaTime;

                if (confirmImage != null)
                {
                    float t = Mathf.Clamp01(holdTimer / holdTimeRequired);
                    confirmImage.color = Color.Lerp(normalColor, holdColor, t);
                }

                if (holdTimer >= holdTimeRequired)
                {
                    OnConfirm();
                }
            }
            else
            {
                if (isHoldingTrigger)
                {
                    isHoldingTrigger = false;
                    holdTimer = 0f;
                    if (confirmImage != null)
                        confirmImage.color = normalColor;
                }
            }
        }
    }

    void SetupCanvasForXR()
    {
        if (safetyCanvas == null)
            safetyCanvas = panel.GetComponentInParent<Canvas>();

        safetyCanvas.renderMode = RenderMode.WorldSpace;
        safetyCanvas.transform.localScale = Vector3.one * 0.002f;
    }

    void PositionPanelInFrontOfPlayer()
    {
        Camera xrCamera = Camera.main;
        if (xrCamera == null)
        {
            GameObject xrOrigin = GameObject.Find("XR Origin") ??
                                  GameObject.Find("XR Origin (XR Rig)") ??
                                  GameObject.Find("XROrigin");
            if (xrOrigin != null)
                xrCamera = xrOrigin.GetComponentInChildren<Camera>();
        }

        if (xrCamera != null)
        {
            Vector3 forward = xrCamera.transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 position = xrCamera.transform.position + forward * distanceFromPlayer;
            position.y = xrCamera.transform.position.y + heightOffset;

            safetyCanvas.transform.position = position;
            safetyCanvas.transform.LookAt(xrCamera.transform);
            safetyCanvas.transform.Rotate(0, 180, 0);
        }
    }

    void DisablePlayerMovement()
    {
        GameObject xrOrigin = GameObject.Find("XR Origin") ??
                             GameObject.Find("XR Origin (XR Rig)") ??
                             GameObject.Find("XROrigin");

        if (xrOrigin != null)
        {
            teleportationProvider = xrOrigin.GetComponentInChildren<TeleportationProvider>();
            if (teleportationProvider) teleportationProvider.enabled = false;

            continuousMove = xrOrigin.GetComponentInChildren<ContinuousMoveProvider>();
            if (continuousMove) continuousMove.enabled = false;

            continuousTurn = xrOrigin.GetComponentInChildren<ContinuousTurnProvider>();
            if (continuousTurn) continuousTurn.enabled = false;

            snapTurn = xrOrigin.GetComponentInChildren<SnapTurnProvider>();
            if (snapTurn) snapTurn.enabled = false;
        }

        teleportAnchors.AddRange(FindObjectsOfType<TeleportationAnchor>());
        teleportAnchors.ForEach(a => a.enabled = false);

        teleportAreas.AddRange(FindObjectsOfType<TeleportationArea>());
        teleportAreas.ForEach(a => a.enabled = false);

        rayInteractors.AddRange(FindObjectsOfType<XRRayInteractor>());
        foreach (var ray in rayInteractors)
        {
            originalRaycastMasks[ray] = ray.raycastMask;
            ray.enableUIInteraction = false;
        }

        directInteractors.AddRange(FindObjectsOfType<XRDirectInteractor>());
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

    void OnConfirm()
    {
        PlayerPrefs.SetInt(agreedKey, 1);
        PlayerPrefs.Save();
        EnablePlayerMovement();
        panel.SetActive(false);
        enabled = false;
        Debug.Log("Safety warning acknowledged via trigger hold!");
    }

    void OnDestroy()
    {
        EnablePlayerMovement();
    }
}
