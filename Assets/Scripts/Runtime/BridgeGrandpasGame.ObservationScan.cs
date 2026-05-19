using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float ObservationScanRadius = 0.34f;
    private const float ObservationScanSeconds = 2.4f;
    private readonly List<ObservationLead> observationLeads = new List<ObservationLead>();
    private ObservationLead activeObservationLead;
    private string observationScanHint = "SEARCHING";
    private float observationRecordedUntil;

    private sealed class ObservationLead
    {
        public string Label;
        public string Text;
        public Transform Target;
        public Vector3 FallbackPosition;
        public float RequiredZoom;
        public float Progress;
        public bool Discovered;
    }

    private void ResetObservationLeads()
    {
        observationLeads.Clear();
        activeObservationLead = null;
        observationScanHint = "SEARCHING";
        observationRecordedUntil = 0f;
    }

    private void QueueObservationLead(string label, string text, Transform target, Vector3 fallback, float requiredZoom)
    {
        if (string.IsNullOrWhiteSpace(text) || ObservationAlreadyQueuedOrWritten(text))
        {
            return;
        }

        observationLeads.Add(new ObservationLead
        {
            Label = string.IsNullOrWhiteSpace(label) ? "наблюдение" : label,
            Text = text,
            Target = target,
            FallbackPosition = fallback,
            RequiredZoom = Mathf.Clamp01(requiredZoom)
        });
    }

    private bool ObservationAlreadyQueuedOrWritten(string text)
    {
        for (int i = 0; i < observationLeads.Count; i++)
        {
            if (!observationLeads[i].Discovered && observationLeads[i].Text == text)
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

        return false;
    }

    private void UpdateObservationScan(float deltaTime)
    {
        if (!vhsModeEnabled || mainCamera == null)
        {
            activeObservationLead = null;
            return;
        }

        activeObservationLead = FindVisibleObservationLead(out float centerScore, out float zoom01);
        if (activeObservationLead == null)
        {
            observationScanHint = UnloggedObservationCount() > 0 ? "SEARCHING" : "NO UNLOGGED NOTES";
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
        }
        else
        {
            activeObservationLead.Progress = Mathf.Max(0f, activeObservationLead.Progress - deltaTime * 0.08f);
            observationScanHint = zoomEnough ? "HOLD LMB / SPACE" : "ZOOM CLOSER";
        }

        if (activeObservationLead.Progress >= 1f)
        {
            activeObservationLead.Discovered = true;
            AddNotebookObservation(activeObservationLead.Text);
            observationRecordedUntil = Time.time + 2.2f;
            observationScanHint = "RECORDED TO NOTEBOOK";
            activeObservationLead = null;
        }
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
            if (lead.Discovered)
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

    private int UnloggedObservationCount()
    {
        int count = 0;
        for (int i = 0; i < observationLeads.Count; i++)
        {
            if (!observationLeads[i].Discovered)
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
            return "RECORDED TO NOTEBOOK";
        }

        int unlogged = UnloggedObservationCount();
        if (activeObservationLead == null)
        {
            return unlogged > 0 ? "UNLOGGED " + unlogged + "  |  SEARCH" : "NO UNLOGGED OBSERVATIONS";
        }

        int progress = Mathf.RoundToInt(activeObservationLead.Progress * 100f);
        return "UNLOGGED " + unlogged + "  |  " + observationScanHint + "  |  " +
            activeObservationLead.Label + " " + progress + "%";
    }

    private bool IsObservationFocusHeld()
    {
#if ENABLE_INPUT_SYSTEM
        bool mouse = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        return mouse || keyboard;
#else
        return Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
#endif
    }
}
