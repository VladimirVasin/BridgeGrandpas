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
    private void SetupStartMenu()
    {
        EnsureUiFont();
        EnsureMenuEventSystem();

        GameObject canvasObject = new GameObject("Start Menu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        startMenuCanvas = canvasObject.GetComponent<Canvas>();
        startMenuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        startMenuCanvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        CreateMenuBackground(canvasObject.transform);
        CreateMenuAnimationLayers(canvasObject.transform);
        CreateMenuContent(canvasObject.transform);
        SetupMenuMusic();
    }

    private void EnsureMenuEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
        ConfigureRuntimeUiInput(eventSystemObject.GetComponent<InputSystemUIInputModule>());
#else
        eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
    }

    private void CreateMenuBackground(Transform parent)
    {
        Texture2D menuTexture = Resources.Load<Texture2D>("Menu");
        GameObject imageObject = new GameObject("Menu Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        startMenuBackgroundRect = rect;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        RawImage image = imageObject.GetComponent<RawImage>();
        image.texture = menuTexture;
        image.color = menuTexture == null ? new Color(0.025f, 0.020f, 0.018f, 1f) : Color.white;
        image.raycastTarget = false;

        RectTransform shade = CreatePanel("Menu Shade", parent, new Color(0f, 0f, 0f, 0.38f));
        startMenuShadeRect = shade;
        shade.anchorMin = Vector2.zero;
        shade.anchorMax = Vector2.one;
        shade.offsetMin = Vector2.zero;
        shade.offsetMax = Vector2.zero;
        shade.GetComponent<Image>().raycastTarget = false;
    }

    private void CreateMenuContent(Transform parent)
    {
        RectTransform contentRoot = CreateMenuContentRoot(parent);
        Text title = CreateText("Menu Title", contentRoot, 54, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(1f, 0.78f, 0.40f));
        startMenuTitleRect = title.rectTransform;
        title.text = "Мостовые Дедушки";
        title.alignment = TextAnchor.MiddleCenter;
        title.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        title.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        title.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        title.rectTransform.anchoredPosition = new Vector2(0f, 242f);
        title.rectTransform.sizeDelta = new Vector2(760f, 82f);

        Text subtitle = CreateText("Menu Subtitle", contentRoot, 20, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.94f, 0.88f, 0.76f));
        startMenuSubtitleRect = subtitle.rectTransform;
        subtitle.text = "тайная дедовская цивилизация под старым мостом";
        subtitle.alignment = TextAnchor.MiddleCenter;
        subtitle.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        subtitle.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        subtitle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        subtitle.rectTransform.anchoredPosition = new Vector2(0f, 184f);
        subtitle.rectTransform.sizeDelta = new Vector2(620f, 36f);

        RectTransform buttons = CreatePanel("Menu Buttons", contentRoot, new Color(0f, 0f, 0f, 0f));
        startMenuButtonsRect = buttons;
        buttons.GetComponent<Image>().raycastTarget = false;
        startMenuButtonsGroup = buttons.gameObject.AddComponent<CanvasGroup>();
        buttons.anchorMin = new Vector2(0.5f, 0.5f);
        buttons.anchorMax = new Vector2(0.5f, 0.5f);
        buttons.pivot = new Vector2(0.5f, 0.5f);
        buttons.anchoredPosition = new Vector2(0f, 46f);
        buttons.sizeDelta = new Vector2(320f, 228f);

        VerticalLayoutGroup layout = buttons.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 14f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateMenuButton("Новая игра", buttons, BeginStartMenuLoading);
        RectTransform loadButton = CreateMenuButton("Загрузить", buttons, BeginStartMenuLoad);
        Button load = loadButton.GetComponent<Button>();
        if (load != null)
        {
            load.interactable = HasSavedGame();
        }

        CreateMenuButton("Выход", buttons, QuitGameFromMenu);
        CreateMenuLoadingBar(contentRoot);
    }

    private void CreateMenuLoadingBar(Transform parent)
    {
        RectTransform root = CreatePanel("Menu Loading", parent, new Color(0f, 0f, 0f, 0f));
        startMenuLoadingRoot = root;
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = new Vector2(0f, -122f);
        root.sizeDelta = new Vector2(420f, 52f);
        root.gameObject.SetActive(false);

        Text text = CreateText("Menu Loading Text", root, 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.83f, 0.52f));
        startMenuLoadingText = text;
        text.rectTransform.anchorMin = new Vector2(0f, 0.52f);
        text.rectTransform.anchorMax = new Vector2(1f, 1f);
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        text.text = "Готовим место под мостом... 0%";

        RectTransform track = CreatePanel("Menu Loading Track", root, new Color(0.01f, 0.012f, 0.015f, 0.88f));
        track.anchorMin = new Vector2(0.5f, 0f);
        track.anchorMax = new Vector2(0.5f, 0f);
        track.pivot = new Vector2(0.5f, 0f);
        track.anchoredPosition = new Vector2(0f, 4f);
        track.sizeDelta = new Vector2(380f, 16f);

        RectTransform fill = CreatePanel("Menu Loading Fill", track, new Color(1f, 0.63f, 0.22f, 0.98f));
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = new Vector2(2f, 2f);
        fill.offsetMax = new Vector2(-2f, -2f);
        startMenuLoadingFill = fill.GetComponent<Image>();
        startMenuLoadingFill.type = Image.Type.Filled;
        startMenuLoadingFill.fillMethod = Image.FillMethod.Horizontal;
        startMenuLoadingFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        startMenuLoadingFill.fillAmount = 0f;
    }

    private RectTransform CreateMenuContentRoot(Transform parent)
    {
        GameObject root = new GameObject("Menu Content Root", typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(parent, false);
        startMenuContentRoot = root.GetComponent<RectTransform>();
        startMenuContentGroup = root.GetComponent<CanvasGroup>();
        startMenuContentGroup.alpha = 0f;
        startMenuContentRoot.anchorMin = Vector2.zero;
        startMenuContentRoot.anchorMax = Vector2.one;
        startMenuContentRoot.offsetMin = Vector2.zero;
        startMenuContentRoot.offsetMax = Vector2.zero;
        return startMenuContentRoot;
    }

    private RectTransform CreateMenuButton(string label, Transform parent, UnityEngine.Events.UnityAction action)
    {
        RectTransform buttonRect = CreateButton(label, parent, action);
        LayoutElement layout = buttonRect.GetComponent<LayoutElement>();
        if (layout != null)
        {
            layout.minHeight = 58f;
            layout.preferredHeight = 64f;
        }

        return buttonRect;
    }

    private void BeginStartMenuLoading()
    {
        BeginStartMenuLoading(false);
    }

    private void BeginStartMenuLoad()
    {
        if (!HasSavedGame())
        {
            return;
        }

        BeginStartMenuLoading(true);
    }

    private void BeginStartMenuLoading(bool loadSavedGame)
    {
        if (gameStarted || startMenuLoading)
        {
            return;
        }

        startMenuLoadSavedGame = loadSavedGame;
        startMenuLoading = true;
        startMenuLoadingStartedAt = Time.unscaledTime;
        SetStartMenuButtonsInteractable(false);
        if (startMenuLoadingRoot != null)
        {
            startMenuLoadingRoot.gameObject.SetActive(true);
        }

        if (startMenuLoadingFill != null)
        {
            startMenuLoadingFill.fillAmount = 0f;
        }
    }

    private void SetStartMenuButtonsInteractable(bool interactable)
    {
        if (startMenuButtonsGroup != null)
        {
            startMenuButtonsGroup.interactable = interactable;
            startMenuButtonsGroup.blocksRaycasts = interactable;
        }

        if (startMenuButtonsRect == null)
        {
            return;
        }

        Button[] buttons = startMenuButtonsRect.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = interactable;
        }
    }

    private void QuitGameFromMenu()
    {
        Debug.Log("[BridgeGrandpas] Exit requested from start menu.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
