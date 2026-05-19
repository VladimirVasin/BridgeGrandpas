using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private RectTransform CreateNotebookButton(string label, Transform parent, UnityEngine.Events.UnityAction action)
    {
        RectTransform rect = CreatePanel("Notebook Button - " + label, parent, new Color(0.68f, 0.53f, 0.31f, 0.42f));
        rect.sizeDelta = new Vector2(0f, 54f);
        LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = 44f;
        layout.preferredHeight = 58f;
        layout.flexibleWidth = 1f;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.onClick.AddListener(action);
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.68f, 0.53f, 0.31f, 0.42f);
        colors.highlightedColor = new Color(0.88f, 0.70f, 0.38f, 0.72f);
        colors.pressedColor = new Color(0.40f, 0.22f, 0.10f, 0.88f);
        colors.disabledColor = new Color(0.30f, 0.24f, 0.18f, 0.24f);
        button.colors = colors;

        Text text = CreateText("Notebook Button Label", rect, 15, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.12f, 0.07f, 0.035f));
        text.supportRichText = true;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(12f, 4f);
        text.rectTransform.offsetMax = new Vector2(-12f, -4f);
        text.raycastTarget = false;
        text.text = label;

        Outline outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.18f, 0.09f, 0.025f, 0.32f);
        outline.effectDistance = new Vector2(1f, -1f);
        rect.gameObject.AddComponent<BridgeGrandpasHudButtonAudio>();
        return rect;
    }

    private RectTransform CreateNotebookBookmark(string label, Transform parent, Color color, UnityEngine.Events.UnityAction action)
    {
        RectTransform rect = CreatePanel("Notebook Bookmark - " + label, parent, color);
        rect.sizeDelta = new Vector2(46f, 128f);
        LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = 122f;
        layout.preferredHeight = 128f;
        layout.preferredWidth = 46f;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.onClick.AddListener(action);
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, new Color(1f, 0.82f, 0.42f, 1f), 0.42f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.35f);
        colors.disabledColor = new Color(0.25f, 0.20f, 0.16f, 0.45f);
        button.colors = colors;

        Text text = CreateText("Notebook Bookmark Label", rect, 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.12f, 0.055f, 0.025f));
        text.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        text.rectTransform.anchoredPosition = Vector2.zero;
        text.rectTransform.sizeDelta = new Vector2(116f, 42f);
        text.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        text.raycastTarget = false;
        text.text = label;
        rect.gameObject.AddComponent<BridgeGrandpasHudButtonAudio>();
        return rect;
    }
}
