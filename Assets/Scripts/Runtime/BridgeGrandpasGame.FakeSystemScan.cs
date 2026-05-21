using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float FakeSystemScanLineInterval = 0.34f;
    private const int FakeSystemScanRequiredCancelClicks = 3;

    private Canvas fakeSystemScanCanvas;
    private CanvasGroup fakeSystemScanGroup;
    private RectTransform fakeSystemScanPanel;
    private RectTransform fakeSystemScanProgressFill;
    private Text fakeSystemScanTitleText;
    private Text fakeSystemScanBodyText;
    private Text fakeSystemScanStatusText;
    private Text fakeSystemScanCancelText;
    private AudioSource fakeSystemScanAudioSource;
    private AudioClip fakeSystemScanAudioClip;
    private bool fakeSystemScanActive;
    private float fakeSystemScanStartedAt;
    private int fakeSystemScanCancelClicks;
    private int fakeSystemScanDisplayedLines;
    private string[] fakeSystemScanLines = Array.Empty<string>();

    private bool UpdateFakeSystemScan(float deltaTime)
    {
        if (WasFakeSystemScanPressed())
        {
            BeginFakeSystemScan();
        }

        if (!fakeSystemScanActive)
        {
            return false;
        }

        float elapsed = Time.unscaledTime - fakeSystemScanStartedAt;
        UpdateFakeSystemScanVisuals(elapsed);
        return true;
    }

    private void BeginFakeSystemScan()
    {
        if (!gameStarted || escapeMenuOpen || fakeSystemScanActive || fakeMicrophoneCheckActive ||
            fakeAudioRecordingActive || fakeCreditsActive || fakeUnityErrorModalActive ||
            fakeUnityErrorGrandpasHidden || fakeUnityErrorWebcamMenuActive || fakeUnityErrorReturnGlitchActive ||
            escapeMenuBsodActive || fakeCorruptedAccountRevealActive)
        {
            return;
        }

        EnsureFakeSystemScanVisuals();
        EnsureFakeSystemScanAudio();
        fakeSystemScanLines = BuildFakeSystemScanLines();
        fakeSystemScanActive = true;
        fakeSystemScanStartedAt = Time.unscaledTime;
        fakeSystemScanCancelClicks = 0;
        fakeSystemScanDisplayedLines = 0;

        if (fakeSystemScanCanvas != null)
        {
            fakeSystemScanCanvas.gameObject.SetActive(true);
        }

        if (fakeSystemScanGroup != null)
        {
            fakeSystemScanGroup.alpha = 1f;
        }

        if (fakeSystemScanAudioSource != null && fakeSystemScanAudioClip != null)
        {
            fakeSystemScanAudioSource.Stop();
            fakeSystemScanAudioSource.time = 0f;
            fakeSystemScanAudioSource.Play();
        }

        UpdateFakeSystemScanVisuals(0f);
        WriteDebugLog("FAKE_SYSTEM_SCAN", "F6 fake system scan started. Safe allowlisted local values only.");
    }

    private void EndFakeSystemScan()
    {
        if (!fakeSystemScanActive)
        {
            return;
        }

        fakeSystemScanActive = false;
        if (fakeSystemScanAudioSource != null)
        {
            fakeSystemScanAudioSource.Stop();
        }

        if (fakeSystemScanCanvas != null)
        {
            fakeSystemScanCanvas.gameObject.SetActive(false);
        }

        WriteDebugLog("FAKE_SYSTEM_SCAN", "Fake system scan cancelled after " + fakeSystemScanCancelClicks + " clicks.");
    }

    private void OnFakeSystemScanCancelPressed()
    {
        if (!fakeSystemScanActive)
        {
            return;
        }

        fakeSystemScanCancelClicks++;
        if (fakeSystemScanCancelClicks >= FakeSystemScanRequiredCancelClicks)
        {
            EndFakeSystemScan();
            return;
        }

        if (fakeSystemScanStatusText != null)
        {
            fakeSystemScanStatusText.text = fakeSystemScanCancelClicks == 1
                ? "Отмена отклонена. Нужно подтвердить, что вы точно видели шкаф."
                : "Отмена почти принята. Последнее нажатие закроет окно. Возможно.";
        }

        if (fakeSystemScanCancelText != null)
        {
            fakeSystemScanCancelText.text = "Отмена (" + fakeSystemScanCancelClicks + "/" + FakeSystemScanRequiredCancelClicks + ")";
        }

        WriteDebugLog("FAKE_SYSTEM_SCAN", "Cancel click " + fakeSystemScanCancelClicks + "/" + FakeSystemScanRequiredCancelClicks);
    }

    private void UpdateFakeSystemScanVisuals(float elapsed)
    {
        int targetLines = Mathf.Clamp(Mathf.FloorToInt(elapsed / FakeSystemScanLineInterval) + 1, 0, fakeSystemScanLines.Length);
        fakeSystemScanDisplayedLines = Mathf.Max(fakeSystemScanDisplayedLines, targetLines);

        float fade = Mathf.Clamp01(elapsed / 0.12f);
        if (fakeSystemScanGroup != null)
        {
            fakeSystemScanGroup.alpha = Mathf.SmoothStep(0f, 1f, fade);
        }

        if (fakeSystemScanPanel != null)
        {
            int tick = Mathf.FloorToInt(Time.unscaledTime * 28f);
            float shake = fakeSystemScanCancelClicks > 0 ? 1.3f : 0.38f;
            fakeSystemScanPanel.anchoredPosition = new Vector2(
                FakeUnityErrorSignedNoise(tick, 631) * shake,
                FakeUnityErrorSignedNoise(tick, 632) * shake);
        }

        if (fakeSystemScanProgressFill != null)
        {
            float progress = fakeSystemScanLines.Length == 0 ? 0f : fakeSystemScanDisplayedLines / (float)fakeSystemScanLines.Length;
            fakeSystemScanProgressFill.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
        }

        if (fakeSystemScanBodyText != null)
        {
            fakeSystemScanBodyText.text = BuildFakeSystemScanBody(fakeSystemScanDisplayedLines);
        }

        if (fakeSystemScanStatusText != null && fakeSystemScanCancelClicks == 0)
        {
            string dots = new string('.', Mathf.FloorToInt(Time.unscaledTime * 4f) % 4);
            fakeSystemScanStatusText.text = fakeSystemScanDisplayedLines >= fakeSystemScanLines.Length
                ? "Сканирование завершено. Скелеты в шкафу: ожидают подтверждения."
                : "Сканирую текущую систему" + dots;
        }
    }

    private string BuildFakeSystemScanBody(int visibleLines)
    {
        visibleLines = Mathf.Clamp(visibleLines, 0, fakeSystemScanLines.Length);
        List<string> lines = new List<string>();
        for (int i = 0; i < visibleLines; i++)
        {
            lines.Add(fakeSystemScanLines[i]);
        }

        return string.Join("\n", lines.ToArray());
    }

    private string[] BuildFakeSystemScanLines()
    {
        string user = SafeSystemScanValue(Environment.UserName, "observer");
        string machine = SafeSystemScanValue(Environment.MachineName, "UNKNOWN-PC");
        string domain = SafeSystemScanValue(SafeUserDomainName(), machine);
        string profile = SafeSystemScanValue(Environment.GetEnvironmentVariable("USERPROFILE"), "C:\\Users\\" + user);
        string os = SafeSystemScanValue(SystemInfo.operatingSystem, SafeSystemScanValue(Environment.GetEnvironmentVariable("OS"), "Windows_NT"));

        return new[]
        {
            "[real] USERNAME = " + user,
            "[real] COMPUTERNAME = " + machine,
            "[scan] шкаф пользователя найден. ручка тёплая.",
            "[real] USERDOMAIN = " + domain,
            "[fake] skeleton_index.db: 23 записи, 10 удалены неправильно",
            "[real] USERPROFILE = " + profile,
            "[fake] Сканирую на предмет интимных фотографий: " + profile + "\\Pictures\\Личное\\*.jpg",
            "[fake] найдено 0 файлов. подозрительно аккуратно.",
            "[scan] у тебя точно найдутся скелеты в шкафу",
            "[real] OS = " + os,
            "[real] CPU threads = " + SystemInfo.processorCount,
            "[fake] observer.dll: совпадение лица не требуется",
            "[real] RAM approx = " + SystemInfo.systemMemorySize + " MB",
            "[fake] last_silence.wav: тишина слишком громкая",
            "[scan] проверяю папку, которую ты не открывал при людях",
            "[fake] underpass_cache: дедовская подпись найдена",
            "[scan] кнопка отмены перемещена в третье нажатие",
            "[fake] result: шкаф отвечает изнутри"
        };
    }

    private string SafeUserDomainName()
    {
        try
        {
            return Environment.UserDomainName;
        }
        catch (Exception)
        {
            return "";
        }
    }

    private string SafeSystemScanValue(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        value = value.Replace("\r", "").Replace("\n", "").Trim();
        return value.Length <= 72 ? value : value.Substring(0, 69) + "...";
    }

    private void EnsureFakeSystemScanVisuals()
    {
        if (fakeSystemScanCanvas != null)
        {
            return;
        }

        EnsureUiFont();
        GameObject canvasObject = new GameObject("Fake System Scan Overlay", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        fakeSystemScanCanvas = canvasObject.GetComponent<Canvas>();
        fakeSystemScanCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fakeSystemScanCanvas.sortingOrder = 374;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = CreatePanel("Fake System Scan Root", canvasObject.transform, new Color(0f, 0f, 0f, 0.42f));
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        root.GetComponent<Image>().raycastTarget = true;
        fakeSystemScanGroup = root.gameObject.AddComponent<CanvasGroup>();

        fakeSystemScanPanel = CreatePanel("Fake System Scan Panel", root, new Color(0.012f, 0.014f, 0.016f, 0.98f));
        fakeSystemScanPanel.anchorMin = new Vector2(0.5f, 0.5f);
        fakeSystemScanPanel.anchorMax = new Vector2(0.5f, 0.5f);
        fakeSystemScanPanel.pivot = new Vector2(0.5f, 0.5f);
        fakeSystemScanPanel.sizeDelta = new Vector2(940f, 590f);
        Outline outline = fakeSystemScanPanel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.76f, 0.06f, 0.035f, 0.72f);
        outline.effectDistance = new Vector2(2.5f, -2.5f);

        CreateFakeSystemScanHeader(fakeSystemScanPanel);
        CreateFakeSystemScanBody(fakeSystemScanPanel);
        CreateFakeSystemScanFooter(fakeSystemScanPanel);
        fakeSystemScanCanvas.gameObject.SetActive(false);
    }

    private void CreateFakeSystemScanHeader(Transform parent)
    {
        RectTransform header = CreatePanel("Fake System Scan Header", parent, new Color(0.08f, 0.010f, 0.008f, 1f));
        header.anchorMin = new Vector2(0f, 1f);
        header.anchorMax = new Vector2(1f, 1f);
        header.pivot = new Vector2(0.5f, 1f);
        header.offsetMin = new Vector2(0f, -64f);
        header.offsetMax = Vector2.zero;

        fakeSystemScanTitleText = CreateText("Fake System Scan Title", header, 22, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(1f, 0.88f, 0.82f));
        fakeSystemScanTitleText.text = "Local System Scan";
        fakeSystemScanTitleText.rectTransform.anchorMin = Vector2.zero;
        fakeSystemScanTitleText.rectTransform.anchorMax = Vector2.one;
        fakeSystemScanTitleText.rectTransform.offsetMin = new Vector2(22f, 0f);
        fakeSystemScanTitleText.rectTransform.offsetMax = new Vector2(-22f, 0f);

        Text badge = CreateText("Fake System Scan Badge", header, 18, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.22f, 0.15f));
        badge.text = "DO NOT LOOK AWAY";
        badge.rectTransform.anchorMin = Vector2.zero;
        badge.rectTransform.anchorMax = Vector2.one;
        badge.rectTransform.offsetMin = new Vector2(22f, 0f);
        badge.rectTransform.offsetMax = new Vector2(-22f, 0f);
    }

    private void CreateFakeSystemScanBody(Transform parent)
    {
        Text lead = CreateText("Fake System Scan Lead", parent, 19, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.78f, 0.58f));
        lead.text = "Сканирую текущую систему. У тебя точно найдутся скелеты в шкафу.";
        lead.rectTransform.anchorMin = new Vector2(0f, 1f);
        lead.rectTransform.anchorMax = new Vector2(1f, 1f);
        lead.rectTransform.offsetMin = new Vector2(34f, -116f);
        lead.rectTransform.offsetMax = new Vector2(-34f, -78f);

        RectTransform console = CreatePanel("Fake System Scan Console", parent, new Color(0.002f, 0.004f, 0.005f, 0.92f));
        console.anchorMin = new Vector2(0f, 0f);
        console.anchorMax = new Vector2(1f, 1f);
        console.offsetMin = new Vector2(34f, 110f);
        console.offsetMax = new Vector2(-34f, -128f);
        Outline outline = console.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.28f, 0.56f, 0.55f, 0.35f);
        outline.effectDistance = new Vector2(1f, -1f);

        fakeSystemScanBodyText = CreateText("Fake System Scan Lines", console, 17, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.70f, 0.96f, 0.88f));
        fakeSystemScanBodyText.rectTransform.anchorMin = Vector2.zero;
        fakeSystemScanBodyText.rectTransform.anchorMax = Vector2.one;
        fakeSystemScanBodyText.rectTransform.offsetMin = new Vector2(18f, 14f);
        fakeSystemScanBodyText.rectTransform.offsetMax = new Vector2(-18f, -14f);
        fakeSystemScanBodyText.verticalOverflow = VerticalWrapMode.Truncate;
    }

    private void CreateFakeSystemScanFooter(Transform parent)
    {
        RectTransform track = CreatePanel("Fake System Scan Progress Track", parent, new Color(0.045f, 0.055f, 0.060f, 1f));
        track.anchorMin = new Vector2(0f, 0f);
        track.anchorMax = new Vector2(1f, 0f);
        track.pivot = new Vector2(0.5f, 0f);
        track.offsetMin = new Vector2(34f, 72f);
        track.offsetMax = new Vector2(-250f, 94f);

        fakeSystemScanProgressFill = CreatePanel("Fake System Scan Progress Fill", track, new Color(0.88f, 0.08f, 0.045f, 0.95f));
        fakeSystemScanProgressFill.anchorMin = Vector2.zero;
        fakeSystemScanProgressFill.anchorMax = new Vector2(0f, 1f);
        fakeSystemScanProgressFill.offsetMin = new Vector2(2f, 2f);
        fakeSystemScanProgressFill.offsetMax = new Vector2(-2f, -2f);

        RectTransform cancel = CreateButton("Отмена", parent, OnFakeSystemScanCancelPressed);
        cancel.anchorMin = new Vector2(1f, 0f);
        cancel.anchorMax = new Vector2(1f, 0f);
        cancel.pivot = new Vector2(1f, 0f);
        cancel.anchoredPosition = new Vector2(-34f, 52f);
        cancel.sizeDelta = new Vector2(188f, 56f);
        fakeSystemScanCancelText = cancel.GetComponentInChildren<Text>(true);

        fakeSystemScanStatusText = CreateText("Fake System Scan Status", parent, 16, FontStyle.Italic, TextAnchor.MiddleLeft, new Color(1f, 0.68f, 0.55f));
        fakeSystemScanStatusText.rectTransform.anchorMin = new Vector2(0f, 0f);
        fakeSystemScanStatusText.rectTransform.anchorMax = new Vector2(1f, 0f);
        fakeSystemScanStatusText.rectTransform.offsetMin = new Vector2(34f, 22f);
        fakeSystemScanStatusText.rectTransform.offsetMax = new Vector2(-34f, 52f);
    }

    private void EnsureFakeSystemScanAudio()
    {
        if (fakeSystemScanAudioSource == null)
        {
            GameObject audioObject = new GameObject("Fake System Scan Audio");
            audioObject.transform.SetParent(transform, false);
            fakeSystemScanAudioSource = audioObject.AddComponent<AudioSource>();
            fakeSystemScanAudioSource.loop = true;
            fakeSystemScanAudioSource.playOnAwake = false;
            fakeSystemScanAudioSource.spatialBlend = 0f;
            fakeSystemScanAudioSource.volume = 0.36f;
            fakeSystemScanAudioSource.priority = 45;
            RouteAudioSource(fakeSystemScanAudioSource, BridgeAudioBus.Vhs);

            AudioLowPassFilter lowPass = audioObject.AddComponent<AudioLowPassFilter>();
            lowPass.cutoffFrequency = 1300f;
            lowPass.lowpassResonanceQ = 1.7f;

            AudioDistortionFilter distortion = audioObject.AddComponent<AudioDistortionFilter>();
            distortion.distortionLevel = 0.42f;
        }

        if (fakeSystemScanAudioClip == null)
        {
            fakeSystemScanAudioClip = CreateFakeSystemScanAudioClip();
        }

        fakeSystemScanAudioSource.clip = fakeSystemScanAudioClip;
    }

    private AudioClip CreateFakeSystemScanAudioClip()
    {
        const int sampleRate = 44100;
        const int seconds = 5;
        float[] data = new float[sampleRate * seconds];
        uint seed = 0x723401u;

        for (int i = 0; i < data.Length; i++)
        {
            float t = i / (float)sampleRate;
            seed = seed * 1103515245u + 12345u;
            float noise = ((seed >> 10) / 4194303f) * 2f - 1f;
            float hum = Mathf.Sin(t * Mathf.PI * 2f * 37f) * 0.13f;
            float click = Mathf.Repeat(t * 8.5f, 1f) < 0.025f ? Mathf.Sin(t * Mathf.PI * 2f * 720f) * 0.34f : 0f;
            float sweep = Mathf.Sin(t * Mathf.PI * 2f * (83f + Mathf.Sin(t * 0.8f) * 12f)) * 0.05f;
            data[i] = Mathf.Clamp(hum + click + sweep + noise * 0.035f, -0.9f, 0.9f);
        }

        AudioClip clip = AudioClip.Create("FakeSystemScanLoop", data.Length, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private bool WasFakeSystemScanPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f6Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F6);
#endif
    }
}
