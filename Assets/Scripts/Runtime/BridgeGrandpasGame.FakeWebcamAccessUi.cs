using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private void EnsureFakeWebcamAccessVisuals()
    {
        if (fakeWebcamCanvas != null)
        {
            return;
        }

        EnsureUiFont();
        GameObject canvasObject = new GameObject("Fake Webcam Access Overlay", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        fakeWebcamCanvas = canvasObject.GetComponent<Canvas>();
        fakeWebcamCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fakeWebcamCanvas.sortingOrder = 376;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = CreatePanel("Fake Webcam Root", canvasObject.transform, new Color(0f, 0f, 0f, 0.62f));
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        root.GetComponent<Image>().raycastTarget = true;
        fakeWebcamGroup = root.gameObject.AddComponent<CanvasGroup>();

        fakeWebcamPanel = CreatePanel("Fake Webcam Panel", root, new Color(0.011f, 0.012f, 0.014f, 0.98f));
        fakeWebcamPanel.anchorMin = new Vector2(0.5f, 0.5f);
        fakeWebcamPanel.anchorMax = new Vector2(0.5f, 0.5f);
        fakeWebcamPanel.pivot = new Vector2(0.5f, 0.5f);
        fakeWebcamPanel.sizeDelta = new Vector2(1040f, 654f);
        Outline outline = fakeWebcamPanel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.05f, 0.92f, 0.96f, 0.42f);
        outline.effectDistance = new Vector2(2f, -2f);

        CreateFakeWebcamHeader(fakeWebcamPanel);
        CreateFakeWebcamPreview(fakeWebcamPanel);
        CreateFakeWebcamInfo(fakeWebcamPanel);
        CreateFakeWebcamFooter(fakeWebcamPanel);
        fakeWebcamCanvas.gameObject.SetActive(false);
    }

    private void CreateFakeWebcamHeader(Transform parent)
    {
        RectTransform header = CreatePanel("Fake Webcam Header", parent, new Color(0.05f, 0.010f, 0.012f, 1f));
        header.anchorMin = new Vector2(0f, 1f);
        header.anchorMax = new Vector2(1f, 1f);
        header.pivot = new Vector2(0.5f, 1f);
        header.offsetMin = new Vector2(0f, -68f);
        header.offsetMax = Vector2.zero;

        fakeWebcamTitleText = CreateText("Fake Webcam Title", header, 22, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(1f, 0.90f, 0.84f));
        fakeWebcamTitleText.rectTransform.anchorMin = Vector2.zero;
        fakeWebcamTitleText.rectTransform.anchorMax = Vector2.one;
        fakeWebcamTitleText.rectTransform.offsetMin = new Vector2(24f, 0f);
        fakeWebcamTitleText.rectTransform.offsetMax = new Vector2(-250f, 0f);

        Text live = CreateText("Fake Webcam Live Badge", header, 18, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.12f, 0.08f));
        live.text = "CAMERA / OBSERVER";
        live.rectTransform.anchorMin = Vector2.zero;
        live.rectTransform.anchorMax = Vector2.one;
        live.rectTransform.offsetMin = new Vector2(24f, 0f);
        live.rectTransform.offsetMax = new Vector2(-24f, 0f);
    }

    private void CreateFakeWebcamPreview(Transform parent)
    {
        fakeWebcamPreviewFrame = CreatePanel("Fake Webcam Preview Frame", parent, new Color(0.002f, 0.003f, 0.004f, 1f));
        fakeWebcamPreviewFrame.anchorMin = new Vector2(0f, 0f);
        fakeWebcamPreviewFrame.anchorMax = new Vector2(0f, 0f);
        fakeWebcamPreviewFrame.pivot = new Vector2(0f, 0f);
        fakeWebcamPreviewFrame.anchoredPosition = new Vector2(34f, 104f);
        fakeWebcamPreviewFrame.sizeDelta = new Vector2(642f, 446f);
        Outline outline = fakeWebcamPreviewFrame.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.55f, 0.98f, 1f, 0.32f);
        outline.effectDistance = new Vector2(1.4f, -1.4f);

        RectTransform viewport = CreatePanel("Fake Webcam Viewport", fakeWebcamPreviewFrame, Color.black);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(16f, 16f);
        viewport.offsetMax = new Vector2(-16f, -16f);

        GameObject rawObject = new GameObject("Fake Webcam Raw Preview", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        rawObject.transform.SetParent(viewport, false);
        fakeWebcamPreviewImage = rawObject.GetComponent<RawImage>();
        fakeWebcamPreviewImage.rectTransform.anchorMin = Vector2.zero;
        fakeWebcamPreviewImage.rectTransform.anchorMax = Vector2.one;
        fakeWebcamPreviewImage.rectTransform.offsetMin = Vector2.zero;
        fakeWebcamPreviewImage.rectTransform.offsetMax = Vector2.zero;
        fakeWebcamPreviewImage.color = new Color(0f, 0f, 0f, 0.55f);
        fakeWebcamPreviewImage.raycastTarget = false;

        fakeWebcamFallbackImage = CreatePanel("Fake Webcam Noise Wash", viewport, new Color(0.05f, 0.65f, 0.70f, 0.40f)).GetComponent<Image>();
        fakeWebcamFallbackImage.rectTransform.anchorMin = Vector2.zero;
        fakeWebcamFallbackImage.rectTransform.anchorMax = Vector2.one;
        fakeWebcamFallbackImage.rectTransform.offsetMin = Vector2.zero;
        fakeWebcamFallbackImage.rectTransform.offsetMax = Vector2.zero;
        fakeWebcamFallbackImage.raycastTarget = false;

        fakeWebcamGlitchBars = new RectTransform[FakeWebcamGlitchBarCount];
        fakeWebcamGlitchImages = new Image[FakeWebcamGlitchBarCount];
        for (int i = 0; i < fakeWebcamGlitchBars.Length; i++)
        {
            RectTransform bar = CreatePanel("Fake Webcam Glitch Bar " + i, viewport, new Color(0.08f, 0.95f, 1f, 0.30f));
            bar.anchorMin = new Vector2(0.5f, 0.5f);
            bar.anchorMax = new Vector2(0.5f, 0.5f);
            bar.pivot = new Vector2(0.5f, 0.5f);
            bar.sizeDelta = new Vector2(260f, 8f);
            bar.GetComponent<Image>().raycastTarget = false;
            fakeWebcamGlitchBars[i] = bar;
            fakeWebcamGlitchImages[i] = bar.GetComponent<Image>();
            bar.gameObject.SetActive(false);
        }

        fakeWebcamScanlines = new RectTransform[FakeWebcamScanlineCount];
        for (int i = 0; i < fakeWebcamScanlines.Length; i++)
        {
            RectTransform line = CreatePanel("Fake Webcam Scanline " + i, viewport, new Color(0f, 0f, 0f, 0.26f));
            line.anchorMin = new Vector2(0f, 0.5f);
            line.anchorMax = new Vector2(1f, 0.5f);
            line.pivot = new Vector2(0.5f, 0.5f);
            line.sizeDelta = new Vector2(0f, 2f);
            line.GetComponent<Image>().raycastTarget = false;
            fakeWebcamScanlines[i] = line;
        }
    }

    private void CreateFakeWebcamInfo(Transform parent)
    {
        fakeWebcamBodyText = CreateText("Fake Webcam Body", parent, 18, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.86f, 0.96f, 0.94f));
        fakeWebcamBodyText.rectTransform.anchorMin = new Vector2(1f, 0f);
        fakeWebcamBodyText.rectTransform.anchorMax = new Vector2(1f, 1f);
        fakeWebcamBodyText.rectTransform.pivot = new Vector2(1f, 1f);
        fakeWebcamBodyText.rectTransform.anchoredPosition = new Vector2(-34f, -96f);
        fakeWebcamBodyText.rectTransform.sizeDelta = new Vector2(300f, 330f);
        fakeWebcamBodyText.verticalOverflow = VerticalWrapMode.Truncate;
    }

    private void CreateFakeWebcamFooter(Transform parent)
    {
        fakeWebcamStatusText = CreateText("Fake Webcam Status", parent, 17, FontStyle.BoldAndItalic, TextAnchor.MiddleLeft, new Color(1f, 0.64f, 0.48f));
        fakeWebcamStatusText.rectTransform.anchorMin = new Vector2(0f, 0f);
        fakeWebcamStatusText.rectTransform.anchorMax = new Vector2(1f, 0f);
        fakeWebcamStatusText.rectTransform.offsetMin = new Vector2(34f, 30f);
        fakeWebcamStatusText.rectTransform.offsetMax = new Vector2(-690f, 74f);

        fakeWebcamAllowButtonRoot = CreateFakeWebcamFooterButton("Разрешить", parent, -474f, OnFakeWebcamAllowPressed);
        fakeWebcamDenyButtonRoot = CreateFakeWebcamFooterButton("Нет", parent, -254f, OnFakeWebcamDenyPressed);
        fakeWebcamCloseButtonRoot = CreateFakeWebcamFooterButton("Закрыть глаз", parent, -34f, delegate { EndFakeWebcamAccess("button"); });
        ApplyFakeWebcamConsentButtons();
    }

    private RectTransform CreateFakeWebcamFooterButton(string label, Transform parent, float x, UnityEngine.Events.UnityAction action)
    {
        RectTransform button = CreateButton(label, parent, action);
        button.anchorMin = new Vector2(1f, 0f);
        button.anchorMax = new Vector2(1f, 0f);
        button.pivot = new Vector2(1f, 0f);
        button.anchoredPosition = new Vector2(x, 28f);
        button.sizeDelta = new Vector2(196f, 54f);
        return button;
    }
}
