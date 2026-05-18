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
        title.rectTransform.anchoredPosition = new Vector2(0f, 212f);
        title.rectTransform.sizeDelta = new Vector2(760f, 82f);

        Text subtitle = CreateText("Menu Subtitle", contentRoot, 20, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.94f, 0.88f, 0.76f));
        startMenuSubtitleRect = subtitle.rectTransform;
        subtitle.text = "тайная дедовская цивилизация под старым мостом";
        subtitle.alignment = TextAnchor.MiddleCenter;
        subtitle.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        subtitle.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        subtitle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        subtitle.rectTransform.anchoredPosition = new Vector2(0f, 154f);
        subtitle.rectTransform.sizeDelta = new Vector2(620f, 36f);

        RectTransform buttons = CreatePanel("Menu Buttons", contentRoot, new Color(0f, 0f, 0f, 0f));
        startMenuButtonsRect = buttons;
        buttons.GetComponent<Image>().raycastTarget = false;
        buttons.anchorMin = new Vector2(0.5f, 0.5f);
        buttons.anchorMax = new Vector2(0.5f, 0.5f);
        buttons.pivot = new Vector2(0.5f, 0.5f);
        buttons.anchoredPosition = new Vector2(0f, 18f);
        buttons.sizeDelta = new Vector2(320f, 150f);

        VerticalLayoutGroup layout = buttons.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 14f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateMenuButton("Новая игра", buttons, StartNewGame);
        CreateMenuButton("Выход", buttons, QuitGameFromMenu);
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

    private void CreateMenuButton(string label, Transform parent, UnityEngine.Events.UnityAction action)
    {
        RectTransform buttonRect = CreateButton(label, parent, action);
        LayoutElement layout = buttonRect.GetComponent<LayoutElement>();
        if (layout != null)
        {
            layout.minHeight = 58f;
            layout.preferredHeight = 64f;
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
