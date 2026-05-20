using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private Canvas watchCanvas;
    private CanvasGroup watchCanvasGroup;
    private RectTransform watchRoot;
    private RectTransform watchVisualRoot;
    private RectTransform watchCaseRoot;
    private RectTransform watchShadow;
    private Image watchLcdImage;
    private Text watchTimeText;
    private Text watchBrandText;
    private Text watchModeText;

    private void SetupWatchInterface()
    {
        GameObject canvasObject = new GameObject("Observer Watch UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        watchCanvas = canvasObject.GetComponent<Canvas>();
        watchCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        watchCanvas.sortingOrder = 132;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        watchRoot = CreateWatchPanel("Watch Root", canvasObject.transform, new Color(0f, 0f, 0f, 0f));
        watchRoot.anchorMin = Vector2.zero;
        watchRoot.anchorMax = Vector2.one;
        watchRoot.offsetMin = Vector2.zero;
        watchRoot.offsetMax = Vector2.zero;
        watchCanvasGroup = watchRoot.gameObject.AddComponent<CanvasGroup>();
        watchCanvasGroup.interactable = false;

        watchShadow = CreateWatchPanel("Watch Hand Shadow", watchRoot, new Color(0f, 0f, 0f, 0.46f));
        watchShadow.anchorMin = new Vector2(0.5f, 0.5f);
        watchShadow.anchorMax = new Vector2(0.5f, 0.5f);
        watchShadow.pivot = new Vector2(0.5f, 0.5f);
        watchShadow.sizeDelta = new Vector2(520f, 128f);

        watchVisualRoot = CreateWatchPanel("Watch In Hand", watchRoot, new Color(0f, 0f, 0f, 0f));
        watchVisualRoot.anchorMin = new Vector2(0.5f, 0.5f);
        watchVisualRoot.anchorMax = new Vector2(0.5f, 0.5f);
        watchVisualRoot.pivot = new Vector2(0.5f, 0.5f);
        watchVisualRoot.sizeDelta = new Vector2(520f, 560f);

        CreateWatchHand();
        CreateWatchBody();
        ApplyWatchUiPose(0f);
        watchRoot.gameObject.SetActive(false);
    }

    private void CreateWatchHand()
    {
        RectTransform sleeve = CreateWatchPanel("Watch Sleeve", watchVisualRoot, new Color(0.025f, 0.030f, 0.040f, 0.98f));
        PlaceWatchPart(sleeve, new Vector2(0f, -230f), new Vector2(390f, 170f), -4f);

        RectTransform wrist = CreateWatchPanel("Watch Wrist", watchVisualRoot, new Color(0.42f, 0.31f, 0.23f, 0.98f));
        PlaceWatchPart(wrist, new Vector2(0f, -145f), new Vector2(270f, 134f), -4f);

        RectTransform wristShade = CreateWatchPanel("Watch Wrist Shade", watchVisualRoot, new Color(0.18f, 0.105f, 0.075f, 0.35f));
        PlaceWatchPart(wristShade, new Vector2(0f, -112f), new Vector2(250f, 28f), -4f);
    }

    private void CreateWatchBody()
    {
        RectTransform upperStrap = CreateWatchPanel("Watch Upper Strap", watchVisualRoot, new Color(0.018f, 0.020f, 0.023f, 1f));
        PlaceWatchPart(upperStrap, new Vector2(0f, 148f), new Vector2(118f, 250f), 0f);
        AddWatchStrapDetails(upperStrap, 5, true);

        RectTransform lowerStrap = CreateWatchPanel("Watch Lower Strap", watchVisualRoot, new Color(0.015f, 0.017f, 0.020f, 1f));
        PlaceWatchPart(lowerStrap, new Vector2(0f, -146f), new Vector2(126f, 220f), 0f);
        AddWatchStrapDetails(lowerStrap, 4, false);

        watchCaseRoot = CreateWatchPanel("Watch Case Root", watchVisualRoot, new Color(0.030f, 0.032f, 0.035f, 1f));
        PlaceWatchPart(watchCaseRoot, new Vector2(0f, 6f), new Vector2(290f, 220f), 0f);
        AddWatchOutline(watchCaseRoot, new Color(0.11f, 0.12f, 0.13f, 0.9f), new Vector2(3f, -3f));

        RectTransform bevel = CreateWatchPanel("Watch Inner Bevel", watchCaseRoot, new Color(0.075f, 0.079f, 0.080f, 1f));
        PlaceWatchPart(bevel, new Vector2(0f, 0f), new Vector2(246f, 168f), 0f);

        RectTransform face = CreateWatchPanel("Watch Face", watchCaseRoot, new Color(0.012f, 0.014f, 0.015f, 1f));
        PlaceWatchPart(face, new Vector2(0f, -2f), new Vector2(220f, 138f), 0f);

        RectTransform lcd = CreateWatchPanel("Watch LCD", face, new Color(0.50f, 0.57f, 0.49f, 1f));
        PlaceWatchPart(lcd, new Vector2(0f, -8f), new Vector2(184f, 76f), 0f);
        watchLcdImage = lcd.GetComponent<Image>();

        RectTransform glare = CreateWatchPanel("Watch LCD Glare", lcd, new Color(0.84f, 0.92f, 0.78f, 0.16f));
        PlaceWatchPart(glare, new Vector2(-40f, 20f), new Vector2(92f, 10f), 0f);

        watchBrandText = CreateWatchText("Watch Brand", face, 18, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.62f, 0.66f, 0.62f, 1f));
        watchBrandText.text = "CASSIO";
        PlaceWatchText(watchBrandText, new Vector2(0f, 49f), new Vector2(160f, 24f));

        watchModeText = CreateWatchText("Watch Mode", face, 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.44f, 0.48f, 0.44f, 1f));
        watchModeText.text = "ALARM  CHRONO  LIGHT";
        PlaceWatchText(watchModeText, new Vector2(0f, -55f), new Vector2(190f, 20f));

        watchTimeText = CreateWatchText("Watch Time", lcd, 62, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.045f, 0.075f, 0.055f, 1f));
        watchTimeText.horizontalOverflow = HorizontalWrapMode.Overflow;
        watchTimeText.text = FormatWatchTime();
        PlaceWatchText(watchTimeText, Vector2.zero, new Vector2(180f, 76f));

        CreateWatchButtons();
    }

    private void CreateWatchButtons()
    {
        RectTransform leftTop = CreateWatchPanel("Watch Button Left Top", watchCaseRoot, new Color(0.075f, 0.078f, 0.080f, 1f));
        PlaceWatchPart(leftTop, new Vector2(-158f, 44f), new Vector2(28f, 38f), 0f);
        RectTransform leftBottom = CreateWatchPanel("Watch Button Left Bottom", watchCaseRoot, new Color(0.070f, 0.073f, 0.076f, 1f));
        PlaceWatchPart(leftBottom, new Vector2(-158f, -48f), new Vector2(28f, 38f), 0f);
        RectTransform rightTop = CreateWatchPanel("Watch Button Right Top", watchCaseRoot, new Color(0.075f, 0.078f, 0.080f, 1f));
        PlaceWatchPart(rightTop, new Vector2(158f, 44f), new Vector2(28f, 38f), 0f);
        RectTransform rightBottom = CreateWatchPanel("Watch Button Right Bottom", watchCaseRoot, new Color(0.070f, 0.073f, 0.076f, 1f));
        PlaceWatchPart(rightBottom, new Vector2(158f, -48f), new Vector2(28f, 38f), 0f);
    }

    private void AddWatchStrapDetails(RectTransform strap, int holes, bool upper)
    {
        for (int i = 0; i < holes; i++)
        {
            float y = (upper ? 74f : -52f) + (upper ? i * 27f : i * -27f);
            RectTransform hole = CreateWatchPanel("Watch Strap Hole", strap, new Color(0.055f, 0.058f, 0.062f, 1f));
            PlaceWatchPart(hole, new Vector2(0f, y), new Vector2(34f, 9f), 0f);
        }

        RectTransform seamLeft = CreateWatchPanel("Watch Strap Seam L", strap, new Color(0.12f, 0.13f, 0.14f, 0.45f));
        PlaceWatchPart(seamLeft, new Vector2(-43f, 0f), new Vector2(3f, strap.sizeDelta.y - 24f), 0f);
        RectTransform seamRight = CreateWatchPanel("Watch Strap Seam R", strap, new Color(0.12f, 0.13f, 0.14f, 0.45f));
        PlaceWatchPart(seamRight, new Vector2(43f, 0f), new Vector2(3f, strap.sizeDelta.y - 24f), 0f);
    }

    private void ApplyWatchUiPose(float deltaTime)
    {
        if (watchRoot == null)
        {
            return;
        }

        bool visible = watchModeEnabled || watchOpenAmount > 0.012f;
        watchRoot.gameObject.SetActive(visible);
        if (!visible)
        {
            return;
        }

        UpdateWatchTimeText();
        float t = Mathf.SmoothStep(0f, 1f, watchOpenAmount);
        float handShake = watchModeEnabled ? Mathf.Sin(Time.time * 5.6f) * 2.2f + Mathf.Sin(Time.time * 11.1f) * 0.7f : 0f;
        float tilt = Mathf.Lerp(-21f, -3.2f, t) + handShake * 0.18f;
        float y = Mathf.Lerp(-610f, -86f, t) + handShake;
        float x = Mathf.Lerp(135f, 0f, t) + Mathf.Sin(Time.time * 4.1f) * 2.4f * t;
        float scale = Mathf.Lerp(0.78f, 1.06f, t);

        watchVisualRoot.anchoredPosition = new Vector2(x, y);
        watchVisualRoot.localRotation = Quaternion.Euler(0f, 0f, tilt);
        watchVisualRoot.localScale = new Vector3(scale, scale, 1f);

        watchShadow.anchoredPosition = new Vector2(x + 8f, y - 44f);
        watchShadow.localRotation = Quaternion.Euler(0f, 0f, tilt * 0.35f);
        watchShadow.localScale = new Vector3(Mathf.Lerp(0.72f, 1.08f, t), Mathf.Lerp(0.20f, 0.58f, t), 1f);

        if (watchCanvasGroup != null)
        {
            watchCanvasGroup.alpha = t;
            watchCanvasGroup.blocksRaycasts = t > 0.45f;
        }

        if (watchLcdImage != null)
        {
            float pulse = 0.018f + Mathf.Sin(Time.time * 13f) * 0.008f;
            watchLcdImage.color = new Color(0.50f + pulse, 0.57f + pulse, 0.49f + pulse, 1f);
        }
    }

    private void UpdateWatchTimeText()
    {
        if (watchTimeText != null)
        {
            watchTimeText.text = FormatWatchTime();
        }
    }

    private RectTransform CreateWatchPanel(string name, Transform parent, Color color)
    {
        RectTransform rect = CreatePanel(name, parent, color);
        Image image = rect.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = false;
        }

        return rect;
    }

    private Text CreateWatchText(string name, Transform parent, int size, FontStyle style, TextAnchor anchor, Color color)
    {
        Text text = CreateText(name, parent, size, style, anchor, color);
        text.raycastTarget = false;
        return text;
    }

    private void PlaceWatchPart(RectTransform rect, Vector2 position, Vector2 size, float rotation)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }

    private void PlaceWatchText(Text text, Vector2 position, Vector2 size)
    {
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private void AddWatchOutline(RectTransform rect, Color color, Vector2 distance)
    {
        Outline outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
    }
}
