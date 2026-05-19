using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
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
            string name = building.Name + " ур. " + building.Level + " -> " + (building.Level + 1);
            RectTransform button = CreateButton(BuildResourceActionLabel(name, cost, "Можно улучшить"), trayBody, delegate
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
                TrySpinRadio();
            });
            button.GetComponent<Button>().interactable = stock.Coins >= 1f;
        }
    }

    private void TrySpinRadio()
    {
        if (stock.Coins < 1f)
        {
            Notify("Нужна хотя бы 1 монетка на батарейки.");
            return;
        }

        stock.Coins -= 1f;
        if (!EventAutoTriggerEnabled)
        {
            TriggerRandomEvent();
            RefreshAllUi();
            return;
        }

        nextEventIn = Mathf.Min(nextEventIn, 5f);
        Notify("Радио щёлкнуло. Слух почти пойман.");
        RefreshAllUi();
    }

    private void BuildGrandpasTray()
    {
        trayTitleText.text = "Дедушки";
        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            string state = grandpa.IsOnExpedition
                ? "в вылазке " + Mathf.CeilToInt(grandpa.ExpeditionUntil - Time.time) + "с"
                : "почкование " + Mathf.FloorToInt(grandpa.Budding) + "%";
            string label = grandpa.Name + " | " + RoleName(grandpa.Role) + " | " + state;
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
