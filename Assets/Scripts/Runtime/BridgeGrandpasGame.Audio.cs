using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float BackgroundMusicStartDelay = 10f;
    private const float BackgroundMusicTrackGap = 10f;

    private AudioSource menuMusicSource;
    private AudioSource musicSource;
    private AudioClip[] ingameMusicClips = Array.Empty<AudioClip>();
    private AudioClip[] footstepClips;
    private int currentMusicClipIndex = -1;
    private int footstepClipCursor;
    private float nextMusicStartAt;
    private bool waitingForNextMusic;

    private void SetupMenuMusic()
    {
        AudioClip clip = FindMusicClip(true);
        if (clip == null)
        {
            Debug.LogWarning("[BridgeGrandpas] No menu music clip found in Resources/Music.");
            return;
        }

        GameObject audioObject = new GameObject("Start Menu Music");
        audioObject.transform.SetParent(transform, false);

        menuMusicSource = audioObject.AddComponent<AudioSource>();
        menuMusicSource.clip = clip;
        menuMusicSource.loop = true;
        menuMusicSource.playOnAwake = false;
        menuMusicSource.spatialBlend = 0f;
        menuMusicSource.volume = 0.34f;
        menuMusicSource.priority = 150;
        menuMusicSource.Play();
        Debug.Log("[BridgeGrandpas] Menu music started: " + clip.name);
    }

    private void StopMenuMusic()
    {
        if (menuMusicSource == null)
        {
            return;
        }

        Destroy(menuMusicSource.gameObject);
        menuMusicSource = null;
    }

    private void SetupBackgroundMusic()
    {
        SetupFootstepClips();

        ingameMusicClips = FindIngameMusicClips();
        if (ingameMusicClips.Length == 0)
        {
            Debug.LogWarning("[BridgeGrandpas] No music clips found in Resources/Music.");
            return;
        }

        GameObject audioObject = new GameObject("Under Bridge Background Music");
        audioObject.transform.SetParent(transform, false);

        musicSource = audioObject.AddComponent<AudioSource>();
        musicSource.loop = false;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 0.16f;
        musicSource.pitch = 1f;
        musicSource.priority = 160;

        AudioReverbFilter reverb = audioObject.AddComponent<AudioReverbFilter>();
        reverb.reverbPreset = AudioReverbPreset.User;
        reverb.dryLevel = 0f;
        reverb.room = -950;
        reverb.roomHF = -1450;
        reverb.decayTime = 2.35f;
        reverb.decayHFRatio = 0.58f;
        reverb.reflectionsLevel = -1350f;
        reverb.reverbLevel = -560f;

        AudioEchoFilter echo = audioObject.AddComponent<AudioEchoFilter>();
        echo.delay = 245f;
        echo.decayRatio = 0.34f;
        echo.wetMix = 0.22f;
        echo.dryMix = 0.86f;

        waitingForNextMusic = true;
        nextMusicStartAt = Time.time + BackgroundMusicStartDelay;
        Debug.Log("[BridgeGrandpas] Background music rotation ready: " + ingameMusicClips.Length + " clips.");
    }

    private void UpdateBackgroundMusic()
    {
        if (musicSource == null || ingameMusicClips == null || ingameMusicClips.Length == 0)
        {
            return;
        }

        if (musicSource.isPlaying)
        {
            waitingForNextMusic = false;
            return;
        }

        if (!waitingForNextMusic)
        {
            waitingForNextMusic = true;
            nextMusicStartAt = Time.time + BackgroundMusicTrackGap;
            return;
        }

        if (Time.time < nextMusicStartAt)
        {
            return;
        }

        PlayNextBackgroundMusicTrack();
    }

    private void PlayNextBackgroundMusicTrack()
    {
        currentMusicClipIndex = PickNextMusicClipIndex();
        AudioClip clip = ingameMusicClips[currentMusicClipIndex];
        musicSource.clip = clip;
        musicSource.pitch = UnityEngine.Random.Range(0.985f, 1.015f);
        musicSource.Play();
        waitingForNextMusic = false;
        Debug.Log("[BridgeGrandpas] Background music started: " + clip.name);
    }

    private int PickNextMusicClipIndex()
    {
        int index = UnityEngine.Random.Range(0, ingameMusicClips.Length);
        if (ingameMusicClips.Length > 1 && index == currentMusicClipIndex)
        {
            index = (index + UnityEngine.Random.Range(1, ingameMusicClips.Length)) % ingameMusicClips.Length;
        }

        return index;
    }

    private AudioClip[] FindIngameMusicClips()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Music");
        if (clips == null || clips.Length == 0)
        {
            return Array.Empty<AudioClip>();
        }

        List<AudioClip> ingame = new List<AudioClip>();
        List<AudioClip> fallback = new List<AudioClip>();
        for (int i = 0; i < clips.Length; i++)
        {
            string name = clips[i].name;
            bool isMenu = name.IndexOf("menu", StringComparison.OrdinalIgnoreCase) >= 0;
            if (isMenu)
            {
                continue;
            }

            if (name.IndexOf("ingame", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ingame.Add(clips[i]);
            }
            else
            {
                fallback.Add(clips[i]);
            }
        }

        return ingame.Count > 0 ? ingame.ToArray() : fallback.ToArray();
    }

    private AudioClip FindMusicClip(bool menu)
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Music");
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        AudioClip preferred = FindNamedMusicClip(clips, menu ? "menu" : "ingame");
        if (preferred != null)
        {
            return preferred;
        }

        AudioClip fallback = null;
        for (int i = 0; i < clips.Length; i++)
        {
            bool isMenu = clips[i].name.IndexOf("menu", StringComparison.OrdinalIgnoreCase) >= 0;
            if (isMenu == menu)
            {
                return clips[i];
            }

            if (fallback == null)
            {
                fallback = clips[i];
            }
        }

        return fallback;
    }

    private AudioClip FindNamedMusicClip(AudioClip[] clips, string token)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return clips[i];
            }
        }

        return null;
    }

    private void SetupFootstepClips()
    {
        footstepClips = Resources.LoadAll<AudioClip>("Footsteps - Essentials/Footsteps_Grass/Footsteps_Grass_Walk");
        if (footstepClips == null || footstepClips.Length == 0)
        {
            footstepClips = Resources.LoadAll<AudioClip>("Footsteps - Essentials");
        }

        if (footstepClips == null || footstepClips.Length == 0)
        {
            Debug.LogWarning("[BridgeGrandpas] No footstep clips found in Resources/Footsteps - Essentials.");
            footstepClips = Array.Empty<AudioClip>();
            return;
        }

        Debug.Log("[BridgeGrandpas] Footstep clips loaded: " + footstepClips.Length);
    }

    private void AttachGrandpaFootstepSource(Grandpa grandpa)
    {
        if (grandpa.Root == null)
        {
            return;
        }

        Transform parent = grandpa.ImportedModelRoot != null ? grandpa.ImportedModelRoot : grandpa.Root.transform;
        GameObject audioObject = new GameObject("Footstep Audio");
        audioObject.transform.SetParent(parent, false);
        audioObject.transform.localPosition = Vector3.zero;

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0.9f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 5.5f;
        source.maxDistance = 34f;
        source.volume = 0.95f;
        source.pitch = 1f;
        source.priority = 96;
        source.dopplerLevel = 0f;
        source.reverbZoneMix = 0.42f;
        AddFootstepEffects(audioObject);
        grandpa.FootstepSource = source;
    }

    private void AddFootstepEffects(GameObject audioObject)
    {
        AudioReverbFilter reverb = audioObject.AddComponent<AudioReverbFilter>();
        reverb.reverbPreset = AudioReverbPreset.User;
        reverb.dryLevel = 0f;
        reverb.room = -650;
        reverb.roomHF = -1850;
        reverb.decayTime = 1.65f;
        reverb.decayHFRatio = 0.46f;
        reverb.reflectionsLevel = -1250f;
        reverb.reverbLevel = -820f;

        AudioEchoFilter echo = audioObject.AddComponent<AudioEchoFilter>();
        echo.delay = 118f;
        echo.decayRatio = 0.24f;
        echo.wetMix = 0.17f;
        echo.dryMix = 0.92f;
    }

    private void EnsureMasterAudioEffects()
    {
        if (mainCamera == null)
        {
            return;
        }

        AudioReverbFilter reverb = mainCamera.GetComponent<AudioReverbFilter>();
        if (reverb == null)
        {
            reverb = mainCamera.gameObject.AddComponent<AudioReverbFilter>();
        }

        reverb.reverbPreset = AudioReverbPreset.User;
        reverb.dryLevel = 0f;
        reverb.room = -520;
        reverb.roomHF = -1650;
        reverb.decayTime = 2.65f;
        reverb.decayHFRatio = 0.54f;
        reverb.reflectionsLevel = -1050f;
        reverb.reverbLevel = -520f;

        AudioEchoFilter echo = mainCamera.GetComponent<AudioEchoFilter>();
        if (echo == null)
        {
            echo = mainCamera.gameObject.AddComponent<AudioEchoFilter>();
        }

        echo.delay = 155f;
        echo.decayRatio = 0.27f;
        echo.wetMix = 0.16f;
        echo.dryMix = 0.95f;
    }

    private void MaybePlayGrandpaFootstep(Grandpa grandpa, bool walking, float animTime)
    {
        if (!walking || footstepClips == null || footstepClips.Length == 0)
        {
            grandpa.FootstepCyclePhase = -1f;
            return;
        }

        float phase = Mathf.Repeat(animTime / (Mathf.PI * 2f), 1f);
        bool stepMoment = CrossedCyclePoint(grandpa.FootstepCyclePhase, phase, 0.25f) ||
            CrossedCyclePoint(grandpa.FootstepCyclePhase, phase, 0.75f);
        grandpa.FootstepCyclePhase = phase;

        if (!stepMoment || Time.time < grandpa.NextFootstepAt)
        {
            return;
        }

        grandpa.NextFootstepAt = Time.time + 0.18f;
        PlayGrandpaFootstep(grandpa);
    }

    private bool CrossedCyclePoint(float previous, float current, float point)
    {
        if (previous < 0f)
        {
            return false;
        }

        if (current >= previous)
        {
            return previous < point && current >= point;
        }

        return previous < point || current >= point;
    }

    private void PlayGrandpaFootstep(Grandpa grandpa)
    {
        if (grandpa.FootstepSource == null)
        {
            AttachGrandpaFootstepSource(grandpa);
        }

        if (grandpa.FootstepSource == null)
        {
            return;
        }

        AudioClip clip = NextFootstepClip();
        grandpa.FootstepSource.transform.position = grandpa.Root.transform.position + Vector3.up * 0.06f;
        grandpa.FootstepSource.pitch = UnityEngine.Random.Range(0.92f, 1.08f);
        grandpa.FootstepSource.volume = UnityEngine.Random.Range(0.82f, 1.0f);
        grandpa.FootstepSource.PlayOneShot(clip, 1.0f);
    }

    private AudioClip NextFootstepClip()
    {
        if (footstepClips.Length == 1)
        {
            return footstepClips[0];
        }

        int step = UnityEngine.Random.Range(1, footstepClips.Length);
        footstepClipCursor = (footstepClipCursor + step) % footstepClips.Length;
        return footstepClips[footstepClipCursor];
    }
}
