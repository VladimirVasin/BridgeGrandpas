using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const int NotebookArchiveLinkedFontSize = 16;
    private const float NotebookArchiveLinkedLineScale = 1f;
    private const float DayThreeLinkedObservationHeight = 392f;
    private static readonly Color NotebookLinkColor = new Color(0.42f, 0.12f, 0.055f, 1f);

    private void AddDayThreeArchiveObservationWithLinks(Transform parent)
    {
        RectTransform block = CreatePanel("Day 3 Linked Observation", parent, new Color(0f, 0f, 0f, 0f));
        block.GetComponent<Image>().raycastTarget = false;
        LayoutElement layout = block.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = DayThreeLinkedObservationHeight;
        layout.preferredHeight = DayThreeLinkedObservationHeight;
        layout.flexibleWidth = 1f;

        AddNotebookArchiveLine(block, 0f, "Чем дольше я за ними наблюдаю, тем больше деталей начинаю подмечать.");

        float x = AddNotebookArchiveSegment(block, 38f, 0f, "Теперь я отчётливо вижу: они копят ", false, NotebookPage.Observations);
        x = AddNotebookArchiveSegment(block, 38f, x, "ресурсы", true, NotebookPage.Summary);
        AddNotebookArchiveSegment(block, 38f, x, " и продолжают", false, NotebookPage.Observations);

        x = AddNotebookArchiveSegment(block, 62f, 0f, "обустраивать", true, NotebookPage.Build);
        AddNotebookArchiveSegment(block, 62f, x, " свой лагерь.", false, NotebookPage.Observations);
        AddNotebookArchiveLine(block, 86f, "Доски, тряпьё, жестяные кружки, чай — всё у них, кажется,");
        AddNotebookArchiveLine(block, 110f, "имеет назначение. Думаю, мне удастся это подсчитать.");

        AddNotebookArchiveLine(block, 146f, "Ветер доносит до меня обрывки их разговоров.");
        x = AddNotebookArchiveSegment(block, 170f, 0f, "Я уже могу различить отдельные ", false, NotebookPage.Observations);
        x = AddNotebookArchiveSegment(block, 170f, x, "имена", true, NotebookPage.Grandpas);
        AddNotebookArchiveSegment(block, 170f, x, ", характерное ворчание и...", false, NotebookPage.Observations);
        AddNotebookArchiveLine(block, 194f, "кажется, степень готовности к дальнейшему почкованию.");
        AddNotebookArchiveLine(block, 218f, "Какая мерзость.");

        AddNotebookArchiveLine(block, 254f, "Кроме того, судя по всему, они готовятся к вылазкам в город.");

        AddNotebookArchiveLine(block, 290f, "Я обязан всё просчитать и задокументировать");
        AddNotebookArchiveLine(block, 314f, "самым подробным образом.");
        AddNotebookArchiveLine(block, 338f, "Всё яснее становится: под мостом зарождается не просто лагерь.");
        AddNotebookArchiveLine(block, 362f, "Они строят своё подпольное государство.");
    }

    private void AddNotebookArchiveLine(RectTransform parent, float y, string text)
    {
        Text line = CreateText("Archive Line", parent, NotebookArchiveLinkedFontSize, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.13f, 0.075f, 0.04f));
        line.text = text;
        line.raycastTarget = false;
        line.rectTransform.anchorMin = new Vector2(0f, 1f);
        line.rectTransform.anchorMax = new Vector2(1f, 1f);
        line.rectTransform.pivot = new Vector2(0f, 1f);
        line.rectTransform.anchoredPosition = new Vector2(0f, -y * NotebookArchiveLinkedLineScale);
        line.rectTransform.sizeDelta = new Vector2(0f, 28f);
        line.verticalOverflow = VerticalWrapMode.Overflow;
    }

    private float AddNotebookArchiveSegment(RectTransform parent, float y, float x, string text, bool link, NotebookPage page)
    {
        Text segment = CreateText("Archive Segment", parent, NotebookArchiveLinkedFontSize, link ? FontStyle.Bold : FontStyle.Normal,
            TextAnchor.UpperLeft, link ? NotebookLinkColor : new Color(0.13f, 0.075f, 0.04f));
        segment.text = text;
        segment.horizontalOverflow = HorizontalWrapMode.Overflow;
        segment.verticalOverflow = VerticalWrapMode.Overflow;
        segment.raycastTarget = link;

        float width = Mathf.Ceil(segment.preferredWidth) + (link ? 6f : 1f);
        RectTransform rect = segment.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y * NotebookArchiveLinkedLineScale);
        rect.sizeDelta = new Vector2(width, 28f);

        if (link)
        {
            Button button = segment.gameObject.AddComponent<Button>();
            button.targetGraphic = segment;
            button.onClick.AddListener(delegate { SetNotebookPage(page); });
            segment.gameObject.AddComponent<BridgeGrandpasHudButtonAudio>();
            AddNotebookArchiveUnderline(parent, x, y, width - 4f);
        }

        return x + width;
    }

    private void AddNotebookArchiveUnderline(RectTransform parent, float x, float y, float width)
    {
        RectTransform underline = CreatePanel("Archive Link Underline", parent, NotebookLinkColor);
        underline.anchorMin = new Vector2(0f, 1f);
        underline.anchorMax = new Vector2(0f, 1f);
        underline.pivot = new Vector2(0f, 1f);
        underline.anchoredPosition = new Vector2(x, -y * NotebookArchiveLinkedLineScale - 20f);
        underline.sizeDelta = new Vector2(Mathf.Max(8f, width), 1.2f);
        underline.GetComponent<Image>().raycastTarget = false;
    }
}
