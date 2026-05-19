using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const string NatureEssentialsAssetPath = "Assets/Nature - Essentials";
    private static readonly string[] NatureEssentialsResourcePaths =
    {
        "Nature - Essentials",
        "NatureEssentials"
    };

    private AudioSource fireSmallAmbienceSource;
    private AudioSource fireMediumAmbienceSource;
    private AudioSource fireBigAmbienceSource;

    private void SetupNatureEssentialsAmbience()
    {
        AudioClip[] clips = LoadNatureEssentialsClips();
        if (clips.Length == 0)
        {
            return;
        }

        List<AudioClip> rain = new List<AudioClip>();
        List<AudioClip> wind = new List<AudioClip>();
        AudioClip fireSmall = null;
        AudioClip fireMedium = null;
        AudioClip fireBig = null;

        for (int i = 0; i < clips.Length; i++)
        {
            string name = clips[i].name.ToLowerInvariant();
            if (name.Contains("fire") && name.Contains("small"))
            {
                fireSmall = clips[i];
            }
            else if (name.Contains("fire") && name.Contains("medium"))
            {
                fireMedium = clips[i];
            }
            else if (name.Contains("fire") && name.Contains("big"))
            {
                fireBig = clips[i];
            }
            else if (name.Contains("rain"))
            {
                rain.Add(clips[i]);
            }
            else if (name.Contains("wind"))
            {
                wind.Add(clips[i]);
            }
        }

        AddAmbienceLayer("Nature Rain Calm", rain.ToArray(), 0.18f, 58f, 112f, 9f, 0.10f);
        AddAmbienceLayer("Nature Wind Wash", wind.ToArray(), 0.045f, 92f, 158f, 12f, 0.32f);
        SetupFireBarrelAmbience(fireSmall, fireMedium, fireBig);
        Debug.Log("[BridgeGrandpas] Nature ambience clips loaded: " + clips.Length);
    }

    private AudioClip[] LoadNatureEssentialsClips()
    {
        List<AudioClip> loaded = new List<AudioClip>();
        HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < NatureEssentialsResourcePaths.Length; i++)
        {
            AudioClip[] clips = Resources.LoadAll<AudioClip>(NatureEssentialsResourcePaths[i]);
            AddUniqueNatureClips(loaded, seen, clips);
        }

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { NatureEssentialsAssetPath });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null && seen.Add(clip.name))
            {
                loaded.Add(clip);
            }
        }
#endif

        AudioClip[] result = loaded.ToArray();
        SortClips(result);
        return result;
    }

    private static void AddUniqueNatureClips(List<AudioClip> loaded, HashSet<string> seen, AudioClip[] clips)
    {
        if (clips == null)
        {
            return;
        }

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && seen.Add(clips[i].name))
            {
                loaded.Add(clips[i]);
            }
        }
    }

    private void SetupFireBarrelAmbience(AudioClip small, AudioClip medium, AudioClip big)
    {
        fireSmallAmbienceSource = CreateFireAmbienceSource("Fire Barrel Small Ambience", small);
        fireMediumAmbienceSource = CreateFireAmbienceSource("Fire Barrel Medium Ambience", medium);
        fireBigAmbienceSource = CreateFireAmbienceSource("Fire Barrel Big Ambience", big);
    }

    private AudioSource CreateFireAmbienceSource(string name, AudioClip clip)
    {
        if (clip == null)
        {
            return null;
        }

        GameObject sourceObject = new GameObject(name);
        sourceObject.transform.SetParent(ambienceRoot.transform, false);
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0.92f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 2.4f;
        source.maxDistance = 27f;
        source.volume = 0f;
        source.pitch = UnityEngine.Random.Range(0.985f, 1.015f);
        source.priority = 104;
        source.dopplerLevel = 0f;
        source.reverbZoneMix = 0.52f;
        RouteAudioSource(source, BridgeAudioBus.Ambience);
        AddFireAmbienceEffects(sourceObject);
        source.Play();
        return source;
    }

    private void AddFireAmbienceEffects(GameObject audioObject)
    {
        AudioReverbFilter reverb = audioObject.AddComponent<AudioReverbFilter>();
        reverb.reverbPreset = AudioReverbPreset.User;
        reverb.dryLevel = 0f;
        reverb.room = -860;
        reverb.roomHF = -2200;
        reverb.decayTime = 1.42f;
        reverb.decayHFRatio = 0.42f;
        reverb.reflectionsLevel = -1220f;
        reverb.reverbLevel = -760f;
    }

    private void UpdateNatureEssentialsAmbience(float deltaTime)
    {
        UpdateFireBarrelAmbience(deltaTime);
    }

    private void UpdateFireBarrelAmbience(float deltaTime)
    {
        if (fireSmallAmbienceSource == null && fireMediumAmbienceSource == null && fireBigAmbienceSource == null)
        {
            return;
        }

        Vector3 position;
        bool active = TryGetFireBarrelAmbiencePosition(out position);
        SetFireSourcePosition(fireSmallAmbienceSource, position);
        SetFireSourcePosition(fireMediumAmbienceSource, position);
        SetFireSourcePosition(fireBigAmbienceSource, position);

        int level = CurrentFireBarrelLevel();
        float smallWeight;
        float mediumWeight;
        float bigWeight;
        FireAmbienceWeights(level, out smallWeight, out mediumWeight, out bigWeight);

        float blocked = IsFireBarrelAmbienceBlocked() ? 0.38f : 1f;
        float pulse = 0.90f + Mathf.PerlinNoise(Time.time * 1.9f, 14.2f) * 0.18f;
        float master = active ? blocked * pulse : 0f;
        FadeFireSource(fireSmallAmbienceSource, smallWeight * master, 0.28f, deltaTime);
        FadeFireSource(fireMediumAmbienceSource, mediumWeight * master, 0.34f, deltaTime);
        FadeFireSource(fireBigAmbienceSource, bigWeight * master, 0.42f, deltaTime);
    }

    private bool TryGetFireBarrelAmbiencePosition(out Vector3 position)
    {
        Building fire;
        if (buildings.TryGetValue(BuildingType.FireBarrel, out fire) && fire.Built && fire.Root != null)
        {
            position = fire.Root.transform.position + new Vector3(0f, 0.78f, -0.08f);
            return true;
        }

        position = settlementRoot != null ? settlementRoot.TransformPoint(new Vector3(0f, 0.7f, -0.1f)) : Vector3.zero;
        return false;
    }

    private bool IsFireBarrelAmbienceBlocked()
    {
        Building fire;
        return buildings.TryGetValue(BuildingType.FireBarrel, out fire) && fire.Built && fire.IsBlocked;
    }

    private static void FireAmbienceWeights(int level, out float small, out float medium, out float big)
    {
        if (level <= 1)
        {
            small = 1f;
            medium = 0.12f;
            big = 0f;
        }
        else if (level == 2)
        {
            small = 0.38f;
            medium = 1f;
            big = 0.12f;
        }
        else if (level == 3)
        {
            small = 0f;
            medium = 0.62f;
            big = 0.72f;
        }
        else
        {
            small = 0f;
            medium = 0.18f;
            big = Mathf.Clamp01(0.84f + (level - 4) * 0.08f);
        }
    }

    private static void SetFireSourcePosition(AudioSource source, Vector3 position)
    {
        if (source != null)
        {
            source.transform.position = position;
        }
    }

    private static void FadeFireSource(AudioSource source, float weight, float maxVolume, float deltaTime)
    {
        if (source == null)
        {
            return;
        }

        float target = Mathf.Clamp01(weight) * maxVolume;
        source.volume = Mathf.Lerp(source.volume, target, deltaTime * 2.35f);
    }
}
