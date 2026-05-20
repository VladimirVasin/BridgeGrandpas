using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float EscapeMenuHumFadeInSeconds = 10f;
    private const float EscapeMenuHumTargetVolume = 0.48f;
    private const float EscapeMenuHumDistortionDelaySeconds = 20f;
    private const float EscapeMenuHumDistortionRampSeconds = 120f;
    private const float EscapeMenuHumMaxDistortion = 0.95f;

    private AudioSource escapeMenuHumSource;
    private AudioClip escapeMenuHumClip;
    private AudioDistortionFilter escapeMenuHumDistortion;
    private float escapeMenuHumStartedAt;

    private void StartEscapeMenuHum()
    {
        EnsureEscapeMenuHumAudio();
        if (escapeMenuHumSource == null || escapeMenuHumClip == null)
        {
            return;
        }

        escapeMenuHumStartedAt = Time.unscaledTime;
        escapeMenuHumSource.volume = 0f;
        if (escapeMenuHumDistortion != null)
        {
            escapeMenuHumDistortion.distortionLevel = 0f;
        }

        escapeMenuHumSource.Stop();
        escapeMenuHumSource.time = 0f;
        escapeMenuHumSource.Play();
        WriteDebugLog("ESCAPE_MENU_HUM", "Started low glitched hum. fadeIn=" + EscapeMenuHumFadeInSeconds +
            " distortionDelay=" + EscapeMenuHumDistortionDelaySeconds + " rampSeconds=" + EscapeMenuHumDistortionRampSeconds);
    }

    private void UpdateEscapeMenuHum()
    {
        if (escapeMenuHumSource == null || !escapeMenuHumSource.isPlaying || escapeMenuHumDistortion == null)
        {
            return;
        }

        float elapsed = Time.unscaledTime - escapeMenuHumStartedAt;
        float fade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / EscapeMenuHumFadeInSeconds));
        escapeMenuHumSource.volume = Mathf.Lerp(0f, EscapeMenuHumTargetVolume, fade);

        float distortionDuration = Mathf.Max(1f, EscapeMenuHumDistortionRampSeconds - EscapeMenuHumDistortionDelaySeconds);
        float distortionAmount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - EscapeMenuHumDistortionDelaySeconds) / distortionDuration));
        escapeMenuHumDistortion.distortionLevel = Mathf.Lerp(0f, EscapeMenuHumMaxDistortion, distortionAmount);
    }

    private void StopEscapeMenuHum()
    {
        if (escapeMenuHumSource != null)
        {
            escapeMenuHumSource.Stop();
            escapeMenuHumSource.volume = 0f;
        }

        if (escapeMenuHumDistortion != null)
        {
            escapeMenuHumDistortion.distortionLevel = 0f;
        }
    }

    private void EnsureEscapeMenuHumAudio()
    {
        if (escapeMenuHumSource != null)
        {
            return;
        }

        GameObject audioObject = new GameObject("Escape Menu Glitched Hum");
        audioObject.transform.SetParent(transform, false);

        escapeMenuHumClip = CreateEscapeMenuHumClip();
        escapeMenuHumSource = audioObject.AddComponent<AudioSource>();
        escapeMenuHumSource.clip = escapeMenuHumClip;
        escapeMenuHumSource.loop = true;
        escapeMenuHumSource.playOnAwake = false;
        escapeMenuHumSource.spatialBlend = 0f;
        escapeMenuHumSource.volume = 0f;
        escapeMenuHumSource.pitch = 0.72f;
        escapeMenuHumSource.priority = 4;
        RouteAudioSource(escapeMenuHumSource, BridgeAudioBus.Ambience);

        AudioReverbFilter reverb = audioObject.AddComponent<AudioReverbFilter>();
        reverb.reverbPreset = AudioReverbPreset.User;
        reverb.dryLevel = -420f;
        reverb.room = -80f;
        reverb.roomHF = -980f;
        reverb.roomLF = 0f;
        reverb.decayTime = 13.5f;
        reverb.decayHFRatio = 0.62f;
        reverb.reflectionsLevel = -260f;
        reverb.reflectionsDelay = 0.075f;
        reverb.reverbLevel = 1120f;
        reverb.reverbDelay = 0.092f;
        reverb.diffusion = 96f;
        reverb.density = 100f;

        AudioEchoFilter echo = audioObject.AddComponent<AudioEchoFilter>();
        echo.delay = 468f;
        echo.decayRatio = 0.78f;
        echo.wetMix = 0.82f;
        echo.dryMix = 0.46f;

        escapeMenuHumDistortion = audioObject.AddComponent<AudioDistortionFilter>();
        escapeMenuHumDistortion.distortionLevel = 0f;
    }

    private AudioClip CreateEscapeMenuHumClip()
    {
        const int sampleRate = 44100;
        const float duration = 8f;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];
        const float tau = Mathf.PI * 2f;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float low = Mathf.Sin(tau * 31.25f * t) * 0.48f +
                Mathf.Sin(tau * 43.75f * t + 1.20f) * 0.22f +
                Mathf.Sin(tau * 56.25f * t + 2.10f) * 0.14f;
            float wobble = 0.72f +
                Mathf.Sin(tau * 0.25f * t + 0.60f) * 0.14f +
                Mathf.Sin(tau * 0.625f * t + 2.00f) * 0.08f;
            float gateA = Mathf.Pow(Mathf.Clamp01(Mathf.Sin(tau * 0.875f * t + 0.40f) * 0.5f + 0.5f), 18f);
            float gateB = Mathf.Pow(Mathf.Clamp01(Mathf.Sin(tau * 1.625f * t + 2.70f) * 0.5f + 0.5f), 24f);
            float dropout = Mathf.Clamp01(1f - gateA * 0.34f - gateB * 0.22f);
            float digital = Mathf.Sign(Mathf.Sin(tau * 353.125f * t)) *
                Mathf.Sin(tau * 101.25f * t + 0.80f) * (gateA * 0.038f + gateB * 0.024f);
            float subBend = Mathf.Sin(tau * 18.75f * t + Mathf.Sin(tau * 0.125f * t) * 0.35f) * 0.10f;
            float sample = (low + subBend) * wobble * dropout + digital;
            data[i] = Mathf.Clamp(sample * 0.72f, -0.92f, 0.92f);
        }

        AudioClip clip = AudioClip.Create("Generated Escape Menu Glitched Hum", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
