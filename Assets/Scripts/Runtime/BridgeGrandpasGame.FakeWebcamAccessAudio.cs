using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private void EnsureFakeWebcamAccessAudio()
    {
        if (fakeWebcamAudioSource == null)
        {
            GameObject audioObject = new GameObject("Fake Webcam Access Audio");
            audioObject.transform.SetParent(transform, false);
            fakeWebcamAudioSource = audioObject.AddComponent<AudioSource>();
            fakeWebcamAudioSource.loop = true;
            fakeWebcamAudioSource.playOnAwake = false;
            fakeWebcamAudioSource.spatialBlend = 0f;
            fakeWebcamAudioSource.volume = 0.42f;
            fakeWebcamAudioSource.priority = 42;
            RouteAudioSource(fakeWebcamAudioSource, BridgeAudioBus.Vhs);

            AudioLowPassFilter lowPass = audioObject.AddComponent<AudioLowPassFilter>();
            lowPass.cutoffFrequency = 1150f;
            lowPass.lowpassResonanceQ = 1.55f;

            AudioEchoFilter echo = audioObject.AddComponent<AudioEchoFilter>();
            echo.delay = 118f;
            echo.decayRatio = 0.20f;
            echo.wetMix = 0.16f;
            echo.dryMix = 0.86f;

            AudioDistortionFilter distortion = audioObject.AddComponent<AudioDistortionFilter>();
            distortion.distortionLevel = 0.36f;
        }

        if (fakeWebcamAudioClip == null)
        {
            fakeWebcamAudioClip = CreateFakeWebcamAccessAudioClip();
        }

        fakeWebcamAudioSource.clip = fakeWebcamAudioClip;
    }

    private AudioClip CreateFakeWebcamAccessAudioClip()
    {
        const int sampleRate = 44100;
        const int seconds = 6;
        float[] data = new float[sampleRate * seconds];
        uint seed = 0x9B1227u;
        float drift = 0f;

        for (int i = 0; i < data.Length; i++)
        {
            float t = i / (float)sampleRate;
            seed = seed * 1664525u + 1013904223u;
            float noise = ((seed >> 8) / 16777215f) * 2f - 1f;
            drift = Mathf.Lerp(drift, noise, 0.004f);

            float motor = Mathf.Sin(t * Mathf.PI * 2f * 41.5f) * 0.14f;
            float servo = Mathf.Sin(t * Mathf.PI * 2f * (92f + Mathf.Sin(t * 1.7f) * 18f)) * 0.045f;
            float tick = Mathf.Repeat(t * 5.2f, 1f) < 0.018f
                ? Mathf.Sin(t * Mathf.PI * 2f * 1250f) * 0.24f
                : 0f;
            float breath = Mathf.Sin(t * Mathf.PI * 2f * 0.43f) * 0.05f;
            data[i] = Mathf.Clamp(motor + servo + tick + breath + drift * 0.22f + noise * 0.025f, -0.9f, 0.9f);
        }

        AudioClip clip = AudioClip.Create("FakeWebcamAccessLoop", data.Length, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
