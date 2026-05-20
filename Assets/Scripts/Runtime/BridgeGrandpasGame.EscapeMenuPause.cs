using System.Collections.Generic;
using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float EscapeMenuGameAudioFadeOutSeconds = 0.75f;
    private const float EscapeMenuGameAudioFadeInSeconds = 0.55f;

    private sealed class EscapeMenuAudioState
    {
        public float BaseVolume;
        public bool WasPlaying;
        public bool PausedByEscape;
    }

    private readonly Dictionary<AudioSource, EscapeMenuAudioState> escapeMenuAudioStates =
        new Dictionary<AudioSource, EscapeMenuAudioState>();

    private bool escapeMenuGamePaused;
    private bool escapeMenuAudioFadeActive;
    private float escapeMenuPreviousTimeScale = 1f;
    private float escapeMenuPreviousFixedDeltaTime = 0.02f;
    private float escapeMenuAudioMultiplier = 1f;
    private float escapeMenuAudioTarget = 1f;

    private void PauseGameForEscapeMenu()
    {
        CaptureEscapeMenuAudioSources();
        escapeMenuAudioTarget = 0f;
        escapeMenuAudioFadeActive = true;

        if (!escapeMenuGamePaused)
        {
            escapeMenuPreviousTimeScale = Time.timeScale;
            escapeMenuPreviousFixedDeltaTime = Time.fixedDeltaTime;
            Time.timeScale = 0f;
            escapeMenuGamePaused = true;
        }

        WriteDebugLog("ESCAPE_PAUSE", "Game paused for escape menu. sources=" + escapeMenuAudioStates.Count);
    }

    private void ResumeGameFromEscapeMenu()
    {
        if (escapeMenuGamePaused)
        {
            Time.timeScale = escapeMenuPreviousTimeScale;
            Time.fixedDeltaTime = escapeMenuPreviousFixedDeltaTime;
            escapeMenuGamePaused = false;
        }

        UnpauseEscapeMenuAudioSources();
        escapeMenuAudioTarget = 1f;
        escapeMenuAudioFadeActive = true;
        WriteDebugLog("ESCAPE_PAUSE", "Game resumed from escape menu. restoringSources=" + escapeMenuAudioStates.Count);
    }

    private void LateUpdate()
    {
        UpdateEscapeMenuAudioFade(Time.unscaledDeltaTime);
    }

    private void UpdateEscapeMenuAudioFade(float deltaTime)
    {
        if (!escapeMenuAudioFadeActive)
        {
            return;
        }

        if (escapeMenuAudioTarget <= 0f)
        {
            CaptureEscapeMenuAudioSources();
        }

        float fadeSeconds = escapeMenuAudioTarget <= escapeMenuAudioMultiplier
            ? EscapeMenuGameAudioFadeOutSeconds
            : EscapeMenuGameAudioFadeInSeconds;
        float step = fadeSeconds <= 0.01f ? 1f : deltaTime / fadeSeconds;
        escapeMenuAudioMultiplier = Mathf.MoveTowards(escapeMenuAudioMultiplier, escapeMenuAudioTarget, step);
        ApplyEscapeMenuAudioMultiplier();

        if (escapeMenuAudioTarget <= 0f && escapeMenuAudioMultiplier <= 0.001f)
        {
            PauseEscapeMenuAudioSourcesAtZero();
            return;
        }

        if (escapeMenuAudioTarget >= 1f && escapeMenuAudioMultiplier >= 0.999f)
        {
            RestoreEscapeMenuAudioVolumes();
            escapeMenuAudioFadeActive = false;
        }
    }

    private void CaptureEscapeMenuAudioSources()
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude);
        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = sources[i];
            if (!ShouldDuckEscapeMenuAudioSource(source) || escapeMenuAudioStates.ContainsKey(source))
            {
                continue;
            }

            EscapeMenuAudioState state = new EscapeMenuAudioState();
            state.BaseVolume = source.volume;
            state.WasPlaying = source.isPlaying;
            escapeMenuAudioStates.Add(source, state);
        }
    }

    private bool ShouldDuckEscapeMenuAudioSource(AudioSource source)
    {
        if (source == null || source == escapeMenuHumSource || source == menuMusicSource)
        {
            return false;
        }

        return source.gameObject != null && source.gameObject.activeInHierarchy;
    }

    private void ApplyEscapeMenuAudioMultiplier()
    {
        List<AudioSource> deadSources = null;
        foreach (KeyValuePair<AudioSource, EscapeMenuAudioState> pair in escapeMenuAudioStates)
        {
            if (pair.Key == null)
            {
                if (deadSources == null)
                {
                    deadSources = new List<AudioSource>();
                }

                deadSources.Add(pair.Key);
                continue;
            }

            pair.Key.volume = pair.Value.BaseVolume * escapeMenuAudioMultiplier;
        }

        RemoveDeadEscapeMenuAudioSources(deadSources);
    }

    private void PauseEscapeMenuAudioSourcesAtZero()
    {
        foreach (KeyValuePair<AudioSource, EscapeMenuAudioState> pair in escapeMenuAudioStates)
        {
            if (pair.Key != null && pair.Value.WasPlaying && !pair.Value.PausedByEscape && pair.Key.isPlaying)
            {
                pair.Key.Pause();
                pair.Value.PausedByEscape = true;
            }
        }
    }

    private void UnpauseEscapeMenuAudioSources()
    {
        foreach (KeyValuePair<AudioSource, EscapeMenuAudioState> pair in escapeMenuAudioStates)
        {
            if (pair.Key != null && pair.Value.PausedByEscape)
            {
                pair.Key.UnPause();
                pair.Value.PausedByEscape = false;
            }
        }
    }

    private void RestoreEscapeMenuAudioVolumes()
    {
        foreach (KeyValuePair<AudioSource, EscapeMenuAudioState> pair in escapeMenuAudioStates)
        {
            if (pair.Key != null)
            {
                pair.Key.volume = pair.Value.BaseVolume;
            }
        }

        escapeMenuAudioStates.Clear();
        escapeMenuAudioMultiplier = 1f;
        escapeMenuAudioTarget = 1f;
    }

    private void RemoveDeadEscapeMenuAudioSources(List<AudioSource> deadSources)
    {
        if (deadSources == null)
        {
            return;
        }

        for (int i = 0; i < deadSources.Count; i++)
        {
            escapeMenuAudioStates.Remove(deadSources[i]);
        }
    }

    private void RestoreEscapeMenuPauseForShutdown()
    {
        if (escapeMenuGamePaused)
        {
            Time.timeScale = escapeMenuPreviousTimeScale;
            Time.fixedDeltaTime = escapeMenuPreviousFixedDeltaTime;
            escapeMenuGamePaused = false;
        }

        UnpauseEscapeMenuAudioSources();
        RestoreEscapeMenuAudioVolumes();
        escapeMenuAudioFadeActive = false;
    }
}
