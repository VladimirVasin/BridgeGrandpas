using System;
using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private AudioClip[] CreateProceduralAmbienceFallbackClips()
    {
        return new[]
        {
            CreateNoiseAmbienceClip("City Ambience Procedural Bridge Traffic", 11f, 0.18f, 0.42f, 0.015f, 3),
            CreateNoiseAmbienceClip("Rain - Dropping Procedural Underpass", 9f, 0.28f, 0.92f, 0.018f, 7),
            CreateNoiseAmbienceClip("Wind Procedural Under Arch", 13f, 0.22f, 0.18f, 0.030f, 11),
            CreateNoiseAmbienceClip("Rain - Distant Thunder Procedural Bed", 15f, 0.14f, 0.09f, 0.045f, 17),
            CreateNoiseAmbienceClip("Thunder - Lightning - I - Without rain Procedural Accent", 4f, 0.30f, 0.06f, 0.070f, 23)
        };
    }

    private AudioClip CreateNoiseAmbienceClip(string name, float seconds, float volume, float grain, float drift, int seed)
    {
        const int sampleRate = 22050;
        int sampleCount = Mathf.Max(sampleRate, Mathf.RoundToInt(sampleRate * seconds));
        float[] data = new float[sampleCount];
        System.Random generator = new System.Random(seed);
        float low = 0f;
        float mid = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float white = (float)(generator.NextDouble() * 2.0 - 1.0);
            low = Mathf.Lerp(low, white, drift);
            mid = Mathf.Lerp(mid, white, grain);
            float pulse = Mathf.Sin(t * 0.37f + seed) * 0.18f + Mathf.Sin(t * 0.11f + seed * 0.3f) * 0.12f;
            float fade = Mathf.SmoothStep(0f, 1f, Mathf.Min(i, sampleCount - 1 - i) / (sampleRate * 0.45f));
            data[i] = Mathf.Clamp((low * 0.72f + mid * 0.22f + pulse) * volume * fade, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
