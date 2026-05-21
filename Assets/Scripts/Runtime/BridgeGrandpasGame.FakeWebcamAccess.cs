using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const int FakeWebcamGlitchBarCount = 14;
    private const int FakeWebcamScanlineCount = 18;

    private Canvas fakeWebcamCanvas;
    private CanvasGroup fakeWebcamGroup;
    private RectTransform fakeWebcamPanel;
    private RectTransform fakeWebcamPreviewFrame;
    private RawImage fakeWebcamPreviewImage;
    private Image fakeWebcamFallbackImage;
    private Text fakeWebcamTitleText;
    private Text fakeWebcamBodyText;
    private Text fakeWebcamStatusText;
    private RectTransform fakeWebcamAllowButtonRoot;
    private RectTransform fakeWebcamDenyButtonRoot;
    private RectTransform fakeWebcamCloseButtonRoot;
    private RectTransform[] fakeWebcamGlitchBars;
    private Image[] fakeWebcamGlitchImages;
    private RectTransform[] fakeWebcamScanlines;
    private AudioSource fakeWebcamAudioSource;
    private AudioClip fakeWebcamAudioClip;
    private WebCamTexture fakeWebcamTexture;
    private Texture2D fakeWebcamPixelTexture;
    private Color32[] fakeWebcamPixelBuffer;
    private Coroutine fakeWebcamRequestRoutine;
    private bool fakeWebcamAccessActive;
    private bool fakeWebcamAwaitingConsent;
    private bool fakeWebcamDeniedInGame;
    private bool fakeWebcamPermissionResolved;
    private bool fakeWebcamPermissionGranted;
    private bool fakeWebcamDeviceMissing;
    private float fakeWebcamStartedAt;
    private float fakeWebcamNextPixelUpdateAt;

    private bool UpdateFakeWebcamAccess(float deltaTime)
    {
        if (WasFakeWebcamAccessPressed())
        {
            BeginFakeWebcamAccess();
        }

        if (!fakeWebcamAccessActive)
        {
            return false;
        }

        if (WasEscapePressed())
        {
            EndFakeWebcamAccess("escape");
            return true;
        }

        UpdateFakeWebcamAccessVisuals(Time.unscaledTime - fakeWebcamStartedAt);
        return true;
    }

    private void BeginFakeWebcamAccess()
    {
        if (!gameStarted || escapeMenuOpen || fakeWebcamAccessActive || fakeSystemScanActive ||
            fakeMicrophoneCheckActive || fakeAudioRecordingActive || fakeCreditsActive ||
            fakeUnityErrorModalActive || fakeUnityErrorGrandpasHidden || fakeUnityErrorWebcamMenuActive ||
            fakeUnityErrorReturnGlitchActive || escapeMenuBsodActive || fakeCorruptedAccountRevealActive)
        {
            return;
        }

        EnsureFakeWebcamAccessVisuals();
        EnsureFakeWebcamAccessAudio();
        fakeWebcamAccessActive = true;
        fakeWebcamAwaitingConsent = true;
        fakeWebcamDeniedInGame = false;
        fakeWebcamPermissionResolved = false;
        fakeWebcamPermissionGranted = false;
        fakeWebcamDeviceMissing = false;
        fakeWebcamStartedAt = Time.unscaledTime;

        if (fakeWebcamCanvas != null)
        {
            fakeWebcamCanvas.gameObject.SetActive(true);
        }

        if (fakeWebcamGroup != null)
        {
            fakeWebcamGroup.alpha = 1f;
        }

        if (fakeWebcamPreviewImage != null)
        {
            fakeWebcamPreviewImage.texture = null;
            fakeWebcamPreviewImage.color = new Color(0f, 0f, 0f, 0.55f);
        }

        if (fakeWebcamAudioSource != null && fakeWebcamAudioClip != null)
        {
            fakeWebcamAudioSource.Stop();
            fakeWebcamAudioSource.time = 0f;
            fakeWebcamAudioSource.Play();
        }

        ApplyFakeWebcamConsentButtons();
        UpdateFakeWebcamAccessText();
        WriteDebugLog("FAKE_WEBCAM", "F7 webcam consent gate opened. Webcam not touched yet.");
    }

    private void OnFakeWebcamAllowPressed()
    {
        if (!fakeWebcamAccessActive || !fakeWebcamAwaitingConsent || fakeWebcamRequestRoutine != null)
        {
            return;
        }

        fakeWebcamAwaitingConsent = false;
        fakeWebcamDeniedInGame = false;
        ApplyFakeWebcamConsentButtons();
        fakeWebcamRequestRoutine = StartCoroutine(RequestFakeWebcamAuthorization());
        UpdateFakeWebcamAccessText();
        WriteDebugLog("FAKE_WEBCAM", "In-game webcam consent accepted. Requesting OS authorization.");
    }

    private void OnFakeWebcamDenyPressed()
    {
        if (!fakeWebcamAccessActive || !fakeWebcamAwaitingConsent)
        {
            return;
        }

        fakeWebcamAwaitingConsent = false;
        fakeWebcamDeniedInGame = true;
        fakeWebcamPermissionResolved = true;
        fakeWebcamPermissionGranted = false;
        fakeWebcamDeviceMissing = false;
        StopFakeWebcamTexture();
        ApplyFakeWebcamConsentButtons();
        UpdateFakeWebcamAccessText();
        WriteDebugLog("FAKE_WEBCAM", "In-game webcam consent denied. OS authorization was not requested.");
    }

    private IEnumerator RequestFakeWebcamAuthorization()
    {
        yield return null;
        AsyncOperation operation = Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (operation != null)
        {
            yield return operation;
        }

        fakeWebcamPermissionResolved = true;
        fakeWebcamPermissionGranted = Application.HasUserAuthorization(UserAuthorization.WebCam);
        if (fakeWebcamPermissionGranted)
        {
            StartFakeWebcamPreview();
        }

        UpdateFakeWebcamAccessText();
        ApplyFakeWebcamConsentButtons();
        fakeWebcamRequestRoutine = null;
    }

    private void StartFakeWebcamPreview()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices == null || devices.Length == 0)
        {
            fakeWebcamDeviceMissing = true;
            fakeWebcamPermissionGranted = false;
            WriteDebugLog("FAKE_WEBCAM", "Webcam permission granted but no camera devices found.");
            return;
        }

        StopFakeWebcamTexture();
        fakeWebcamTexture = new WebCamTexture(devices[0].name, 640, 480, 15);
        fakeWebcamTexture.Play();
        if (fakeWebcamPreviewImage != null)
        {
            fakeWebcamPreviewImage.texture = null;
            fakeWebcamPreviewImage.color = Color.white;
        }

        WriteDebugLog("FAKE_WEBCAM", "Webcam preview started. deviceCount=" + devices.Length);
    }

    private void EndFakeWebcamAccess(string reason)
    {
        if (!fakeWebcamAccessActive)
        {
            return;
        }

        fakeWebcamAccessActive = false;
        fakeWebcamAwaitingConsent = false;
        if (fakeWebcamRequestRoutine != null)
        {
            StopCoroutine(fakeWebcamRequestRoutine);
            fakeWebcamRequestRoutine = null;
        }

        StopFakeWebcamTexture();
        if (fakeWebcamAudioSource != null)
        {
            fakeWebcamAudioSource.Stop();
        }

        if (fakeWebcamCanvas != null)
        {
            fakeWebcamCanvas.gameObject.SetActive(false);
        }

        WriteDebugLog("FAKE_WEBCAM", "Webcam overlay closed. reason=" + reason);
    }

    private void StopFakeWebcamTexture()
    {
        if (fakeWebcamTexture != null)
        {
            fakeWebcamTexture.Stop();
            fakeWebcamTexture = null;
        }

        if (fakeWebcamPixelTexture != null)
        {
            Destroy(fakeWebcamPixelTexture);
            fakeWebcamPixelTexture = null;
            fakeWebcamPixelBuffer = null;
        }

        if (fakeWebcamPreviewImage != null)
        {
            fakeWebcamPreviewImage.texture = null;
        }
    }

    private void UpdateFakeWebcamAccessVisuals(float elapsed)
    {
        float fade = Mathf.Clamp01(elapsed / 0.16f);
        if (fakeWebcamGroup != null)
        {
            fakeWebcamGroup.alpha = Mathf.SmoothStep(0f, 1f, fade);
        }

        int tick = Mathf.FloorToInt(Time.unscaledTime * 42f);
        if (fakeWebcamPanel != null)
        {
            float shake = fakeWebcamPermissionGranted ? 1.8f : 0.55f;
            fakeWebcamPanel.anchoredPosition = new Vector2(
                FakeUnityErrorSignedNoise(tick, 811) * shake,
                FakeUnityErrorSignedNoise(tick, 812) * shake);
        }

        if (fakeWebcamPreviewFrame != null)
        {
            float scale = 1f + (fakeWebcamPermissionGranted ? Mathf.Sin(Time.unscaledTime * 19f) * 0.006f : 0f);
            fakeWebcamPreviewFrame.localScale = new Vector3(scale, scale, 1f);
        }

        if (fakeWebcamPermissionGranted)
        {
            UpdateFakeWebcamPixelatedPreview(tick);
        }

        if (fakeWebcamPreviewImage != null && fakeWebcamPreviewImage.texture != null)
        {
            float offsetX = FakeWebcamNoise01(tick, 5) * 0.020f - 0.010f;
            float offsetY = FakeWebcamNoise01(tick, 6) * 0.012f - 0.006f;
            fakeWebcamPreviewImage.uvRect = new Rect(offsetX, offsetY, 1f, 1f);
            float tint = 0.90f + FakeWebcamNoise01(tick, 7) * 0.10f;
            fakeWebcamPreviewImage.color = new Color(tint, tint * 0.82f, tint * 0.78f, 1f);
        }

        UpdateFakeWebcamGlitches(tick);
        UpdateFakeWebcamAccessText();
    }

    private void UpdateFakeWebcamPixelatedPreview(int tick)
    {
        if (fakeWebcamTexture == null || fakeWebcamPreviewImage == null || Time.unscaledTime < fakeWebcamNextPixelUpdateAt)
        {
            return;
        }

        if (fakeWebcamTexture.width < 32 || fakeWebcamTexture.height < 32)
        {
            return;
        }

        fakeWebcamNextPixelUpdateAt = Time.unscaledTime + 0.055f;
        const int pixelWidth = 96;
        const int pixelHeight = 72;
        if (fakeWebcamPixelTexture == null)
        {
            fakeWebcamPixelTexture = new Texture2D(pixelWidth, pixelHeight, TextureFormat.RGBA32, false);
            fakeWebcamPixelTexture.filterMode = FilterMode.Point;
            fakeWebcamPixelTexture.wrapMode = TextureWrapMode.Clamp;
            fakeWebcamPixelBuffer = new Color32[pixelWidth * pixelHeight];
            fakeWebcamPreviewImage.texture = fakeWebcamPixelTexture;
        }

        Color32[] source = fakeWebcamTexture.GetPixels32();
        int sourceWidth = fakeWebcamTexture.width;
        int sourceHeight = fakeWebcamTexture.height;
        if (source == null || source.Length < sourceWidth * sourceHeight)
        {
            return;
        }

        for (int y = 0; y < pixelHeight; y++)
        {
            int sourceY = Mathf.Clamp(Mathf.RoundToInt((y + 0.5f) / pixelHeight * sourceHeight), 0, sourceHeight - 1);
            for (int x = 0; x < pixelWidth; x++)
            {
                int sourceX = Mathf.Clamp(Mathf.RoundToInt((x + 0.5f) / pixelWidth * sourceWidth), 0, sourceWidth - 1);
                Color32 color = source[sourceY * sourceWidth + sourceX];
                fakeWebcamPixelBuffer[y * pixelWidth + x] = FakeWebcamObscuredPixel(color, x, y, tick);
            }
        }

        fakeWebcamPixelTexture.SetPixels32(fakeWebcamPixelBuffer);
        fakeWebcamPixelTexture.Apply(false);
    }

    private Color32 FakeWebcamObscuredPixel(Color32 source, int x, int y, int tick)
    {
        float grayscale = (source.r * 0.24f + source.g * 0.58f + source.b * 0.18f) / 255f;
        float noise = FakeWebcamNoise01(tick + x * 11 + y * 17, 51);
        float band = Mathf.Sin((y + tick * 0.15f) * 0.42f) * 0.5f + 0.5f;
        float posterized = Mathf.Floor(Mathf.Clamp01(grayscale + (noise - 0.5f) * 0.34f) * 4f) / 4f;
        float mask = noise < 0.055f || (x + tick) % 37 == 0 ? 0.0f : 1f;
        byte r = (byte)Mathf.Clamp(Mathf.RoundToInt((posterized * 190f + band * 28f) * mask), 0, 255);
        byte g = (byte)Mathf.Clamp(Mathf.RoundToInt((posterized * 132f + noise * 65f) * mask), 0, 255);
        byte b = (byte)Mathf.Clamp(Mathf.RoundToInt((posterized * 118f + 34f) * mask), 0, 255);
        return new Color32(r, g, b, 255);
    }

    private void UpdateFakeWebcamAccessText()
    {
        if (fakeWebcamTitleText == null || fakeWebcamBodyText == null || fakeWebcamStatusText == null)
        {
            return;
        }

        fakeWebcamTitleText.text = "observer.dll запрашивает доступ к камере";
        if (fakeWebcamAwaitingConsent)
        {
            fakeWebcamBodyText.text =
                "Камера пока не включена.\n\n" +
                "Сначала нужно твоё разрешение внутри игры.\n" +
                "После этого Windows/Unity может показать настоящий системный запрос.\n" +
                "Кадры не сохраняются.";
            fakeWebcamStatusText.text = "WAITING FOR IN-GAME CONSENT";
            return;
        }

        if (fakeWebcamDeniedInGame)
        {
            fakeWebcamBodyText.text =
                "Ты сказал нет.\n\n" +
                "Камера не была запрошена.\n" +
                "observer.dll делает вид, что уважает границы.\n" +
                "Очень убедительно.";
            fakeWebcamStatusText.text = "LOCAL CONSENT DENIED";
            return;
        }

        if (!fakeWebcamPermissionResolved)
        {
            fakeWebcamBodyText.text =
                "Системный запрос уже отправлен.\n\n" +
                "Разрешение нужно только для живого превью.\n" +
                "Кадры не сохраняются.\n" +
                "Он всё равно считает секунды.";
            fakeWebcamStatusText.text = "WAITING FOR OS PERMISSION";
            return;
        }

        if (fakeWebcamDeviceMissing)
        {
            fakeWebcamBodyText.text =
                "Разрешение получено.\n\n" +
                "Камера не найдена.\n" +
                "Очень удобно.";
            fakeWebcamStatusText.text = "NO CAMERA DEVICE";
            return;
        }

        if (!fakeWebcamPermissionGranted)
        {
            fakeWebcamBodyText.text =
                "Доступ запрещён.\n\n" +
                "Запрос был замечен.\n" +
                "Отказ записан не туда.";
            fakeWebcamStatusText.text = "ACCESS DENIED";
            return;
        }

        fakeWebcamBodyText.text =
            "Доступ разрешён.\n\n" +
            "LIVE PREVIEW: active\n" +
            "observer_face: unstable\n" +
            "save_frame: false";
        fakeWebcamStatusText.text = "LIVE / OBSERVER LOCATED";
    }

    private void UpdateFakeWebcamGlitches(int tick)
    {
        if (fakeWebcamFallbackImage != null)
        {
            float alpha = fakeWebcamPermissionGranted ? 0.28f + FakeWebcamNoise01(tick, 21) * 0.24f : 0.48f;
            fakeWebcamFallbackImage.color = new Color(0.05f, 0.65f, 0.70f, alpha);
        }

        for (int i = 0; fakeWebcamGlitchBars != null && i < fakeWebcamGlitchBars.Length; i++)
        {
            bool visible = fakeWebcamPermissionGranted
                ? (tick + i * 7) % 11 < 3
                : (tick + i * 5) % 23 == 0;
            fakeWebcamGlitchBars[i].gameObject.SetActive(visible);
            if (!visible)
            {
                continue;
            }

            float y = Mathf.Lerp(-188f, 188f, FakeWebcamNoise01(tick + i, 31));
            float x = Mathf.Lerp(-24f, 24f, FakeWebcamNoise01(tick + i, 32));
            fakeWebcamGlitchBars[i].anchoredPosition = new Vector2(x, y);
            fakeWebcamGlitchBars[i].sizeDelta = new Vector2(
                Mathf.Lerp(120f, 720f, FakeWebcamNoise01(tick + i, 33)),
                Mathf.Lerp(4f, 28f, FakeWebcamNoise01(tick + i, 34)));
            if (fakeWebcamGlitchImages != null && i < fakeWebcamGlitchImages.Length)
            {
                fakeWebcamGlitchImages[i].color = i % 3 == 0
                    ? new Color(0.75f, 0.05f, 0.04f, 0.48f)
                    : new Color(0.08f, 0.95f, 1f, 0.34f);
            }
        }

        for (int i = 0; fakeWebcamScanlines != null && i < fakeWebcamScanlines.Length; i++)
        {
            float y = -202f + i * 24f + Mathf.Sin(Time.unscaledTime * 4f + i) * 2f;
            fakeWebcamScanlines[i].anchoredPosition = new Vector2(0f, y);
        }
    }

    private float FakeWebcamNoise01(int tick, int salt)
    {
        return Mathf.Repeat(Mathf.Sin((tick + 1) * (19.143f + salt * 5.37f)) * 24634.634f, 1f);
    }

    private void ApplyFakeWebcamConsentButtons()
    {
        if (fakeWebcamAllowButtonRoot != null)
        {
            fakeWebcamAllowButtonRoot.gameObject.SetActive(fakeWebcamAwaitingConsent);
        }

        if (fakeWebcamDenyButtonRoot != null)
        {
            fakeWebcamDenyButtonRoot.gameObject.SetActive(fakeWebcamAwaitingConsent);
        }

        if (fakeWebcamCloseButtonRoot != null)
        {
            fakeWebcamCloseButtonRoot.gameObject.SetActive(true);
        }
    }

    private bool WasFakeWebcamAccessPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f7Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F7);
#endif
    }
}
