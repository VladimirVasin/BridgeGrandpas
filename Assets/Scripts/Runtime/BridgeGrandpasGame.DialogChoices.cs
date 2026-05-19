using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private RectTransform CreateDialogChoiceButton(string title, string preview, Transform parent, UnityEngine.Events.UnityAction action)
    {
        RectTransform rect = CreatePanel("Choice - " + title, parent, new Color(0.12f, 0.13f, 0.15f, 0.98f));
        rect.sizeDelta = new Vector2(0f, 78f);

        LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = 76f;
        layout.preferredHeight = 78f;
        layout.flexibleWidth = 1f;

        Button button = rect.gameObject.AddComponent<Button>();
        Image image = rect.GetComponent<Image>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.12f, 0.13f, 0.15f, 0.98f);
        colors.highlightedColor = new Color(0.22f, 0.25f, 0.27f, 1f);
        colors.pressedColor = new Color(0.72f, 0.43f, 0.18f, 1f);
        colors.disabledColor = new Color(0.09f, 0.09f, 0.10f, 0.72f);
        button.colors = colors;

        Text titleText = CreateText("Choice Title", rect, 16, FontStyle.Bold, TextAnchor.UpperLeft, Color.white);
        titleText.supportRichText = true;
        titleText.raycastTarget = false;
        titleText.rectTransform.anchorMin = new Vector2(0f, 0.48f);
        titleText.rectTransform.anchorMax = Vector2.one;
        titleText.rectTransform.offsetMin = new Vector2(18f, 0f);
        titleText.rectTransform.offsetMax = new Vector2(-18f, -9f);
        titleText.text = title;

        Text previewText = CreateText("Choice Preview", rect, 14, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.78f, 0.86f, 0.82f));
        previewText.supportRichText = true;
        previewText.raycastTarget = false;
        previewText.rectTransform.anchorMin = Vector2.zero;
        previewText.rectTransform.anchorMax = new Vector2(1f, 0.50f);
        previewText.rectTransform.offsetMin = new Vector2(18f, 8f);
        previewText.rectTransform.offsetMax = new Vector2(-18f, -1f);
        previewText.text = string.IsNullOrEmpty(preview) ? "<color=#9aa3a8>Последствия неизвестны</color>" : preview;

        Outline outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.95f, 0.62f, 0.24f, 0.22f);
        outline.effectDistance = new Vector2(1.4f, -1.4f);

        BridgeGrandpasButtonVisual visual = rect.gameObject.AddComponent<BridgeGrandpasButtonVisual>();
        visual.Setup(button, image, titleText, outline);
        rect.gameObject.AddComponent<BridgeGrandpasHudButtonAudio>();
        return rect;
    }
}
