using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float EscapeMenuPostBsodContinueDelay = 0.58f;

    private AudioSource escapeMenuPostBsodClickSource;
    private AudioClip escapeMenuPostBsodClickClip;
    private AudioDistortionFilter escapeMenuPostBsodClickDistortion;
    private bool escapeMenuPostBsodMenuActive;
    private bool escapeMenuPostBsodContinueActive;
    private float escapeMenuPostBsodContinueStartedAt;

    private void ApplyEscapeMenuPostBsodMenuState()
    {
        escapeMenuPostBsodMenuActive = true;
        escapeMenuPostBsodContinueActive = false;
        SetEscapeMenuPostBsodTitleVisible(false);
        ApplyEscapeMenuPostBsodButtonMode();
        ResetEscapeMenuPostBsodContinueVisual();
        WriteDebugLog("ESCAPE_POST_BSOD", "Escape menu transformed after fake BSOD.");
    }

    private void ApplyEscapeMenuPostBsodButtonMode()
    {
        ConfigureStartMenuButton(startMenuPrimaryButton, startMenuPrimaryButtonText,
            "Продолжить?", BeginEscapeMenuPostBsodContinue);
        ConfigureStartMenuButton(startMenuLoadButton, startMenuLoadButtonText,
            "Сохранить", BeginStartMenuSave);

        if (startMenuLoadButton != null)
        {
            startMenuLoadButton.interactable = true;
        }
    }

    private void ResetEscapeMenuPostBsodMenuState()
    {
        if (!escapeMenuPostBsodMenuActive && !escapeMenuPostBsodContinueActive)
        {
            SetEscapeMenuPostBsodTitleVisible(true);
            return;
        }

        escapeMenuPostBsodMenuActive = false;
        escapeMenuPostBsodContinueActive = false;
        SetEscapeMenuPostBsodTitleVisible(true);
        ResetEscapeMenuPostBsodContinueVisual();
        StopEscapeMenuPostBsodClickSound();
    }

    private void SetEscapeMenuPostBsodTitleVisible(bool visible)
    {
        if (startMenuTitleRect != null)
        {
            startMenuTitleRect.gameObject.SetActive(visible);
        }

        if (startMenuSubtitleRect != null)
        {
            startMenuSubtitleRect.gameObject.SetActive(visible);
        }
    }

    private void BeginEscapeMenuPostBsodContinue()
    {
        if (!escapeMenuPostBsodMenuActive || escapeMenuPostBsodContinueActive)
        {
            return;
        }

        escapeMenuPostBsodContinueActive = true;
        escapeMenuPostBsodContinueStartedAt = Time.unscaledTime;
        if (startMenuButtonsGroup != null)
        {
            startMenuButtonsGroup.interactable = false;
            startMenuButtonsGroup.blocksRaycasts = false;
        }

        PlayEscapeMenuPostBsodClickSound();
        WriteDebugLog("ESCAPE_POST_BSOD", "Malformed continue button pressed after fake BSOD.");
    }

    private void UpdateEscapeMenuPostBsod(float deltaTime)
    {
        if (!escapeMenuPostBsodMenuActive)
        {
            return;
        }

        SetEscapeMenuPostBsodTitleVisible(false);
        if (!escapeMenuPostBsodContinueActive)
        {
            return;
        }

        float elapsed = Time.unscaledTime - escapeMenuPostBsodContinueStartedAt;
        ApplyEscapeMenuPostBsodContinueVisual(elapsed);
        if (elapsed >= EscapeMenuPostBsodContinueDelay)
        {
            CloseEscapeMenu();
        }
    }

    private void ApplyEscapeMenuPostBsodContinueVisual(float elapsed)
    {
        float progress = Mathf.Clamp01(elapsed / EscapeMenuPostBsodContinueDelay);
        float fade = 1f - progress;
        int tick = Mathf.FloorToInt(Time.unscaledTime * 42f);

        if (startMenuPrimaryButtonText != null)
        {
            string text = "Продолжить?";
            if (tick % 7 == 0)
            {
                text = "НЕ НАДО";
            }
            else if (tick % 5 == 0)
            {
                text = "Продолжить??";
            }

            startMenuPrimaryButtonText.text = text;
            startMenuPrimaryButtonText.color = tick % 2 == 0
                ? new Color(1f, 0.22f, 0.18f, 1f)
                : new Color(0.90f, 0.96f, 1f, 1f);
        }

        RectTransform primaryRect = startMenuPrimaryButton == null ? null : startMenuPrimaryButton.transform as RectTransform;
        if (primaryRect != null)
        {
            float kick = EscapePostBsodNoiseSigned(tick, 17);
            float squash = Mathf.Sin(elapsed * 82f) * 0.055f * fade;
            primaryRect.localScale = new Vector3(1f + squash, 1f - squash * 0.85f, 1f);
            primaryRect.localRotation = Quaternion.Euler(0f, 0f, kick * 6.5f * fade);
        }

        if (startMenuButtonsRect != null)
        {
            Vector2 jitter = new Vector2(EscapePostBsodNoiseSigned(tick, 23) * 16f,
                EscapePostBsodNoiseSigned(tick, 31) * 9f) * fade;
            startMenuButtonsRect.anchoredPosition += jitter;
        }
    }

    private void ResetEscapeMenuPostBsodContinueVisual()
    {
        RectTransform primaryRect = startMenuPrimaryButton == null ? null : startMenuPrimaryButton.transform as RectTransform;
        if (primaryRect != null)
        {
            primaryRect.localScale = Vector3.one;
            primaryRect.localRotation = Quaternion.identity;
        }

        if (startMenuPrimaryButtonText != null)
        {
            startMenuPrimaryButtonText.color = Color.white;
            if (escapeMenuPostBsodMenuActive)
            {
                startMenuPrimaryButtonText.text = "Продолжить?";
            }
        }
    }

    private void PlayEscapeMenuPostBsodClickSound()
    {
        EnsureEscapeMenuPostBsodClickAudio();
        if (escapeMenuPostBsodClickSource == null || escapeMenuPostBsodClickClip == null)
        {
            return;
        }

        if (escapeMenuPostBsodClickDistortion != null)
        {
            escapeMenuPostBsodClickDistortion.distortionLevel = 0.86f;
        }

        escapeMenuPostBsodClickSource.Stop();
        escapeMenuPostBsodClickSource.time = 0f;
        escapeMenuPostBsodClickSource.volume = 0.72f;
        escapeMenuPostBsodClickSource.pitch = 1f;
        escapeMenuPostBsodClickSource.Play();
    }

    private void StopEscapeMenuPostBsodClickSound()
    {
        if (escapeMenuPostBsodClickSource != null)
        {
            escapeMenuPostBsodClickSource.Stop();
            escapeMenuPostBsodClickSource.volume = 0f;
        }

        if (escapeMenuPostBsodClickDistortion != null)
        {
            escapeMenuPostBsodClickDistortion.distortionLevel = 0f;
        }
    }

    private void EnsureEscapeMenuPostBsodClickAudio()
    {
        if (escapeMenuPostBsodClickSource != null)
        {
            return;
        }

        GameObject audioObject = new GameObject("Escape Post BSOD Wrong Click");
        audioObject.transform.SetParent(transform, false);
        escapeMenuPostBsodClickClip = CreateEscapeMenuPostBsodClickClip();
        escapeMenuPostBsodClickSource = audioObject.AddComponent<AudioSource>();
        escapeMenuPostBsodClickSource.clip = escapeMenuPostBsodClickClip;
        escapeMenuPostBsodClickSource.loop = false;
        escapeMenuPostBsodClickSource.playOnAwake = false;
        escapeMenuPostBsodClickSource.spatialBlend = 0f;
        escapeMenuPostBsodClickSource.volume = 0f;
        escapeMenuPostBsodClickSource.priority = 1;
        RouteAudioSource(escapeMenuPostBsodClickSource, BridgeAudioBus.Vhs);

        escapeMenuPostBsodClickDistortion = audioObject.AddComponent<AudioDistortionFilter>();
        escapeMenuPostBsodClickDistortion.distortionLevel = 0f;
    }

    private AudioClip CreateEscapeMenuPostBsodClickClip()
    {
        const int sampleRate = 44100;
        const float duration = 0.38f;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];
        const float tau = Mathf.PI * 2f;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float env = Mathf.Exp(-t / 0.105f);
            float bite = Mathf.Exp(-Mathf.Max(0f, t - 0.028f) / 0.032f);
            float chirp = Mathf.Sin(tau * (2100f + 900f * Mathf.Sin(tau * 19f * t)) * t) * env;
            float scrape = Mathf.Sign(Mathf.Sin(tau * (520f + 180f * Mathf.Sin(tau * 7f * t)) * t)) * bite;
            float thump = Mathf.Sin(tau * 76f * t) * Mathf.Exp(-t / 0.18f) * 0.36f;
            float sample = chirp * 0.42f + scrape * 0.36f + thump;
            data[i] = Mathf.Clamp(sample, -0.94f, 0.94f);
        }

        AudioClip clip = AudioClip.Create("Generated Escape Post BSOD Wrong Click", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private float EscapePostBsodNoiseSigned(int seed, int salt)
    {
        return Mathf.Repeat(Mathf.Sin((seed + 1) * 12.9898f + salt * 78.233f) * 43758.5453f, 1f) * 2f - 1f;
    }
}
