using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const string AmbienceResourcesPath = "Ambience/GregorQuendel";
    private const string AmbienceAssetPath = "Assets/Gregor Quendel - Free General Ambience Sounds";

    private readonly List<AmbienceLayer> ambienceLayers = new List<AmbienceLayer>();
    private GameObject ambienceRoot;
    private AmbienceLayer cityAmbienceLayer;
    private AudioClip[] thunderAccentClips = Array.Empty<AudioClip>();
    private AudioSource thunderAccentSource;
    private float nextThunderAccentAt;
    private float cityAmbienceBoostUntil;

    private sealed class AmbienceLayer
    {
        public string Name;
        public AudioClip[] Clips;
        public AudioSource SourceA;
        public AudioSource SourceB;
        public bool ActiveA = true;
        public bool Fading;
        public int CurrentIndex = -1;
        public int NextIndex = -1;
        public float BaseVolume;
        public float MinSwitchSeconds;
        public float MaxSwitchSeconds;
        public float FadeSeconds;
        public float FadeElapsed;
        public float NextSwitchAt;
        public float BreathDepth;
        public float BreathSpeed;
        public float Phase;
    }

    private void SetupAmbience()
    {
        if (ambienceRoot != null)
        {
            return;
        }

        AudioClip[] clips = LoadAmbienceClips();
        if (clips.Length == 0)
        {
            Debug.LogWarning("[BridgeGrandpas] No ambience clips found.");
            return;
        }

        ambienceRoot = new GameObject("Under Bridge Ambience");
        ambienceRoot.transform.SetParent(transform, false);
        ambienceLayers.Clear();

        List<AudioClip> city = new List<AudioClip>();
        List<AudioClip> rain = new List<AudioClip>();
        List<AudioClip> wind = new List<AudioClip>();
        List<AudioClip> thunderBeds = new List<AudioClip>();
        List<AudioClip> thunderAccents = new List<AudioClip>();
        ClassifyAmbienceClips(clips, city, rain, wind, thunderBeds, thunderAccents);

        cityAmbienceLayer = AddAmbienceLayer("City Above", city.ToArray(), 0.13f, 45f, 82f, 8f, 0.14f);
        AddAmbienceLayer("Rain On Shelter", rain.ToArray(), 0.11f, 34f, 68f, 7f, 0.12f);
        AddAmbienceLayer("Wind Under Arch", wind.ToArray(), 0.08f, 80f, 130f, 10f, 0.42f);
        AddAmbienceLayer("Distant Thunder Bed", thunderBeds.ToArray(), 0.055f, 95f, 170f, 12f, 0.20f);
        SetupThunderAccents(thunderAccents.ToArray());
        Debug.Log("[BridgeGrandpas] Ambience clips loaded: " + clips.Length);
    }

    private void UpdateAmbience(float deltaTime)
    {
        if (ambienceRoot == null)
        {
            return;
        }

        for (int i = 0; i < ambienceLayers.Count; i++)
        {
            UpdateAmbienceLayer(ambienceLayers[i], deltaTime);
        }

        UpdateThunderAccents();
    }

    private AudioClip[] LoadAmbienceClips()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>(AmbienceResourcesPath);
        if (clips != null && clips.Length > 0)
        {
            SortClips(clips);
            return clips;
        }

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { AmbienceAssetPath });
        List<AudioClip> loaded = new List<AudioClip>();
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null)
            {
                loaded.Add(clip);
            }
        }

        clips = loaded.ToArray();
        SortClips(clips);
        return clips;
#else
        return Array.Empty<AudioClip>();
#endif
    }

    private static void SortClips(AudioClip[] clips)
    {
        Array.Sort(clips, delegate(AudioClip a, AudioClip b)
        {
            return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
        });
    }

    private void ClassifyAmbienceClips(AudioClip[] clips, List<AudioClip> city, List<AudioClip> rain, List<AudioClip> wind, List<AudioClip> thunderBeds, List<AudioClip> thunderAccents)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            string name = clips[i].name.ToLowerInvariant();
            if (name.Contains("without rain"))
            {
                thunderAccents.Add(clips[i]);
            }
            else if (name.Contains("distant thunder") || name.Contains("thunder - lightning - ambience"))
            {
                thunderBeds.Add(clips[i]);
            }
            else if (name.Contains("city ambience"))
            {
                city.Add(clips[i]);
            }
            else if (name.Contains("rain - falling") || name.Contains("dropping"))
            {
                rain.Add(clips[i]);
            }
            else if (name.Contains("wind"))
            {
                wind.Add(clips[i]);
            }
        }
    }

    private AmbienceLayer AddAmbienceLayer(string name, AudioClip[] clips, float volume, float minSwitch, float maxSwitch, float fade, float breath)
    {
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        AmbienceLayer layer = new AmbienceLayer();
        layer.Name = name;
        layer.Clips = clips;
        layer.BaseVolume = volume;
        layer.MinSwitchSeconds = minSwitch;
        layer.MaxSwitchSeconds = maxSwitch;
        layer.FadeSeconds = fade;
        layer.BreathDepth = breath;
        layer.BreathSpeed = UnityEngine.Random.Range(0.035f, 0.085f);
        layer.Phase = UnityEngine.Random.Range(0f, 20f);
        layer.SourceA = CreateAmbienceSource(name + " A", true, 132);
        layer.SourceB = CreateAmbienceSource(name + " B", true, 132);
        layer.CurrentIndex = UnityEngine.Random.Range(0, clips.Length);
        layer.SourceA.clip = clips[layer.CurrentIndex];
        layer.SourceA.volume = 0f;
        layer.SourceA.Play();
        ScheduleLayerSwitch(layer);
        ambienceLayers.Add(layer);
        return layer;
    }

    private AudioSource CreateAmbienceSource(string name, bool loop, int priority)
    {
        GameObject sourceObject = new GameObject(name);
        sourceObject.transform.SetParent(ambienceRoot.transform, false);
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = 0f;
        source.pitch = 1f;
        source.priority = priority;
        source.dopplerLevel = 0f;
        AddAmbienceEffects(sourceObject);
        return source;
    }

    private void AddAmbienceEffects(GameObject audioObject)
    {
        AudioReverbFilter reverb = audioObject.AddComponent<AudioReverbFilter>();
        reverb.reverbPreset = AudioReverbPreset.User;
        reverb.dryLevel = 0f;
        reverb.room = -1350;
        reverb.roomHF = -2100;
        reverb.decayTime = 1.85f;
        reverb.decayHFRatio = 0.48f;
        reverb.reflectionsLevel = -1900f;
        reverb.reverbLevel = -1050f;

        AudioEchoFilter echo = audioObject.AddComponent<AudioEchoFilter>();
        echo.delay = 145f;
        echo.decayRatio = 0.15f;
        echo.wetMix = 0.07f;
        echo.dryMix = 0.94f;
    }

    private void UpdateAmbienceLayer(AmbienceLayer layer, float deltaTime)
    {
        if (layer == null)
        {
            return;
        }

        float volume = CalculateLayerVolume(layer);
        AudioSource current = CurrentAmbienceSource(layer);
        AudioSource next = NextAmbienceSource(layer);
        if (layer.Fading)
        {
            layer.FadeElapsed += deltaTime;
            float fade = Mathf.Clamp01(layer.FadeElapsed / Mathf.Max(0.01f, layer.FadeSeconds));
            current.volume = volume * (1f - fade);
            next.volume = volume * fade;
            if (fade >= 1f)
            {
                current.Stop();
                current.clip = null;
                layer.ActiveA = !layer.ActiveA;
                layer.CurrentIndex = layer.NextIndex;
                layer.NextIndex = -1;
                layer.Fading = false;
            }
        }
        else
        {
            current.volume = Mathf.Lerp(current.volume, volume, deltaTime * 1.35f);
            next.volume = 0f;
        }

        if (!layer.Fading && Time.time >= layer.NextSwitchAt && layer.Clips.Length > 1)
        {
            CrossfadeLayerToNextClip(layer);
        }
    }

    private float CalculateLayerVolume(AmbienceLayer layer)
    {
        float pressure = Mathf.Clamp01(suspicion / MaxSuspicion);
        float volume = layer.BaseVolume;
        if (layer == cityAmbienceLayer)
        {
            volume *= 1f + pressure * 0.35f;
            if (Time.time < cityAmbienceBoostUntil)
            {
                volume *= 1.35f;
            }
        }
        else if (layer.Name.IndexOf("Thunder", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            volume *= 0.65f + pressure * 0.85f;
        }
        else if (layer.Name.IndexOf("Wind", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            volume *= Mathf.Lerp(0.25f, 1f, Mathf.PerlinNoise(Time.time * 0.026f, layer.Phase));
        }

        float breath = 1f + Mathf.Sin(Time.time * layer.BreathSpeed + layer.Phase) * layer.BreathDepth;
        return Mathf.Clamp01(volume * breath);
    }

    private void CrossfadeLayerToNextClip(AmbienceLayer layer)
    {
        AudioSource next = NextAmbienceSource(layer);
        layer.NextIndex = PickNextClipIndex(layer);
        next.clip = layer.Clips[layer.NextIndex];
        next.volume = 0f;
        next.pitch = UnityEngine.Random.Range(0.985f, 1.015f);
        next.Play();
        layer.FadeElapsed = 0f;
        layer.Fading = true;
        ScheduleLayerSwitch(layer);
    }

    private int PickNextClipIndex(AmbienceLayer layer)
    {
        int index = UnityEngine.Random.Range(0, layer.Clips.Length);
        if (layer.Clips.Length > 1 && index == layer.CurrentIndex)
        {
            index = (index + 1) % layer.Clips.Length;
        }

        return index;
    }

    private void ScheduleLayerSwitch(AmbienceLayer layer)
    {
        layer.NextSwitchAt = Time.time + UnityEngine.Random.Range(layer.MinSwitchSeconds, layer.MaxSwitchSeconds);
    }

    private static AudioSource CurrentAmbienceSource(AmbienceLayer layer)
    {
        return layer.ActiveA ? layer.SourceA : layer.SourceB;
    }

    private static AudioSource NextAmbienceSource(AmbienceLayer layer)
    {
        return layer.ActiveA ? layer.SourceB : layer.SourceA;
    }

    private void SetupThunderAccents(AudioClip[] clips)
    {
        thunderAccentClips = clips ?? Array.Empty<AudioClip>();
        if (thunderAccentClips.Length == 0)
        {
            return;
        }

        thunderAccentSource = CreateAmbienceSource("Thunder Accent One Shots", false, 118);
        ScheduleThunderAccent();
    }

    private void UpdateThunderAccents()
    {
        if (thunderAccentSource == null || thunderAccentClips.Length == 0 || Time.time < nextThunderAccentAt)
        {
            return;
        }

        float pressure = Mathf.Clamp01(suspicion / MaxSuspicion);
        AudioClip clip = thunderAccentClips[UnityEngine.Random.Range(0, thunderAccentClips.Length)];
        thunderAccentSource.pitch = UnityEngine.Random.Range(0.94f, 1.03f);
        thunderAccentSource.PlayOneShot(clip, UnityEngine.Random.Range(0.18f, 0.34f) * (1f + pressure * 0.35f));
        ScheduleThunderAccent();
    }

    private void ScheduleThunderAccent()
    {
        float pressure = Mathf.Clamp01(suspicion / MaxSuspicion);
        float minDelay = Mathf.Lerp(240f, 110f, pressure);
        float maxDelay = Mathf.Lerp(420f, 190f, pressure);
        nextThunderAccentAt = Time.time + UnityEngine.Random.Range(minDelay, maxDelay);
    }

    private void BoostCityAmbience(float seconds)
    {
        cityAmbienceBoostUntil = Mathf.Max(cityAmbienceBoostUntil, Time.time + seconds);
    }
}
