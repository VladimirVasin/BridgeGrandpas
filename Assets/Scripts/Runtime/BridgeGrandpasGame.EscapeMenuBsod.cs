using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float EscapeMenuBsodDurationSeconds = 7f;
    private const float EscapeMenuBsodRestartAtSeconds = 5.8f;
    private const int EscapeMenuBsodQrSize = 15;

    private RectTransform escapeMenuBsodRoot;
    private CanvasGroup escapeMenuBsodGroup;
    private Text escapeMenuBsodBodyText;
    private Text escapeMenuBsodProgressText;
    private AudioSource escapeMenuBsodSquealSource;
    private AudioClip escapeMenuBsodSquealClip;
    private AudioDistortionFilter escapeMenuBsodDistortion;
    private bool escapeMenuBsodTriggeredThisOpen;
    private bool escapeMenuBsodActive;
    private bool escapeMenuBsodRestarted;
    private float escapeMenuBsodStartedAt;

    private void ResetEscapeMenuBsodForNewMenu()
    {
        escapeMenuBsodTriggeredThisOpen = false;
        escapeMenuBsodActive = false;
        escapeMenuBsodRestarted = false;
        if (escapeMenuBsodRoot != null)
        {
            escapeMenuBsodRoot.gameObject.SetActive(false);
        }
    }

    private void BeginEscapeMenuBsod()
    {
        if (escapeMenuBsodTriggeredThisOpen || !escapeMenuOpen)
        {
            return;
        }

        EnsureEscapeMenuBsodVisuals();
        escapeMenuBsodTriggeredThisOpen = true;
        escapeMenuBsodActive = true;
        escapeMenuBsodRestarted = false;
        escapeMenuBsodStartedAt = Time.unscaledTime;
        StopEscapeMenuHum();
        StartEscapeMenuBsodSqueal();
        SetStartMenuButtonsInteractable(false);

        if (escapeMenuBsodRoot != null)
        {
            escapeMenuBsodRoot.gameObject.SetActive(true);
            escapeMenuBsodRoot.SetAsLastSibling();
        }

        if (escapeMenuBsodGroup != null)
        {
            escapeMenuBsodGroup.alpha = 1f;
            escapeMenuBsodGroup.blocksRaycasts = true;
            escapeMenuBsodGroup.interactable = true;
        }

        WriteDebugLog("ESCAPE_BSOD", "Fake BSOD started after escape madness reached maximum.");
    }

    private void UpdateEscapeMenuBsod()
    {
        if (!escapeMenuBsodActive)
        {
            return;
        }

        float elapsed = Time.unscaledTime - escapeMenuBsodStartedAt;
        float collect = Mathf.Clamp01(elapsed / EscapeMenuBsodRestartAtSeconds);
        int percent = Mathf.Clamp(Mathf.FloorToInt(collect * 100f), 0, 100);
        if (escapeMenuBsodProgressText != null)
        {
            string suffix = elapsed >= EscapeMenuBsodRestartAtSeconds ? "Restarting observation..." : percent + "% complete";
            escapeMenuBsodProgressText.text = suffix;
        }

        if (escapeMenuBsodBodyText != null)
        {
            string phase = elapsed > 7.2f
                ? "The observer process is not responding. Keeping the bridge in memory."
                : "We're collecting some error info, and then we'll restart the observation for you.";
            escapeMenuBsodBodyText.text =
                "Your observation ran into a problem and needs to restart." +
                "\n\n" + phase +
                "\n\nFor more information about this issue, open the notebook and look for pages that were not written by you." +
                "\n\nSTOP CODE: GRANDFATHER_MULTIPLICATION" +
                "\nWhat failed: observer.dll";
        }

        if (!escapeMenuBsodRestarted && elapsed >= EscapeMenuBsodDurationSeconds)
        {
            escapeMenuBsodRestarted = true;
            CompleteEscapeMenuBsodRestart();
        }
    }

    private void CompleteEscapeMenuBsodRestart()
    {
        EndEscapeMenuBsod();
        DisableEscapeMenuMadnessAfterBsod();
        StopEscapeMenuHum();
        SetStartMenuButtonsInteractable(true);
        HideSaveSlotScreenOnly();
        ApplyStartMenuButtonMode(true);
        ApplyEscapeMenuPostBsodMenuState();
        if (startMenuButtonsGroup != null)
        {
            startMenuButtonsGroup.alpha = 1f;
        }

        if (startMenuContentGroup != null)
        {
            startMenuContentGroup.alpha = 1f;
        }

        if (startMenuLoadingRoot != null)
        {
            startMenuLoadingRoot.gameObject.SetActive(false);
        }

        WriteDebugLog("ESCAPE_BSOD", "Fake BSOD finished. Returned to altered escape menu; madness disabled for current open.");
    }

    private void EndEscapeMenuBsod()
    {
        escapeMenuBsodActive = false;
        escapeMenuBsodRestarted = false;
        StopEscapeMenuBsodSqueal();
        if (escapeMenuBsodRoot != null)
        {
            escapeMenuBsodRoot.gameObject.SetActive(false);
        }

        if (escapeMenuBsodGroup != null)
        {
            escapeMenuBsodGroup.blocksRaycasts = false;
            escapeMenuBsodGroup.interactable = false;
        }
    }

    private void EnsureEscapeMenuBsodVisuals()
    {
        if (escapeMenuBsodRoot != null || startMenuCanvas == null)
        {
            return;
        }

        escapeMenuBsodRoot = CreatePanel("Escape Fake BSOD", startMenuCanvas.transform, new Color(0.0f, 0.31f, 0.74f, 1f));
        escapeMenuBsodRoot.anchorMin = Vector2.zero;
        escapeMenuBsodRoot.anchorMax = Vector2.one;
        escapeMenuBsodRoot.offsetMin = Vector2.zero;
        escapeMenuBsodRoot.offsetMax = Vector2.zero;
        escapeMenuBsodRoot.GetComponent<Image>().raycastTarget = true;
        escapeMenuBsodGroup = escapeMenuBsodRoot.gameObject.AddComponent<CanvasGroup>();
        escapeMenuBsodGroup.alpha = 1f;

        Text face = CreateText("BSOD Face", escapeMenuBsodRoot, 82, FontStyle.Normal, TextAnchor.UpperLeft, Color.white);
        face.text = ":(";
        face.rectTransform.anchorMin = new Vector2(0f, 1f);
        face.rectTransform.anchorMax = new Vector2(0f, 1f);
        face.rectTransform.pivot = new Vector2(0f, 1f);
        face.rectTransform.anchoredPosition = new Vector2(134f, -92f);
        face.rectTransform.sizeDelta = new Vector2(220f, 104f);

        escapeMenuBsodBodyText = CreateText("BSOD Body", escapeMenuBsodRoot, 24, FontStyle.Normal, TextAnchor.UpperLeft, Color.white);
        escapeMenuBsodBodyText.rectTransform.anchorMin = new Vector2(0f, 1f);
        escapeMenuBsodBodyText.rectTransform.anchorMax = new Vector2(0f, 1f);
        escapeMenuBsodBodyText.rectTransform.pivot = new Vector2(0f, 1f);
        escapeMenuBsodBodyText.rectTransform.anchoredPosition = new Vector2(134f, -212f);
        escapeMenuBsodBodyText.rectTransform.sizeDelta = new Vector2(1110f, 420f);

        escapeMenuBsodProgressText = CreateText("BSOD Progress", escapeMenuBsodRoot, 23, FontStyle.Normal, TextAnchor.UpperLeft, Color.white);
        escapeMenuBsodProgressText.rectTransform.anchorMin = new Vector2(0f, 1f);
        escapeMenuBsodProgressText.rectTransform.anchorMax = new Vector2(0f, 1f);
        escapeMenuBsodProgressText.rectTransform.pivot = new Vector2(0f, 1f);
        escapeMenuBsodProgressText.rectTransform.anchoredPosition = new Vector2(134f, -612f);
        escapeMenuBsodProgressText.rectTransform.sizeDelta = new Vector2(520f, 42f);

        CreateEscapeMenuBsodQr(escapeMenuBsodRoot);
        escapeMenuBsodRoot.gameObject.SetActive(false);
    }

    private void CreateEscapeMenuBsodQr(Transform parent)
    {
        RectTransform qrRoot = CreatePanel("BSOD Observer QR", parent, Color.white);
        qrRoot.anchorMin = new Vector2(0f, 0f);
        qrRoot.anchorMax = new Vector2(0f, 0f);
        qrRoot.pivot = new Vector2(0f, 0f);
        qrRoot.anchoredPosition = new Vector2(134f, 92f);
        qrRoot.sizeDelta = new Vector2(118f, 118f);

        float pixelSize = 118f / EscapeMenuBsodQrSize;
        for (int y = 0; y < EscapeMenuBsodQrSize; y++)
        {
            for (int x = 0; x < EscapeMenuBsodQrSize; x++)
            {
                bool filled = IsEscapeMenuBsodQrPixelFilled(x, y);
                RectTransform pixel = CreatePanel("BSOD QR Pixel", qrRoot, filled ? new Color(0f, 0.31f, 0.74f, 1f) : Color.white);
                pixel.anchorMin = new Vector2(0f, 0f);
                pixel.anchorMax = new Vector2(0f, 0f);
                pixel.pivot = new Vector2(0f, 0f);
                pixel.anchoredPosition = new Vector2(x * pixelSize, y * pixelSize);
                pixel.sizeDelta = new Vector2(pixelSize + 0.2f, pixelSize + 0.2f);
            }
        }

        Text caption = CreateText("BSOD QR Caption", parent, 17, FontStyle.Normal, TextAnchor.UpperLeft, Color.white);
        caption.text = "If you call a support person, give them this info:\nSTOP CODE: GRANDFATHER_MULTIPLICATION\nWhat failed: observer.dll";
        caption.rectTransform.anchorMin = new Vector2(0f, 0f);
        caption.rectTransform.anchorMax = new Vector2(0f, 0f);
        caption.rectTransform.pivot = new Vector2(0f, 0f);
        caption.rectTransform.anchoredPosition = new Vector2(288f, 96f);
        caption.rectTransform.sizeDelta = new Vector2(700f, 112f);
    }

    private bool IsEscapeMenuBsodQrPixelFilled(int x, int y)
    {
        if (IsEscapeMenuBsodQrFinder(x, y, 1, 1) ||
            IsEscapeMenuBsodQrFinder(x, y, 9, 1) ||
            IsEscapeMenuBsodQrFinder(x, y, 1, 9))
        {
            return true;
        }

        int hash = (x + 3) * 37 ^ (y + 11) * 53 ^ 0x2D4;
        return Mathf.Repeat(Mathf.Sin(hash * 0.173f) * 93.7f, 1f) > 0.56f;
    }

    private bool IsEscapeMenuBsodQrFinder(int x, int y, int originX, int originY)
    {
        int lx = x - originX;
        int ly = y - originY;
        if (lx < 0 || ly < 0 || lx > 4 || ly > 4)
        {
            return false;
        }

        return lx == 0 || ly == 0 || lx == 4 || ly == 4 || (lx == 2 && ly == 2);
    }

    private void StartEscapeMenuBsodSqueal()
    {
        EnsureEscapeMenuBsodSquealAudio();
        if (escapeMenuBsodSquealSource == null || escapeMenuBsodSquealClip == null)
        {
            return;
        }

        if (escapeMenuBsodDistortion != null)
        {
            escapeMenuBsodDistortion.distortionLevel = 0.92f;
        }

        escapeMenuBsodSquealSource.Stop();
        escapeMenuBsodSquealSource.time = 0f;
        escapeMenuBsodSquealSource.volume = 0.42f;
        escapeMenuBsodSquealSource.Play();
        WriteDebugLog("ESCAPE_BSOD_AUDIO", "Started fake BSOD piercing squeal loop.");
    }

    private void StopEscapeMenuBsodSqueal()
    {
        if (escapeMenuBsodSquealSource != null)
        {
            escapeMenuBsodSquealSource.Stop();
            escapeMenuBsodSquealSource.volume = 0f;
        }

        if (escapeMenuBsodDistortion != null)
        {
            escapeMenuBsodDistortion.distortionLevel = 0f;
        }
    }

    private void EnsureEscapeMenuBsodSquealAudio()
    {
        if (escapeMenuBsodSquealSource != null)
        {
            return;
        }

        GameObject audioObject = new GameObject("Escape BSOD Piercing Squeal");
        audioObject.transform.SetParent(transform, false);
        escapeMenuBsodSquealClip = CreateEscapeMenuBsodSquealClip();
        escapeMenuBsodSquealSource = audioObject.AddComponent<AudioSource>();
        escapeMenuBsodSquealSource.clip = escapeMenuBsodSquealClip;
        escapeMenuBsodSquealSource.loop = true;
        escapeMenuBsodSquealSource.playOnAwake = false;
        escapeMenuBsodSquealSource.spatialBlend = 0f;
        escapeMenuBsodSquealSource.volume = 0f;
        escapeMenuBsodSquealSource.pitch = 1f;
        escapeMenuBsodSquealSource.priority = 0;
        RouteAudioSource(escapeMenuBsodSquealSource, BridgeAudioBus.Vhs);

        escapeMenuBsodDistortion = audioObject.AddComponent<AudioDistortionFilter>();
        escapeMenuBsodDistortion.distortionLevel = 0f;
    }

    private AudioClip CreateEscapeMenuBsodSquealClip()
    {
        const int sampleRate = 44100;
        const float duration = 1.6f;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];
        const float tau = Mathf.PI * 2f;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float gate = Mathf.Sign(Mathf.Sin(tau * 7.5f * t)) * 0.5f + 0.5f;
            float warble = Mathf.Sin(tau * 13.0f * t) * 34f + Mathf.Sin(tau * 29.0f * t) * 11f;
            float toneA = Mathf.Sin(tau * (1860f + warble) * t);
            float toneB = Mathf.Sign(Mathf.Sin(tau * (2410f - warble * 0.6f) * t));
            float toneC = Mathf.Sin(tau * 3170f * t + Mathf.Sin(tau * 3.2f * t) * 1.9f);
            float scrape = Mathf.Sign(Mathf.Sin(tau * 101f * t + toneA * 0.65f)) * 0.16f;
            float sample = toneA * 0.38f + toneB * 0.31f + toneC * 0.18f + scrape;
            sample *= Mathf.Lerp(0.42f, 1f, gate);
            data[i] = Mathf.Clamp(sample * 0.68f, -0.96f, 0.96f);
        }

        AudioClip clip = AudioClip.Create("Generated Escape BSOD Piercing Squeal", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
