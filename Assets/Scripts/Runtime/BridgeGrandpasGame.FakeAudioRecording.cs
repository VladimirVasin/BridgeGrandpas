using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float FakeAudioRecordingDurationSeconds = 5.85f;
    private const float FakeAudioRecordingCaptureSeconds = 2.24f;
    private const int FakeAudioRecordingBarCount = 28;

    private Canvas fakeAudioRecordingCanvas;
    private RectTransform fakeAudioRecordingRoot;
    private CanvasGroup fakeAudioRecordingGroup;
    private RectTransform fakeAudioRecordingPanel;
    private Image fakeAudioRecordingLedImage;
    private Text fakeAudioRecordingTimerText;
    private Text fakeAudioRecordingMessageText;
    private Text fakeAudioRecordingMetaText;
    private RectTransform[] fakeAudioRecordingBarRects;
    private Image[] fakeAudioRecordingBarImages;
    private AudioSource fakeAudioRecordingSource;
    private AudioClip fakeAudioRecordingClip;
    private bool fakeAudioRecordingActive;
    private bool fakeAudioRecordingPlaybackStarted;
    private float fakeAudioRecordingStartedAt;

    private bool UpdateFakeAudioRecording(float deltaTime)
    {
        if (WasFakeAudioRecordingPressed())
        {
            BeginFakeAudioRecording();
        }

        if (!fakeAudioRecordingActive)
        {
            return false;
        }

        if (WasEscapePressed())
        {
            EndFakeAudioRecording(true);
            return true;
        }

        float elapsed = Time.unscaledTime - fakeAudioRecordingStartedAt;
        UpdateFakeAudioRecordingVisuals(elapsed);
        if (elapsed >= FakeAudioRecordingDurationSeconds)
        {
            EndFakeAudioRecording(false);
        }

        return fakeAudioRecordingActive;
    }

    private void BeginFakeAudioRecording()
    {
        if (!gameStarted || escapeMenuOpen || fakeAudioRecordingActive || fakeMicrophoneCheckActive ||
            fakeUnityErrorModalActive || fakeUnityErrorGrandpasHidden || fakeUnityErrorWebcamMenuActive ||
            fakeUnityErrorReturnGlitchActive || fakeCreditsActive || escapeMenuBsodActive)
        {
            return;
        }

        EnsureFakeAudioRecordingVisuals();
        EnsureFakeAudioRecordingAudio();
        fakeAudioRecordingActive = true;
        fakeAudioRecordingPlaybackStarted = false;
        fakeAudioRecordingStartedAt = Time.unscaledTime;

        if (fakeAudioRecordingCanvas != null)
        {
            fakeAudioRecordingCanvas.gameObject.SetActive(true);
        }

        if (fakeAudioRecordingGroup != null)
        {
            fakeAudioRecordingGroup.alpha = 1f;
        }

        if (fakeAudioRecordingSource != null && fakeAudioRecordingClip != null)
        {
            fakeAudioRecordingSource.Stop();
            fakeAudioRecordingSource.time = 0f;
            fakeAudioRecordingSource.Play();
        }

        UpdateFakeAudioRecordingVisuals(0f);
        WriteDebugLog("FAKE_AUDIO_RECORDING", "F5 fake room recording started. Generated audio only, no microphone APIs.");
    }

    private void EndFakeAudioRecording(bool cancelled)
    {
        if (!fakeAudioRecordingActive)
        {
            return;
        }

        fakeAudioRecordingActive = false;
        if (fakeAudioRecordingSource != null)
        {
            fakeAudioRecordingSource.Stop();
        }

        if (fakeAudioRecordingCanvas != null)
        {
            fakeAudioRecordingCanvas.gameObject.SetActive(false);
        }

        WriteDebugLog("FAKE_AUDIO_RECORDING", cancelled ? "Fake room recording cancelled by Escape." : "Fake room recording completed.");
    }

    private void UpdateFakeAudioRecordingVisuals(float elapsed)
    {
        bool playback = elapsed >= FakeAudioRecordingCaptureSeconds;
        if (playback && !fakeAudioRecordingPlaybackStarted)
        {
            fakeAudioRecordingPlaybackStarted = true;
            WriteDebugLog("FAKE_AUDIO_RECORDING", "Fake captured silence playback started.");
        }

        float fadeIn = Mathf.Clamp01(elapsed / 0.10f);
        float fadeOut = Mathf.Clamp01((FakeAudioRecordingDurationSeconds - elapsed) / 0.28f);
        if (fakeAudioRecordingGroup != null)
        {
            fakeAudioRecordingGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Min(fadeIn, fadeOut));
        }

        int tick = Mathf.FloorToInt(Time.unscaledTime * (playback ? 36f : 18f));
        if (fakeAudioRecordingPanel != null)
        {
            float shake = playback ? 1.6f : 0.25f;
            fakeAudioRecordingPanel.anchoredPosition = new Vector2(
                FakeUnityErrorSignedNoise(tick, 441) * shake,
                FakeUnityErrorSignedNoise(tick, 442) * shake);
        }

        if (fakeAudioRecordingLedImage != null)
        {
            float ledAlpha = tick % (playback ? 3 : 8) == 0 ? 0.26f : 1f;
            fakeAudioRecordingLedImage.color = new Color(1f, 0.05f, 0.035f, ledAlpha);
        }

        if (fakeAudioRecordingTimerText != null)
        {
            int seconds = playback ? 2 : Mathf.Clamp(Mathf.FloorToInt(elapsed) + 1, 1, 2);
            fakeAudioRecordingTimerText.text = playback ? "PLAY 00:00:02" : "REC 00:00:" + seconds.ToString("00");
        }

        if (fakeAudioRecordingMessageText != null)
        {
            fakeAudioRecordingMessageText.text = playback ? "Тишина записана." : "Запись звука...";
        }

        if (fakeAudioRecordingMetaText != null)
        {
            fakeAudioRecordingMetaText.text = playback ? "room_silence_23101998.wav / source: underpass" :
                "input level: 0.00 dB / waiting for voice";
        }

        UpdateFakeAudioRecordingBars(tick, playback);
    }

    private void UpdateFakeAudioRecordingBars(int tick, bool playback)
    {
        if (fakeAudioRecordingBarRects == null)
        {
            return;
        }

        for (int i = 0; i < fakeAudioRecordingBarRects.Length; i++)
        {
            float n = FakeAudioRecordingNoise01(tick + i * 13, 17);
            float slow = Mathf.Sin((Time.unscaledTime * 2.2f + i * 0.35f) * Mathf.PI) * 0.5f + 0.5f;
            float height = playback ? Mathf.Lerp(8f, 72f, n * 0.45f + slow * 0.55f) : Mathf.Lerp(5f, 18f, n);
            fakeAudioRecordingBarRects[i].sizeDelta = new Vector2(8f, height);
            if (fakeAudioRecordingBarImages != null && i < fakeAudioRecordingBarImages.Length)
            {
                float alpha = playback ? Mathf.Lerp(0.45f, 0.95f, n) : Mathf.Lerp(0.18f, 0.42f, n);
                fakeAudioRecordingBarImages[i].color = new Color(0.70f, 0.95f, 1f, alpha);
            }
        }
    }

    private void EnsureFakeAudioRecordingVisuals()
    {
        if (fakeAudioRecordingCanvas != null)
        {
            return;
        }

        EnsureUiFont();
        GameObject canvasObject = new GameObject("Fake Audio Recording Overlay", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        fakeAudioRecordingCanvas = canvasObject.GetComponent<Canvas>();
        fakeAudioRecordingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fakeAudioRecordingCanvas.sortingOrder = 371;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        fakeAudioRecordingRoot = CreatePanel("Fake Audio Recording Root", canvasObject.transform, new Color(0f, 0f, 0f, 0.10f));
        fakeAudioRecordingRoot.anchorMin = Vector2.zero;
        fakeAudioRecordingRoot.anchorMax = Vector2.one;
        fakeAudioRecordingRoot.offsetMin = Vector2.zero;
        fakeAudioRecordingRoot.offsetMax = Vector2.zero;
        fakeAudioRecordingRoot.GetComponent<Image>().raycastTarget = true;
        fakeAudioRecordingGroup = fakeAudioRecordingRoot.gameObject.AddComponent<CanvasGroup>();

        CreateFakeAudioRecordingPanel(fakeAudioRecordingRoot);
        fakeAudioRecordingCanvas.gameObject.SetActive(false);
    }

    private void CreateFakeAudioRecordingPanel(Transform parent)
    {
        fakeAudioRecordingPanel = CreatePanel("Fake Audio Recording Panel", parent, new Color(0.015f, 0.018f, 0.020f, 0.96f));
        fakeAudioRecordingPanel.anchorMin = new Vector2(0.5f, 0.5f);
        fakeAudioRecordingPanel.anchorMax = new Vector2(0.5f, 0.5f);
        fakeAudioRecordingPanel.pivot = new Vector2(0.5f, 0.5f);
        fakeAudioRecordingPanel.sizeDelta = new Vector2(620f, 250f);

        Outline outline = fakeAudioRecordingPanel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.92f, 0.04f, 0.03f, 0.52f);
        outline.effectDistance = new Vector2(2f, -2f);

        fakeAudioRecordingLedImage = CreatePanel("Fake Audio Recording LED", fakeAudioRecordingPanel,
            new Color(1f, 0.05f, 0.035f, 1f)).GetComponent<Image>();
        fakeAudioRecordingLedImage.rectTransform.anchorMin = new Vector2(0f, 1f);
        fakeAudioRecordingLedImage.rectTransform.anchorMax = new Vector2(0f, 1f);
        fakeAudioRecordingLedImage.rectTransform.pivot = new Vector2(0f, 1f);
        fakeAudioRecordingLedImage.rectTransform.anchoredPosition = new Vector2(30f, -31f);
        fakeAudioRecordingLedImage.rectTransform.sizeDelta = new Vector2(18f, 18f);
        fakeAudioRecordingLedImage.raycastTarget = false;

        fakeAudioRecordingTimerText = CreateText("Fake Audio Recording Timer", fakeAudioRecordingPanel, 31,
            FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        fakeAudioRecordingTimerText.rectTransform.anchorMin = new Vector2(0f, 1f);
        fakeAudioRecordingTimerText.rectTransform.anchorMax = new Vector2(1f, 1f);
        fakeAudioRecordingTimerText.rectTransform.offsetMin = new Vector2(62f, -58f);
        fakeAudioRecordingTimerText.rectTransform.offsetMax = new Vector2(-28f, -16f);

        fakeAudioRecordingMessageText = CreateText("Fake Audio Recording Message", fakeAudioRecordingPanel, 28,
            FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.92f, 0.96f, 0.94f));
        fakeAudioRecordingMessageText.rectTransform.anchorMin = new Vector2(0f, 0f);
        fakeAudioRecordingMessageText.rectTransform.anchorMax = new Vector2(1f, 1f);
        fakeAudioRecordingMessageText.rectTransform.offsetMin = new Vector2(30f, 80f);
        fakeAudioRecordingMessageText.rectTransform.offsetMax = new Vector2(-30f, -78f);

        RectTransform waveRoot = CreatePanel("Fake Audio Recording Waveform", fakeAudioRecordingPanel, new Color(0f, 0f, 0f, 0f));
        waveRoot.anchorMin = new Vector2(0f, 0f);
        waveRoot.anchorMax = new Vector2(1f, 0f);
        waveRoot.pivot = new Vector2(0.5f, 0f);
        waveRoot.offsetMin = new Vector2(30f, 54f);
        waveRoot.offsetMax = new Vector2(-30f, 126f);

        fakeAudioRecordingBarRects = new RectTransform[FakeAudioRecordingBarCount];
        fakeAudioRecordingBarImages = new Image[FakeAudioRecordingBarCount];
        for (int i = 0; i < FakeAudioRecordingBarCount; i++)
        {
            RectTransform bar = CreatePanel("Fake Audio Recording Bar " + i, waveRoot, new Color(0.70f, 0.95f, 1f, 0.30f));
            bar.anchorMin = new Vector2(i / (float)(FakeAudioRecordingBarCount - 1), 0.5f);
            bar.anchorMax = bar.anchorMin;
            bar.pivot = new Vector2(0.5f, 0.5f);
            bar.sizeDelta = new Vector2(8f, 8f);
            bar.GetComponent<Image>().raycastTarget = false;
            fakeAudioRecordingBarRects[i] = bar;
            fakeAudioRecordingBarImages[i] = bar.GetComponent<Image>();
        }

        fakeAudioRecordingMetaText = CreateText("Fake Audio Recording Meta", fakeAudioRecordingPanel, 13,
            FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.56f, 0.66f, 0.69f));
        fakeAudioRecordingMetaText.rectTransform.anchorMin = new Vector2(0f, 0f);
        fakeAudioRecordingMetaText.rectTransform.anchorMax = new Vector2(1f, 0f);
        fakeAudioRecordingMetaText.rectTransform.offsetMin = new Vector2(30f, 16f);
        fakeAudioRecordingMetaText.rectTransform.offsetMax = new Vector2(-30f, 44f);
    }

    private void EnsureFakeAudioRecordingAudio()
    {
        if (fakeAudioRecordingSource != null)
        {
            return;
        }

        GameObject audioObject = new GameObject("Fake Audio Recording Sound");
        audioObject.transform.SetParent(transform, false);
        fakeAudioRecordingClip = CreateFakeAudioRecordingClip();
        fakeAudioRecordingSource = audioObject.AddComponent<AudioSource>();
        fakeAudioRecordingSource.clip = fakeAudioRecordingClip;
        fakeAudioRecordingSource.loop = false;
        fakeAudioRecordingSource.playOnAwake = false;
        fakeAudioRecordingSource.spatialBlend = 0f;
        fakeAudioRecordingSource.volume = 0.76f;
        fakeAudioRecordingSource.pitch = 1f;
        fakeAudioRecordingSource.priority = 1;
        RouteAudioSource(fakeAudioRecordingSource, BridgeAudioBus.Vhs);
    }

    private AudioClip CreateFakeAudioRecordingClip()
    {
        const int sampleRate = 44100;
        const int channels = 2;
        int samples = Mathf.RoundToInt(sampleRate * FakeAudioRecordingDurationSeconds);
        float[] data = new float[samples * channels];
        const float tau = Mathf.PI * 2f;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float playback = Mathf.Clamp01((t - FakeAudioRecordingCaptureSeconds) / 0.36f);
            float recorderPulse = Mathf.Sin(tau * 1760f * t) * FakeAudioRecordingPulse(t, 0.08f, 0.035f) * 0.25f;
            recorderPulse += Mathf.Sin(tau * 1320f * t) * FakeAudioRecordingPulse(t, 1.08f, 0.045f) * 0.18f;
            float room = Mathf.Sin(tau * 37f * t + Mathf.Sin(tau * 1.2f * t) * 1.7f) * 0.42f;
            room += Mathf.Sin(tau * 53f * t + Mathf.Sin(tau * 0.8f * t) * 2.1f) * 0.26f;
            room += Mathf.Sin(tau * 91f * t) * 0.10f;
            float breath = Mathf.Sin(tau * 0.62f * t) * 0.5f + 0.5f;
            float digitalDirt = FakeAudioRecordingNoise01(i, 91) * 2f - 1f;
            float sample = recorderPulse + playback * (room * Mathf.Lerp(0.65f, 1f, breath) + digitalDirt * 0.045f);
            sample = Mathf.Clamp(sample, -0.96f, 0.96f);
            float stereo = (FakeAudioRecordingNoise01(i + 331, 97) * 2f - 1f) * 0.055f;
            data[i * channels] = Mathf.Clamp(sample * (1f - stereo), -0.96f, 0.96f);
            data[i * channels + 1] = Mathf.Clamp(sample * (1f + stereo), -0.96f, 0.96f);
        }

        AudioClip clip = AudioClip.Create("Generated Fake Room Silence Recording", samples, channels, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private float FakeAudioRecordingPulse(float t, float start, float decay)
    {
        if (t < start)
        {
            return 0f;
        }

        return Mathf.Exp(-(t - start) / Mathf.Max(0.001f, decay));
    }

    private float FakeAudioRecordingNoise01(int seed, int salt)
    {
        return Mathf.Repeat(Mathf.Sin((seed + 1) * 12.9898f + salt * 78.233f) * 43758.5453f, 1f);
    }

    private void RestoreFakeAudioRecordingForShutdown()
    {
        fakeAudioRecordingActive = false;
        if (fakeAudioRecordingSource != null)
        {
            fakeAudioRecordingSource.Stop();
        }
    }

    private bool WasFakeAudioRecordingPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F5);
#endif
    }
}
