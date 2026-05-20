using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const int NotebookBodyFontBoost = 2;
    private const float NotebookBodyHeightScale = 1.12f;

    private void ApplyNotebookUiPose()
    {
        if (notebookRoot == null)
        {
            return;
        }

        float t = Mathf.SmoothStep(0f, 1f, notebookOpenAmount);
        float height = Mathf.Lerp(86f, 500f, t);
        notebookRoot.anchoredPosition = new Vector2(0f, Mathf.Lerp(-34f, 12f, t));
        notebookRoot.sizeDelta = new Vector2(0f, height);
        notebookRoot.localRotation = Quaternion.Euler(Mathf.Lerp(-2.5f, 0.8f, t), 0f, 0f);
        ApplyNotebookFlipPose();

        if (notebookCanvasGroup != null)
        {
            notebookCanvasGroup.alpha = vhsModeEnabled ? 0f : 1f;
            notebookCanvasGroup.blocksRaycasts = !vhsModeEnabled;
            notebookCanvasGroup.interactable = !vhsModeEnabled;
        }

        if (notebookBackdropGroup != null)
        {
            notebookBackdropGroup.alpha = 0f;
            notebookBackdropGroup.blocksRaycasts = !vhsModeEnabled && t > 0.42f;
            notebookBackdropGroup.interactable = !vhsModeEnabled && t > 0.42f;
        }

        if (notebookContentGroup != null)
        {
            notebookContentGroup.alpha = t;
            notebookContentGroup.blocksRaycasts = t > 0.35f;
            notebookContentGroup.interactable = t > 0.35f;
        }

        if (notebookPeekGroup != null)
        {
            notebookPeekGroup.alpha = 1f - t;
            notebookPeekGroup.blocksRaycasts = !vhsModeEnabled && t < 0.25f;
            notebookPeekGroup.interactable = !vhsModeEnabled && t < 0.25f;
        }

        if (notebookTabGroup != null)
        {
            notebookTabGroup.alpha = t;
            notebookTabGroup.blocksRaycasts = !vhsModeEnabled && t > 0.35f;
            notebookTabGroup.interactable = !vhsModeEnabled && t > 0.35f;
        }

        if (notebookLegendText != null)
        {
            notebookLegendText.transform.parent.gameObject.SetActive(!vhsModeEnabled && t < 0.25f);
        }
    }

    private void RefreshNotebookUi()
    {
        if (notebookPageContent == null)
        {
            return;
        }

        RefreshNotebookPeek();
        ClearChildren(notebookLeftPageContent);
        ClearChildren(notebookPageContent);
        if (currentNotebookPage == NotebookPage.Observations)
        {
            BuildNotebookObservationsSpread();
        }
        else
        {
            notebookTitleText.text = NotebookPageTitle(currentNotebookPage);
            if (currentNotebookPage == NotebookPage.Summary)
            {
                BuildNotebookSummaryLeftPage();
            }
            else
            {
                AddNotebookTextTo(notebookLeftPageContent, NotebookPageBody(currentNotebookPage), 17, FontStyle.Normal, 340f);
            }

            switch (currentNotebookPage)
            {
                case NotebookPage.Summary:
                    BuildNotebookSummaryRightPage();
                    break;
                case NotebookPage.Build:
                    BuildNotebookBuildPage();
                    break;
                case NotebookPage.Grandpas:
                    BuildNotebookGrandpasPage();
                    break;
                case NotebookPage.Events:
                    BuildNotebookEventsPage();
                    break;
                case NotebookPage.Expeditions:
                    BuildNotebookExpeditionsPage();
                    break;
            }
        }

        RefreshObservationPageCorners();

        if (notebookScroll != null)
        {
            Canvas.ForceUpdateCanvases();
            notebookPageContent.sizeDelta = new Vector2(notebookPageContent.sizeDelta.x, Mathf.Max(720f, notebookPageContent.rect.height));
            LayoutRebuilder.ForceRebuildLayoutImmediate(notebookPageContent);
            notebookScroll.verticalNormalizedPosition = 1f;
        }
    }

    private void RefreshNotebookPeek()
    {
        if (notebookPeekText == null)
        {
            return;
        }

        notebookPeekText.text = "";
    }

    private string NotebookPageTitle(NotebookPage page)
    {
        switch (page)
        {
            case NotebookPage.Observations:
                return "Наблюдения";
            case NotebookPage.Build:
                return "Следы строительства";
            case NotebookPage.Grandpas:
                return "Замеченные дедушки";
            case NotebookPage.Events:
                return "Слухи и версии";
            case NotebookPage.Expeditions:
                return "Записи о вылазках";
            default:
                return "Сводка наблюдателя";
        }
    }

    private string NotebookPageBody(NotebookPage page)
    {
        if (page == NotebookPage.Summary)
        {
            return "";
        }

        if (page == NotebookPage.Observations)
        {
            return "";
        }

        if (page == NotebookPage.Events && pendingEvent != null)
        {
            return "<b>" + pendingEvent.Title + "</b>\n" + pendingEvent.Body +
                "\n\nНаблюдатель выбирает, какую версию произошедшего внести в блокнот.";
        }

        if (page == NotebookPage.Expeditions)
        {
            return BuildNotebookExpeditionIntro();
        }

        if (page == NotebookPage.Build)
        {
            return "Зелёные пометки — наблюдение можно подтвердить сразу. Красные — пока не хватает вещей под мостом.";
        }

        if (page == NotebookPage.Grandpas)
        {
            return "Каждый дедушка записан как отдельное явление. Наблюдатель пока не уверен, считать ли это населением.";
        }

        return "Записи ведутся дрожащей рукой: не команды, а заметки человека, который слишком долго смотрит под мост.";
    }

    private string BuildNotebookResourceLine()
    {
        return "чай " + F(stock.Tea) + "  тепло " + F(stock.Heat) + "  картон " + F(stock.Cardboard) +
            "  ворч. " + F(stock.Grumble) + "  мон. " + F(stock.Coins);
    }

    private void BuildNotebookSummaryLeftPage()
    {
        if (victoryShown)
        {
            AddNotebookTextTo(notebookLeftPageContent,
                "<b>Под мостом образовалась устойчивая дедовская цивилизация.</b>\nEndless-режим продолжается.",
                15, FontStyle.Normal, 48f);
        }

        AddNotebookSummarySectionTitle("Запасы под мостом");
        AddNotebookSummaryResourceGrid();
        AddNotebookSummarySectionTitle("Состояние коммуны");
        AddNotebookSummaryStateGrid();
        AddNotebookSummaryServiceNote();
    }

    private void AddNotebookSummarySectionTitle(string title)
    {
        Text text = AddNotebookTextTo(notebookLeftPageContent, "<b>" + title + "</b>", 16, FontStyle.Bold, 22f);
        text.color = new Color(0.18f, 0.095f, 0.04f);
    }

    private void AddNotebookSummaryResourceGrid()
    {
        ResourceStock income = CurrentResourceIncomePerSecond();
        RectTransform grid = CreateNotebookSummaryBlock("Summary Resource Grid", 112f);
        VerticalLayoutGroup vertical = grid.gameObject.AddComponent<VerticalLayoutGroup>();
        vertical.spacing = 5f;
        vertical.childControlWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandWidth = true;
        vertical.childForceExpandHeight = false;

        RectTransform rowA = CreateNotebookSummaryRow(grid);
        AddNotebookSummaryResourceCell(rowA, TextTea, stock.Tea, income.Tea);
        AddNotebookSummaryResourceCell(rowA, TextHeat, stock.Heat, income.Heat);

        RectTransform rowB = CreateNotebookSummaryRow(grid);
        AddNotebookSummaryResourceCell(rowB, TextCardboard, stock.Cardboard, income.Cardboard);
        AddNotebookSummaryResourceCell(rowB, TextGrumble, stock.Grumble, income.Grumble);

        RectTransform rowC = CreateNotebookSummaryRow(grid);
        AddNotebookSummaryResourceCell(rowC, TextCoins, stock.Coins, income.Coins);
        AddNotebookSummaryStateCell(rowC, "Уют", CozyStat().Replace("Уют ", ""));
    }

    private void AddNotebookSummaryStateGrid()
    {
        RectTransform grid = CreateNotebookSummaryBlock("Summary State Grid", 76f);
        VerticalLayoutGroup vertical = grid.gameObject.AddComponent<VerticalLayoutGroup>();
        vertical.spacing = 5f;
        vertical.childControlWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandWidth = true;
        vertical.childForceExpandHeight = false;

        RectTransform rowA = CreateNotebookSummaryRow(grid);
        AddNotebookSummaryStateCell(rowA, "Дедушки", grandpas.Count + "/" + PopulationCap());
        AddNotebookSummaryStateCell(rowA, "Проверки", inspectionsSurvived + "/" + VictoryInspections);

        RectTransform rowB = CreateNotebookSummaryRow(grid);
        AddNotebookSummaryStateCell(rowB, "Постройки", BuiltCount() + "/" + buildings.Count);
        AddNotebookSummaryStateCell(rowB, "Подозрение", Mathf.RoundToInt(suspicion) + "%");
    }

    private void AddNotebookSummaryServiceNote()
    {
        RectTransform block = CreateNotebookSummaryBlock("Summary Service Note", 104f);
        Text text = CreateText("Summary Service Note Text", block, 16, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.13f, 0.075f, 0.04f));
        text.supportRichText = true;
        text.text = "<b>Служебная пометка</b>\n" +
            "N закрывает блокнот\n" +
            "F включает VHS-наблюдение\n" +
            "Клик вне страниц прерывает записи";
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(10f, 5f);
        text.rectTransform.offsetMax = new Vector2(-10f, -5f);
        text.raycastTarget = false;
    }

    private RectTransform CreateNotebookSummaryBlock(string name, float height)
    {
        RectTransform block = CreatePanel(name, notebookLeftPageContent, new Color(0.54f, 0.38f, 0.18f, 0.12f));
        block.GetComponent<Image>().raycastTarget = false;
        LayoutElement layout = block.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleWidth = 1f;
        return block;
    }

    private RectTransform CreateNotebookSummaryRow(Transform parent)
    {
        RectTransform row = CreatePanel("Summary Row", parent, new Color(0f, 0f, 0f, 0f));
        row.GetComponent<Image>().raycastTarget = false;
        LayoutElement layout = row.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = 32f;
        layout.preferredHeight = 32f;
        layout.flexibleWidth = 1f;

        HorizontalLayoutGroup horizontal = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        horizontal.spacing = 6f;
        horizontal.childControlWidth = true;
        horizontal.childControlHeight = true;
        horizontal.childForceExpandWidth = true;
        horizontal.childForceExpandHeight = true;
        return row;
    }

    private void AddNotebookSummaryResourceCell(Transform parent, string label, float amount, float perSecond)
    {
        float perMinute = perSecond * 60f;
        string color = Mathf.Abs(perMinute) < 0.05f ? "#6f7a86" : "#226b23";
        AddNotebookSummaryCell(parent, label, F(amount), "<color=" + color + ">+" + RateF(perMinute) + TextPerMinute + "</color>");
    }

    private void AddNotebookSummaryStateCell(Transform parent, string label, string value)
    {
        AddNotebookSummaryCell(parent, label, value, "");
    }

    private void AddNotebookSummaryCell(Transform parent, string label, string value, string suffix)
    {
        RectTransform cell = CreatePanel("Summary Cell - " + label, parent, new Color(0.78f, 0.65f, 0.43f, 0.18f));
        cell.GetComponent<Image>().raycastTarget = false;
        LayoutElement layout = cell.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = 32f;
        layout.preferredHeight = 32f;
        layout.flexibleWidth = 1f;

        Text text = CreateText("Summary Cell Text", cell, 15, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.13f, 0.075f, 0.04f));
        text.supportRichText = true;
        text.text = label + " <b>" + value + "</b>" + (string.IsNullOrEmpty(suffix) ? "" : "  " + suffix);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(8f, 1f);
        text.rectTransform.offsetMax = new Vector2(-8f, -1f);
        text.raycastTarget = false;
    }

    private string NotebookSummaryPreview(string text, int maxLength)
    {
        string plain = PlainNotebookText(text).Replace("\n", " ");
        while (plain.Contains("  "))
        {
            plain = plain.Replace("  ", " ");
        }

        if (plain.Length <= maxLength)
        {
            return plain;
        }

        return plain.Substring(0, maxLength - 3).TrimEnd() + "...";
    }

    private Text AddNotebookText(string text, int size, FontStyle style, float minHeight)
    {
        return AddNotebookTextTo(notebookPageContent, text, size, style, minHeight);
    }

    private Text AddNotebookTextTo(Transform parent, string text, int size, FontStyle style, float minHeight)
    {
        float adjustedHeight = Mathf.Ceil(minHeight * NotebookBodyHeightScale);
        Text note = CreateText("Notebook Text", parent, size + NotebookBodyFontBoost, style, TextAnchor.UpperLeft, new Color(0.13f, 0.075f, 0.04f));
        note.supportRichText = true;
        note.verticalOverflow = VerticalWrapMode.Overflow;
        note.text = text;
        LayoutElement layout = note.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = adjustedHeight;
        layout.preferredHeight = adjustedHeight;
        return note;
    }

    private void ApplyNotebookFlipPose()
    {
        if (notebookFlipPage == null)
        {
            return;
        }

        bool active = notebookPageFlip > 0.001f && notebookOpenAmount > 0.35f && !vhsModeEnabled;
        notebookFlipPage.gameObject.SetActive(active);
        if (!active)
        {
            return;
        }

        bool forward = notebookPageFlipDirection >= 0;
        notebookFlipPage.anchorMin = forward ? new Vector2(0.5f, 0f) : new Vector2(0f, 0f);
        notebookFlipPage.anchorMax = forward ? new Vector2(1f, 1f) : new Vector2(0.5f, 1f);
        notebookFlipPage.pivot = forward ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
        notebookFlipPage.offsetMin = forward ? new Vector2(4f, 0f) : Vector2.zero;
        notebookFlipPage.offsetMax = forward ? Vector2.zero : new Vector2(-4f, 0f);

        float t = 1f - notebookPageFlip;
        float width = Mathf.Lerp(1.0f, 0.05f, Mathf.Sin(t * Mathf.PI));
        notebookFlipPage.localScale = new Vector3(width, 1f, 1f);
        notebookFlipPage.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI) * (forward ? -3.8f : 3.8f));
        Image image = notebookFlipPage.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.83f, 0.72f, 0.52f, Mathf.Sin(t * Mathf.PI) * 0.72f);
        }
    }
}
