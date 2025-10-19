using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRPullInteractable : XRBaseInteractable
{
    public event Action<float> PullActionReleased;
    public event Action<float> PullUpdated;
    public event Action PullStarted;
    public event Action PullEnded;

    [Header("Pull Settings")]
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;
    [SerializeField] private GameObject _notchPoint;

    public float pullAmount { get; private set; } = 0.0f;

    private LineRenderer _lineRenderer;
    private IXRSelectInteractor _pullingInteractor = null;

    protected override void Awake()
    {
        base.Awake();
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetPullInteractor(SelectEnterEventArgs args)
    {
        _pullingInteractor = args.interactorObject;
        PullStarted?.Invoke();
    }

    public void Release()
    {
        PullActionReleased?.Invoke(pullAmount);
        PullEnded?.Invoke();
        _pullingInteractor = null;
        pullAmount = 0f;
        _notchPoint.transform.localPosition = new Vector3(_notchPoint.transform.localPosition.x, _notchPoint.transform.localPosition.y, 0f);
        UpdateStringAndNotch();
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {

            if (isSelected && _pullingInteractor != null)
            {
                Vector3 pullPosition = _pullingInteractor.GetAttachTransform(this).position;
                float previousPull = pullAmount;
                pullAmount = CalculatePull(pullPosition);

                if (previousPull != pullAmount)
                {
                    PullUpdated?.Invoke(pullAmount);
                }

                UpdateStringAndNotch();
                HandleHaptics();
            }
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        SetPullInteractor(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        Release();
    }

    private float CalculatePull(Vector3 pullPosition)
    {
        Vector3 pullDirection = pullPosition - _startPoint.position;
        Vector3 targetDirection = _endPoint.position - _startPoint.position;
        float maxLength = targetDirection.magnitude;

        targetDirection.Normalize();
        float pullValue = Vector3.Dot(pullDirection, targetDirection) / maxLength;
        return Mathf.Clamp(pullValue, 0, 1);
    }

    private void UpdateStringAndNotch()
    {
        Vector3 linePosition = Vector3.forward * pullAmount;
        _notchPoint.transform.localPosition = linePosition;

        if (_lineRenderer != null)
        {
            _lineRenderer.SetPosition(1, linePosition);
        }
    }

    private void HandleHaptics()
    {
        // Haptics implementation depends on your XR setup
    }
}