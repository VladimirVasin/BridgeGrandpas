using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private Canvas notebookCanvas;
    private CanvasGroup notebookCanvasGroup;
    private CanvasGroup notebookContentGroup;
    private CanvasGroup notebookPeekGroup;
    private CanvasGroup notebookBackdropGroup;
    private CanvasGroup notebookTabGroup;
    private RectTransform notebookRoot;
    private RectTransform notebookLeftPage;
    private RectTransform notebookRightPage;
    private RectTransform notebookLeftPageContent;
    private RectTransform notebookTabRoot;
    private RectTransform notebookPageContent;
    private RectTransform notebookFlipPage;
    private RectTransform notebookPreviousObservationCorner;
    private RectTransform notebookNextObservationCorner;
    private ScrollRect notebookScroll;
    private Text notebookPeekText;
    private Text notebookLegendText;
    private Text notebookTitleText;

    private void SetupNotebookInterface()
    {
        GameObject canvasObject = new GameObject("Observer Notebook UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        notebookCanvas = canvasObject.GetComponent<Canvas>();
        notebookCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        notebookCanvas.sortingOrder = 64;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;
        notebookCanvasGroup = canvasObject.AddComponent<CanvasGroup>();
        SetupNotebookWorldVisuals();
        CreateNotebookBackdrop(canvasObject.transform);

        notebookRoot = CreatePanel("Notebook Root", canvasObject.transform, new Color(0.26f, 0.14f, 0.065f, 0.98f));
        notebookRoot.anchorMin = new Vector2(0.06f, 0f);
        notebookRoot.anchorMax = new Vector2(0.94f, 0f);
        notebookRoot.pivot = new Vector2(0.5f, 0f);
        notebookRoot.anchoredPosition = Vector2.zero;
        notebookRoot.sizeDelta = new Vector2(0f, 92f);

        CreateNotebookPeek();
        CreateNotebookLegend(canvasObject.transform);
        CreateNotebookContent();
        ApplyNotebookUiPose();
        RefreshNotebookUi();
    }

    private void CreateNotebookBackdrop(Transform parent)
    {
        RectTransform backdrop = CreatePanel("Notebook Outside Close Area", parent, new Color(0f, 0f, 0f, 0f));
        backdrop.anchorMin = Vector2.zero;
        backdrop.anchorMax = Vector2.one;
        backdrop.offsetMin = Vector2.zero;
        backdrop.offsetMax = Vector2.zero;
        notebookBackdropGroup = backdrop.gameObject.AddComponent<CanvasGroup>();
        Button close = backdrop.gameObject.AddComponent<Button>();
        close.targetGraphic = backdrop.GetComponent<Image>();
        close.onClick.AddListener(delegate { SetNotebookMode(false); });
    }

    private void CreateNotebookPeek()
    {
        RectTransform strip = CreatePanel("Notebook Edge", notebookRoot, new Color(0.18f, 0.095f, 0.045f, 1f));
        strip.anchorMin = Vector2.zero;
        strip.anchorMax = Vector2.one;
        strip.offsetMin = Vector2.zero;
        strip.offsetMax = Vector2.zero;
        notebookPeekGroup = strip.gameObject.AddComponent<CanvasGroup>();

        Button opener = strip.gameObject.AddComponent<Button>();
        opener.targetGraphic = strip.GetComponent<Image>();
        opener.onClick.AddListener(delegate { SetNotebookMode(true); });
        ColorBlock colors = opener.colors;
        colors.normalColor = new Color(0.18f, 0.095f, 0.045f, 1f);
        colors.highlightedColor = new Color(0.30f, 0.17f, 0.08f, 1f);
        colors.pressedColor = new Color(0.11f, 0.055f, 0.025f, 1f);
        opener.colors = colors;
        strip.gameObject.AddComponent<BridgeGrandpasHudButtonAudio>();

        notebookPeekText = CreateText("Notebook Peek Text", strip, 16, FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.94f, 0.82f, 0.58f, 0f));
        notebookPeekText.supportRichText = true;
        notebookPeekText.rectTransform.anchorMin = Vector2.zero;
        notebookPeekText.rectTransform.anchorMax = Vector2.one;
        notebookPeekText.rectTransform.offsetMin = new Vector2(18f, 4f);
        notebookPeekText.rectTransform.offsetMax = new Vector2(-18f, -4f);
        notebookPeekText.raycastTarget = false;
    }

    private void CreateNotebookLegend(Transform parent)
    {
        RectTransform panel = CreatePanel("Notebook Controls Legend", parent, new Color(0.025f, 0.026f, 0.030f, 0.58f));
        panel.anchorMin = new Vector2(1f, 0f);
        panel.anchorMax = new Vector2(1f, 0f);
        panel.pivot = new Vector2(1f, 0f);
        panel.anchoredPosition = new Vector2(-24f, 24f);
        panel.sizeDelta = new Vector2(180f, 66f);
        panel.GetComponent<Image>().raycastTarget = false;

        notebookLegendText = CreateText("Notebook Legend Text", panel, 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.94f, 0.82f, 0.58f));
        notebookLegendText.text = "F  камера\nN  блокнот\nT  часы";
        notebookLegendText.rectTransform.anchorMin = Vector2.zero;
        notebookLegendText.rectTransform.anchorMax = Vector2.one;
        notebookLegendText.rectTransform.offsetMin = new Vector2(16f, 5f);
        notebookLegendText.rectTransform.offsetMax = new Vector2(-12f, -5f);
        notebookLegendText.raycastTarget = false;
    }

    private void CreateNotebookContent()
    {
        RectTransform pages = CreatePanel("Notebook Open Pages", notebookRoot, new Color(0f, 0f, 0f, 0f));
        pages.anchorMin = Vector2.zero;
        pages.anchorMax = Vector2.one;
        pages.offsetMin = new Vector2(22f, 18f);
        pages.offsetMax = new Vector2(-22f, -18f);
        notebookContentGroup = pages.gameObject.AddComponent<CanvasGroup>();

        notebookLeftPage = CreateNotebookPage("Left Page", pages, new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(-5f, 0f));
        notebookRightPage = CreateNotebookPage("Right Page", pages, new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Vector2(5f, 0f), new Vector2(0f, 0f));
        RectTransform spine = CreatePanel("Notebook Spine", pages, new Color(0.36f, 0.23f, 0.11f, 0.86f));
        spine.anchorMin = new Vector2(0.5f, 0f);
        spine.anchorMax = new Vector2(0.5f, 1f);
        spine.pivot = new Vector2(0.5f, 0.5f);
        spine.sizeDelta = new Vector2(12f, 0f);

        notebookTitleText = CreateText("Notebook Title", notebookLeftPage, 29, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.16f, 0.09f, 0.045f));
        notebookTitleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        notebookTitleText.rectTransform.anchorMax = new Vector2(1f, 1f);
        notebookTitleText.rectTransform.pivot = new Vector2(0.5f, 1f);
        notebookTitleText.rectTransform.offsetMin = new Vector2(28f, -60f);
        notebookTitleText.rectTransform.offsetMax = new Vector2(-28f, -16f);

        notebookLeftPageContent = CreatePanel("Notebook Left Page Text", notebookLeftPage, new Color(0f, 0f, 0f, 0f));
        notebookLeftPageContent.anchorMin = Vector2.zero;
        notebookLeftPageContent.anchorMax = Vector2.one;
        notebookLeftPageContent.offsetMin = new Vector2(28f, 28f);
        notebookLeftPageContent.offsetMax = new Vector2(-28f, -78f);
        VerticalLayoutGroup leftLayout = notebookLeftPageContent.gameObject.AddComponent<VerticalLayoutGroup>();
        leftLayout.spacing = 7f;
        leftLayout.childForceExpandWidth = true;
        leftLayout.childForceExpandHeight = false;
        ContentSizeFitter leftFitter = notebookLeftPageContent.gameObject.AddComponent<ContentSizeFitter>();
        leftFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CreateNotebookScroll(notebookRightPage);
        CreateNotebookTabs(pages);
        CreateNotebookFlipPage(pages);
        CreateNotebookDice(notebookRightPage);
        CreateNotebookObservationPageCorners();
    }

    private RectTransform CreateNotebookPage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform page = CreatePanel("Notebook " + name, parent, new Color(0.77f, 0.66f, 0.47f, 0.98f));
        page.anchorMin = anchorMin;
        page.anchorMax = anchorMax;
        page.offsetMin = offsetMin;
        page.offsetMax = offsetMax;
        CreateNotebookPageLines(page);
        return page;
    }

    private void CreateNotebookPageLines(RectTransform page)
    {
        for (int i = 0; i < 9; i++)
        {
            RectTransform line = CreatePanel("Notebook Page Line", page, new Color(0.36f, 0.24f, 0.12f, 0.14f));
            line.anchorMin = new Vector2(0f, 1f);
            line.anchorMax = new Vector2(1f, 1f);
            line.pivot = new Vector2(0.5f, 1f);
            line.anchoredPosition = new Vector2(0f, -88f - i * 34f);
            line.sizeDelta = new Vector2(-50f, 1.2f);
            line.GetComponent<Image>().raycastTarget = false;
        }
    }

    private void CreateNotebookScroll(RectTransform paper)
    {
        RectTransform viewport = CreatePanel("Notebook Viewport", paper, new Color(0f, 0f, 0f, 0f));
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = new Vector2(28f, 30f);
        viewport.offsetMax = new Vector2(-28f, -30f);
        viewport.GetComponent<Image>().raycastTarget = false;
        viewport.gameObject.AddComponent<RectMask2D>();

        notebookPageContent = CreatePanel("Notebook Page Content", viewport, new Color(0f, 0f, 0f, 0f));
        notebookPageContent.anchorMin = new Vector2(0f, 1f);
        notebookPageContent.anchorMax = new Vector2(1f, 1f);
        notebookPageContent.pivot = new Vector2(0.5f, 1f);
        notebookPageContent.anchoredPosition = Vector2.zero;
        notebookPageContent.offsetMin = Vector2.zero;
        notebookPageContent.offsetMax = Vector2.zero;
        notebookPageContent.sizeDelta = new Vector2(0f, 720f);
        VerticalLayoutGroup layout = notebookPageContent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.padding = new RectOffset(0, 10, 8, 0);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = notebookPageContent.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        notebookScroll = viewport.gameObject.AddComponent<ScrollRect>();
        notebookScroll.content = notebookPageContent;
        notebookScroll.viewport = viewport;
        notebookScroll.horizontal = false;
        notebookScroll.vertical = true;
        notebookScroll.scrollSensitivity = 26f;
    }

    private void CreateNotebookTabs(RectTransform pages)
    {
        notebookTabRoot = CreatePanel("Notebook Page Bookmarks", pages, new Color(0f, 0f, 0f, 0f));
        notebookTabRoot.anchorMin = new Vector2(0.78f, 1f);
        notebookTabRoot.anchorMax = new Vector2(0.78f, 1f);
        notebookTabRoot.pivot = new Vector2(0.5f, 0f);
        notebookTabRoot.anchoredPosition = new Vector2(0f, -8f);
        notebookTabRoot.sizeDelta = new Vector2(380f, 132f);
        notebookTabGroup = notebookTabRoot.gameObject.AddComponent<CanvasGroup>();
        HorizontalLayoutGroup tabs = notebookTabRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        tabs.spacing = 9f;
        tabs.childAlignment = TextAnchor.LowerLeft;
        tabs.childControlWidth = false;
        tabs.childControlHeight = false;
        tabs.childForceExpandWidth = false;
        tabs.childForceExpandHeight = false;
        CreateNotebookTabIfAvailable("Наблюдения", NotebookPage.Observations, new Color(0.80f, 0.52f, 0.25f, 0.98f));
        CreateNotebookTabIfAvailable("Сводка", NotebookPage.Summary, new Color(0.82f, 0.56f, 0.27f, 0.98f));
        CreateNotebookTabIfAvailable("Строительство", NotebookPage.Build, new Color(0.77f, 0.49f, 0.23f, 0.98f));
        CreateNotebookTabIfAvailable("Деды", NotebookPage.Grandpas, new Color(0.72f, 0.44f, 0.21f, 0.98f));
        CreateNotebookTabIfAvailable("События", NotebookPage.Events, new Color(0.67f, 0.40f, 0.19f, 0.98f));
        CreateNotebookTabIfAvailable("Вылазки", NotebookPage.Expeditions, new Color(0.62f, 0.36f, 0.18f, 0.98f));
    }

    private void CreateNotebookFlipPage(RectTransform pages)
    {
        notebookFlipPage = CreatePanel("Notebook Turning Page", pages, new Color(0.83f, 0.72f, 0.52f, 0.86f));
        notebookFlipPage.anchorMin = new Vector2(0.5f, 0f);
        notebookFlipPage.anchorMax = new Vector2(1f, 1f);
        notebookFlipPage.pivot = new Vector2(0f, 0.5f);
        notebookFlipPage.offsetMin = new Vector2(4f, 0f);
        notebookFlipPage.offsetMax = Vector2.zero;
        notebookFlipPage.gameObject.SetActive(false);
        notebookFlipPage.GetComponent<Image>().raycastTarget = false;
    }

    private void CreateNotebookDice(RectTransform paper)
    {
        expeditionDicePanel = CreatePanel("Notebook Expedition Dice", paper, new Color(0.11f, 0.075f, 0.035f, 0.96f));
        expeditionDicePanel.anchorMin = new Vector2(0.5f, 0.5f);
        expeditionDicePanel.anchorMax = new Vector2(0.5f, 0.5f);
        expeditionDicePanel.pivot = new Vector2(0.5f, 0.5f);
        expeditionDicePanel.anchoredPosition = new Vector2(-86f, -26f);
        expeditionDicePanel.sizeDelta = new Vector2(92f, 92f);
        expeditionDicePanel.gameObject.SetActive(false);

        expeditionDiceText = CreateText("Notebook Dice Text", expeditionDicePanel, 44, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.78f, 0.36f));
        expeditionDiceText.rectTransform.anchorMin = Vector2.zero;
        expeditionDiceText.rectTransform.anchorMax = Vector2.one;
        expeditionDiceText.rectTransform.offsetMin = Vector2.zero;
        expeditionDiceText.rectTransform.offsetMax = Vector2.zero;

        expeditionDiceCaptionText = CreateText("Notebook Dice Caption", paper, 15, FontStyle.BoldAndItalic, TextAnchor.MiddleLeft, new Color(0.18f, 0.08f, 0.035f));
        expeditionDiceCaptionText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        expeditionDiceCaptionText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        expeditionDiceCaptionText.rectTransform.pivot = new Vector2(0f, 0.5f);
        expeditionDiceCaptionText.rectTransform.anchoredPosition = new Vector2(-22f, -26f);
        expeditionDiceCaptionText.rectTransform.sizeDelta = new Vector2(420f, 54f);
        expeditionDiceCaptionText.gameObject.SetActive(false);
    }

    private void CreateNotebookTab(string label, NotebookPage page, Color color)
    {
        RectTransform button = CreateNotebookBookmark(label, notebookTabRoot, color, delegate { SetNotebookPage(page); });
        LayoutElement layout = button.GetComponent<LayoutElement>();
        layout.preferredHeight = 48f;
    }

    private void CreateNotebookTabIfAvailable(string label, NotebookPage page, Color color)
    {
        if (IsNotebookTabAvailableByDefault(page))
        {
            CreateNotebookTab(label, page, color);
        }
    }
}
