using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
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
        notebookTitleText.text = NotebookPageTitle(currentNotebookPage);
        AddNotebookTextTo(notebookLeftPageContent, NotebookPageBody(currentNotebookPage), 17, FontStyle.Normal, 340f);

        switch (currentNotebookPage)
        {
            case NotebookPage.Summary:
                BuildNotebookSummaryRightPage();
                break;
            case NotebookPage.Observations:
                BuildNotebookObservationsPage();
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
            return BuildNotebookSummary();
        }

        if (page == NotebookPage.Observations)
        {
            return BuildNotebookObservationsIntro();
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

    private string BuildNotebookSummary()
    {
        string victory = victoryShown
            ? "<b>Под мостом образовалась устойчивая дедовская цивилизация.</b>\nEndless-режим продолжается.\n\n"
            : "";
        return victory + BuildTopResourceStats() + "\n\n" +
            "Дедушки: " + grandpas.Count + "/" + PopulationCap() +
            "    проверки: " + inspectionsSurvived + "/" + VictoryInspections +
            "    " + CozyStat() + "\n" +
            "Постройки: " + BuiltCount() + "/" + buildings.Count +
            "    подозрение города: " + Mathf.RoundToInt(suspicion) + "%\n\n" +
            "Последняя запись: " + LatestNotebookObservation();
    }

    private Text AddNotebookText(string text, int size, FontStyle style, float minHeight)
    {
        return AddNotebookTextTo(notebookPageContent, text, size, style, minHeight);
    }

    private Text AddNotebookTextTo(Transform parent, string text, int size, FontStyle style, float minHeight)
    {
        Text note = CreateText("Notebook Text", parent, size, style, TextAnchor.UpperLeft, new Color(0.13f, 0.075f, 0.04f));
        note.supportRichText = true;
        note.verticalOverflow = VerticalWrapMode.Overflow;
        note.text = text;
        LayoutElement layout = note.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = minHeight;
        layout.preferredHeight = minHeight;
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

        float t = 1f - notebookPageFlip;
        float width = Mathf.Lerp(1.0f, 0.05f, Mathf.Sin(t * Mathf.PI));
        notebookFlipPage.localScale = new Vector3(width, 1f, 1f);
        notebookFlipPage.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI) * -3.8f);
        Image image = notebookFlipPage.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.83f, 0.72f, 0.52f, Mathf.Sin(t * Mathf.PI) * 0.72f);
        }
    }
}
