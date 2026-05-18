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
    private void SelectOverview()
    {
        selectedGrandpa = null;
        selectedBuilding = null;
        microHudUntil = 0f;
        RefreshDetails();
    }

    private void SelectGrandpa(Grandpa grandpa)
    {
        selectedGrandpa = grandpa;
        selectedBuilding = null;
        microHudUntil = 0f;
        RefreshDetails();
    }

    private void SelectBuilding(Building building)
    {
        selectedBuilding = building;
        selectedGrandpa = null;
        microHudUntil = 0f;
        RefreshDetails();
    }

    private int PopulationCap()
    {
        int cap = 3;
        Building bedroom = buildings[BuildingType.Bedroom];
        if (bedroom.Built)
        {
            cap += bedroom.Level * 5;
        }

        return cap;
    }

    private int BuiltCount()
    {
        int count = 0;
        foreach (Building building in buildings.Values)
        {
            if (building.Built)
            {
                count++;
            }
        }

        return count;
    }

    private int CountRole(GrandpaRole role)
    {
        int count = 0;
        for (int i = 0; i < grandpas.Count; i++)
        {
            if (grandpas[i].Role == role)
            {
                count++;
            }
        }

        return count;
    }

    private bool Spend(ResourceStock cost)
    {
        if (!CanAfford(cost))
        {
            return false;
        }

        stock.Tea -= cost.Tea;
        stock.Heat -= cost.Heat;
        stock.Cardboard -= cost.Cardboard;
        stock.Grumble -= cost.Grumble;
        stock.Coins -= cost.Coins;
        stock.ClampNonNegative();
        return true;
    }

    private bool CanAfford(ResourceStock cost)
    {
        return stock.Tea >= cost.Tea &&
               stock.Heat >= cost.Heat &&
               stock.Cardboard >= cost.Cardboard &&
               stock.Grumble >= cost.Grumble &&
               stock.Coins >= cost.Coins;
    }

    private ResourceStock BuddingCost()
    {
        float populationTax = Mathf.Max(0f, grandpas.Count - 6) * 0.35f;
        return new ResourceStock(10f + populationTax, 8f + populationTax, 0f, 10f + populationTax, 0f);
    }

    private ResourceStock UpgradeCost(Building building)
    {
        float level = building.Level + 1;
        switch (building.Type)
        {
            case BuildingType.FireBarrel:
                return new ResourceStock(0f, 0f, 8f * level, 5f * level, Mathf.Floor(level / 3f));
            case BuildingType.Samovar:
                return new ResourceStock(4f * level, 0f, 9f * level, 3f * level, Mathf.Floor(level / 4f));
            case BuildingType.Bedroom:
                return new ResourceStock(0f, 0f, 12f * level, 5f * level, Mathf.Floor(level / 3f));
            case BuildingType.GrumbleBench:
                return new ResourceStock(4f * level, 0f, 8f * level, 6f * level, 0f);
            case BuildingType.CarpetCurtain:
                return new ResourceStock(0f, 0f, 10f * level, 10f * level, Mathf.Floor(level / 3f));
            case BuildingType.RadioMayak:
                return new ResourceStock(8f * level, 0f, 14f * level, 10f * level, 2f * level);
            default:
                return new ResourceStock(0f, 0f, 0f, 0f, 0f);
        }
    }

    private string BuildingEffectText(Building building)
    {
        switch (building.Type)
        {
            case BuildingType.FireBarrel:
                return "Эффект: тепло +" + F((0.23f + building.Level * 0.11f) * 60f) + "/мин.";
            case BuildingType.Samovar:
                return "Эффект: чай +" + F((0.16f + building.Level * 0.08f + CountRole(GrandpaRole.SamovarKeeper) * 0.11f) * 60f) + "/мин.";
            case BuildingType.Bedroom:
                return "Эффект: лимит населения +" + (building.Level * 5) + ".";
            case BuildingType.GrumbleBench:
                return "Эффект: ворчание +" + F((0.11f + building.Level * 0.06f + CountRole(GrandpaRole.Mutterer) * 0.14f) * 60f) + "/мин.";
            case BuildingType.CarpetCurtain:
                return "Эффект: снижает рост подозрения на " + F((0.030f + building.Level * 0.015f) * 60f) + " ед./мин.";
            case BuildingType.RadioMayak:
                return "Эффект: события чаще, радиодеды приносят монетки.";
            default:
                return "";
        }
    }

    private string ProductionDescription(GrandpaRole role)
    {
        switch (role)
        {
            case GrandpaRole.SamovarKeeper:
                return "Производит: ускоряет чай у самовара.";
            case GrandpaRole.Cardboarder:
                return "Производит: приносит картон с краёв сцены.";
            case GrandpaRole.Mutterer:
                return "Производит: много ворчания.";
            case GrandpaRole.Guard:
                return "Производит: снижает рост подозрения города.";
            case GrandpaRole.Philosopher:
                return "Производит: странные улучшения в событиях.";
            case GrandpaRole.RadioReceiver:
                return "Производит: монетки и ранние слухи при Радио.";
            default:
                return "Производит: немного ворчания и картона.";
        }
    }

    private string GrandpaMood(Grandpa grandpa)
    {
        if (suspicion > 82f)
        {
            return "делает вид, что он ящик";
        }

        if (stock.Tea < 4f)
        {
            return "скучает по чаю";
        }

        if (grandpa.Budding > 90f)
        {
            return "почти почкуется";
        }

        if (stock.Heat < 5f)
        {
            return "бережёт шарф";
        }

        return "доволен настолько, насколько прилично";
    }

    private void Notify(string message)
    {
        lastAlert = message;
        alertUntil = Time.time + 7f;
        Debug.Log("[BridgeGrandpas] " + message);
    }

    private void ShowThought(Grandpa grandpa, string thought, float seconds)
    {
        if (grandpa.ThoughtText == null)
        {
            return;
        }

        grandpa.ThoughtText.text = thought;
        grandpa.ThoughtUntil = Time.time + seconds;
        grandpa.ThoughtText.gameObject.SetActive(true);
    }

    private bool Roll(float chance)
    {
        return UnityEngine.Random.value < chance;
    }

    private Vector3 RandomSpawnPosition()
    {
        return new Vector3(UnityEngine.Random.Range(-2.7f, 2.8f), 0f, UnityEngine.Random.Range(-2.4f, 0.85f));
    }

    private Vector3 Jitter(Vector3 center, float radius)
    {
        Vector2 offset = UnityEngine.Random.insideUnitCircle * radius;
        return new Vector3(center.x + offset.x, 0f, center.z + offset.y);
    }

    private string RandomGrandpaName()
    {
        string[] names =
        {
            "Дед Тихон", "Дед Степан", "Дед Пафнутий", "Дед Гриша", "Дед Фома",
            "Дед Савелий", "Дед Матвей", "Дед Егор", "Дед Прохор", "Дед Аркадий",
            "Дед Кузьма", "Дед Вениамин", "Дед Илья", "Дед Харитон", "Дед Борис"
        };

        string baseName = names[random.Next(names.Length)];
        return baseName + " #" + nextGrandpaId;
    }

    private string RoleName(GrandpaRole role)
    {
        switch (role)
        {
            case GrandpaRole.SamovarKeeper:
                return "Дед-самоварщик";
            case GrandpaRole.Cardboarder:
                return "Дед-картонщик";
            case GrandpaRole.Mutterer:
                return "Дед-бормотун";
            case GrandpaRole.Guard:
                return "Дед-сторож";
            case GrandpaRole.Philosopher:
                return "Дед-философ";
            case GrandpaRole.RadioReceiver:
                return "Дед-радиоприёмник";
            default:
                return "Обычный дед";
        }
    }

    private Color RoleColor(GrandpaRole role)
    {
        switch (role)
        {
            case GrandpaRole.SamovarKeeper:
                return new Color(0.62f, 0.20f, 0.12f);
            case GrandpaRole.Cardboarder:
                return new Color(0.48f, 0.32f, 0.14f);
            case GrandpaRole.Mutterer:
                return new Color(0.28f, 0.24f, 0.42f);
            case GrandpaRole.Guard:
                return new Color(0.14f, 0.34f, 0.24f);
            case GrandpaRole.Philosopher:
                return new Color(0.24f, 0.16f, 0.48f);
            case GrandpaRole.RadioReceiver:
                return new Color(0.08f, 0.42f, 0.40f);
            default:
                return new Color(0.22f, 0.25f, 0.31f);
        }
    }

    private string ObjectiveLine(string label, bool done)
    {
        return (done ? "<color=#9cff93>✓</color> " : "<color=#ffcf7a>•</color> ") + label;
    }

    private string F(float value)
    {
        return Mathf.FloorToInt(value).ToString();
    }

    private Vector2 GetPointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif
        return Input.mousePosition;
    }

    private bool WasPrimaryPointerPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }
#endif
        return Input.GetMouseButtonDown(0);
    }

}

