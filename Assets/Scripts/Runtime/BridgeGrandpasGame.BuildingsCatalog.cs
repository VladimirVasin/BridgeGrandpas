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
    private void SetupBuildings()
    {
        AddBuilding(BuildingType.FireBarrel, "Бочка с огнём", new Vector3(0f, 0f, -0.1f), new ResourceStock(0f, 0f, 0f, 0f, 0f), "Даёт тепло и центрирует всю дедовскую цивилизацию.");
        AddBuilding(BuildingType.Samovar, "Самоварный узел", new Vector3(-2.4f, 0f, -0.1f), new ResourceStock(0f, 0f, 8f, 0f, 0f), "Производит чай. Самоварщики делают его убедительнее.");
        AddBuilding(BuildingType.Bedroom, "Картонная спальня", new Vector3(2.35f, 0f, -0.25f), new ResourceStock(0f, 0f, 10f, 0f, 0f), "Увеличивает лимит населения на 5 за уровень.");
        AddBuilding(BuildingType.GrumbleBench, "Скамейка ворчания", new Vector3(-2.15f, 0f, -2.0f), new ResourceStock(5f, 0f, 8f, 0f, 0f), "Собирает ворчание в пригодную к управлению форму.");
        AddBuilding(BuildingType.CarpetCurtain, "Занавес из ковров", new Vector3(2.45f, 0f, 1.75f), new ResourceStock(0f, 0f, 12f, 8f, 0f), "Снижает рост подозрения города.");
        AddBuilding(BuildingType.RadioMayak, "Радио \"Маяк\"", new Vector3(2.2f, 0f, -2.05f), new ResourceStock(12f, 0f, 18f, 10f, 4f), "Включает частые события и городские слухи.");
    }

    private void AddBuilding(BuildingType type, string name, Vector3 position, ResourceStock cost, string description)
    {
        buildings[type] = new Building
        {
            Type = type,
            Name = name,
            Position = position,
            BuildCost = cost,
            Description = description
        };
    }

    private void BuildInitialState()
    {
        ResetNotebookObservations();
        TryBuild(BuildingType.FireBarrel, true);
        CreateStarterCommuneProps();
        Grandpa first = SpawnGrandpa(GrandpaRole.Common, new Vector3(-0.6f, 0f, -1.15f));
        QueueObservationLead("первый дед", first.Name + " замечен первым. Пока это не толпа, а один очень уверенный дед.",
            first.Root != null ? first.Root.transform : null, first.Target, 0.08f);
        Notify("Первый дедушка обжил сухое пятно под мостом. Государство пока помещается в одном пальто.");
        QueueObservationLead("первое государство", "Государство пока помещается в одном пальто.",
            first.Root != null ? first.Root.transform : null, first.Target, 0.05f);
    }
}

