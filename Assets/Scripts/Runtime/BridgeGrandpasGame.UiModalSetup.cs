using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
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
        if (!ExpeditionNarrativeAutoTriggerEnabled)
        {
            return;
        }

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
