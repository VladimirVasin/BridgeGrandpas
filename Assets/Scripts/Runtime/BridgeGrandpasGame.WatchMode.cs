using System.Globalization;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float DayClockRealDurationSeconds = 900f;
    private const int DayClockMinutesPerDay = 24 * 60;

    private bool watchModeEnabled;
    private float watchOpenAmount;
    private float watchTargetOpen;
    private float dayClockElapsedSeconds;

    private void ResetDayClock()
    {
        dayClockElapsedSeconds = 0f;
        UpdateWatchTimeText();
        WriteDebugLog("DAY_CLOCK", "Clock reset to 00:00.");
    }

    private void UpdateDayClock(float deltaTime)
    {
        int previousDay = DayClockIndex(dayClockElapsedSeconds);
        dayClockElapsedSeconds = Mathf.Max(0f, dayClockElapsedSeconds + deltaTime);
        int currentDay = DayClockIndex(dayClockElapsedSeconds);
        if (currentDay > previousDay)
        {
            WriteDebugLog("DAY_ROLLOVER", "Day clock advanced previous=" + previousDay + " current=" + currentDay +
                " elapsed=" + dayClockElapsedSeconds.ToString("0.00", CultureInfo.InvariantCulture));
            EndGrandpaWorkModesAtDayEnd();
        }
    }

    private void UpdateWatchMode(float deltaTime)
    {
        if (WasWatchTogglePressed())
        {
            SetWatchMode(!watchModeEnabled);
        }

        watchTargetOpen = watchModeEnabled ? 1f : 0f;
        watchOpenAmount = Mathf.Lerp(watchOpenAmount, watchTargetOpen, 1f - Mathf.Exp(-deltaTime * 11.5f));
        ApplyWatchUiPose(deltaTime);
    }

    private void SetWatchMode(bool enabled)
    {
        if (enabled && IsAnyDialogOpen())
        {
            return;
        }

        if (enabled)
        {
            if (vhsModeEnabled)
            {
                SetVhsMode(false);
            }

            if (notebookModeEnabled)
            {
                SetNotebookMode(false);
            }

            CloseTray();
        }

        if (watchModeEnabled == enabled)
        {
            ApplyLegacyHudVisibility();
            return;
        }

        watchModeEnabled = enabled;
        if (watchRoot != null && enabled)
        {
            watchRoot.gameObject.SetActive(true);
        }

        ApplyLegacyHudVisibility();
        RefreshInteractionMode();
    }

    private string FormatWatchTime()
    {
        return FormatDayClockElapsedSeconds(dayClockElapsedSeconds);
    }

    private float CurrentDayClockElapsedSeconds()
    {
        return Mathf.Repeat(dayClockElapsedSeconds, DayClockRealDurationSeconds);
    }

    private int DayClockIndex(float elapsedSeconds)
    {
        return Mathf.FloorToInt(Mathf.Max(0f, elapsedSeconds) / DayClockRealDurationSeconds);
    }

    private string FormatDayClockElapsedSeconds(float elapsedSeconds)
    {
        float dayPosition = Mathf.Repeat(elapsedSeconds, DayClockRealDurationSeconds) / DayClockRealDurationSeconds;
        int totalMinutes = Mathf.FloorToInt(dayPosition * DayClockMinutesPerDay) % DayClockMinutesPerDay;
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        return hours.ToString("00") + ":" + minutes.ToString("00");
    }

    private bool WasWatchTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current.tKey.wasPressedThisFrame;
        }
#endif
        return Input.GetKeyDown(KeyCode.T);
    }
}
