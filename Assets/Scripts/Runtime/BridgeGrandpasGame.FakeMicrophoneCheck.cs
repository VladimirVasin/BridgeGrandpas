using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float FakeMicrophoneCheckDurationSeconds = 6.35f;
    private const float FakeMicrophoneNoVoiceAtSeconds = 2.05f;
    private const float FakeMicrophoneHeHearsAtSeconds = 4.15f;

    private Canvas fakeMicrophoneCanvas;
    private RectTransform fakeMicrophoneRoot;
    private CanvasGroup fakeMicrophoneGroup;
    private RectTransform fakeMicrophoneDialog;
    private RectTransform fakeMicrophoneProgressFill;
    private Image fakeMicrophoneLedImage;
    private Text fakeMicrophoneTitleText;
    private Text fakeMicrophoneBodyText;
    private Text fakeMicrophoneStatusText;
    private AudioSource fakeMicrophoneAudioSource;
    private AudioClip fakeMicrophoneAudioClip;
    private bool fakeMicrophoneCheckActive;
    private float fakeMicrophoneCheckStartedAt;
    private int fakeMicrophoneStage = -1;

    private bool UpdateFakeMicrophoneCheck(float deltaTime)
    {
        if (WasFakeMicrophoneCheckPressed())
        {
            BeginFakeMicrophoneCheck();
        }

        if (!fakeMicrophoneCheckActive)
        {
            return false;
        }

        if (WasEscapePressed())
        {
            EndFakeMicrophoneCheck(true);
            return true;
        }

        float elapsed = Time.unscaledTime - fakeMicrophoneCheckStartedAt;
        UpdateFakeMicrophoneCheckVisuals(elapsed);
        if (elapsed >= FakeMicrophoneCheckDurationSeconds)
        {
            EndFakeMicrophoneCheck(false);
        }

        return fakeMicrophoneCheckActive;
    }

    private void BeginFakeMicrophoneCheck()
    {
        if (!gameStarted || escapeMenuOpen || fakeMicrophoneCheckActive || fakeUnityErrorModalActive ||
            fakeUnityErrorGrandpasHidden || fakeUnityErrorWebcamMenuActive || fakeUnityErrorReturnGlitchActive ||
            fakeCreditsActive || escapeMenuBsodActive)
        {
            return;
        }

        EnsureFakeMicrophoneCheckVisuals();
        EnsureFakeMicrophoneCheckAudio();
        fakeMicrophoneCheckActive = true;
        fakeMicrophoneCheckStartedAt = Time.unscaledTime;
        fakeMicrophoneStage = -1;

        if (fakeMicrophoneCanvas != null)
        {
            fakeMicrophoneCanvas.gameObject.SetActive(true);
        }

        if (fakeMicrophoneGroup != null)
        {
            fakeMicrophoneGroup.alpha = 1f;
        }

        if (fakeMicrophoneAudioSource != null && fakeMicrophoneAudioClip != null)
        {
            fakeMicrophoneAudioSource.Stop();
            fakeMicrophoneAudioSource.time = 0f;
            fakeMicrophoneAudioSource.Play();
        }

        UpdateFakeMicrophoneCheckVisuals(0f);
        WriteDebugLog("FAKE_MICROPHONE", "F4 fake microphone check started. No real microphone APIs are used.");
    }

    private void EndFakeMicrophoneCheck(bool cancelled)
    {
        if (!fakeMicrophoneCheckActive)
        {
            return;
        }

        fakeMicrophoneCheckActive = false;
        if (fakeMicrophoneAudioSource != null)
        {
            fakeMicrophoneAudioSource.Stop();
        }

        if (fakeMicrophoneCanvas != null)
        {
            fakeMicrophoneCanvas.gameObject.SetActive(false);
        }

        WriteDebugLog("FAKE_MICROPHONE", cancelled ? "Fake microphone check cancelled by Escape." : "Fake microphone check completed.");
    }

    private void UpdateFakeMicrophoneCheckVisuals(float elapsed)
    {
        int stage = elapsed >= FakeMicrophoneHeHearsAtSeconds ? 2 :
            elapsed >= FakeMicrophoneNoVoiceAtSeconds ? 1 : 0;
        if (stage != fakeMicrophoneStage)
        {
            SetFakeMicrophoneStage(stage);
        }

        float fadeIn = Mathf.Clamp01(elapsed / 0.18f);
        float fadeOut = Mathf.Clamp01((FakeMicrophoneCheckDurationSeconds - elapsed) / 0.34f);
        if (fakeMicrophoneGroup != null)
        {
            fakeMicrophoneGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Min(fadeIn, fadeOut));
        }

        float progress = Mathf.Clamp01(elapsed / FakeMicrophoneCheckDurationSeconds);
        if (fakeMicrophoneProgressFill != null)
        {
            fakeMicrophoneProgressFill.anchorMax = new Vector2(progress, 1f);
        }

        int tick = Mathf.FloorToInt(Time.unscaledTime * 32f);
        if (fakeMicrophoneLedImage != null)
        {
            float ledPulse = stage == 2 ? (tick % 3 == 0 ? 0.35f : 1f) : (tick % 8 < 4 ? 1f : 0.45f);
            Color baseColor = stage == 2 ? new Color(1f, 0.10f, 0.07f) : new Color(0.17f, 0.86f, 0.60f);
            fakeMicrophoneLedImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, ledPulse);
        }

        if (fakeMicrophoneDialog != null)
        {
            float shake = stage == 2 ? 1f : 0.25f;
            fakeMicrophoneDialog.anchoredPosition = new Vector2(
                FakeUnityErrorSignedNoise(tick, 311) * shake,
                FakeUnityErrorSignedNoise(tick, 312) * shake);
        }

        if (fakeMicrophoneStatusText != null)
        {
            string dots = new string('.', tick % 4);
            if (stage == 0)
            {
                fakeMicrophoneStatusText.text = "LISTENING" + dots;
            }
        }
    }

    private void SetFakeMicrophoneStage(int stage)
    {
        fakeMicrophoneStage = stage;
        if (fakeMicrophoneTitleText == null || fakeMicrophoneBodyText == null || fakeMicrophoneStatusText == null)
        {
            return;
        }

        if (stage == 0)
        {
            fakeMicrophoneTitleText.text = "Input Device Diagnostic";
            fakeMicrophoneBodyText.text =
                "Проверка входного устройства...\n\n" +
                "Устройство: Primary Input\n" +
                "Состояние: синхронизация шума\n" +
                "Доступ к микрофону: не требуется";
            fakeMicrophoneStatusText.text = "LISTENING";
            return;
        }

        if (stage == 1)
        {
            fakeMicrophoneTitleText.text = "Input Device Diagnostic";
            fakeMicrophoneBodyText.text =
                "Голос наблюдателя не обнаружен.\n\n" +
                "Уровень входа: 0.00 dB\n" +
                "Порог распознавания: не пройден\n" +
                "Повторная сверка тишины...";
            fakeMicrophoneStatusText.text = "NO VOICE";
            return;
        }

        fakeMicrophoneTitleText.text = "Input Device Diagnostic";
        fakeMicrophoneBodyText.text =
            "Он всё равно слышит.\n\n" +
            "observer_input: accepted\n" +
            "silence_profile: matched\n" +
            "source: underpass";
        fakeMicrophoneStatusText.text = "HE HEARS";
    }

    private void EnsureFakeMicrophoneCheckVisuals()
    {
        if (fakeMicrophoneCanvas != null)
        {
            return;
        }

        EnsureUiFont();
        GameObject canvasObject = new GameObject("Fake Microphone Check Overlay", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        fakeMicrophoneCanvas = canvasObject.GetComponent<Canvas>();
        fakeMicrophoneCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fakeMicrophoneCanvas.sortingOrder = 370;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        fakeMicrophoneRoot = CreatePanel("Fake Microphone Root", canvasObject.transform, new Color(0f, 0f, 0f, 0.18f));
        fakeMicrophoneRoot.anchorMin = Vector2.zero;
        fakeMicrophoneRoot.anchorMax = Vector2.one;
        fakeMicrophoneRoot.offsetMin = Vector2.zero;
        fakeMicrophoneRoot.offsetMax = Vector2.zero;
        fakeMicrophoneRoot.GetComponent<Image>().raycastTarget = true;
        fakeMicrophoneGroup = fakeMicrophoneRoot.gameObject.AddComponent<CanvasGroup>();

        CreateFakeMicrophoneDialog(fakeMicrophoneRoot);
        fakeMicrophoneCanvas.gameObject.SetActive(false);
    }

    private void CreateFakeMicrophoneDialog(Transform parent)
    {
        fakeMicrophoneDialog = CreatePanel("Fake Microphone Dialog", parent, new Color(0.91f, 0.91f, 0.88f, 1f));
        fakeMicrophoneDialog.anchorMin = new Vector2(0.5f, 0.5f);
        fakeMicrophoneDialog.anchorMax = new Vector2(0.5f, 0.5f);
        fakeMicrophoneDialog.pivot = new Vector2(0.5f, 0.5f);
        fakeMicrophoneDialog.sizeDelta = new Vector2(650f, 336f);

        Outline outline = fakeMicrophoneDialog.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.52f);
        outline.effectDistance = new Vector2(2f, -2f);

        RectTransform titleBar = CreatePanel("Fake Microphone Title Bar", fakeMicrophoneDialog, new Color(0.12f, 0.14f, 0.17f, 1f));
        titleBar.anchorMin = new Vector2(0f, 1f);
        titleBar.anchorMax = new Vector2(1f, 1f);
        titleBar.pivot = new Vector2(0.5f, 1f);
        titleBar.sizeDelta = new Vector2(0f, 38f);

        fakeMicrophoneTitleText = CreateText("Fake Microphone Title", titleBar, 15, FontStyle.Bold,
            TextAnchor.MiddleLeft, Color.white);
        fakeMicrophoneTitleText.rectTransform.anchorMin = Vector2.zero;
        fakeMicrophoneTitleText.rectTransform.anchorMax = Vector2.one;
        fakeMicrophoneTitleText.rectTransform.offsetMin = new Vector2(16f, 0f);
        fakeMicrophoneTitleText.rectTransform.offsetMax = new Vector2(-16f, 0f);

        fakeMicrophoneLedImage = CreatePanel("Fake Microphone LED", fakeMicrophoneDialog, new Color(0.17f, 0.86f, 0.60f, 1f)).GetComponent<Image>();
        fakeMicrophoneLedImage.rectTransform.anchorMin = new Vector2(0f, 1f);
        fakeMicrophoneLedImage.rectTransform.anchorMax = new Vector2(0f, 1f);
        fakeMicrophoneLedImage.rectTransform.pivot = new Vector2(0f, 1f);
        fakeMicrophoneLedImage.rectTransform.anchoredPosition = new Vector2(34f, -70f);
        fakeMicrophoneLedImage.rectTransform.sizeDelta = new Vector2(18f, 18f);
        fakeMicrophoneLedImage.raycastTarget = false;

        Text label = CreateText("Fake Microphone Device Label", fakeMicrophoneDialog, 13, FontStyle.Bold,
            TextAnchor.UpperLeft, new Color(0.10f, 0.11f, 0.12f));
        label.rectTransform.anchorMin = new Vector2(0f, 1f);
        label.rectTransform.anchorMax = new Vector2(1f, 1f);
        label.rectTransform.offsetMin = new Vector2(66f, -94f);
        label.rectTransform.offsetMax = new Vector2(-34f, -64f);
        label.text = "Bridge Audio Subsystem / Input Monitor";

        fakeMicrophoneBodyText = CreateText("Fake Microphone Body", fakeMicrophoneDialog, 19, FontStyle.Normal,
            TextAnchor.UpperLeft, Color.black);
        fakeMicrophoneBodyText.rectTransform.anchorMin = Vector2.zero;
        fakeMicrophoneBodyText.rectTransform.anchorMax = Vector2.one;
        fakeMicrophoneBodyText.rectTransform.offsetMin = new Vector2(34f, 86f);
        fakeMicrophoneBodyText.rectTransform.offsetMax = new Vector2(-34f, -112f);

        RectTransform progressTrack = CreatePanel("Fake Microphone Progress Track", fakeMicrophoneDialog, new Color(0.18f, 0.19f, 0.20f, 1f));
        progressTrack.anchorMin = new Vector2(0f, 0f);
        progressTrack.anchorMax = new Vector2(1f, 0f);
        progressTrack.pivot = new Vector2(0.5f, 0f);
        progressTrack.offsetMin = new Vector2(34f, 50f);
        progressTrack.offsetMax = new Vector2(-34f, 66f);

        fakeMicrophoneProgressFill = CreatePanel("Fake Microphone Progress Fill", progressTrack, new Color(0.21f, 0.70f, 0.88f, 1f));
        fakeMicrophoneProgressFill.anchorMin = Vector2.zero;
        fakeMicrophoneProgressFill.anchorMax = new Vector2(0f, 1f);
        fakeMicrophoneProgressFill.offsetMin = Vector2.zero;
        fakeMicrophoneProgressFill.offsetMax = Vector2.zero;

        fakeMicrophoneStatusText = CreateText("Fake Microphone Status", fakeMicrophoneDialog, 14, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Color(0.10f, 0.11f, 0.12f));
        fakeMicrophoneStatusText.rectTransform.anchorMin = new Vector2(0f, 0f);
        fakeMicrophoneStatusText.rectTransform.anchorMax = new Vector2(1f, 0f);
        fakeMicrophoneStatusText.rectTransform.offsetMin = new Vector2(34f, 14f);
        fakeMicrophoneStatusText.rectTransform.offsetMax = new Vector2(-34f, 42f);
    }

    private void EnsureFakeMicrophoneCheckAudio()
    {
        if (fakeMicrophoneAudioSource != null)
        {
            return;
        }

        GameObject audioObject = new GameObject("Fake Microphone Check Audio");
        audioObject.transform.SetParent(transform, false);
        fakeMicrophoneAudioClip = CreateFakeMicrophoneCheckAudioClip();
        fakeMicrophoneAudioSource = audioObject.AddComponent<AudioSource>();
        fakeMicrophoneAudioSource.clip = fakeMicrophoneAudioClip;
        fakeMicrophoneAudioSource.loop = false;
        fakeMicrophoneAudioSource.playOnAwake = false;
        fakeMicrophoneAudioSource.spatialBlend = 0f;
        fakeMicrophoneAudioSource.volume = 0.72f;
        fakeMicrophoneAudioSource.pitch = 1f;
        fakeMicrophoneAudioSource.priority = 2;
        RouteAudioSource(fakeMicrophoneAudioSource, BridgeAudioBus.Vhs);
    }

    private AudioClip CreateFakeMicrophoneCheckAudioClip()
    {
        const int sampleRate = 44100;
        const int channels = 2;
        int samples = Mathf.RoundToInt(sampleRate * FakeMicrophoneCheckDurationSeconds);
        float[] data = new float[samples * channels];
        const float tau = Mathf.PI * 2f;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float hum = Mathf.Sin(tau * 43f * t) * 0.26f + Mathf.Sin(tau * 57f * t + 0.6f) * 0.11f;
            float initialBeep = Mathf.Sin(tau * 2140f * t) * FakeMicrophonePulse(t, 0.08f, 0.06f) * 0.44f;
            float scanBeep = Mathf.Sin(tau * 1660f * t) * FakeMicrophonePulse(t, FakeMicrophoneNoVoiceAtSeconds, 0.08f) * 0.35f;
            float finalBeep = Mathf.Sign(Mathf.Sin(tau * 2480f * t)) * FakeMicrophonePulse(t, FakeMicrophoneHeHearsAtSeconds, 0.13f) * 0.28f;
            float rumbleRise = Mathf.Clamp01((t - FakeMicrophoneHeHearsAtSeconds) / 1.4f);
            float lowRise = Mathf.Sin(tau * 31f * t + Mathf.Sin(tau * 3f * t)) * rumbleRise * 0.24f;
            float noise = FakeMicrophoneNoise(i) * (0.015f + rumbleRise * 0.025f);
            float sample = Mathf.Clamp(hum + lowRise + initialBeep + scanBeep + finalBeep + noise, -0.95f, 0.95f);
            float stereo = FakeMicrophoneNoise(i + 813) * 0.04f;
            data[i * channels] = Mathf.Clamp(sample * (1f - stereo), -0.95f, 0.95f);
            data[i * channels + 1] = Mathf.Clamp(sample * (1f + stereo), -0.95f, 0.95f);
        }

        AudioClip clip = AudioClip.Create("Generated Fake Microphone Check", samples, channels, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private float FakeMicrophonePulse(float t, float start, float decay)
    {
        if (t < start)
        {
            return 0f;
        }

        return Mathf.Exp(-(t - start) / Mathf.Max(0.001f, decay));
    }

    private float FakeMicrophoneNoise(int sample)
    {
        return Mathf.Repeat(Mathf.Sin((sample + 11) * 12.9898f) * 43758.5453f, 1f) * 2f - 1f;
    }

    private void RestoreFakeMicrophoneCheckForShutdown()
    {
        fakeMicrophoneCheckActive = false;
        if (fakeMicrophoneAudioSource != null)
        {
            fakeMicrophoneAudioSource.Stop();
        }
    }

    private bool WasFakeMicrophoneCheckPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f4Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F4);
#endif
    }
}
