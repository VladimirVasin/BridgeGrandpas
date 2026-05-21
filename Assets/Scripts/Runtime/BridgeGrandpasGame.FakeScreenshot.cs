using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float FakeScreenshotDurationSeconds = 2.65f;
    private const float FakeScreenshotToastInSeconds = 0.16f;
    private const float FakeScreenshotToastHoldSeconds = 1.72f;
    private const float FakeScreenshotToastOutSeconds = 0.34f;

    private Canvas fakeScreenshotCanvas;
    private CanvasGroup fakeScreenshotGroup;
    private Image fakeScreenshotFlashImage;
    private RectTransform fakeScreenshotFrameRoot;
    private CanvasGroup fakeScreenshotFrameGroup;
    private RectTransform fakeScreenshotToastRoot;
    private CanvasGroup fakeScreenshotToastGroup;
    private RawImage fakeScreenshotThumbnailImage;
    private Texture2D fakeScreenshotCapturedTexture;
    private Text fakeScreenshotTitleText;
    private Text fakeScreenshotPathText;
    private AudioSource fakeScreenshotAudioSource;
    private AudioClip fakeScreenshotAudioClip;
    private bool fakeScreenshotActive;
    private float fakeScreenshotStartedAt;
    private int fakeScreenshotIndex;

    private void UpdateFakeScreenshot(float deltaTime)
    {
        if (WasFakeScreenshotPressed() && !fakeScreenshotActive)
        {
            BeginFakeScreenshot("F1 debug trigger");
        }

        if (!fakeScreenshotActive)
        {
            return;
        }

        float elapsed = Time.unscaledTime - fakeScreenshotStartedAt;
        UpdateFakeScreenshotVisuals(elapsed);
        if (elapsed >= FakeScreenshotDurationSeconds)
        {
            EndFakeScreenshot();
        }
    }

    private void BeginFakeScreenshot(string reason)
    {
        EnsureFakeScreenshotVisuals();
        EnsureFakeScreenshotAudio();
        CaptureFakeScreenshotThumbnail();
        fakeScreenshotActive = true;
        fakeScreenshotStartedAt = Time.unscaledTime;
        fakeScreenshotIndex++;

        if (fakeScreenshotCanvas != null)
        {
            fakeScreenshotCanvas.gameObject.SetActive(true);
        }

        if (fakeScreenshotGroup != null)
        {
            fakeScreenshotGroup.alpha = 1f;
        }

        string filename = "observer_frame_" + fakeScreenshotIndex.ToString("0000") + ".png";
        if (fakeScreenshotTitleText != null)
        {
            fakeScreenshotTitleText.text = "Screenshot saved";
        }

        if (fakeScreenshotPathText != null)
        {
            fakeScreenshotPathText.text = filename;
        }

        if (fakeScreenshotAudioSource != null && fakeScreenshotAudioClip != null)
        {
            fakeScreenshotAudioSource.Stop();
            fakeScreenshotAudioSource.time = 0f;
            fakeScreenshotAudioSource.Play();
        }

        WriteDebugLog("FAKE_SCREENSHOT", "Triggered fake screenshot. reason=" + reason + " file=" + filename);
        UpdateFakeScreenshotVisuals(0f);
    }

    private void EndFakeScreenshot()
    {
        fakeScreenshotActive = false;
        if (fakeScreenshotCanvas != null)
        {
            fakeScreenshotCanvas.gameObject.SetActive(false);
        }
    }

    private void EnsureFakeScreenshotVisuals()
    {
        if (fakeScreenshotCanvas != null)
        {
            return;
        }

        EnsureUiFont();
        GameObject canvasObject = new GameObject("Fake Screenshot Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        fakeScreenshotCanvas = canvasObject.GetComponent<Canvas>();
        fakeScreenshotCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fakeScreenshotCanvas.sortingOrder = 340;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = CreatePanel("Fake Screenshot Root", canvasObject.transform, new Color(0f, 0f, 0f, 0f));
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        root.GetComponent<Image>().raycastTarget = false;
        fakeScreenshotGroup = root.gameObject.AddComponent<CanvasGroup>();
        fakeScreenshotGroup.interactable = false;
        fakeScreenshotGroup.blocksRaycasts = false;

        RectTransform flash = CreatePanel("Fake Screenshot Flash", root, Color.white);
        flash.anchorMin = Vector2.zero;
        flash.anchorMax = Vector2.one;
        flash.offsetMin = Vector2.zero;
        flash.offsetMax = Vector2.zero;
        flash.GetComponent<Image>().raycastTarget = false;
        fakeScreenshotFlashImage = flash.GetComponent<Image>();

        CreateFakeScreenshotCaptureFrame(root);
        CreateFakeScreenshotToast(root);
        fakeScreenshotCanvas.gameObject.SetActive(false);
    }

    private void CreateFakeScreenshotCaptureFrame(Transform parent)
    {
        fakeScreenshotFrameRoot = CreatePanel("Fake Screenshot Capture Frame", parent, new Color(0f, 0f, 0f, 0f));
        fakeScreenshotFrameRoot.anchorMin = Vector2.zero;
        fakeScreenshotFrameRoot.anchorMax = Vector2.one;
        fakeScreenshotFrameRoot.offsetMin = new Vector2(34f, 28f);
        fakeScreenshotFrameRoot.offsetMax = new Vector2(-34f, -28f);
        fakeScreenshotFrameRoot.GetComponent<Image>().raycastTarget = false;
        fakeScreenshotFrameGroup = fakeScreenshotFrameRoot.gameObject.AddComponent<CanvasGroup>();
        fakeScreenshotFrameGroup.interactable = false;
        fakeScreenshotFrameGroup.blocksRaycasts = false;

        CreateFakeScreenshotCorner(fakeScreenshotFrameRoot, "Top Left", new Vector2(0f, 1f), 1f, -1f);
        CreateFakeScreenshotCorner(fakeScreenshotFrameRoot, "Top Right", new Vector2(1f, 1f), -1f, -1f);
        CreateFakeScreenshotCorner(fakeScreenshotFrameRoot, "Bottom Left", new Vector2(0f, 0f), 1f, 1f);
        CreateFakeScreenshotCorner(fakeScreenshotFrameRoot, "Bottom Right", new Vector2(1f, 0f), -1f, 1f);
    }

    private void CreateFakeScreenshotCorner(Transform parent, string name, Vector2 anchor, float xSign, float ySign)
    {
        RectTransform horizontal = CreatePanel("Fake Screenshot Corner H " + name, parent, new Color(1f, 1f, 1f, 0.82f));
        horizontal.anchorMin = anchor;
        horizontal.anchorMax = anchor;
        horizontal.pivot = new Vector2(xSign > 0f ? 0f : 1f, ySign > 0f ? 0f : 1f);
        horizontal.anchoredPosition = Vector2.zero;
        horizontal.sizeDelta = new Vector2(84f, 3f);
        horizontal.GetComponent<Image>().raycastTarget = false;

        RectTransform vertical = CreatePanel("Fake Screenshot Corner V " + name, parent, new Color(1f, 1f, 1f, 0.82f));
        vertical.anchorMin = anchor;
        vertical.anchorMax = anchor;
        vertical.pivot = new Vector2(xSign > 0f ? 0f : 1f, ySign > 0f ? 0f : 1f);
        vertical.anchoredPosition = Vector2.zero;
        vertical.sizeDelta = new Vector2(3f, 84f);
        vertical.GetComponent<Image>().raycastTarget = false;
    }

    private void CreateFakeScreenshotToast(Transform parent)
    {
        fakeScreenshotToastRoot = CreatePanel("Fake Steam Screenshot Toast", parent, new Color(0.045f, 0.052f, 0.060f, 0.94f));
        fakeScreenshotToastRoot.anchorMin = new Vector2(1f, 0f);
        fakeScreenshotToastRoot.anchorMax = new Vector2(1f, 0f);
        fakeScreenshotToastRoot.pivot = new Vector2(1f, 0f);
        fakeScreenshotToastRoot.sizeDelta = new Vector2(390f, 92f);
        fakeScreenshotToastRoot.GetComponent<Image>().raycastTarget = false;
        fakeScreenshotToastGroup = fakeScreenshotToastRoot.gameObject.AddComponent<CanvasGroup>();
        fakeScreenshotToastGroup.interactable = false;
        fakeScreenshotToastGroup.blocksRaycasts = false;

        Outline outline = fakeScreenshotToastRoot.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.65f);
        outline.effectDistance = new Vector2(2f, -2f);

        RectTransform thumbnail = CreatePanel("Fake Screenshot Thumbnail", fakeScreenshotToastRoot, new Color(0.020f, 0.025f, 0.030f, 1f));
        thumbnail.anchorMin = new Vector2(0f, 0.5f);
        thumbnail.anchorMax = new Vector2(0f, 0.5f);
        thumbnail.pivot = new Vector2(0f, 0.5f);
        thumbnail.anchoredPosition = new Vector2(14f, 0f);
        thumbnail.sizeDelta = new Vector2(96f, 56f);
        thumbnail.GetComponent<Image>().raycastTarget = false;
        CreateFakeScreenshotThumbnailDetails(thumbnail);
        CreateFakeScreenshotCapturedThumbnail(thumbnail);

        fakeScreenshotTitleText = CreateText("Fake Screenshot Title", fakeScreenshotToastRoot, 18, FontStyle.Bold, TextAnchor.UpperLeft, Color.white);
        fakeScreenshotTitleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        fakeScreenshotTitleText.rectTransform.anchorMax = new Vector2(1f, 1f);
        fakeScreenshotTitleText.rectTransform.offsetMin = new Vector2(126f, -42f);
        fakeScreenshotTitleText.rectTransform.offsetMax = new Vector2(-18f, -13f);

        fakeScreenshotPathText = CreateText("Fake Screenshot Path", fakeScreenshotToastRoot, 13, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.68f, 0.77f, 0.86f));
        fakeScreenshotPathText.rectTransform.anchorMin = new Vector2(0f, 0f);
        fakeScreenshotPathText.rectTransform.anchorMax = new Vector2(1f, 1f);
        fakeScreenshotPathText.rectTransform.offsetMin = new Vector2(126f, 14f);
        fakeScreenshotPathText.rectTransform.offsetMax = new Vector2(-18f, -48f);
    }

    private void CreateFakeScreenshotCapturedThumbnail(Transform parent)
    {
        GameObject imageObject = new GameObject("Fake Screenshot Captured Thumbnail", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(RawImage));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(2f, 2f);
        rect.offsetMax = new Vector2(-2f, -2f);

        fakeScreenshotThumbnailImage = imageObject.GetComponent<RawImage>();
        fakeScreenshotThumbnailImage.color = new Color(1f, 1f, 1f, 0f);
        fakeScreenshotThumbnailImage.raycastTarget = false;
    }

    private void CreateFakeScreenshotThumbnailDetails(Transform parent)
    {
        RectTransform horizon = CreatePanel("Thumbnail Horizon", parent, new Color(0.18f, 0.26f, 0.33f, 1f));
        horizon.anchorMin = new Vector2(0f, 0.35f);
        horizon.anchorMax = new Vector2(1f, 0.35f);
        horizon.sizeDelta = new Vector2(0f, 3f);

        RectTransform fire = CreatePanel("Thumbnail Fire", parent, new Color(1f, 0.38f, 0.10f, 1f));
        fire.anchorMin = new Vector2(0.52f, 0.46f);
        fire.anchorMax = new Vector2(0.52f, 0.46f);
        fire.pivot = new Vector2(0.5f, 0.5f);
        fire.anchoredPosition = Vector2.zero;
        fire.sizeDelta = new Vector2(14f, 18f);

        RectTransform figure = CreatePanel("Thumbnail Figure", parent, new Color(0.54f, 0.48f, 0.40f, 1f));
        figure.anchorMin = new Vector2(0.38f, 0.35f);
        figure.anchorMax = new Vector2(0.38f, 0.35f);
        figure.pivot = new Vector2(0.5f, 0f);
        figure.anchoredPosition = Vector2.zero;
        figure.sizeDelta = new Vector2(8f, 23f);
    }

    private void UpdateFakeScreenshotVisuals(float elapsed)
    {
        float flashAlpha = elapsed < 0.035f ? 0.22f : Mathf.Clamp01(1f - (elapsed - 0.035f) / 0.12f) * 0.18f;
        flashAlpha = Mathf.Pow(flashAlpha, 1.65f);
        if (fakeScreenshotFlashImage != null)
        {
            fakeScreenshotFlashImage.color = new Color(1f, 1f, 1f, flashAlpha);
        }

        if (fakeScreenshotFrameGroup != null)
        {
            fakeScreenshotFrameGroup.alpha = 0f;
        }

        UpdateFakeScreenshotToastVisual(elapsed);
    }

    private void UpdateFakeScreenshotToastVisual(float elapsed)
    {
        if (fakeScreenshotToastRoot == null || fakeScreenshotToastGroup == null)
        {
            return;
        }

        float toastEnd = FakeScreenshotToastInSeconds + FakeScreenshotToastHoldSeconds;
        float inT = Mathf.Clamp01(elapsed / FakeScreenshotToastInSeconds);
        float outT = Mathf.Clamp01((elapsed - toastEnd) / FakeScreenshotToastOutSeconds);
        float easedIn = 1f - Mathf.Pow(1f - inT, 3f);
        float alpha = Mathf.Clamp01(easedIn * (1f - outT));
        fakeScreenshotToastGroup.alpha = alpha;
        fakeScreenshotToastRoot.anchoredPosition = new Vector2(
            Mathf.Lerp(430f, -28f, easedIn) + outT * 18f,
            28f);
    }

    private void EnsureFakeScreenshotAudio()
    {
        if (fakeScreenshotAudioSource != null)
        {
            return;
        }

        GameObject audioObject = new GameObject("Fake Steam Screenshot Audio");
        audioObject.transform.SetParent(transform, false);
        fakeScreenshotAudioClip = CreateFakeScreenshotAudioClip();
        fakeScreenshotAudioSource = audioObject.AddComponent<AudioSource>();
        fakeScreenshotAudioSource.clip = fakeScreenshotAudioClip;
        fakeScreenshotAudioSource.loop = false;
        fakeScreenshotAudioSource.playOnAwake = false;
        fakeScreenshotAudioSource.spatialBlend = 0f;
        fakeScreenshotAudioSource.volume = 0.78f;
        fakeScreenshotAudioSource.pitch = 1f;
        fakeScreenshotAudioSource.priority = 8;
        RouteAudioSource(fakeScreenshotAudioSource, BridgeAudioBus.Vhs);
    }

    private void CaptureFakeScreenshotThumbnail()
    {
        ReleaseFakeScreenshotCapturedTexture();
        if (fakeScreenshotThumbnailImage != null)
        {
            fakeScreenshotThumbnailImage.texture = null;
            fakeScreenshotThumbnailImage.color = new Color(1f, 1f, 1f, 0f);
        }

        try
        {
            fakeScreenshotCapturedTexture = ScreenCapture.CaptureScreenshotAsTexture(1);
        }
        catch (Exception exception)
        {
            WriteDebugWarningLog("FAKE_SCREENSHOT", "CaptureScreenshotAsTexture failed: " + exception.Message);
            fakeScreenshotCapturedTexture = null;
        }

        if (fakeScreenshotCapturedTexture == null || fakeScreenshotThumbnailImage == null)
        {
            return;
        }

        fakeScreenshotCapturedTexture.name = "Fake Screenshot Captured Thumbnail";
        fakeScreenshotThumbnailImage.texture = fakeScreenshotCapturedTexture;
        fakeScreenshotThumbnailImage.color = Color.white;
    }

    private void ReleaseFakeScreenshotCapturedTexture()
    {
        if (fakeScreenshotCapturedTexture == null)
        {
            return;
        }

        Destroy(fakeScreenshotCapturedTexture);
        fakeScreenshotCapturedTexture = null;
    }

    private void ReleaseFakeScreenshotResourcesForShutdown()
    {
        ReleaseFakeScreenshotCapturedTexture();
    }

    private AudioClip CreateFakeScreenshotAudioClip()
    {
        const int sampleRate = 44100;
        const float duration = 0.56f;
        const int channels = 2;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples * channels];
        const float tau = Mathf.PI * 2f;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float snapA = FakeScreenshotClickEnvelope(t, 0.012f, 0.030f);
            float snapB = FakeScreenshotClickEnvelope(t, 0.078f, 0.042f);
            float body = Mathf.Sin(tau * 156f * t) * FakeScreenshotClickEnvelope(t, 0.010f, 0.16f) * 0.18f;
            float noise = FakeScreenshotNoise(i) * (snapA * 0.72f + snapB * 0.58f);
            float shutter = Mathf.Sin(tau * 760f * t) * snapA * 0.16f +
                Mathf.Sin(tau * 1080f * t + 0.32f) * snapB * 0.20f;
            float tick = Mathf.Sign(Mathf.Sin(tau * 1850f * t)) * snapB * 0.10f;
            float tail = Mathf.Sin(tau * 1220f * t) * FakeScreenshotClickEnvelope(t, 0.176f, 0.10f) * 0.035f;
            float sample = Mathf.Clamp((noise + shutter + body + tick + tail) * 0.78f, -0.95f, 0.95f);
            float stereo = FakeScreenshotNoise(i + 911) * 0.035f;
            data[i * channels] = Mathf.Clamp(sample * (1f - stereo), -0.95f, 0.95f);
            data[i * channels + 1] = Mathf.Clamp(sample * (1f + stereo), -0.95f, 0.95f);
        }

        AudioClip clip = AudioClip.Create("Generated Fake Steam Screenshot", samples, channels, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private float FakeScreenshotClickEnvelope(float t, float start, float decay)
    {
        if (t < start)
        {
            return 0f;
        }

        return Mathf.Exp(-(t - start) / Mathf.Max(0.001f, decay));
    }

    private float FakeScreenshotNoise(int sample)
    {
        return Mathf.Repeat(Mathf.Sin((sample + 1) * 12.9898f) * 43758.5453f, 1f) * 2f - 1f;
    }

    private bool WasFakeScreenshotPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F1);
#endif
    }
}
