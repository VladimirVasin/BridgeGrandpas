using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private AudioSource fakeUnityErrorAudioSource;
    private AudioClip fakeUnityErrorPopupClip;
    private AudioClip fakeUnityErrorWebcamClip;
    private AudioClip fakeUnityErrorPhotoClip;
    private AudioDistortionFilter fakeUnityErrorAudioDistortion;

    private void PlayFakeUnityErrorPopupSound()
    {
        EnsureFakeUnityErrorAudio();
        PlayFakeUnityErrorClip(fakeUnityErrorPopupClip, 0.68f, 0.72f);
    }

    private void PlayFakeUnityErrorWebcamSound()
    {
        EnsureFakeUnityErrorAudio();
        PlayFakeUnityErrorClip(fakeUnityErrorWebcamClip, 0.88f, 0.94f);
    }

    private void PlayFakeUnityErrorPhotoSound()
    {
        EnsureFakeUnityErrorAudio();
        PlayFakeUnityErrorClip(fakeUnityErrorPhotoClip, 0.78f, 0.58f);
    }

    private void PlayFakeUnityErrorClip(AudioClip clip, float volume, float distortion)
    {
        if (fakeUnityErrorAudioSource == null || clip == null)
        {
            return;
        }

        if (fakeUnityErrorAudioDistortion != null)
        {
            fakeUnityErrorAudioDistortion.distortionLevel = distortion;
        }

        fakeUnityErrorAudioSource.Stop();
        fakeUnityErrorAudioSource.clip = clip;
        fakeUnityErrorAudioSource.time = 0f;
        fakeUnityErrorAudioSource.volume = volume;
        fakeUnityErrorAudioSource.pitch = 1f;
        fakeUnityErrorAudioSource.Play();
    }

    private void EnsureFakeUnityErrorAudio()
    {
        if (fakeUnityErrorAudioSource != null)
        {
            return;
        }

        GameObject audioObject = new GameObject("Fake Unity Error Audio");
        audioObject.transform.SetParent(transform, false);
        fakeUnityErrorPopupClip = CreateFakeUnityErrorPopupClip();
        fakeUnityErrorWebcamClip = CreateFakeUnityErrorWebcamClip();
        fakeUnityErrorPhotoClip = CreateFakeUnityErrorPhotoClip();

        fakeUnityErrorAudioSource = audioObject.AddComponent<AudioSource>();
        fakeUnityErrorAudioSource.loop = false;
        fakeUnityErrorAudioSource.playOnAwake = false;
        fakeUnityErrorAudioSource.spatialBlend = 0f;
        fakeUnityErrorAudioSource.volume = 0f;
        fakeUnityErrorAudioSource.priority = 0;
        RouteAudioSource(fakeUnityErrorAudioSource, BridgeAudioBus.Vhs);

        fakeUnityErrorAudioDistortion = audioObject.AddComponent<AudioDistortionFilter>();
        fakeUnityErrorAudioDistortion.distortionLevel = 0f;
    }

    private AudioClip CreateFakeUnityErrorPopupClip()
    {
        const int sampleRate = 44100;
        const float duration = 0.46f;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];
        const float tau = Mathf.PI * 2f;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float env = Mathf.Exp(-t / 0.11f);
            float stab = Mathf.Exp(-Mathf.Max(0f, t - 0.035f) / 0.028f);
            float high = Mathf.Sin(tau * (1730f + Mathf.Sin(tau * 19f * t) * 260f) * t) * env;
            float square = Mathf.Sign(Mathf.Sin(tau * 910f * t)) * stab;
            float low = Mathf.Sin(tau * 82f * t) * Mathf.Exp(-t / 0.22f) * 0.38f;
            data[i] = Mathf.Clamp(high * 0.46f + square * 0.32f + low, -0.96f, 0.96f);
        }

        AudioClip clip = AudioClip.Create("Generated Fake Unity Error Popup", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateFakeUnityErrorWebcamClip()
    {
        const int sampleRate = 44100;
        const float duration = 0.82f;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];
        const float tau = Mathf.PI * 2f;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float attack = Mathf.Clamp01(t / 0.012f);
            float env = attack * Mathf.Exp(-t / 0.25f);
            float rip = Mathf.Sign(Mathf.Sin(tau * (2250f + Mathf.Sin(tau * 33f * t) * 420f) * t));
            float blade = Mathf.Sin(tau * 3160f * t + Mathf.Sin(tau * 8f * t) * 3.2f);
            float pulse = Mathf.Sign(Mathf.Sin(tau * 13f * t)) * 0.5f + 0.5f;
            float noise = (Mathf.Repeat(Mathf.Sin((i + 17) * 12.9898f) * 43758.5453f, 1f) * 2f - 1f) * 0.18f;
            float sample = (rip * 0.42f + blade * 0.30f + noise) * Mathf.Lerp(0.55f, 1f, pulse) * env;
            data[i] = Mathf.Clamp(sample, -0.98f, 0.98f);
        }

        AudioClip clip = AudioClip.Create("Generated Fake Unity Webcam Warning", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateFakeUnityErrorPhotoClip()
    {
        const int sampleRate = 44100;
        const float duration = 0.64f;
        const int channels = 2;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples * channels];
        const float tau = Mathf.PI * 2f;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float clickA = Mathf.Exp(-Mathf.Max(0f, t - 0.018f) / 0.030f);
            float clickB = Mathf.Exp(-Mathf.Max(0f, t - 0.092f) / 0.045f);
            float gateA = t < 0.018f ? 0f : 1f;
            float gateB = t < 0.092f ? 0f : 1f;
            float motor = Mathf.Sin(tau * 116f * t) * Mathf.Exp(-t / 0.21f) * 0.22f;
            float snap = Mathf.Sign(Mathf.Sin(tau * 1460f * t)) * clickA * gateA * 0.34f;
            float shutter = Mathf.Sin(tau * 890f * t + 0.2f) * clickB * gateB * 0.24f;
            float digital = Mathf.Sin(tau * 2350f * t) * Mathf.Exp(-Mathf.Max(0f, t - 0.18f) / 0.075f) * (t < 0.18f ? 0f : 0.08f);
            float sample = Mathf.Clamp(motor + snap + shutter + digital, -0.94f, 0.94f);
            float spread = Mathf.Sin(tau * 7f * t) * 0.05f;
            data[i * channels] = Mathf.Clamp(sample * (1f - spread), -0.94f, 0.94f);
            data[i * channels + 1] = Mathf.Clamp(sample * (1f + spread), -0.94f, 0.94f);
        }

        AudioClip clip = AudioClip.Create("Generated Fake Unity Webcam Photo", samples, channels, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
