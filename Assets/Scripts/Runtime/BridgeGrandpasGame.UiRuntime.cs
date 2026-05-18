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
    private void HandlePointer()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            hoveredTarget = null;
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(GetPointerPosition());
        RaycastHit hit;
        BridgeGrandpasSelectionTarget target = null;
        if (Physics.Raycast(ray, out hit, 80f))
        {
            target = hit.collider.GetComponentInParent<BridgeGrandpasSelectionTarget>();
        }

        hoveredTarget = target;

        if (WasPrimaryPointerPressed())
        {
            if (target == null)
            {
                SelectOverview();
            }
            else if (target.Kind == SelectionKind.Grandpa)
            {
                SelectGrandpa(target.Grandpa as Grandpa);
            }
            else if (target.Kind == SelectionKind.Building)
            {
                SelectBuilding(target.Building as Building);
            }
        }
    }

    private void UpdateMarkers()
    {
        if (selectionMarker != null)
        {
            bool hasSelection = selectedGrandpa != null || selectedBuilding != null;
            selectionMarker.SetActive(hasSelection);
            if (hasSelection)
            {
                Vector3 position = selectedGrandpa != null ? selectedGrandpa.Root.transform.position : selectedBuilding.Position;
                selectionMarker.transform.position = new Vector3(position.x, 0.035f, position.z);
            }
        }

        if (hoverMarker != null)
        {
            hoverMarker.SetActive(hoveredTarget != null);
            if (hoveredTarget != null)
            {
                Grandpa hoveredGrandpa = hoveredTarget.Grandpa as Grandpa;
                Building hoveredBuilding = hoveredTarget.Building as Building;
                Vector3 position = hoveredTarget.Kind == SelectionKind.Grandpa && hoveredGrandpa != null
                    ? hoveredGrandpa.Root.transform.position
                    : hoveredBuilding != null ? hoveredBuilding.Position : Vector3.zero;
                hoverMarker.transform.position = new Vector3(position.x, 0.04f, position.z);
            }
        }
    }

    private void UpdateBillboards()
    {
        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            if (grandpa.ThoughtText == null)
            {
                continue;
            }

            bool visible = Time.time < grandpa.ThoughtUntil;
            grandpa.ThoughtText.gameObject.SetActive(visible);
            if (!visible)
            {
                continue;
            }

            grandpa.ThoughtText.transform.position = grandpa.Root.transform.position + new Vector3(0f, 1.25f, 0f);
            grandpa.ThoughtText.transform.rotation = mainCamera.transform.rotation;
        }
    }

    private void UpdateAmbientUi()
    {
        if (Time.frameCount % 8 != 0)
        {
            return;
        }

        RefreshTopStats();
        RefreshDetails();
        RefreshSuspicionBar();
        CheckVictory();
    }

    private void RefreshAllUi()
    {
        RefreshTopStats();
        RefreshDetails();
        RefreshTray();
        RefreshSuspicionBar();
    }

    private void ToggleTray(UiTab tab)
    {
        if (trayOpen && currentTab == tab)
        {
            CloseTray();
            return;
        }

        currentTab = tab;
        trayOpen = true;
        microHudUntil = 0f;
        RefreshDetails();
        RefreshTray();
    }

    private void CloseTray()
    {
        trayOpen = false;
        if (trayPanel != null)
        {
            trayPanel.gameObject.SetActive(false);
        }

        RefreshDetails();
    }

    private void ShowMicroHudMessage(string title, string body, float seconds)
    {
        microHudTitle = title;
        microHudBody = body;
        microHudUntil = Time.time + seconds;
        RefreshDetails();
    }

    private void UpdateMicroHudPanel(float deltaTime)
    {
        if (detailPanel == null)
        {
            return;
        }

        bool wanted = ShouldShowMicroHud();
        if (wanted && !detailPanel.gameObject.activeSelf)
        {
            detailPanel.gameObject.SetActive(true);
        }

        float target = wanted ? 1f : 0f;
        detailPanelSlide = Mathf.Lerp(detailPanelSlide, target, 1f - Mathf.Exp(-deltaTime * 12f));
        ApplyMicroHudPanelPose();

        if (!wanted && detailPanelSlide <= 0.01f)
        {
            detailPanel.gameObject.SetActive(false);
        }
    }

    private bool ShouldShowMicroHud()
    {
        return selectedGrandpa != null || selectedBuilding != null || trayOpen || Time.time < microHudUntil;
    }

    private void ApplyMicroHudPanelPose()
    {
        if (detailPanel == null)
        {
            return;
        }

        float t = Mathf.SmoothStep(0f, 1f, detailPanelSlide);
        detailPanel.offsetMin = Vector2.Lerp(detailPanelHiddenOffsetMin, detailPanelShownOffsetMin, t);
        detailPanel.offsetMax = Vector2.Lerp(detailPanelHiddenOffsetMax, detailPanelShownOffsetMax, t);
        if (detailPanelGroup != null)
        {
            detailPanelGroup.alpha = t;
        }
    }

    private void RefreshTopStats()
    {
        int cap = PopulationCap();
        topStatsText.text =
            "Чай " + F(stock.Tea) +
            "   Тепло " + F(stock.Heat) +
            "   Картон " + F(stock.Cardboard) +
            "   Ворчание " + F(stock.Grumble) +
            "   Монетки " + F(stock.Coins) +
            "   Дедушки " + grandpas.Count + "/" + cap +
            "   Проверки " + inspectionsSurvived + "/3";

        alertText.text = Time.time < alertUntil ? lastAlert : "";
    }

    private void RefreshSuspicionBar()
    {
        if (suspicionFill == null)
        {
            return;
        }

        RectTransform rect = suspicionFill.rectTransform;
        RectTransform parent = rect.parent as RectTransform;
        float width = parent == null ? 300f : parent.rect.width;
        rect.sizeDelta = new Vector2(width * Mathf.Clamp01(suspicion / MaxSuspicion), 0f);
    }

    private void RefreshDetails()
    {
        if (detailText == null)
        {
            return;
        }

        if (selectedGrandpa != null)
        {
            detailText.text =
                "<b>" + selectedGrandpa.Name + "</b>\n" +
                RoleName(selectedGrandpa.Role) + "\n\n" +
                ProductionDescription(selectedGrandpa.Role) + "\n\n" +
                "Состояние: " + GrandpaMood(selectedGrandpa) + "\n" +
                "Готовность к почкованию: " + Mathf.FloorToInt(selectedGrandpa.Budding) + "%\n\n" +
                "Стоимость почкования:\n" + BuddingCost().ColoredCost(stock) + "\n\n" +
                "Мысль: " + (Time.time < selectedGrandpa.ThoughtUntil ? selectedGrandpa.ThoughtText.text : RandomThought(selectedGrandpa));
            return;
        }

        if (selectedBuilding != null)
        {
            string blocked = selectedBuilding.IsBlocked ? "\n<color=#ff9b72>Комиссия блокирует: " + Mathf.CeilToInt(selectedBuilding.BlockedUntil - Time.time) + "с</color>" : "";
            detailText.text =
                "<b>" + selectedBuilding.Name + "</b>\n" +
                "Уровень: " + selectedBuilding.Level + (selectedBuilding.Built ? "" : " (не построено)") + blocked + "\n\n" +
                selectedBuilding.Description + "\n\n" +
                BuildingEffectText(selectedBuilding) + "\n\n" +
                (selectedBuilding.Built
                    ? "Улучшение:\n" + UpgradeCost(selectedBuilding).ColoredCost(stock)
                    : "Постройка:\n" + selectedBuilding.BuildCost.ColoredCost(stock));
            return;
        }

        if (Time.time < microHudUntil)
        {
            detailText.text = "<b>" + microHudTitle + "</b>\n\n" + microHudBody;
            return;
        }

        if (trayOpen)
        {
            detailText.text = BuildTrayMicroHudText();
            return;
        }

        detailText.text = "";
    }

    private string BuildTrayMicroHudText()
    {
        switch (currentTab)
        {
            case UiTab.Build:
                return "<b>Постройки</b>\n\n" +
                    "Выбери объект в меню слева. Доступные строки подсвечены зелёным, недоступные показывают нехватку ресурсов.\n\n" +
                    "Построено: " + BuiltCount() + "/" + buildings.Count + "\n" +
                    "Картон: " + F(stock.Cardboard) + "   Монетки: " + F(stock.Coins);
            case UiTab.Upgrade:
                return "<b>Улучшения</b>\n\n" +
                    "Улучшать можно только уже построенные объекты. Клик по строке сразу выберет объект и покажет его карточку.\n\n" +
                    "Построено объектов: " + BuiltCount();
            case UiTab.Events:
                return "<b>События</b>\n\n" +
                    (pendingEvent != null
                        ? "Есть событие: " + pendingEvent.Title + ". Открой карточку и выбери реакцию коммуны."
                        : "Следующее событие примерно через " + Mathf.CeilToInt(nextEventIn) + "с.\nРадио делает город слышнее.");
            case UiTab.Grandpas:
                return "<b>Дедушки</b>\n\n" +
                    "Клик по дедушке в списке выберет его на сцене и откроет персональный microHUD.\n\n" +
                    "Население: " + grandpas.Count + "/" + PopulationCap() + "\n" +
                    "Самый готовый: " + Mathf.FloorToInt(BestBuddingPercent()) + "%";
            default:
                return "";
        }
    }

    private string BuildBuddingMicroHudText()
    {
        return "Почкование требует готового дедушку, свободное место и ресурсы.\n\n" +
            "Самый готовый дедушка: " + Mathf.FloorToInt(BestBuddingPercent()) + "%\n" +
            "Население: " + grandpas.Count + "/" + PopulationCap() + "\n\n" +
            "Стоимость:\n" + BuddingCost().ColoredCost(stock);
    }

    private float BestBuddingPercent()
    {
        float best = 0f;
        for (int i = 0; i < grandpas.Count; i++)
        {
            best = Mathf.Max(best, grandpas[i].Budding);
        }

        return best;
    }

    private void RefreshTray()
    {
        if (trayBody == null)
        {
            return;
        }

        if (!trayOpen)
        {
            ClearChildren(trayBody);
            if (trayPanel != null)
            {
                trayPanel.gameObject.SetActive(false);
            }

            return;
        }

        if (trayPanel != null)
        {
            trayPanel.gameObject.SetActive(true);
        }

        ClearChildren(trayBody);

        switch (currentTab)
        {
            case UiTab.Build:
                BuildBuildTray();
                break;
            case UiTab.Upgrade:
                BuildUpgradeTray();
                break;
            case UiTab.Events:
                BuildEventsTray();
                break;
            case UiTab.Grandpas:
                BuildGrandpasTray();
                break;
        }

        if (trayScroll != null)
        {
            Canvas.ForceUpdateCanvases();
            trayScroll.verticalNormalizedPosition = 1f;
        }
    }

    private void BuildBuildTray()
    {
        trayTitleText.text = "Постройки";
        foreach (Building building in buildings.Values)
        {
            if (building.Type == BuildingType.FireBarrel)
            {
                continue;
            }

            string label = building.Built
                ? building.Name + "\n<color=#9cff93>Построено, уровень " + building.Level + "</color>"
                : BuildResourceActionLabel(building.Name, building.BuildCost, "Можно построить");

            RectTransform button = CreateButton(label, trayBody, delegate
            {
                TryBuild(building.Type, false);
                SelectBuilding(building);
                RefreshAllUi();
            });
            button.GetComponent<Button>().interactable = !building.Built && CanAfford(building.BuildCost);
        }

        if (BuiltCount() >= buildings.Count)
        {
            AddTrayNote("Все базовые объекты построены. Теперь цивилизация растёт через улучшения и почкование.");
        }
    }

    private void BuildUpgradeTray()
    {
        trayTitleText.text = "Улучшения";
        foreach (Building building in buildings.Values)
        {
            if (!building.Built)
            {
                continue;
            }

            ResourceStock cost = UpgradeCost(building);
            string label = BuildResourceActionLabel(building.Name + " ур. " + building.Level + " -> " + (building.Level + 1), cost, "Можно улучшить");
            RectTransform button = CreateButton(label, trayBody, delegate
            {
                TryUpgrade(building);
                SelectBuilding(building);
                RefreshAllUi();
            });
            button.GetComponent<Button>().interactable = CanAfford(cost);
        }
    }

    private void BuildEventsTray()
    {
        trayTitleText.text = "События и слухи";
        if (pendingEvent != null)
        {
            CreateButton("Открыть событие: " + pendingEvent.Title, trayBody, delegate
            {
                ShowEventModal(pendingEvent);
            });
            return;
        }

        bool radioBuilt = buildings[BuildingType.RadioMayak].Built;
        AddTrayNote(radioBuilt
            ? "Радио работает. Следующее событие примерно через " + Mathf.CeilToInt(nextEventIn) + "с."
            : "Без радио события редкие. Построй Радио \"Маяк\", чтобы город начал звучать.");

        if (radioBuilt)
        {
            RectTransform button = CreateButton("Покрутить ручку радио", trayBody, delegate
            {
                if (stock.Coins >= 1f)
                {
                    stock.Coins -= 1f;
                    nextEventIn = Mathf.Min(nextEventIn, 5f);
                    Notify("Радио щёлкнуло. Слух почти пойман.");
                    RefreshAllUi();
                }
                else
                {
                    Notify("Нужна хотя бы 1 монетка на батарейки.");
                }
            });
            button.GetComponent<Button>().interactable = stock.Coins >= 1f;
        }
    }

    private void BuildGrandpasTray()
    {
        trayTitleText.text = "Дедушки";
        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            string label = grandpa.Name + " | " + RoleName(grandpa.Role) + " | почкование " + Mathf.FloorToInt(grandpa.Budding) + "%";
            CreateButton(label, trayBody, delegate
            {
                SelectGrandpa(grandpa);
                RefreshAllUi();
            });
        }
    }

    private void AddTrayNote(string text)
    {
        Text note = CreateText("Tray Note", trayBody, 16, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.86f, 0.88f, 0.88f));
        note.text = text;
        note.rectTransform.sizeDelta = new Vector2(0f, 44f);
        LayoutElement layout = note.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = 44f;
        layout.preferredHeight = 52f;
    }

    private string BuildResourceActionLabel(string title, ResourceStock cost, string availableText)
    {
        if (CanAfford(cost))
        {
            return title + "\n<color=#9cff93>" + availableText + "</color>  <color=#d7c08a>" + cost.ShortText() + "</color>";
        }

        return title + "\n<color=#ff8f7a>Не хватает: " + MissingResourceText(cost) + "</color>";
    }

    private string MissingResourceText(ResourceStock cost)
    {
        List<string> missing = new List<string>();
        AddMissingResource(missing, "чай", cost.Tea, stock.Tea);
        AddMissingResource(missing, "тепло", cost.Heat, stock.Heat);
        AddMissingResource(missing, "картон", cost.Cardboard, stock.Cardboard);
        AddMissingResource(missing, "ворчание", cost.Grumble, stock.Grumble);
        AddMissingResource(missing, "монетки", cost.Coins, stock.Coins);
        return missing.Count == 0 ? "ничего" : string.Join(", ", missing.ToArray());
    }

    private void AddMissingResource(List<string> missing, string label, float need, float have)
    {
        float deficit = Mathf.Ceil(need - have);
        if (deficit > 0f)
        {
            missing.Add(label + " " + Mathf.CeilToInt(deficit));
        }
    }

}

