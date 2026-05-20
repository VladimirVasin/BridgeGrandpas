using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;
public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private void SetupUi()
    {
        EnsureUiFont();

        GameObject canvasObject = new GameObject("MVP HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        if (EventSystem.current == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
            ConfigureRuntimeUiInput(eventSystemObject.GetComponent<InputSystemUIInputModule>());
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
            eventSystemObject.transform.SetParent(canvasObject.transform, false);
        }

        CreateTopPanel(canvasObject.transform);
        CreateRightPanel(canvasObject.transform);
        CreateBottomPanel(canvasObject.transform);
        CreateLogPanel(canvasObject.transform);
        CreateTray(canvasObject.transform);
        CreateEventModal(canvasObject.transform);
        CreateExpeditionModal(canvasObject.transform);
        CreateVictoryModal(canvasObject.transform);
        SetupVhsOverlay();
        SetupNotebookInterface();
        SetupObservationCardInterface();
        SetupWatchInterface();
        SetupStartIrisFade();
        ApplyLegacyHudVisibility();
    }

    private void CreateTopPanel(Transform parent)
    {
        RectTransform panel = CreatePanel("Top Stats", parent, new Color(0.045f, 0.050f, 0.062f, 0.78f));
        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(1f, 1f);
        panel.pivot = new Vector2(0.5f, 1f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = new Vector2(0f, 58f);

        topStatsText = CreateText("Top Stats Text", panel, 16, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        topStatsText.rectTransform.anchorMin = new Vector2(0f, 0f);
        topStatsText.rectTransform.anchorMax = new Vector2(1f, 1f);
        topStatsText.rectTransform.offsetMin = new Vector2(18f, 0f);
        topStatsText.rectTransform.offsetMax = new Vector2(-360f, 0f);
        topStatsText.supportRichText = true;

        Text suspicionLabel = CreateText("Suspicion Label", panel, 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(1f, 0.78f, 0.52f));
        suspicionLabel.text = TextCitySuspicion;
        suspicionLabel.rectTransform.anchorMin = new Vector2(1f, 0.5f);
        suspicionLabel.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        suspicionLabel.rectTransform.pivot = new Vector2(1f, 0.5f);
        suspicionLabel.rectTransform.anchoredPosition = new Vector2(-190f, 12f);
        suspicionLabel.rectTransform.sizeDelta = new Vector2(160f, 28f);

        RectTransform barBack = CreatePanel("Suspicion Bar Back", panel, new Color(0.16f, 0.10f, 0.10f, 0.95f));
        barBack.anchorMin = new Vector2(1f, 0.5f);
        barBack.anchorMax = new Vector2(1f, 0.5f);
        barBack.pivot = new Vector2(1f, 0.5f);
        barBack.anchoredPosition = new Vector2(-24f, -12f);
        barBack.sizeDelta = new Vector2(310f, 18f);

        RectTransform fillTransform = CreatePanel("Suspicion Bar Fill", barBack, new Color(0.88f, 0.28f, 0.22f, 1f));
        fillTransform.anchorMin = new Vector2(0f, 0f);
        fillTransform.anchorMax = new Vector2(0f, 1f);
        fillTransform.pivot = new Vector2(0f, 0.5f);
        fillTransform.anchoredPosition = Vector2.zero;
        fillTransform.sizeDelta = new Vector2(80f, 0f);
        suspicionFill = fillTransform.GetComponent<Image>();
    }

    private void CreateLogPanel(Transform parent)
    {
        RectTransform panel = CreatePanel("Event Log", parent, new Color(0f, 0f, 0f, 0f));
        panel.anchorMin = new Vector2(0f, 0f);
        panel.anchorMax = new Vector2(1f, 0f);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.anchoredPosition = new Vector2(0f, 64f);
        panel.sizeDelta = new Vector2(0f, 30f);
        panel.GetComponent<Image>().raycastTarget = false;

        alertText = CreateText("Event Log Text", panel, 14, FontStyle.Italic, TextAnchor.MiddleLeft, new Color(0.78f, 0.88f, 1f));
        alertText.rectTransform.anchorMin = new Vector2(0f, 0f);
        alertText.rectTransform.anchorMax = new Vector2(1f, 1f);
        alertText.rectTransform.offsetMin = new Vector2(18f, 0f);
        alertText.rectTransform.offsetMax = new Vector2(-18f, 0f);
        Outline outline = alertText.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);
    }

#if ENABLE_INPUT_SYSTEM
    private void ConfigureRuntimeUiInput(InputSystemUIInputModule module)
    {
        InputActionAsset asset = ScriptableObject.CreateInstance<InputActionAsset>();
        asset.name = "Runtime UI Input";

        InputActionMap map = new InputActionMap("UI");
        InputAction point = map.AddAction("Point", InputActionType.PassThrough, "<Pointer>/position");
        InputAction click = map.AddAction("Click", InputActionType.PassThrough, "<Pointer>/press");
        InputAction rightClick = map.AddAction("RightClick", InputActionType.PassThrough, "<Mouse>/rightButton");
        InputAction middleClick = map.AddAction("MiddleClick", InputActionType.PassThrough, "<Mouse>/middleButton");
        InputAction scroll = map.AddAction("ScrollWheel", InputActionType.PassThrough, "<Pointer>/scroll");
        InputAction move = map.AddAction("Navigate", InputActionType.Value);
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");
        InputAction submit = map.AddAction("Submit", InputActionType.Button, "<Keyboard>/enter");
        submit.AddBinding("<Keyboard>/space");
        InputAction cancel = map.AddAction("Cancel", InputActionType.Button, "<Keyboard>/escape");

        asset.AddActionMap(map);
        map.Enable();

        module.actionsAsset = asset;
        module.point = InputActionReference.Create(point);
        module.leftClick = InputActionReference.Create(click);
        module.rightClick = InputActionReference.Create(rightClick);
        module.middleClick = InputActionReference.Create(middleClick);
        module.scrollWheel = InputActionReference.Create(scroll);
        module.move = InputActionReference.Create(move);
        module.submit = InputActionReference.Create(submit);
        module.cancel = InputActionReference.Create(cancel);
    }
#endif

    private void CreateRightPanel(Transform parent)
    {
        RectTransform panel = CreatePanel("Selection Panel", parent, new Color(0.055f, 0.065f, 0.075f, 0.94f));
        detailPanel = panel;
        panel.anchorMin = new Vector2(1f, 0f);
        panel.anchorMax = new Vector2(1f, 1f);
        panel.pivot = new Vector2(1f, 0.5f);
        panel.offsetMin = new Vector2(-340f, 118f);
        panel.offsetMax = new Vector2(-14f, -84f);
        detailPanelGroup = panel.gameObject.AddComponent<CanvasGroup>();
        detailPanelShownOffsetMin = panel.offsetMin;
        detailPanelShownOffsetMax = panel.offsetMax;
        detailPanelHiddenOffsetMin = detailPanelShownOffsetMin + new Vector2(380f, 0f);
        detailPanelHiddenOffsetMax = detailPanelShownOffsetMax + new Vector2(380f, 0f);

        detailText = CreateText("Detail Text", panel, 16, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.92f, 0.93f, 0.90f));
        detailText.supportRichText = true;
        detailText.rectTransform.anchorMin = new Vector2(0f, 0f);
        detailText.rectTransform.anchorMax = new Vector2(1f, 1f);
        detailText.rectTransform.offsetMin = new Vector2(18f, 18f);
        detailText.rectTransform.offsetMax = new Vector2(-18f, -18f);
        detailPanelSlide = 0f;
        ApplyMicroHudPanelPose();
        panel.gameObject.SetActive(false);
    }

    private void CreateBottomPanel(Transform parent)
    {
        RectTransform panel = CreatePanel("Bottom Controls", parent, new Color(0.035f, 0.039f, 0.046f, 0.74f));
        panel.anchorMin = new Vector2(0f, 0f);
        panel.anchorMax = new Vector2(1f, 0f);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = new Vector2(0f, 62f);

        HorizontalLayoutGroup layout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 8, 8);
        layout.spacing = 8f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        CreateButton("Построить", panel, delegate
        {
            ToggleTray(UiTab.Build);
        });
        CreateButton("Улучшить", panel, delegate
        {
            ToggleTray(UiTab.Upgrade);
        });
        CreateButton("Почковать", panel, delegate
        {
            TryBudSelected();
            CloseTray();
            if (selectedGrandpa == null && selectedBuilding == null)
            {
                ShowMicroHudMessage("Почкование", BuildBuddingMicroHudText(), 4f);
            }

            RefreshAllUi();
        });
        CreateButton("События", panel, delegate
        {
            ToggleTray(UiTab.Events);
        });
        CreateButton("Вылазки", panel, delegate
        {
            ToggleTray(UiTab.Expeditions);
        });
        CreateButton("Дедушки", panel, delegate
        {
            ToggleTray(UiTab.Grandpas);
        });
    }

    private void EnsureUiFont()
    {
        if (uiFont != null)
        {
            return;
        }

        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null)
        {
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }

    private void CreateTray(Transform parent)
    {
        RectTransform panel = CreatePanel("Action Tray", parent, new Color(0.050f, 0.055f, 0.064f, 0.88f));
        trayPanel = panel;
        panel.anchorMin = new Vector2(0f, 0f);
        panel.anchorMax = new Vector2(0f, 0f);
        panel.pivot = new Vector2(0f, 0f);
        panel.anchoredPosition = new Vector2(16f, 72f);
        panel.sizeDelta = new Vector2(600f, 260f);

        trayTitleText = CreateText("Tray Title", panel, 19, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(1f, 0.84f, 0.56f));
        trayTitleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        trayTitleText.rectTransform.anchorMax = new Vector2(1f, 1f);
        trayTitleText.rectTransform.pivot = new Vector2(0.5f, 1f);
        trayTitleText.rectTransform.anchoredPosition = new Vector2(0f, -8f);
        trayTitleText.rectTransform.sizeDelta = new Vector2(-24f, 34f);

        RectTransform viewport = CreatePanel("Tray Viewport", panel, new Color(0f, 0f, 0f, 0.01f));
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = new Vector2(12f, 12f);
        viewport.offsetMax = new Vector2(-30f, -52f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        RectTransform scrollbarRoot = CreatePanel("Tray Scrollbar", panel, new Color(0.02f, 0.025f, 0.03f, 0.95f));
        scrollbarRoot.anchorMin = new Vector2(1f, 0f);
        scrollbarRoot.anchorMax = new Vector2(1f, 1f);
        scrollbarRoot.pivot = new Vector2(1f, 0.5f);
        scrollbarRoot.offsetMin = new Vector2(-22f, 12f);
        scrollbarRoot.offsetMax = new Vector2(-10f, -52f);

        RectTransform handle = CreatePanel("Tray Scrollbar Handle", scrollbarRoot, new Color(1f, 0.72f, 0.30f, 0.95f));
        handle.anchorMin = new Vector2(0f, 0f);
        handle.anchorMax = new Vector2(1f, 1f);
        handle.offsetMin = new Vector2(2f, 2f);
        handle.offsetMax = new Vector2(-2f, -2f);

        Scrollbar scrollbar = scrollbarRoot.gameObject.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.targetGraphic = handle.GetComponent<Image>();
        scrollbar.handleRect = handle;

        trayBody = CreatePanel("Tray Body", viewport, new Color(0f, 0f, 0f, 0f));
        trayBody.anchorMin = new Vector2(0f, 1f);
        trayBody.anchorMax = new Vector2(1f, 1f);
        trayBody.pivot = new Vector2(0.5f, 1f);
        trayBody.anchoredPosition = Vector2.zero;
        trayBody.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = trayBody.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 7f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        ContentSizeFitter fitter = trayBody.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        trayScroll = panel.gameObject.AddComponent<ScrollRect>();
        trayScroll.content = trayBody;
        trayScroll.viewport = viewport;
        trayScroll.horizontal = false;
        trayScroll.vertical = true;
        trayScroll.movementType = ScrollRect.MovementType.Clamped;
        trayScroll.scrollSensitivity = 24f;
        trayScroll.verticalScrollbar = scrollbar;
        trayScroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

        trayOpen = false;
        panel.gameObject.SetActive(false);
    }

}

