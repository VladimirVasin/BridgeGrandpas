using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class BridgeGrandpasHudButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerClickHandler
{
    private const float HudVolumeScale = 0.18f;

    private static AudioClip hoverClip;
    private static AudioClip clickClip;
    private static AudioClip confirmClip;
    private static AudioSource source;
    private static float nextHoverAt;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanPlay())
        {
            return;
        }

        if (Time.unscaledTime < nextHoverAt)
        {
            return;
        }

        nextHoverAt = Time.unscaledTime + 0.045f;
        Play(LoadHoverClip(), 0.42f, Random.Range(0.96f, 1.06f));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanPlay())
        {
            return;
        }

        Play(LoadClickClip(), 0.62f, Random.Range(0.94f, 1.04f));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanPlay())
        {
            return;
        }

        Play(LoadConfirmClip(), 0.34f, Random.Range(0.98f, 1.08f));
    }

    private bool CanPlay()
    {
        Button button = GetComponent<Button>();
        return button == null || button.interactable;
    }

    private static AudioClip LoadHoverClip()
    {
        if (hoverClip == null)
        {
            hoverClip = Resources.Load<AudioClip>("Sfx/HUD/HudHover");
        }

        return hoverClip;
    }

    private static AudioClip LoadClickClip()
    {
        if (clickClip == null)
        {
            clickClip = Resources.Load<AudioClip>("Sfx/HUD/HudClick");
        }

        return clickClip;
    }

    private static AudioClip LoadConfirmClip()
    {
        if (confirmClip == null)
        {
            confirmClip = Resources.Load<AudioClip>("Sfx/HUD/HudConfirm");
        }

        return confirmClip;
    }

    private static void Play(AudioClip clip, float volume, float pitch)
    {
        if (clip == null)
        {
            return;
        }

        EnsureSource();
        source.pitch = pitch;
        source.PlayOneShot(clip, volume * HudVolumeScale);
    }

    private static void EnsureSource()
    {
        if (source != null)
        {
            return;
        }

        GameObject audioObject = new GameObject("HUD ASMR Audio");
        Object.DontDestroyOnLoad(audioObject);
        source = audioObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 0.72f;
        source.priority = 72;

        AudioReverbFilter reverb = audioObject.AddComponent<AudioReverbFilter>();
        reverb.reverbPreset = AudioReverbPreset.User;
        reverb.room = -1600;
        reverb.roomHF = -2400;
        reverb.decayTime = 0.72f;
        reverb.decayHFRatio = 0.38f;
        reverb.reverbLevel = -1450f;

        AudioEchoFilter echo = audioObject.AddComponent<AudioEchoFilter>();
        echo.delay = 62f;
        echo.decayRatio = 0.13f;
        echo.wetMix = 0.07f;
        echo.dryMix = 0.96f;
    }
}
