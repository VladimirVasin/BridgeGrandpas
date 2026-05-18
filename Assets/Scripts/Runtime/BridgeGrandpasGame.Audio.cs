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
    private AudioSource menuMusicSource;
    private AudioSource musicSource;
    private AudioClip[] footstepClips;
    private int footstepClipCursor;

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

        AudioClip clip = FindMusicClip(false);
        if (clip == null)
        {
            Debug.LogWarning("[BridgeGrandpas] No music clips found in Resources/Music.");
            return;
        }

        GameObject audioObject = new GameObject("Under Bridge Background Music");
        audioObject.transform.SetParent(transform, false);

        musicSource = audioObject.AddComponent<AudioSource>();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 0.16f;
        musicSource.pitch = 1f;
        musicSource.priority = 160;

        AudioReverbFilter reverb = audioObject.AddComponent<AudioReverbFilter>();
        reverb.reverbPreset = AudioReverbPreset.User;
        reverb.dryLevel = 0f;
        reverb.room = -1200;
        reverb.roomHF = -1800;
        reverb.decayTime = 1.55f;
        reverb.decayHFRatio = 0.52f;
        reverb.reflectionsLevel = -1800f;
        reverb.reverbLevel = -950f;

        AudioEchoFilter echo = audioObject.AddComponent<AudioEchoFilter>();
        echo.delay = 210f;
        echo.decayRatio = 0.24f;
        echo.wetMix = 0.12f;
        echo.dryMix = 0.88f;

        musicSource.Play();
        Debug.Log("[BridgeGrandpas] Background music started: " + clip.name);
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
        reverb.room = -900;
        reverb.roomHF = -2100;
        reverb.decayTime = 1.1f;
        reverb.decayHFRatio = 0.42f;
        reverb.reflectionsLevel = -1600f;
        reverb.reverbLevel = -1150f;

        AudioEchoFilter echo = audioObject.AddComponent<AudioEchoFilter>();
        echo.delay = 95f;
        echo.decayRatio = 0.16f;
        echo.wetMix = 0.10f;
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
