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
        suspicionLabel.text = "Подозрение города";
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

    private void CreateEventModal(Transform parent)
    {
        eventModal = CreatePanel("Event Modal", parent, new Color(0.055f, 0.05f, 0.065f, 0.98f));
        eventModal.anchorMin = new Vector2(0.5f, 0.5f);
        eventModal.anchorMax = new Vector2(0.5f, 0.5f);
        eventModal.pivot = new Vector2(0.5f, 0.5f);
        eventModal.anchoredPosition = new Vector2(0f, 26f);
        eventModal.sizeDelta = new Vector2(660f, 520f);
        eventModal.gameObject.SetActive(false);

        eventTitleText = CreateText("Event Title", eventModal, 24, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.82f, 0.48f));
        eventTitleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        eventTitleText.rectTransform.anchorMax = new Vector2(1f, 1f);
        eventTitleText.rectTransform.offsetMin = new Vector2(22f, -78f);
        eventTitleText.rectTransform.offsetMax = new Vector2(-22f, -18f);

        eventBodyText = CreateText("Event Body", eventModal, 17, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.92f, 0.92f, 0.88f));
        eventBodyText.rectTransform.anchorMin = new Vector2(0f, 0f);
        eventBodyText.rectTransform.anchorMax = new Vector2(1f, 1f);
        eventBodyText.rectTransform.offsetMin = new Vector2(22f, 294f);
        eventBodyText.rectTransform.offsetMax = new Vector2(-22f, -92f);

        eventChoicesRoot = CreatePanel("Event Choices", eventModal, new Color(0f, 0f, 0f, 0f));
        eventChoicesRoot.anchorMin = new Vector2(0f, 0f);
        eventChoicesRoot.anchorMax = new Vector2(1f, 0f);
        eventChoicesRoot.offsetMin = new Vector2(22f, 22f);
        eventChoicesRoot.offsetMax = new Vector2(-22f, 282f);
        VerticalLayoutGroup layout = eventChoicesRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    private void CreateExpeditionModal(Transform parent)
    {
        expeditionModal = CreatePanel("Expedition Modal", parent, new Color(0.050f, 0.046f, 0.058f, 0.98f));
        expeditionModal.anchorMin = new Vector2(0.5f, 0.5f);
        expeditionModal.anchorMax = new Vector2(0.5f, 0.5f);
        expeditionModal.pivot = new Vector2(0.5f, 0.5f);
        expeditionModal.anchoredPosition = new Vector2(0f, 18f);
        expeditionModal.sizeDelta = new Vector2(660f, 500f);
        expeditionModal.gameObject.SetActive(false);

        expeditionTitleText = CreateText("Expedition Title", expeditionModal, 23, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.82f, 0.48f));
        expeditionTitleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        expeditionTitleText.rectTransform.anchorMax = new Vector2(1f, 1f);
        expeditionTitleText.rectTransform.offsetMin = new Vector2(22f, -70f);
        expeditionTitleText.rectTransform.offsetMax = new Vector2(-22f, -16f);

        expeditionBodyText = CreateText("Expedition Body", expeditionModal, 16, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.91f, 0.90f, 0.84f));
        expeditionBodyText.rectTransform.anchorMin = new Vector2(0f, 0f);
        expeditionBodyText.rectTransform.anchorMax = new Vector2(1f, 1f);
        expeditionBodyText.rectTransform.offsetMin = new Vector2(22f, 292f);
        expeditionBodyText.rectTransform.offsetMax = new Vector2(-22f, -84f);

        expeditionDicePanel = CreatePanel("Expedition Dice", expeditionModal, new Color(0.095f, 0.075f, 0.055f, 0.98f));
        expeditionDicePanel.anchorMin = new Vector2(0.5f, 0.5f);
        expeditionDicePanel.anchorMax = new Vector2(0.5f, 0.5f);
        expeditionDicePanel.pivot = new Vector2(0.5f, 0.5f);
        expeditionDicePanel.anchoredPosition = new Vector2(0f, -22f);
        expeditionDicePanel.sizeDelta = new Vector2(86f, 86f);
        expeditionDicePanel.gameObject.SetActive(false);

        expeditionDiceText = CreateText("Expedition Dice Text", expeditionDicePanel, 44, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.52f));
        expeditionDiceText.rectTransform.anchorMin = Vector2.zero;
        expeditionDiceText.rectTransform.anchorMax = Vector2.one;
        expeditionDiceText.rectTransform.offsetMin = Vector2.zero;
        expeditionDiceText.rectTransform.offsetMax = Vector2.zero;

        expeditionDiceCaptionText = CreateText("Expedition Dice Caption", expeditionModal, 14, FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.78f, 0.86f, 0.90f));
        expeditionDiceCaptionText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        expeditionDiceCaptionText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        expeditionDiceCaptionText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        expeditionDiceCaptionText.rectTransform.anchoredPosition = new Vector2(0f, -78f);
        expeditionDiceCaptionText.rectTransform.sizeDelta = new Vector2(500f, 28f);
        expeditionDiceCaptionText.gameObject.SetActive(false);

        expeditionChoicesRoot = CreatePanel("Expedition Choices", expeditionModal, new Color(0f, 0f, 0f, 0f));
        expeditionChoicesRoot.anchorMin = new Vector2(0f, 0f);
        expeditionChoicesRoot.anchorMax = new Vector2(1f, 0f);
        expeditionChoicesRoot.offsetMin = new Vector2(22f, 20f);
        expeditionChoicesRoot.offsetMax = new Vector2(-22f, 280f);
        VerticalLayoutGroup layout = expeditionChoicesRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    private void ShowExpeditionNarrativeModal(Grandpa grandpa)
    {
        if (grandpa == null || grandpa.ExpeditionNarrativeResolved)
        {
            return;
        }

        if (notebookCanvas != null)
        {
            selectedGrandpa = grandpa;
            if (!notebookModeEnabled || currentNotebookPage != NotebookPage.Expeditions)
            {
                SetNotebookPage(NotebookPage.Expeditions);
                SetNotebookMode(true);
                MarkNotebookDirty();
            }

            return;
        }

        if (expeditionModal == null)
        {
            return;
        }

        if (expeditionModal.gameObject.activeSelf || eventModal.gameObject.activeSelf || victoryModal.gameObject.activeSelf)
        {
            return;
        }

        ResetExpeditionDice();
        ClearChildren(expeditionChoicesRoot);
        expeditionTitleText.text = "Вылазка: " + ExpeditionName(grandpa.ExpeditionType);
        expeditionBodyText.text = grandpa.Name + " поднялся из-под моста. Наверху мокрый свет, чужие ботинки и шанс вернуться не с пустыми руками.\n\nВыбери, как он поведёт себя в городе.";
        AddExpeditionChoice(grandpa, "Тише под перилами", "<color=#9cff93>меньше подозрения</color> | <color=#ffcf7a>добычи меньше</color> | бросок кубика", 0.82f, 0.45f, "пошёл тихо, почти как тень с авоськой");
        AddExpeditionChoice(grandpa, "Собрать всё блестящее", "<color=#9cff93>добычи больше</color> | <color=#ff8f7a>подозрение выше</color> | бросок кубика", 1.35f, 1.45f, "решил, что осторожность сегодня не главный ресурс");
        AddExpeditionChoice(grandpa, "Действовать по-дедовски", "<color=#9cff93>сбалансированная добыча</color> | <color=#ffcf7a>обычный риск</color> | бросок кубика", 1.08f, 0.92f, "применил старую тактику: выглядеть так, будто всё так и было");
        expeditionModal.gameObject.SetActive(true);
    }

    private void AddExpeditionChoice(Grandpa grandpa, string label, string preview, float reward, float risk, string result)
    {
        CreateDialogChoiceButton(label, preview, expeditionChoicesRoot, delegate
        {
            StartExpeditionDiceRoll(grandpa, reward, risk, result);
        });
    }

    private void CreateVictoryModal(Transform parent)
    {
        victoryModal = CreatePanel("Victory Modal", parent, new Color(0.06f, 0.055f, 0.07f, 0.98f));
        victoryModal.anchorMin = new Vector2(0.5f, 0.5f);
        victoryModal.anchorMax = new Vector2(0.5f, 0.5f);
        victoryModal.pivot = new Vector2(0.5f, 0.5f);
        victoryModal.anchoredPosition = Vector2.zero;
        victoryModal.sizeDelta = new Vector2(620f, 300f);
        victoryModal.gameObject.SetActive(false);

        Text title = CreateText("Victory Title", victoryModal, 25, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.83f, 0.46f));
        title.text = "Под мостом образовалась устойчивая дедовская цивилизация.";
        title.rectTransform.anchorMin = new Vector2(0f, 1f);
        title.rectTransform.anchorMax = new Vector2(1f, 1f);
        title.rectTransform.offsetMin = new Vector2(26f, -112f);
        title.rectTransform.offsetMax = new Vector2(-26f, -24f);

        Text body = CreateText("Victory Body", victoryModal, 17, FontStyle.Normal, TextAnchor.UpperCenter, new Color(0.9f, 0.92f, 0.9f));
        body.text = "Цель MVP выполнена: 20 дедушек, 5 объектов, 3 проверки и редкая мутация.\nМожно продолжать в endless-режиме и смотреть, как коммуна становится всё страннее.";
        body.rectTransform.anchorMin = new Vector2(0f, 0f);
        body.rectTransform.anchorMax = new Vector2(1f, 1f);
        body.rectTransform.offsetMin = new Vector2(36f, 86f);
        body.rectTransform.offsetMax = new Vector2(-36f, -118f);

        RectTransform button = CreateButton("Продолжать", victoryModal, delegate
        {
            victoryModal.gameObject.SetActive(false);
        });
        button.anchorMin = new Vector2(0.5f, 0f);
        button.anchorMax = new Vector2(0.5f, 0f);
        button.pivot = new Vector2(0.5f, 0f);
        button.anchoredPosition = new Vector2(0f, 26f);
        button.sizeDelta = new Vector2(190f, 48f);
    }

}

