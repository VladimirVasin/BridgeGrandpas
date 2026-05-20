using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float ObservationScanRadius = 0.34f;
    private const float ObservationScanSeconds = 2.4f;
    private const float ObservationScanFocusZoomIn = 0.38f;
    private const float ObservationScanFocusSpeed = 8.5f;
    private const float ObservationScanZoomSpeed = 6.2f;
    private enum ObservationLeadState
    {
        Queued,
        CardReady,
        Written
    }

    private readonly List<ObservationLead> observationLeads = new List<ObservationLead>();
    private ObservationLead activeObservationLead;
    private string observationScanHint = "SEARCHING";
    private float observationRecordedUntil;
    private int nextObservationLeadId = 1;
    private bool observationScanCameraActive;
    private ObservationLead observationScanCameraLead;
    private float observationScanReturnZoom;

    private sealed class ObservationLead
    {
        public int Id;
        public string Label;
        public string Text;
        public Transform Target;
        public Vector3 FallbackPosition;
        public float RequiredZoom;
        public float Progress;
        public ObservationLeadState State;
        public RectTransform HighlightRoot;
        public CanvasGroup HighlightGroup;
        public Text HighlightText;
        public readonly List<GameObject> HighlightObjectParts = new List<GameObject>();
    }

    private void ResetObservationLeads()
    {
        ClearObservationHighlights();
        observationLeads.Clear();
        activeObservationLead = null;
        observationScanHint = "SEARCHING";
        observationRecordedUntil = 0f;
        nextObservationLeadId = 1;
        observationScanCameraActive = false;
        observationScanCameraLead = null;
    }

    private void QueueObservationLead(string label, string text, Transform target, Vector3 fallback, float requiredZoom)
    {
        if (string.IsNullOrWhiteSpace(text) || ObservationAlreadyQueuedOrWritten(text))
        {
            return;
        }

        observationLeads.Add(new ObservationLead
        {
            Id = nextObservationLeadId++,
            Label = string.IsNullOrWhiteSpace(label) ? "наблюдение" : label,
            Text = text,
            Target = target != null ? target : DefaultObservationTarget(),
            FallbackPosition = fallback,
            RequiredZoom = Mathf.Clamp01(requiredZoom),
            State = ObservationLeadState.Queued
        });
    }

    private bool ObservationAlreadyQueuedOrWritten(string text)
    {
        for (int i = 0; i < observationLeads.Count; i++)
        {
            if (observationLeads[i].State != ObservationLeadState.Written && observationLeads[i].Text == text)
            {
                return true;
            }
        }

        for (int i = 0; i < notebookObservations.Count; i++)
        {
            if (notebookObservations[i].Text == text)
            {
                return true;
            }
        }

        return ObservationCardAlreadyExists(text);
    }

    private void UpdateObservationScan(float deltaTime)
    {
        if (!vhsModeEnabled || mainCamera == null)
        {
            activeObservationLead = null;
            EndObservationScanCameraAssist();
            return;
        }

        activeObservationLead = FindVisibleObservationLead(out float centerScore, out float zoom01);
        if (activeObservationLead == null)
        {
            EndObservationScanCameraAssist();
            observationScanHint = UnloggedObservationCount() > 0 ? "SEARCHING" : "NO UNSCANNED NOTES";
            return;
        }

        bool zoomEnough = zoom01 >= activeObservationLead.RequiredZoom;
        bool focusHeld = IsObservationFocusHeld();
        if (zoomEnough && focusHeld)
        {
            float speed = (0.62f + centerScore * 0.48f) / ObservationScanSeconds;
            activeObservationLead.Progress = Mathf.Clamp01(activeObservationLead.Progress + deltaTime * speed);
            observationScanHint = "RECORDING";
            vhsTrackingPulse = Mathf.Max(vhsTrackingPulse, 0.32f);
            UpdateObservationScanCameraAssist(activeObservationLead, deltaTime);
        }
        else
        {
            EndObservationScanCameraAssist();
            activeObservationLead.Progress = Mathf.Max(0f, activeObservationLead.Progress - deltaTime * 0.08f);
            observationScanHint = zoomEnough ? "HOLD RMB / SPACE" : "ZOOM CLOSER";
        }

        if (activeObservationLead.Progress >= 1f)
        {
            activeObservationLead.State = ObservationLeadState.CardReady;
            CreateObservationCard(activeObservationLead);
            observationRecordedUntil = Time.time + 2.2f;
            observationScanHint = "CARD PRINTED";
            EndObservationScanCameraAssist();
            activeObservationLead = null;
        }
    }

    private void UpdateObservationScanCameraAssist(ObservationLead lead, float deltaTime)
    {
        if (lead == null || mainCamera == null)
        {
            EndObservationScanCameraAssist();
            return;
        }

        if (!observationScanCameraActive || observationScanCameraLead != lead)
        {
            observationScanCameraActive = true;
            observationScanCameraLead = lead;
            observationScanReturnZoom = Mathf.Clamp(cameraZoomTarget, CameraMinZoom, CameraMaxZoom);
            delayedVhsZoomAmount = 0f;
            delayedVhsZoomWait = 0f;
        }

        Vector3 viewport = mainCamera.WorldToViewportPoint(ObservationLeadPosition(lead));
        if (viewport.z > 0f)
        {
            float viewHeight = mainCamera.orthographicSize * 2f;
            float viewWidth = viewHeight * Mathf.Max(0.01f, mainCamera.aspect);
            Vector2 correction = new Vector2(
                (viewport.x - 0.5f) * viewWidth,
                (viewport.y - 0.5f) * viewHeight * 0.72f);
            float focusT = 1f - Mathf.Exp(-deltaTime * ObservationScanFocusSpeed);
            cameraPanOffset += correction * focusT;
            ClampCameraPan();
        }

        float targetZoom = Mathf.Clamp(observationScanReturnZoom - ObservationScanFocusZoomIn, CameraMinZoom, CameraMaxZoom);
        float zoomT = 1f - Mathf.Exp(-deltaTime * ObservationScanZoomSpeed);
        cameraZoomTarget = Mathf.Lerp(cameraZoomTarget, targetZoom, zoomT);
    }

    private void EndObservationScanCameraAssist()
    {
        if (!observationScanCameraActive)
        {
            return;
        }

        cameraZoomTarget = Mathf.Clamp(observationScanReturnZoom, CameraMinZoom, CameraMaxZoom);
        vhsSavedZoom = cameraZoomTarget;
        delayedVhsZoomAmount = 0f;
        delayedVhsZoomWait = 0f;
        observationScanCameraActive = false;
        observationScanCameraLead = null;
    }

    private ObservationLead FindVisibleObservationLead(out float centerScore, out float zoom01)
    {
        centerScore = 0f;
        zoom01 = Mathf.InverseLerp(CameraMaxZoom, CameraMinZoom, mainCamera.orthographicSize);
        ObservationLead best = null;
        float bestScore = -1f;
        for (int i = 0; i < observationLeads.Count; i++)
        {
            ObservationLead lead = observationLeads[i];
            if (lead.State != ObservationLeadState.Queued)
            {
                continue;
            }

            Vector3 viewport = mainCamera.WorldToViewportPoint(ObservationLeadPosition(lead));
            if (viewport.z <= 0f || viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f)
            {
                continue;
            }

            float distance = Vector2.Distance(new Vector2(viewport.x, viewport.y), new Vector2(0.5f, 0.5f));
            if (distance > ObservationScanRadius)
            {
                continue;
            }

            float score = 1f - distance / ObservationScanRadius;
            if (score > bestScore)
            {
                bestScore = score;
                best = lead;
            }
        }

        centerScore = Mathf.Clamp01(bestScore);
        return best;
    }

    private Vector3 ObservationLeadPosition(ObservationLead lead)
    {
        if (lead.Target != null && lead.Target.gameObject.activeInHierarchy)
        {
            return lead.Target.position + new Vector3(0f, 0.75f, 0f);
        }

        return lead.FallbackPosition + new Vector3(0f, 0.65f, 0f);
    }

    private Vector3 DefaultObservationPosition()
    {
        Building fire;
        if (buildings.TryGetValue(BuildingType.FireBarrel, out fire))
        {
            return fire.Root != null ? fire.Root.transform.position : fire.Position;
        }

        return Vector3.zero;
    }

    private Transform DefaultObservationTarget()
    {
        Building fire;
        if (buildings.TryGetValue(BuildingType.FireBarrel, out fire) && fire.Root != null)
        {
            return fire.Root.transform;
        }

        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            if (grandpa.Root != null && grandpa.Root.activeInHierarchy)
            {
                return grandpa.Root.transform;
            }
        }

        return settlementRoot;
    }

    private Transform EventObservationTarget()
    {
        Building radio;
        if (buildings.TryGetValue(BuildingType.RadioMayak, out radio) && radio.Root != null)
        {
            return radio.Root.transform;
        }

        return DefaultObservationTarget();
    }

    private int UnloggedObservationCount()
    {
        int count = 0;
        for (int i = 0; i < observationLeads.Count; i++)
        {
            if (observationLeads[i].State == ObservationLeadState.Queued)
            {
                count++;
            }
        }

        return count;
    }

    private float VhsObservationProgress01()
    {
        return activeObservationLead == null ? 0f : activeObservationLead.Progress;
    }

    private string BuildVhsObservationReadout()
    {
        if (Time.time < observationRecordedUntil)
        {
            return "CARD PRINTED";
        }

        int unscanned = UnloggedObservationCount();
        int cards = PendingObservationCardCount();
        if (activeObservationLead == null)
        {
            if (unscanned > 0)
            {
                return "UNSCANNED " + unscanned + "  |  TARGETS HIGHLIGHTED";
            }

            return cards > 0 ? "CARDS " + cards + "  |  OPEN NOTEBOOK" : "NO UNSCANNED OBSERVATIONS";
        }

        int progress = Mathf.RoundToInt(activeObservationLead.Progress * 100f);
        return "UNSCANNED " + unscanned + "  |  " + observationScanHint + "  |  " +
            activeObservationLead.Label + " " + progress + "%";
    }

    private bool IsObservationFocusHeld()
    {
#if ENABLE_INPUT_SYSTEM
        bool mouse = Mouse.current != null && Mouse.current.rightButton.isPressed;
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        return mouse || keyboard;
#else
        return Input.GetMouseButton(1) || Input.GetKey(KeyCode.Space);
#endif
    }
}
