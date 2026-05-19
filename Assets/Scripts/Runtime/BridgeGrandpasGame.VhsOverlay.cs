using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private Canvas vhsCanvas;
    private RectTransform vhsRoot;
    private RectTransform vhsFrameRoot;
    private RectTransform vhsLeftMatte;
    private RectTransform vhsRightMatte;
    private RectTransform vhsTopMatte;
    private RectTransform vhsBottomMatte;
    private RectTransform vhsTrackingBand;
    private RectTransform vhsHeadSwitchBand;
    private RectTransform vhsWhiteTear;
    private RectTransform vhsZoomTrack;
    private RectTransform vhsZoomFill;
    private RectTransform[] vhsNoiseBlocks;
    private Text vhsRecText;
    private Text vhsTimeText;
    private Text vhsZoomText;
    private Text vhsStatusText;
    private Image vhsRecDot;
    private Image vhsTrackingImage;
    private Image vhsHeadSwitchImage;
    private Image vhsWhiteTearImage;
    private Image[] vhsNoiseImages;
    private CanvasGroup hudCanvasGroup;
    private CanvasGroup vhsGroup;
    private CanvasGroup vhsScanlineGroup;
    private float vhsRecordTime;
    private float vhsZoomPulse;
    private float vhsTrackingPulse;
    private bool vhsModeEnabled;

    private void SetupVhsOverlay()
    {
        GameObject canvasObject = new GameObject("VHS Camera Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        vhsCanvas = canvasObject.GetComponent<Canvas>();
        vhsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        vhsCanvas.sortingOrder = 90;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        vhsRoot = CreatePanel("VHS Root", canvasObject.transform, new Color(0f, 0f, 0f, 0f));
        vhsRoot.anchorMin = Vector2.zero;
        vhsRoot.anchorMax = Vector2.one;
        vhsRoot.offsetMin = Vector2.zero;
        vhsRoot.offsetMax = Vector2.zero;
        vhsGroup = vhsRoot.gameObject.AddComponent<CanvasGroup>();
        vhsGroup.interactable = false;
        vhsGroup.blocksRaycasts = false;
        hudCanvasGroup = canvas == null ? null : canvas.GetComponent<CanvasGroup>();
        if (canvas != null && hudCanvasGroup == null)
        {
            hudCanvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
        }

        CreateVhsFrame();
        CreateVhsReadouts();
        CreateVhsScanlines();
        CreateVhsTrackingBand();
        CreateVhsNoise();
        SetVhsMode(false);
    }

    private void CreateVhsFrame()
    {
        vhsLeftMatte = CreateVhsPanel("VHS Left Matte", vhsRoot, Color.black);
        vhsRightMatte = CreateVhsPanel("VHS Right Matte", vhsRoot, Color.black);
        vhsTopMatte = CreateVhsPanel("VHS Top Matte", vhsRoot, Color.black);
        vhsBottomMatte = CreateVhsPanel("VHS Bottom Matte", vhsRoot, Color.black);
        vhsFrameRoot = CreateVhsPanel("VHS 4x3 Frame", vhsRoot, new Color(0f, 0f, 0f, 0f));
        vhsFrameRoot.anchorMin = new Vector2(0.5f, 0.5f);
        vhsFrameRoot.anchorMax = new Vector2(0.5f, 0.5f);
        vhsFrameRoot.pivot = new Vector2(0.5f, 0.5f);
        vhsFrameRoot.anchoredPosition = Vector2.zero;
        vhsFrameRoot.sizeDelta = new Vector2(1200f, 900f);

        CreateCorner(vhsFrameRoot, "Top Left", new Vector2(0f, 1f), new Vector2(1f, -1f));
        CreateCorner(vhsFrameRoot, "Top Right", new Vector2(1f, 1f), new Vector2(-1f, -1f));
        CreateCorner(vhsFrameRoot, "Bottom Left", new Vector2(0f, 0f), new Vector2(1f, 1f));
        CreateCorner(vhsFrameRoot, "Bottom Right", new Vector2(1f, 0f), new Vector2(-1f, 1f));
    }

    private void CreateVhsReadouts()
    {
        vhsRecText = CreateVhsText("VHS Rec", vhsFrameRoot, 18, TextAnchor.MiddleLeft, new Color(1f, 0.33f, 0.28f));
        PlaceVhsText(vhsRecText.rectTransform, new Vector2(0f, 1f), new Vector2(78f, -82f), new Vector2(120f, 28f));
        vhsRecText.text = "REC";

        RectTransform dot = CreateVhsPanel("VHS Rec Dot", vhsFrameRoot, new Color(1f, 0.08f, 0.05f, 0.95f));
        dot.anchorMin = new Vector2(0f, 1f);
        dot.anchorMax = new Vector2(0f, 1f);
        dot.pivot = new Vector2(0.5f, 0.5f);
        dot.anchoredPosition = new Vector2(52f, -68f);
        dot.sizeDelta = new Vector2(14f, 14f);
        vhsRecDot = dot.GetComponent<Image>();

        vhsTimeText = CreateVhsText("VHS Timecode", vhsFrameRoot, 18, TextAnchor.MiddleRight, new Color(0.84f, 0.96f, 0.92f));
        PlaceVhsText(vhsTimeText.rectTransform, new Vector2(1f, 1f), new Vector2(-42f, -82f), new Vector2(280f, 28f));

        vhsZoomText = CreateVhsText("VHS Zoom", vhsFrameRoot, 18, TextAnchor.MiddleLeft, new Color(0.82f, 0.94f, 1f));
        PlaceVhsText(vhsZoomText.rectTransform, new Vector2(0f, 0f), new Vector2(48f, 88f), new Vector2(260f, 28f));
        vhsZoomTrack = CreateVhsPanel("VHS Zoom Track", vhsFrameRoot, new Color(0.18f, 0.28f, 0.32f, 0.50f));
        vhsZoomTrack.anchorMin = new Vector2(0f, 0f);
        vhsZoomTrack.anchorMax = new Vector2(0f, 0f);
        vhsZoomTrack.pivot = new Vector2(0f, 0.5f);
        vhsZoomTrack.anchoredPosition = new Vector2(48f, 68f);
        vhsZoomTrack.sizeDelta = new Vector2(164f, 8f);
        vhsZoomFill = CreateVhsPanel("VHS Zoom Fill", vhsZoomTrack, new Color(0.76f, 0.96f, 1f, 0.86f));
        vhsZoomFill.anchorMin = new Vector2(0f, 0f);
        vhsZoomFill.anchorMax = new Vector2(0f, 1f);
        vhsZoomFill.pivot = new Vector2(0f, 0.5f);
        vhsZoomFill.anchoredPosition = Vector2.zero;
        vhsZoomFill.sizeDelta = new Vector2(50f, 0f);

        vhsStatusText = CreateVhsText("VHS Status", vhsFrameRoot, 16, TextAnchor.MiddleRight, new Color(0.86f, 0.92f, 0.88f));
        PlaceVhsText(vhsStatusText.rectTransform, new Vector2(1f, 0f), new Vector2(-42f, 88f), new Vector2(440f, 28f));
        vhsStatusText.text = "CAM 03  UNDERPASS  SP";
    }

    private void CreateVhsScanlines()
    {
        RectTransform root = CreateVhsPanel("VHS Scanlines", vhsFrameRoot, new Color(0f, 0f, 0f, 0f));
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        vhsScanlineGroup = root.gameObject.AddComponent<CanvasGroup>();
        vhsScanlineGroup.interactable = false;
        vhsScanlineGroup.blocksRaycasts = false;

        for (int i = 0; i < 30; i++)
        {
            RectTransform line = CreateVhsPanel("VHS Scanline " + i, root, new Color(0f, 0f, 0f, 0.18f));
            line.anchorMin = new Vector2(0f, i / 30f);
            line.anchorMax = new Vector2(1f, i / 30f);
            line.pivot = new Vector2(0.5f, 0.5f);
            line.sizeDelta = new Vector2(0f, 1.4f);
            line.anchoredPosition = Vector2.zero;
        }
    }

    private void CreateVhsTrackingBand()
    {
        vhsTrackingBand = CreateVhsPanel("VHS Tracking Band", vhsFrameRoot, new Color(0.70f, 0.88f, 1f, 0.12f));
        vhsTrackingBand.anchorMin = new Vector2(0f, 0.5f);
        vhsTrackingBand.anchorMax = new Vector2(1f, 0.5f);
        vhsTrackingBand.pivot = new Vector2(0.5f, 0.5f);
        vhsTrackingBand.sizeDelta = new Vector2(0f, 20f);
        vhsTrackingImage = vhsTrackingBand.GetComponent<Image>();
    }

    private void CreateVhsNoise()
    {
        vhsHeadSwitchBand = CreateVhsPanel("VHS Head Switch Band", vhsFrameRoot, new Color(0.78f, 0.90f, 1f, 0.10f));
        vhsHeadSwitchBand.anchorMin = new Vector2(0f, 0f);
        vhsHeadSwitchBand.anchorMax = new Vector2(1f, 0f);
        vhsHeadSwitchBand.pivot = new Vector2(0.5f, 0f);
        vhsHeadSwitchBand.sizeDelta = new Vector2(0f, 22f);
        vhsHeadSwitchImage = vhsHeadSwitchBand.GetComponent<Image>();

        vhsWhiteTear = CreateVhsPanel("VHS White Tear", vhsFrameRoot, new Color(0.90f, 0.96f, 1f, 0.18f));
        vhsWhiteTear.anchorMin = new Vector2(0f, 0.5f);
        vhsWhiteTear.anchorMax = new Vector2(1f, 0.5f);
        vhsWhiteTear.pivot = new Vector2(0.5f, 0.5f);
        vhsWhiteTear.sizeDelta = new Vector2(0f, 8f);
        vhsWhiteTearImage = vhsWhiteTear.GetComponent<Image>();

        vhsNoiseBlocks = new RectTransform[18];
        vhsNoiseImages = new Image[vhsNoiseBlocks.Length];
        for (int i = 0; i < vhsNoiseBlocks.Length; i++)
        {
            RectTransform block = CreateVhsPanel("VHS Dropout " + i, vhsFrameRoot, new Color(0.86f, 0.94f, 1f, 0.08f));
            block.anchorMin = new Vector2(0f, 0f);
            block.anchorMax = new Vector2(0f, 0f);
            block.pivot = new Vector2(0.5f, 0.5f);
            block.sizeDelta = new Vector2(18f + i % 5 * 9f, 3f + i % 3 * 4f);
            vhsNoiseBlocks[i] = block;
            vhsNoiseImages[i] = block.GetComponent<Image>();
        }
    }

    private void UpdateVhsOverlay(float deltaTime)
    {
        if (WasVhsTogglePressed())
        {
            SetVhsMode(!vhsModeEnabled);
        }

        if (vhsRoot == null || !vhsModeEnabled)
        {
            ApplyVhsPostEffects(false, 0f, 0f);
            return;
        }

        vhsRecordTime += deltaTime;
        vhsZoomPulse = Mathf.Max(0f, vhsZoomPulse - deltaTime * 2.8f);
        vhsTrackingPulse = Mathf.Max(0f, vhsTrackingPulse - deltaTime * 1.7f);
        ApplyVhsCameraViewport(true);
        UpdateVhsFrame();
        UpdateVhsReadouts();
        UpdateVhsMotion();
        ApplyVhsPostEffects(true, vhsZoomPulse, vhsTrackingPulse);
    }

    private void UpdateVhsFrame()
    {
        float height = vhsRoot.rect.height;
        float width = vhsRoot.rect.width;
        Rect viewport = mainCamera == null ? new Rect(0f, 0f, 1f, 1f) : mainCamera.rect;
        float left = viewport.xMin * width;
        float right = (1f - viewport.xMax) * width;
        float bottom = viewport.yMin * height;
        float top = (1f - viewport.yMax) * height;
        float frameWidth = viewport.width * width;
        float frameHeight = viewport.height * height;

        vhsLeftMatte.anchorMin = new Vector2(0f, 0f);
        vhsLeftMatte.anchorMax = new Vector2(0f, 1f);
        vhsLeftMatte.pivot = new Vector2(0f, 0.5f);
        vhsLeftMatte.sizeDelta = new Vector2(left, 0f);
        vhsLeftMatte.anchoredPosition = Vector2.zero;
        vhsRightMatte.anchorMin = new Vector2(1f, 0f);
        vhsRightMatte.anchorMax = new Vector2(1f, 1f);
        vhsRightMatte.pivot = new Vector2(1f, 0.5f);
        vhsRightMatte.sizeDelta = new Vector2(right, 0f);
        vhsRightMatte.anchoredPosition = Vector2.zero;

        vhsTopMatte.anchorMin = new Vector2(0f, 1f);
        vhsTopMatte.anchorMax = new Vector2(1f, 1f);
        vhsTopMatte.pivot = new Vector2(0.5f, 1f);
        vhsTopMatte.sizeDelta = new Vector2(0f, top);
        vhsTopMatte.anchoredPosition = Vector2.zero;
        vhsBottomMatte.anchorMin = new Vector2(0f, 0f);
        vhsBottomMatte.anchorMax = new Vector2(1f, 0f);
        vhsBottomMatte.pivot = new Vector2(0.5f, 0f);
        vhsBottomMatte.sizeDelta = new Vector2(0f, bottom);
        vhsBottomMatte.anchoredPosition = Vector2.zero;
        vhsFrameRoot.anchoredPosition = new Vector2((left - right) * 0.5f, (bottom - top) * 0.5f);
        vhsFrameRoot.sizeDelta = new Vector2(frameWidth, frameHeight);
    }

    private void UpdateVhsReadouts()
    {
        int totalSeconds = Mathf.FloorToInt(vhsRecordTime);
        int frames = Mathf.FloorToInt((vhsRecordTime - totalSeconds) * 25f);
        int seconds = totalSeconds % 60;
        int minutes = totalSeconds / 60 % 60;
        int hours = totalSeconds / 3600;
        vhsTimeText.text = hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + frames.ToString("00") + "  23.10.1998";

        float zoom = mainCamera == null ? 1f : CameraDefaultZoom / Mathf.Max(0.01f, mainCamera.orthographicSize);
        vhsZoomText.text = vhsZoomPulse > 0.02f ? "ZOOM x" + zoom.ToString("0.0") : "AUTO TRACKING";
        UpdateVhsZoomSlider();
        float blink = Mathf.PingPong(Time.time * 2.4f, 1f);
        vhsRecDot.color = new Color(1f, 0.08f, 0.05f, 0.35f + blink * 0.60f);
    }

    private void UpdateVhsZoomSlider()
    {
        if (mainCamera == null || vhsZoomFill == null)
        {
            return;
        }

        float zoom01 = Mathf.InverseLerp(CameraMaxZoom, CameraMinZoom, mainCamera.orthographicSize);
        vhsZoomFill.sizeDelta = new Vector2(Mathf.Lerp(14f, 164f, zoom01), 0f);
    }

    private void UpdateVhsMotion()
    {
        vhsFrameRoot.anchoredPosition = Vector2.zero;
        vhsGroup.alpha = 1f;
        vhsScanlineGroup.alpha = 0.55f + Mathf.Sin(Time.time * 9.3f) * 0.18f;
        float bandY = Mathf.Sin(Time.time * 0.65f) * vhsRoot.rect.height * 0.28f;
        vhsTrackingBand.anchoredPosition = new Vector2(0f, bandY);
        vhsTrackingBand.sizeDelta = new Vector2(0f, 12f + vhsTrackingPulse * 42f);
        UpdateVhsNoise();
    }

    private void UpdateVhsNoise()
    {
        float frameWidth = vhsFrameRoot.rect.width;
        float frameHeight = vhsFrameRoot.rect.height;
        float tracking = Mathf.PerlinNoise(Time.time * 2.1f, 8.2f);
        if (vhsTrackingImage != null)
        {
            vhsTrackingImage.color = new Color(0.70f, 0.88f, 1f, 0.08f + tracking * 0.13f + vhsTrackingPulse * 0.18f);
        }

        if (vhsHeadSwitchBand != null)
        {
            float crawl = Mathf.Sin(Time.time * 4.8f) * 6f;
            vhsHeadSwitchBand.anchoredPosition = new Vector2(crawl, 18f + Mathf.Sin(Time.time * 14f) * 3f);
            vhsHeadSwitchBand.sizeDelta = new Vector2(0f, 18f + tracking * 24f);
        }

        if (vhsHeadSwitchImage != null)
        {
            vhsHeadSwitchImage.color = new Color(0.70f, 0.90f, 1f, 0.07f + tracking * 0.12f);
        }

        float tear = Mathf.PerlinNoise(4.4f, Time.time * 1.9f);
        if (vhsWhiteTear != null)
        {
            float y = Mathf.Lerp(-frameHeight * 0.33f, frameHeight * 0.36f, tear);
            vhsWhiteTear.anchoredPosition = new Vector2(Mathf.Sin(Time.time * 18f) * 18f, y);
            vhsWhiteTear.sizeDelta = new Vector2(0f, 3f + tear * 14f + vhsTrackingPulse * 12f);
        }

        if (vhsWhiteTearImage != null)
        {
            float alpha = Mathf.Max(0f, tear - 0.68f) * 0.75f + vhsTrackingPulse * 0.06f;
            vhsWhiteTearImage.color = new Color(0.88f, 0.96f, 1f, alpha);
        }

        for (int i = 0; i < vhsNoiseBlocks.Length; i++)
        {
            float n = Mathf.PerlinNoise(i * 0.37f, Time.time * (1.7f + i * 0.03f));
            RectTransform block = vhsNoiseBlocks[i];
            block.anchoredPosition = new Vector2(
                Mathf.Lerp(-frameWidth * 0.46f, frameWidth * 0.46f, Mathf.Repeat(n + i * 0.173f, 1f)),
                Mathf.Lerp(-frameHeight * 0.44f, frameHeight * 0.44f, Mathf.Repeat(n * 1.7f + i * 0.097f, 1f)));
            block.sizeDelta = new Vector2(8f + n * 54f, 2f + n * 13f);
            if (vhsNoiseImages[i] != null)
            {
                float alpha = n > 0.72f ? 0.10f + (n - 0.72f) * 0.9f : 0.0f;
                vhsNoiseImages[i].color = new Color(0.82f, 0.92f, 1f, alpha);
            }
        }
    }

    private void TriggerVhsZoomPulse()
    {
        if (!vhsModeEnabled)
        {
            return;
        }

        vhsZoomPulse = 1f;
        vhsTrackingPulse = Mathf.Max(vhsTrackingPulse, 0.65f);
    }

    private void SetVhsMode(bool enabled)
    {
        bool wasEnabled = vhsModeEnabled;
        if (enabled)
        {
            vhsModeEnabled = true;
            RestoreVhsCameraZoom();
        }
        else
        {
            if (wasEnabled)
            {
                ReturnToNormalCameraZoom();
            }

            vhsModeEnabled = false;
        }

        if (vhsRoot != null)
        {
            vhsRoot.gameObject.SetActive(enabled);
        }

        ApplyVhsCameraViewport(enabled);
        SetCameraBreathingLoop(enabled);

        if (hudCanvasGroup != null)
        {
            hudCanvasGroup.alpha = enabled ? 0f : 1f;
            hudCanvasGroup.interactable = !enabled;
            hudCanvasGroup.blocksRaycasts = !enabled;
        }

        if (!enabled)
        {
            ApplyVhsPostEffects(false, 0f, 0f);
        }
    }

    private void ApplyVhsCameraViewport(bool enabled)
    {
        if (mainCamera == null)
        {
            return;
        }

        if (!enabled)
        {
            mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
            return;
        }

        float width = Mathf.Max(1f, Screen.width);
        float height = Mathf.Max(1f, Screen.height);
        float current = width / height;
        const float target = 4f / 3f;
        if (current >= target)
        {
            float viewportWidth = target / current;
            mainCamera.rect = new Rect((1f - viewportWidth) * 0.5f, 0f, viewportWidth, 1f);
        }
        else
        {
            float viewportHeight = current / target;
            mainCamera.rect = new Rect(0f, (1f - viewportHeight) * 0.5f, 1f, viewportHeight);
        }
    }

    private bool WasVhsTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current.fKey.wasPressedThisFrame;
        }
#endif
        return Input.GetKeyDown(KeyCode.F);
    }

    private RectTransform CreateVhsPanel(string name, Transform parent, Color color)
    {
        RectTransform rect = CreatePanel(name, parent, color);
        Image image = rect.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = false;
        }

        return rect;
    }

    private Text CreateVhsText(string name, Transform parent, int size, TextAnchor anchor, Color color)
    {
        Text text = CreateText(name, parent, size, FontStyle.Bold, anchor, color);
        text.raycastTarget = false;
        return text;
    }

    private void PlaceVhsText(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private void CreateCorner(Transform parent, string name, Vector2 anchor, Vector2 sign)
    {
        RectTransform horizontal = CreateVhsPanel("VHS Corner H " + name, parent, new Color(0.78f, 0.92f, 0.86f, 0.50f));
        horizontal.anchorMin = anchor;
        horizontal.anchorMax = anchor;
        horizontal.pivot = anchor;
        horizontal.anchoredPosition = new Vector2(sign.x * 18f, sign.y * 18f);
        horizontal.sizeDelta = new Vector2(86f, 2f);

        RectTransform vertical = CreateVhsPanel("VHS Corner V " + name, parent, new Color(0.78f, 0.92f, 0.86f, 0.50f));
        vertical.anchorMin = anchor;
        vertical.anchorMax = anchor;
        vertical.pivot = anchor;
        vertical.anchoredPosition = new Vector2(sign.x * 18f, sign.y * 18f);
        vertical.sizeDelta = new Vector2(2f, 58f);
    }
}
