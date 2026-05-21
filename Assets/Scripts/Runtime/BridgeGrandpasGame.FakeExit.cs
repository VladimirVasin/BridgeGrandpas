using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float FakeExitDurationSeconds = 9.1f;
    private const int FakeExitGlitchBandCount = 10;

    private Canvas fakeExitCanvas;
    private CanvasGroup fakeExitGroup;
    private Text fakeExitStatusText;
    private Text fakeExitSmallText;
    private Image fakeExitVignetteImage;
    private RectTransform[] fakeExitBandRects;
    private Image[] fakeExitBandImages;
    private AudioSource fakeExitAudioSource;
    private AudioClip fakeExitAudioClip;
    private AudioDistortionFilter fakeExitDistortion;
    private bool fakeExitActive;
    private bool fakeExitStartedFromEscapeMenu;
    private float fakeExitStartedAt;
    private int fakeExitStage = -1;

    private bool UpdateFakeExitSequence(float deltaTime)
    {
        if (!fakeExitActive)
        {
            return false;
        }

        float elapsed = Time.unscaledTime - fakeExitStartedAt;
        UpdateFakeExitVisuals(elapsed);
        UpdateFakeExitAudio(elapsed);
        if (elapsed >= FakeExitDurationSeconds)
        {
            EndFakeExitSequence();
        }

        return true;
    }

    private void BeginFakeExitSequence()
    {
        if (fakeExitActive)
        {
            return;
        }

        EnsureFakeExitVisuals();
        EnsureFakeExitAudio();
        fakeExitActive = true;
        fakeExitStartedFromEscapeMenu = escapeMenuOpen;
        fakeExitStartedAt = Time.unscaledTime;
        fakeExitStage = -1;

        HideSaveSlotScreenOnly();
        SetStartMenuButtonsInteractable(false);
        StopEscapeMenuHum();

        if (fakeExitCanvas != null)
        {
            fakeExitCanvas.gameObject.SetActive(true);
        }

        if (fakeExitGroup != null)
        {
            fakeExitGroup.alpha = 1f;
        }

        if (fakeExitAudioSource != null && fakeExitAudioClip != null)
        {
            fakeExitAudioSource.Stop();
            fakeExitAudioSource.time = 0f;
            fakeExitAudioSource.volume = 0.82f;
            fakeExitAudioSource.Play();
        }

        UpdateFakeExitVisuals(0f);
        WriteDebugLog("FAKE_EXIT", "Fake exit sequence started. fromEscapeMenu=" + fakeExitStartedFromEscapeMenu);
    }

    private void EndFakeExitSequence()
    {
        if (!fakeExitActive)
        {
            return;
        }

        fakeExitActive = false;
        if (fakeExitAudioSource != null)
        {
            fakeExitAudioSource.Stop();
        }

        if (fakeExitDistortion != null)
        {
            fakeExitDistortion.distortionLevel = 0f;
        }

        if (fakeExitCanvas != null)
        {
            fakeExitCanvas.gameObject.SetActive(false);
        }

        if (startMenuCanvas != null)
        {
            startMenuCanvas.gameObject.SetActive(true);
        }

        ApplyStartMenuButtonMode(escapeMenuOpen);
        SetStartMenuButtonsInteractable(true);
        if (fakeExitStartedFromEscapeMenu && !escapeMenuOpen)
        {
            OpenEscapeMenu();
        }

        if (fakeExitStartedFromEscapeMenu)
        {
            StartEscapeMenuHum();
        }

        WriteDebugLog("FAKE_EXIT", "Fake exit sequence ended. Returned to menu.");
    }

    private void UpdateFakeExitVisuals(float elapsed)
    {
        int stage = FakeExitStageForElapsed(elapsed);
        if (stage != fakeExitStage)
        {
            fakeExitStage = stage;
            ApplyFakeExitStage(stage);
        }

        float fadeIn = Mathf.Clamp01(elapsed / 0.16f);
        float fadeOut = Mathf.Clamp01((FakeExitDurationSeconds - elapsed) / 0.55f);
        if (fakeExitGroup != null)
        {
            fakeExitGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Min(fadeIn, fadeOut));
        }

        float glitch = elapsed >= 7.25f ? Mathf.Clamp01((elapsed - 7.25f) / 0.9f) : 0f;
        int tick = Mathf.FloorToInt(Time.unscaledTime * Mathf.Lerp(18f, 58f, glitch));
        if (fakeExitStatusText != null)
        {
            float jitter = 3f + glitch * 28f;
            fakeExitStatusText.rectTransform.anchoredPosition = new Vector2(
                FakeExitNoiseSigned(tick, 11) * jitter,
                FakeExitNoiseSigned(tick, 13) * jitter * 0.45f);
            fakeExitStatusText.color = tick % 9 == 0 && glitch > 0.2f
                ? new Color(1f, 0.16f, 0.12f, 0.92f)
                : new Color(0.90f, 0.94f, 0.96f, 1f);
        }

        if (fakeExitVignetteImage != null)
        {
            fakeExitVignetteImage.color = new Color(0f, 0f, 0f, 0.16f + glitch * 0.36f);
        }

        UpdateFakeExitGlitchBands(tick, glitch);
    }

    private int FakeExitStageForElapsed(float elapsed)
    {
        if (elapsed < 1.08f)
        {
            return 0;
        }
        if (elapsed < 2.10f)
        {
            return 1;
        }
        if (elapsed < 3.12f)
        {
            return 2;
        }
        if (elapsed < 4.18f)
        {
            return 3;
        }
        if (elapsed < 5.20f)
        {
            return 4;
        }
        if (elapsed < 6.15f)
        {
            return 5;
        }
        if (elapsed < 7.38f)
        {
            return 6;
        }
        if (elapsed < 8.28f)
        {
            return 7;
        }

        return 8;
    }

    private void ApplyFakeExitStage(int stage)
    {
        if (fakeExitStatusText == null || fakeExitSmallText == null)
        {
            return;
        }

        switch (stage)
        {
            case 0:
                fakeExitStatusText.text = "Завершение приложения...";
                fakeExitSmallText.text = "BridgeGrandpas.exe";
                break;
            case 1:
                fakeExitStatusText.text = "Сохранение состояния наблюдателя...";
                fakeExitSmallText.text = "observer_state.tmp";
                break;
            case 2:
                fakeExitStatusText.text = "Остановка BridgeGrandpas.exe...";
                fakeExitSmallText.text = "process termination requested";
                break;
            case 3:
                fakeExitStatusText.text = "observer.dll: отказано";
                fakeExitSmallText.text = "access denied";
                break;
            case 4:
                fakeExitStatusText.text = "grandfather_process завершить не удалось";
                fakeExitSmallText.text = "retrying";
                break;
            case 5:
                fakeExitStatusText.text = "";
                fakeExitSmallText.text = "";
                break;
            case 6:
                fakeExitStatusText.text = "Игра закрыта.";
                fakeExitSmallText.text = "";
                break;
            case 7:
                fakeExitStatusText.text = "Наблюдение не закрыто.";
                fakeExitSmallText.text = "REC 00:00:00";
                break;
            default:
                fakeExitStatusText.text = "Он останется запущенным.";
                fakeExitSmallText.text = "observer.dll active";
                break;
        }
    }

    private void UpdateFakeExitGlitchBands(int tick, float intensity)
    {
        if (fakeExitBandRects == null)
        {
            return;
        }

        float width = Screen.width <= 0 ? 1600f : Screen.width;
        float height = Screen.height <= 0 ? 900f : Screen.height;
        for (int i = 0; i < fakeExitBandRects.Length; i++)
        {
            RectTransform rect = fakeExitBandRects[i];
            Image image = fakeExitBandImages[i];
            float n = FakeExitNoise01(tick + i * 7, 23);
            rect.anchoredPosition = new Vector2(FakeExitNoiseSigned(tick + i, 41) * 80f * intensity,
                Mathf.Lerp(-height * 0.45f, height * 0.45f, FakeExitNoise01(tick + i * 5, 29)));
            rect.sizeDelta = new Vector2(width * 1.2f, Mathf.Lerp(2f, 28f, n));
            image.color = i % 3 == 0
                ? new Color(0.68f, 0.96f, 1f, intensity * 0.35f)
                : new Color(1f, 0.10f, 0.06f, intensity * 0.22f);
        }
    }

    private void EnsureFakeExitVisuals()
    {
        if (fakeExitCanvas != null)
        {
            return;
        }

        EnsureUiFont();
        GameObject canvasObject = new GameObject("Fake Exit Overlay", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        fakeExitCanvas = canvasObject.GetComponent<Canvas>();
        fakeExitCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fakeExitCanvas.sortingOrder = 430;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = CreatePanel("Fake Exit Root", canvasObject.transform, Color.black);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        root.GetComponent<Image>().raycastTarget = true;
        fakeExitGroup = root.gameObject.AddComponent<CanvasGroup>();

        RectTransform vignette = CreatePanel("Fake Exit Vignette", root, new Color(0f, 0f, 0f, 0.16f));
        vignette.anchorMin = Vector2.zero;
        vignette.anchorMax = Vector2.one;
        vignette.offsetMin = Vector2.zero;
        vignette.offsetMax = Vector2.zero;
        vignette.GetComponent<Image>().raycastTarget = false;
        fakeExitVignetteImage = vignette.GetComponent<Image>();

        CreateFakeExitGlitchBands(root);

        fakeExitStatusText = CreateText("Fake Exit Status", root, 38, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Color(0.90f, 0.94f, 0.96f));
        fakeExitStatusText.rectTransform.anchorMin = Vector2.zero;
        fakeExitStatusText.rectTransform.anchorMax = Vector2.one;
        fakeExitStatusText.rectTransform.offsetMin = new Vector2(140f, 0f);
        fakeExitStatusText.rectTransform.offsetMax = new Vector2(-140f, 0f);

        fakeExitSmallText = CreateText("Fake Exit Small Status", root, 16, FontStyle.Normal,
            TextAnchor.MiddleCenter, new Color(0.38f, 0.56f, 0.62f));
        fakeExitSmallText.rectTransform.anchorMin = Vector2.zero;
        fakeExitSmallText.rectTransform.anchorMax = Vector2.one;
        fakeExitSmallText.rectTransform.offsetMin = new Vector2(140f, -100f);
        fakeExitSmallText.rectTransform.offsetMax = new Vector2(-140f, -44f);

        fakeExitCanvas.gameObject.SetActive(false);
    }

    private void CreateFakeExitGlitchBands(Transform parent)
    {
        fakeExitBandRects = new RectTransform[FakeExitGlitchBandCount];
        fakeExitBandImages = new Image[FakeExitGlitchBandCount];
        for (int i = 0; i < FakeExitGlitchBandCount; i++)
        {
            RectTransform band = CreatePanel("Fake Exit Glitch Band " + i, parent, Color.clear);
            band.anchorMin = new Vector2(0.5f, 0.5f);
            band.anchorMax = new Vector2(0.5f, 0.5f);
            band.pivot = new Vector2(0.5f, 0.5f);
            band.GetComponent<Image>().raycastTarget = false;
            fakeExitBandRects[i] = band;
            fakeExitBandImages[i] = band.GetComponent<Image>();
        }
    }

    private void EnsureFakeExitAudio()
    {
        if (fakeExitAudioSource != null)
        {
            return;
        }

        GameObject audioObject = new GameObject("Fake Exit Audio");
        audioObject.transform.SetParent(transform, false);
        fakeExitAudioClip = CreateFakeExitAudioClip();
        fakeExitAudioSource = audioObject.AddComponent<AudioSource>();
        fakeExitAudioSource.clip = fakeExitAudioClip;
        fakeExitAudioSource.loop = false;
        fakeExitAudioSource.playOnAwake = false;
        fakeExitAudioSource.spatialBlend = 0f;
        fakeExitAudioSource.volume = 0f;
        fakeExitAudioSource.priority = 1;
        RouteAudioSource(fakeExitAudioSource, BridgeAudioBus.Vhs);

        fakeExitDistortion = audioObject.AddComponent<AudioDistortionFilter>();
        fakeExitDistortion.distortionLevel = 0f;
    }

    private void UpdateFakeExitAudio(float elapsed)
    {
        if (fakeExitDistortion == null)
        {
            return;
        }

        float distortion = elapsed >= 7.25f ? Mathf.Clamp01((elapsed - 7.25f) / 1.2f) * 0.62f : 0.08f;
        fakeExitDistortion.distortionLevel = distortion;
    }

    private AudioClip CreateFakeExitAudioClip()
    {
        const int sampleRate = 44100;
        const int channels = 2;
        int samples = Mathf.RoundToInt(sampleRate * FakeExitDurationSeconds);
        float[] data = new float[samples * channels];
        const float tau = Mathf.PI * 2f;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float shutdown = Mathf.Sin(tau * Mathf.Lerp(92f, 26f, Mathf.Clamp01(t / 4.8f)) * t) * Mathf.Exp(-t / 4.2f) * 0.22f;
            float click = Mathf.Sin(tau * 1600f * t) * FakeExitPulse(t, 0.14f, 0.025f) * 0.26f;
            click += Mathf.Sign(Mathf.Sin(tau * 720f * t)) * FakeExitPulse(t, 3.18f, 0.06f) * 0.24f;
            float returnHum = Mathf.Clamp01((t - 6.8f) / 1.0f);
            float hum = Mathf.Sin(tau * 41f * t + Mathf.Sin(tau * 1.1f * t)) * returnHum * 0.34f;
            float dirt = (FakeExitNoise01(i, 73) * 2f - 1f) * (0.014f + returnHum * 0.035f);
            float sample = Mathf.Clamp(shutdown + click + hum + dirt, -0.94f, 0.94f);
            float stereo = (FakeExitNoise01(i + 313, 79) * 2f - 1f) * 0.045f;
            data[i * channels] = Mathf.Clamp(sample * (1f - stereo), -0.94f, 0.94f);
            data[i * channels + 1] = Mathf.Clamp(sample * (1f + stereo), -0.94f, 0.94f);
        }

        AudioClip clip = AudioClip.Create("Generated Fake Exit", samples, channels, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private float FakeExitPulse(float t, float start, float decay)
    {
        if (t < start)
        {
            return 0f;
        }

        return Mathf.Exp(-(t - start) / Mathf.Max(0.001f, decay));
    }

    private float FakeExitNoiseSigned(int seed, int salt)
    {
        return FakeExitNoise01(seed, salt) * 2f - 1f;
    }

    private float FakeExitNoise01(int seed, int salt)
    {
        return Mathf.Repeat(Mathf.Sin((seed + 1) * 12.9898f + salt * 78.233f) * 43758.5453f, 1f);
    }

    private void RestoreFakeExitForShutdown()
    {
        fakeExitActive = false;
        if (fakeExitAudioSource != null)
        {
            fakeExitAudioSource.Stop();
        }

        if (fakeExitDistortion != null)
        {
            fakeExitDistortion.distortionLevel = 0f;
        }
    }
}
