using System;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const int SaveSlotCount = 3;

    private enum SaveSlotScreenMode
    {
        Load,
        Save
    }

    private RectTransform saveSlotScreenRoot;
    private RectTransform saveSlotListRoot;
    private Text saveSlotTitleText;
    private Text saveSlotHintText;
    private bool saveSlotScreenOpen;
    private SaveSlotScreenMode saveSlotScreenMode;
    private int startMenuSelectedSaveSlot = 1;

    private void CreateSaveSlotScreen(Transform parent)
    {
        saveSlotScreenRoot = CreatePanel("Save Slot Screen", parent, new Color(0f, 0f, 0f, 0f));
        saveSlotScreenRoot.anchorMin = Vector2.zero;
        saveSlotScreenRoot.anchorMax = Vector2.one;
        saveSlotScreenRoot.offsetMin = Vector2.zero;
        saveSlotScreenRoot.offsetMax = Vector2.zero;
        saveSlotScreenRoot.gameObject.SetActive(false);

        RectTransform panel = CreatePanel("Save Slot Panel", saveSlotScreenRoot, new Color(0.025f, 0.022f, 0.018f, 0.94f));
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = new Vector2(0f, 18f);
        panel.sizeDelta = new Vector2(660f, 540f);
        Outline outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.92f, 0.56f, 0.20f, 0.38f);
        outline.effectDistance = new Vector2(2f, -2f);

        saveSlotTitleText = CreateText("Save Slot Title", panel, 27, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.78f, 0.40f));
        saveSlotTitleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        saveSlotTitleText.rectTransform.anchorMax = new Vector2(1f, 1f);
        saveSlotTitleText.rectTransform.offsetMin = new Vector2(26f, -76f);
        saveSlotTitleText.rectTransform.offsetMax = new Vector2(-26f, -22f);

        saveSlotHintText = CreateText("Save Slot Hint", panel, 15, FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.90f, 0.82f, 0.68f));
        saveSlotHintText.rectTransform.anchorMin = new Vector2(0f, 1f);
        saveSlotHintText.rectTransform.anchorMax = new Vector2(1f, 1f);
        saveSlotHintText.rectTransform.offsetMin = new Vector2(30f, -112f);
        saveSlotHintText.rectTransform.offsetMax = new Vector2(-30f, -78f);

        saveSlotListRoot = CreatePanel("Save Slot List", panel, new Color(0f, 0f, 0f, 0f));
        saveSlotListRoot.anchorMin = new Vector2(0.5f, 0.5f);
        saveSlotListRoot.anchorMax = new Vector2(0.5f, 0.5f);
        saveSlotListRoot.pivot = new Vector2(0.5f, 0.5f);
        saveSlotListRoot.anchoredPosition = new Vector2(0f, -28f);
        saveSlotListRoot.sizeDelta = new Vector2(560f, 340f);
        VerticalLayoutGroup layout = saveSlotListRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        RectTransform backButton = CreateMenuButton("Назад", panel, HideSaveSlotScreen);
        backButton.anchorMin = new Vector2(0.5f, 0f);
        backButton.anchorMax = new Vector2(0.5f, 0f);
        backButton.pivot = new Vector2(0.5f, 0f);
        backButton.anchoredPosition = new Vector2(0f, 24f);
        backButton.sizeDelta = new Vector2(220f, 52f);
    }

    private void ShowSaveSlotScreen(SaveSlotScreenMode mode)
    {
        if (saveSlotScreenRoot == null)
        {
            return;
        }

        saveSlotScreenMode = mode;
        saveSlotScreenOpen = true;
        saveSlotScreenRoot.gameObject.SetActive(true);
        RefreshSaveSlotScreen();

        if (startMenuLoadingRoot != null)
        {
            startMenuLoadingRoot.gameObject.SetActive(false);
        }

        if (startMenuButtonsGroup != null)
        {
            startMenuButtonsGroup.interactable = false;
            startMenuButtonsGroup.blocksRaycasts = false;
        }

        WriteDebugLog("SAVE_SLOTS", "Opened slot screen mode=" + mode + " hasAnySave=" + HasAnySaveSlot());
    }

    private void HideSaveSlotScreen()
    {
        HideSaveSlotScreenOnly();
        ApplyStartMenuButtonMode(escapeMenuOpen);
        SetStartMenuButtonsInteractable(true);
    }

    private void HideSaveSlotScreenOnly()
    {
        saveSlotScreenOpen = false;
        if (saveSlotScreenRoot != null)
        {
            saveSlotScreenRoot.gameObject.SetActive(false);
        }
    }

    private void RefreshSaveSlotScreen()
    {
        if (saveSlotTitleText != null)
        {
            saveSlotTitleText.text = saveSlotScreenMode == SaveSlotScreenMode.Save ? "Сохранить игру" : "Загрузить игру";
        }

        if (saveSlotHintText != null)
        {
            saveSlotHintText.text = saveSlotScreenMode == SaveSlotScreenMode.Save
                ? "Выбери слот, куда лечь этой тревожной записи."
                : "Выбери слот, из которого поднять старые записи.";
        }

        ClearChildren(saveSlotListRoot);
        for (int slot = 1; slot <= SaveSlotCount; slot++)
        {
            int slotSnapshot = slot;
            RectTransform buttonRect = CreateMenuButton(SaveSlotButtonLabel(slot), saveSlotListRoot, delegate
            {
                SelectSaveSlot(slotSnapshot);
            });

            LayoutElement layout = buttonRect.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.minHeight = 72f;
                layout.preferredHeight = 76f;
            }

            Text label = buttonRect.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.alignment = TextAnchor.MiddleLeft;
                label.fontSize = 16;
                label.rectTransform.offsetMin = new Vector2(20f, 4f);
                label.rectTransform.offsetMax = new Vector2(-20f, -4f);
            }

            Button button = buttonRect.GetComponent<Button>();
            if (button != null && saveSlotScreenMode == SaveSlotScreenMode.Load)
            {
                button.interactable = HasSaveSlot(slot);
            }
        }

        if (saveSlotScreenMode == SaveSlotScreenMode.Load && HasFakeCorruptedSaveSlot())
        {
            CreateFakeCorruptedSaveSlotButton();
        }
    }

    private void SelectSaveSlot(int slotIndex)
    {
        int normalizedSlot = NormalizeSaveSlotIndex(slotIndex);
        startMenuSelectedSaveSlot = normalizedSlot;

        if (saveSlotScreenMode == SaveSlotScreenMode.Save)
        {
            SaveGameToSlot(normalizedSlot);
            HideSaveSlotScreenOnly();
            Notify("Сохранено в слот " + normalizedSlot + ": блокнот прижал страницу, чтобы её не унесло ветром.");
            MarkNotebookDirty();
            RefreshAllUi();
            CloseEscapeMenu();
            return;
        }

        if (!HasSaveSlot(normalizedSlot))
        {
            Notify("Слот " + normalizedSlot + " пуст.");
            RefreshSaveSlotScreen();
            return;
        }

        HideSaveSlotScreenOnly();
        startMenuLoadCorruptedSave = false;
        if (gameStarted)
        {
            if (!TryLoadGameFromSlot(normalizedSlot))
            {
                Notify("Сохранение не прочиталось. Блокнот делает вид, что так и было.");
                ShowSaveSlotScreen(SaveSlotScreenMode.Load);
                return;
            }

            CloseEscapeMenu();
            SelectOverview();
            BeginStartIrisFade();
            MarkNotebookDirty();
            RefreshAllUi();
            Notify("Загружено: дедовская коммуна вернулась к старым записям.");
            return;
        }

        BeginStartMenuLoading(true);
    }

    private string SaveSlotButtonLabel(int slotIndex)
    {
        string json = GetSaveSlotJson(slotIndex);
        if (string.IsNullOrEmpty(json))
        {
            return "<b>Слот " + slotIndex + "</b>\n<color=#8f8372>пусто</color>";
        }

        SaveData data;
        try
        {
            data = JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception)
        {
            return "<b>Слот " + slotIndex + "</b>\n<color=#ff8f7a>запись повреждена</color>";
        }

        if (data == null || data.Grandpas == null)
        {
            return "<b>Слот " + slotIndex + "</b>\n<color=#ff8f7a>запись повреждена</color>";
        }

        int day = CurrentObservationDay + Mathf.Max(0, DayClockIndex(data.DayClockElapsedSeconds));
        int built = 0;
        if (data.Buildings != null)
        {
            for (int i = 0; i < data.Buildings.Count; i++)
            {
                if (data.Buildings[i] != null && data.Buildings[i].Built)
                {
                    built++;
                }
            }
        }

        return "<b>Слот " + slotIndex + "</b>\nДень " + day + " | " +
            FormatDayClockElapsedSeconds(data.DayClockElapsedSeconds) +
            " | деды: " + data.Grandpas.Count + " | постройки: " + built;
    }

    private void CreateFakeCorruptedSaveSlotButton()
    {
        RectTransform buttonRect = CreateMenuButton(FakeCorruptedSaveSlotLabel(), saveSlotListRoot, SelectFakeCorruptedSaveSlot);
        LayoutElement layout = buttonRect.GetComponent<LayoutElement>();
        if (layout != null)
        {
            layout.minHeight = 78f;
            layout.preferredHeight = 84f;
        }

        Text label = buttonRect.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.alignment = TextAnchor.MiddleLeft;
            label.fontSize = 15;
            label.color = new Color(1f, 0.64f, 0.55f);
            label.rectTransform.offsetMin = new Vector2(20f, 4f);
            label.rectTransform.offsetMax = new Vector2(-20f, -4f);
        }
    }

    private string FakeCorruptedSaveSlotLabel()
    {
        return "<b><color=#ff5b4d>Слот ???</color></b>\n" +
            "<color=#b9a0a0>00:00 | observer.dll не отвечает | под мостом ничего нет</color>";
    }

    private bool HasAnySaveSlot()
    {
        for (int slot = 1; slot <= SaveSlotCount; slot++)
        {
            if (HasSaveSlot(slot))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasSaveSlot(int slotIndex)
    {
        return !string.IsNullOrEmpty(GetSaveSlotJson(slotIndex));
    }

    private string GetSaveSlotJson(int slotIndex)
    {
        int normalizedSlot = NormalizeSaveSlotIndex(slotIndex);
        string slotJson = PlayerPrefs.GetString(SaveSlotKey(normalizedSlot), "");
        if (!string.IsNullOrEmpty(slotJson))
        {
            return slotJson;
        }

        return normalizedSlot == 1 ? PlayerPrefs.GetString(SaveKey, "") : "";
    }

    private string SaveSlotSourceKey(int slotIndex)
    {
        int normalizedSlot = NormalizeSaveSlotIndex(slotIndex);
        string slotKey = SaveSlotKey(normalizedSlot);
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString(slotKey, "")))
        {
            return slotKey;
        }

        return normalizedSlot == 1 && !string.IsNullOrEmpty(PlayerPrefs.GetString(SaveKey, "")) ? SaveKey : slotKey;
    }

    private string SaveSlotKey(int slotIndex)
    {
        return SaveKey + ".slot" + NormalizeSaveSlotIndex(slotIndex);
    }

    private int NormalizeSaveSlotIndex(int slotIndex)
    {
        return Mathf.Clamp(slotIndex, 1, SaveSlotCount);
    }
}
