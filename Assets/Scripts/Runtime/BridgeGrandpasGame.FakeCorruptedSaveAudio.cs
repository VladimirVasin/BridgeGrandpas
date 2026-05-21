using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private void EnsureFakeCorruptedHissAudio()
    {
        if (fakeCorruptedHissSource == null)
        {
            GameObject audioObject = new GameObject("Fake Corrupted Save Hiss");
            audioObject.transform.SetParent(transform, false);
            fakeCorruptedHissSource = audioObject.AddComponent<AudioSource>();
            fakeCorruptedHissSource.loop = true;
            fakeCorruptedHissSource.playOnAwake = false;
            fakeCorruptedHissSource.spatialBlend = 0f;
            fakeCorruptedHissSource.volume = 0.58f;
            fakeCorruptedHissSource.priority = 32;
            RouteAudioSource(fakeCorruptedHissSource, BridgeAudioBus.Vhs);

            AudioLowPassFilter lowPass = audioObject.AddComponent<AudioLowPassFilter>();
            lowPass.cutoffFrequency = 1850f;
            lowPass.lowpassResonanceQ = 1.25f;

            AudioDistortionFilter distortion = audioObject.AddComponent<AudioDistortionFilter>();
            distortion.distortionLevel = 0.34f;
        }

        if (fakeCorruptedHissClip == null)
        {
            fakeCorruptedHissClip = CreateFakeCorruptedHissClip();
        }

        fakeCorruptedHissSource.clip = fakeCorruptedHissClip;
        fakeCorruptedHissSource.timeSamples = 0;
        fakeCorruptedHissSource.Play();
    }

    private AudioClip CreateFakeCorruptedHissClip()
    {
        const int sampleRate = 44100;
        const int seconds = 6;
        float[] data = new float[sampleRate * seconds];
        uint seed = 0xA53F19u;
        float rumble = 0f;
        float lastNoise = 0f;

        for (int i = 0; i < data.Length; i++)
        {
            float t = i / (float)sampleRate;
            seed = seed * 1664525u + 1013904223u;
            float white = ((seed >> 8) / 16777215f) * 2f - 1f;
            lastNoise = Mathf.Lerp(lastNoise, white, 0.32f);
            rumble = Mathf.Lerp(rumble, white, 0.0065f);

            float pulse = Mathf.Sin(t * 2f * Mathf.PI * 53.2f) * 0.045f;
            float roomTone = Mathf.Sin(t * 2f * Mathf.PI * 31.7f) * 0.075f;
            float hiss = lastNoise * 0.26f + rumble * 0.44f + pulse + roomTone;
            data[i] = Mathf.Clamp(hiss, -0.92f, 0.92f);
        }

        AudioClip clip = AudioClip.Create("FakeCorruptedSaveHiss", data.Length, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void StopAllAudioExceptCorruptedHiss()
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude);
        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = sources[i];
            if (source == null || source == fakeCorruptedHissSource)
            {
                continue;
            }

            source.Stop();
        }
    }

    private void RestoreFakeCorruptedSaveForShutdown()
    {
        fakeCorruptedSaveActive = false;
        startMenuLoadCorruptedSave = false;
        ClearFakeCorruptedWorldState();
        if (fakeCorruptedHissSource != null)
        {
            Destroy(fakeCorruptedHissSource.gameObject);
            fakeCorruptedHissSource = null;
        }

        fakeCorruptedHissClip = null;
    }
}
