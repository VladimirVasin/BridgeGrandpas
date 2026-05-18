using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class BridgeGrandpasButtonVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private static readonly Color Normal = new Color(0.15f, 0.18f, 0.20f, 0.98f);
    private static readonly Color Hover = new Color(0.28f, 0.34f, 0.36f, 1f);
    private static readonly Color Pressed = new Color(0.95f, 0.58f, 0.18f, 1f);
    private static readonly Color Disabled = new Color(0.10f, 0.105f, 0.11f, 0.84f);
    private static readonly Color NormalOutline = new Color(0.76f, 0.54f, 0.22f, 0.25f);
    private static readonly Color HoverOutline = new Color(1f, 0.80f, 0.36f, 0.95f);
    private static readonly Color PressedOutline = new Color(1f, 0.95f, 0.72f, 1f);
    private static readonly Color DisabledOutline = new Color(0.55f, 0.18f, 0.14f, 0.70f);

    private Button button;
    private Image image;
    private Text label;
    private Outline outline;
    private bool hover;
    private bool down;
    private bool initialized;
    private Color imageColor;
    private Color labelColor;
    private Color outlineColor;
    private Vector2 outlineDistance;
    private Vector3 visualScale = Vector3.one;

    public void Setup(Button targetButton, Image targetImage, Text targetLabel, Outline targetOutline)
    {
        button = targetButton;
        image = targetImage;
        label = targetLabel;
        outline = targetOutline;
        button.transition = Selectable.Transition.None;
        imageColor = Normal;
        labelColor = Color.white;
        outlineColor = NormalOutline;
        outlineDistance = new Vector2(1.5f, -1.5f);
        visualScale = Vector3.one;
        initialized = true;
        Apply();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hover = true;
        Apply();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hover = false;
        down = false;
        Apply();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        down = true;
        Apply();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        down = false;
        Apply();
    }

    private void OnEnable()
    {
        Apply();
    }

    private void Update()
    {
        Apply();
    }

    private void Apply()
    {
        if (button == null || image == null)
        {
            return;
        }

        bool enabled = button.interactable;
        Color targetImage = !enabled ? Disabled : down ? Pressed : hover ? Hover : Normal;
        Color targetOutline = !enabled ? DisabledOutline : down ? PressedOutline : hover ? HoverOutline : NormalOutline;
        Vector2 targetDistance = down ? new Vector2(3f, -3f) : hover ? new Vector2(2.2f, -2.2f) : new Vector2(1.5f, -1.5f);
        Vector3 targetScale = down && enabled ? new Vector3(0.94f, 0.90f, 1f) : hover && enabled ? new Vector3(1.035f, 1.035f, 1f) : Vector3.one;
        float lerp = Application.isPlaying ? 1f - Mathf.Exp(-Time.unscaledDeltaTime * 18f) : 1f;

        imageColor = initialized ? Color.Lerp(imageColor, targetImage, lerp) : targetImage;
        image.color = imageColor;

        if (outline != null)
        {
            outlineColor = initialized ? Color.Lerp(outlineColor, targetOutline, lerp) : targetOutline;
            outlineDistance = initialized ? Vector2.Lerp(outlineDistance, targetDistance, lerp) : targetDistance;
            outline.effectColor = outlineColor;
            outline.effectDistance = outlineDistance;
        }

        if (label != null)
        {
            Color targetLabel = !enabled ? new Color(0.78f, 0.73f, 0.68f, 0.92f) : hover ? new Color(1f, 0.92f, 0.72f, 1f) : Color.white;
            labelColor = initialized ? Color.Lerp(labelColor, targetLabel, lerp) : targetLabel;
            label.color = labelColor;
        }

        visualScale = initialized ? Vector3.Lerp(visualScale, targetScale, lerp) : targetScale;
        transform.localScale = visualScale;
    }
}
