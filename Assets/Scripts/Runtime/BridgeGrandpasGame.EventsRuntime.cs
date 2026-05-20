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
        Building blocked = BlockRandomBuilding(UnityEngine.Random.Range(40f, 75f));
        BoostCityAmbience(55f);
        WriteDebugLog("INSPECTION", "Inspection triggered. survived=" + inspectionsSurvived +
            " blocked=" + (blocked == null ? "none" : blocked.Type.ToString()) + " " + DebugStateSnapshot());
        Notify("Городская комиссия провела проверку. Часть картона забрали, один объект временно опечатан.");
        Transform target = blocked != null && blocked.Root != null ? blocked.Root.transform : DefaultObservationTarget();
        Vector3 fallback = blocked != null ? blocked.Position : DefaultObservationPosition();
        QueueObservationLead("следы комиссии", "Городская комиссия заглянула под мост. Картон поредел, один объект временно притих.",
            target, fallback, 0.10f);
        RefreshAllUi();
    }

    private void TriggerRandomEvent()
    {
        if (events.Count == 0)
        {
            return;
        }

        pendingEvent = events[random.Next(events.Count)];
        WriteDebugLog("EVENT", "Random event triggered title=" + pendingEvent.Title + " choices=" +
            (pendingEvent.Choices == null ? 0 : pendingEvent.Choices.Length));
        Notify("Новое событие: " + pendingEvent.Title + ".");
        QueueObservationLead("новый шорох", "Сверху пришёл новый шорох: \"" + pendingEvent.Title + "\".",
            EventObservationTarget(), DefaultObservationPosition(), 0.08f);
        if (!vhsModeEnabled)
        {
            SetNotebookPage(NotebookPage.Events);
            SetNotebookMode(true);
        }

        MarkNotebookDirty();
        RefreshTray();
    }

    private void ShowEventModal(BridgeEvent bridgeEvent)
    {
        if (bridgeEvent == null || bridgeEvent.Choices == null || bridgeEvent.Choices.Length == 0 ||
            eventTitleText == null || eventBodyText == null || eventChoicesRoot == null || eventModal == null)
        {
            pendingEvent = null;
            return;
        }

        pendingEvent = bridgeEvent;
        eventTitleText.text = bridgeEvent.Title;
        eventBodyText.text = bridgeEvent.Body;
        ClearChildren(eventChoicesRoot);

        for (int i = 0; i < bridgeEvent.Choices.Length; i++)
        {
            EventChoice choiceSnapshot = bridgeEvent.Choices[i];
            if (choiceSnapshot == null)
            {
                continue;
            }

            CreateDialogChoiceButton(choiceSnapshot.Label, choiceSnapshot.Preview, eventChoicesRoot, delegate
            {
                if (pendingEvent != bridgeEvent)
                {
                    Notify("Это событие уже закрыто.");
                    RefreshAllUi();
                    return;
                }

                if (choiceSnapshot.Apply != null)
                {
                    choiceSnapshot.Apply(this);
                }

                WriteDebugLog("EVENT_CHOICE", "Event choice applied title=" + bridgeEvent.Title +
                    " choice=" + choiceSnapshot.Label + " " + DebugStateSnapshot());
                QueueObservationLead("версия события", "Событие \"" + bridgeEvent.Title + "\": записана версия \"" +
                    choiceSnapshot.Label + "\". " + PlainNotebookText(choiceSnapshot.Preview),
                    EventObservationTarget(), DefaultObservationPosition(), 0.12f);
                pendingEvent = null;
                eventModal.gameObject.SetActive(false);
                suspicion = Mathf.Clamp(suspicion, 0f, MaxSuspicion);
                stock.ClampNonNegative();
                RefreshAllUi();
            });
        }

        eventModal.gameObject.SetActive(true);
    }

    private Building BlockRandomBuilding(float seconds)
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
            return null;
        }

        Building selected = candidates[random.Next(candidates.Count)];
        BlockBuilding(selected.Type, seconds);
        return selected;
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
            WriteDebugLog("VICTORY", "MVP victory condition reached. " + DebugStateSnapshot());
            if (victoryModal != null)
            {
                victoryModal.gameObject.SetActive(false);
            }

            SetNotebookPage(NotebookPage.Summary);
            SetNotebookMode(true);
            MarkNotebookDirty();
            Notify("MVP-цель выполнена. Под мостом стало исторически важно.");
        }
    }

}

