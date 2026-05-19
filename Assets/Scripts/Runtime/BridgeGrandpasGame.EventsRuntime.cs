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
    private void TriggerInspection()
    {
        inspectionsSurvived++;
        suspicion = 28f;
        stock.Cardboard *= 0.82f;
        stock.Tea *= 0.90f;
        BlockRandomBuilding(UnityEngine.Random.Range(40f, 75f));
        BoostCityAmbience(55f);
        Notify("Городская комиссия провела проверку. Часть картона забрали, один объект временно опечатан.");
        RefreshAllUi();
    }

    private void TriggerRandomEvent()
    {
        if (events.Count == 0)
        {
            return;
        }

        pendingEvent = events[random.Next(events.Count)];
        Notify("Новое событие: " + pendingEvent.Title + ".");
        ShowEventModal(pendingEvent);
        RefreshTray();
    }

    private void ShowEventModal(BridgeEvent bridgeEvent)
    {
        pendingEvent = bridgeEvent;
        eventTitleText.text = bridgeEvent.Title;
        eventBodyText.text = bridgeEvent.Body;
        ClearChildren(eventChoicesRoot);

        for (int i = 0; i < bridgeEvent.Choices.Length; i++)
        {
            EventChoice choice = bridgeEvent.Choices[i];
            CreateDialogChoiceButton(choice.Label, choice.Preview, eventChoicesRoot, delegate
            {
                choice.Apply(this);
                pendingEvent = null;
                eventModal.gameObject.SetActive(false);
                suspicion = Mathf.Clamp(suspicion, 0f, MaxSuspicion);
                stock.ClampNonNegative();
                RefreshAllUi();
            });
        }

        eventModal.gameObject.SetActive(true);
    }

    private void BlockRandomBuilding(float seconds)
    {
        List<Building> candidates = new List<Building>();
        foreach (Building building in buildings.Values)
        {
            if (building.Built)
            {
                candidates.Add(building);
            }
        }

        if (candidates.Count == 0)
        {
            return;
        }

        Building selected = candidates[random.Next(candidates.Count)];
        BlockBuilding(selected.Type, seconds);
    }

    private void BlockBuilding(BuildingType type, float seconds)
    {
        Building building = buildings[type];
        if (!building.Built)
        {
            return;
        }

        building.BlockedUntil = Time.time + seconds;
    }

    private void UnlockPhilosophicalIdea()
    {
        int roll = random.Next(0, 4);
        if (roll == 0)
        {
            stock.Grumble += 22f;
            Notify("Открыта мысль: \"Коллективное ворчание\". +22 ворчания.");
        }
        else if (roll == 1)
        {
            AddBuddingToAll(10f);
            Notify("Открыта мысль: \"Тёплая мысль\". Все дедушки ближе к почкованию.");
        }
        else if (roll == 2)
        {
            stock.Tea += 18f;
            Notify("Открыта мысль: \"Самоварная дисциплина\". +18 чая.");
        }
        else
        {
            stock.Cardboard += 18f;
            Notify("Открыта мысль: \"Картонная архитектура\". +18 картона.");
        }
    }

    private void AddBuddingToAll(float amount)
    {
        for (int i = 0; i < grandpas.Count; i++)
        {
            grandpas[i].Budding = Mathf.Min(BuddingGoal, grandpas[i].Budding + amount);
        }
    }

    private void CheckVictory()
    {
        if (victoryShown)
        {
            return;
        }

        if (grandpas.Count >= VictoryGrandpas && BuiltCount() >= VictoryBuildings && inspectionsSurvived >= VictoryInspections && rareMutationSeen)
        {
            victoryShown = true;
            victoryModal.gameObject.SetActive(true);
            Notify("MVP-цель выполнена. Под мостом стало исторически важно.");
        }
    }

}

