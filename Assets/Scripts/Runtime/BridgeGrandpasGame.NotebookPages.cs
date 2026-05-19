using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private void BuildNotebookSummaryRightPage()
    {
        AddNotebookText("<b>Признаки устойчивой коммуны</b>\n" +
            "20 дедушек замечены одновременно\n5 объектов прижились под мостом\n3 городские проверки пережиты\n1 редкая мутация записана\n\n" +
            "<b>Служебная пометка</b>\n" +
            "N закрывает блокнот\nF включает VHS-наблюдение\nКлик вне страниц прерывает записи", 16, FontStyle.Normal, 210f);

        CreateNotebookButton("Перечитать журнал наблюдений", notebookPageContent, delegate { SetNotebookPage(NotebookPage.Observations); });
        CreateNotebookButton("Проверить следы строительства", notebookPageContent, delegate { SetNotebookPage(NotebookPage.Build); });
        CreateNotebookButton("Сверить замеченных дедушек", notebookPageContent, delegate { SetNotebookPage(NotebookPage.Grandpas); });
        CreateNotebookButton("Открыть записи о вылазках", notebookPageContent, delegate { SetNotebookPage(NotebookPage.Expeditions); });
        CreateNotebookButton("Сохранить текущие записи", notebookPageContent, SaveGameFromMenu);
        RectTransform load = CreateNotebookButton("Загрузить старые записи", notebookPageContent, LoadGameFromMenu);
        load.GetComponent<Button>().interactable = HasSavedGame();
    }

    private void BuildNotebookBuildPage()
    {
        if (selectedBuilding != null)
        {
            ResourceStock cost = selectedBuilding.Built ? UpgradeCost(selectedBuilding) : selectedBuilding.BuildCost;
            AddNotebookText("<b>Отмеченный объект: " + selectedBuilding.Name + "</b> ур. " + selectedBuilding.Level + "\n" +
                selectedBuilding.Description + "\n" + BuildingEffectText(selectedBuilding) + "\n" +
                NotebookCostLine(cost, selectedBuilding.Built ? "можно записать улучшение" : "можно подтвердить постройку"), 15, FontStyle.Normal, 96f);
        }

        foreach (Building item in buildings.Values)
        {
            Building building = item;
            if (building.Type == BuildingType.FireBarrel)
            {
                continue;
            }

            if (!building.Built)
            {
                bool canBuild = CanAfford(building.BuildCost);
                RectTransform button = CreateNotebookButton(NotebookBuildPhrase(building.Type) + "\n" +
                    NotebookCostLine(building.BuildCost, "можно зафиксировать"), notebookPageContent, delegate
                {
                    TryBuild(building.Type, false);
                    SelectBuilding(building);
                    MarkNotebookDirty();
                    RefreshAllUi();
                });
                button.GetComponent<Button>().interactable = canBuild;
                continue;
            }

            ResourceStock cost = UpgradeCost(building);
            RectTransform upgrade = CreateNotebookButton(NotebookUpgradePhrase(building) +
                "\n" + NotebookCostLine(cost, "можно зафиксировать"), notebookPageContent, delegate
            {
                TryUpgrade(building);
                SelectBuilding(building);
                MarkNotebookDirty();
                RefreshAllUi();
            });
            upgrade.GetComponent<Button>().interactable = CanAfford(cost);
        }
    }

    private void BuildNotebookGrandpasPage()
    {
        if (selectedGrandpa != null)
        {
            string expedition = selectedGrandpa.IsOnExpedition
                ? "в вылазке, вернётся через " + Mathf.CeilToInt(selectedGrandpa.ExpeditionUntil - Time.time) + "с"
                : "готовность к почкованию " + Mathf.FloorToInt(selectedGrandpa.Budding) + "%";
            AddNotebookText("<b>Подробное наблюдение: " + selectedGrandpa.Name + "</b>\n" +
                RoleName(selectedGrandpa.Role) + ". " + NotebookRoleObservation(selectedGrandpa.Role) + "\n" +
                "Текущее состояние: " + expedition + ". Настроение: " + GrandpaMood(selectedGrandpa),
                15, FontStyle.Normal, 88f);
        }

        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            string state = grandpa.IsOnExpedition
                ? "в вылазке, " + Mathf.CeilToInt(grandpa.ExpeditionUntil - Time.time) + "с"
                : "почкование " + Mathf.FloorToInt(grandpa.Budding) + "%";
            CreateNotebookButton(NotebookGrandpaObservation(grandpa) + "\n" +
                state + " | " + GrandpaMood(grandpa), notebookPageContent, delegate
            {
                SelectGrandpa(grandpa);
                MarkNotebookDirty();
                RefreshAllUi();
            });
        }
    }

    private void BuildNotebookEventsPage()
    {
        if (pendingEvent == null)
        {
            AddNotebookText("Событий нет. Следующий шорох примерно через " + Mathf.CeilToInt(nextEventIn) + "с.", 16, FontStyle.Italic, 48f);
            Building radio;
            bool radioBuilt = buildings.TryGetValue(BuildingType.RadioMayak, out radio) && radio.Built;
            if (radioBuilt)
            {
                RectTransform spin = CreateNotebookButton("В записи: радио щёлкнуло в поисках слуха\n-1 монетка, событие ближе", notebookPageContent, delegate
                {
                    TrySpinRadio();
                    Building radioLead;
                    Transform radioTarget = buildings.TryGetValue(BuildingType.RadioMayak, out radioLead) && radioLead.Root != null
                        ? radioLead.Root.transform
                        : null;
                    QueueObservationLead("радио", "Радио щёлкнуло. Наблюдатель решил, что это почти новость.",
                        radioTarget, radioLead != null ? radioLead.Position : DefaultObservationPosition(), 0.18f);
                    MarkNotebookDirty();
                });
                spin.GetComponent<Button>().interactable = stock.Coins >= 1f;
            }

            return;
        }

        BridgeEvent eventSnapshot = pendingEvent;
        if (eventSnapshot == null || eventSnapshot.Choices == null || eventSnapshot.Choices.Length == 0)
        {
            AddNotebookText("Событие уже рассеялось. Наблюдатель оставил пустую строку.", 16, FontStyle.Italic, 48f);
            return;
        }

        for (int i = 0; i < eventSnapshot.Choices.Length; i++)
        {
            EventChoice choiceSnapshot = eventSnapshot.Choices[i];
            if (choiceSnapshot == null)
            {
                continue;
            }

            CreateNotebookButton("Записать версию: " + choiceSnapshot.Label + "\n" + choiceSnapshot.Preview, notebookPageContent, delegate
            {
                if (pendingEvent != eventSnapshot)
                {
                    Notify("Эта запись уже неактуальна.");
                    MarkNotebookDirty();
                    RefreshAllUi();
                    return;
                }

                if (choiceSnapshot.Apply != null)
                {
                    choiceSnapshot.Apply(this);
                }

                QueueObservationLead("версия события", "Событие \"" + eventSnapshot.Title + "\": записана версия \"" +
                    choiceSnapshot.Label + "\". " + PlainNotebookText(choiceSnapshot.Preview), null, DefaultObservationPosition(), 0.12f);
                pendingEvent = null;
                if (eventModal != null)
                {
                    eventModal.gameObject.SetActive(false);
                }

                suspicion = Mathf.Clamp(suspicion, 0f, MaxSuspicion);
                stock.ClampNonNegative();
                MarkNotebookDirty();
                RefreshAllUi();
            });
        }
    }

    private void BuildNotebookExpeditionsPage()
    {
        if (!ExpeditionsEnabled)
        {
            AddNotebookText("Вылазки временно не инициируются. Наблюдатель оставил страницу под будущий триггер.", 16, FontStyle.Italic, 62f);
            AddNotebookExpeditionReturnNotes();
            return;
        }

        Grandpa narrativeGrandpa = PendingExpeditionNarrativeGrandpa();
        if (narrativeGrandpa != null)
        {
            BuildNotebookExpeditionNarrative(narrativeGrandpa);
            return;
        }

        Grandpa chosen = selectedGrandpa != null && !selectedGrandpa.IsOnExpedition ? selectedGrandpa : FirstAvailableGrandpa();
        if (chosen == null)
        {
            AddNotebookText("Свободных дедушек нет. Наверху сейчас слишком много судьбы.", 16, FontStyle.Italic, 50f);
            AddNotebookExpeditionReturnNotes();
            return;
        }

        AddNotebookText("Для верхнего мира выбран: " + chosen.Name + " (" + RoleName(chosen.Role) + ")", 16, FontStyle.Bold, 38f);
        CreateNotebookExpeditionButton(chosen, ExpeditionType.CardboardRun);
        CreateNotebookExpeditionButton(chosen, ExpeditionType.CoinAdvice);
        CreateNotebookExpeditionButton(chosen, ExpeditionType.TeaSalvage);
        CreateNotebookExpeditionButton(chosen, ExpeditionType.CityRumors);
        AddNotebookExpeditionReturnNotes();
    }

    private void CreateNotebookExpeditionButton(Grandpa grandpa, ExpeditionType type)
    {
        ResourceStock cost = ExpeditionCost(type);
        string label = NotebookExpeditionPlan(grandpa, type) + "\n" + ExpeditionHint(type) +
            " | " + Mathf.CeilToInt(ExpeditionDuration(grandpa, type)) + "с | " + cost.ShortText();
        RectTransform button = CreateNotebookButton(label, notebookPageContent, delegate
        {
            TryStartGrandpaExpedition(grandpa, type);
            MarkNotebookDirty();
        });
        button.GetComponent<Button>().interactable = CanAfford(cost);
    }

    private void BuildNotebookExpeditionNarrative(Grandpa grandpa)
    {
        string intro = "<b>" + grandpa.Name + " наверху</b>\n" +
            "Мокрый свет, чужие ботинки и шанс вернуться не с пустыми руками. " +
            "Наблюдатель записывает решение до броска кубика.";
        AddNotebookText(intro, 16, FontStyle.Normal, 86f);

        if (expeditionDiceRolling || expeditionDiceResultVisible)
        {
            AddNotebookText("Кубик уже катится по асфальту. Ждём, какую цифру оставит город.", 16, FontStyle.Italic, 54f);
            return;
        }

        CreateNotebookExpeditionChoice(grandpa, "Тише под перилами",
            "<color=#226b23>меньше подозрения</color> | <color=#8a5a18>добычи меньше</color> | бросок кубика",
            0.82f, 0.45f, "пошёл тихо, почти как тень с авоськой");
        CreateNotebookExpeditionChoice(grandpa, "Собрать всё блестящее",
            "<color=#226b23>добычи больше</color> | <color=#9a2f1e>подозрение выше</color> | бросок кубика",
            1.35f, 1.45f, "решил, что осторожность сегодня не главный ресурс");
        CreateNotebookExpeditionChoice(grandpa, "Действовать по-дедовски",
            "<color=#226b23>сбалансированная добыча</color> | <color=#8a5a18>обычный риск</color> | бросок кубика",
            1.08f, 0.92f, "применил старую тактику: выглядеть так, будто всё так и было");
    }

    private void CreateNotebookExpeditionChoice(Grandpa grandpa, string label, string preview, float reward, float risk, string result)
    {
        CreateNotebookButton(label + "\n" + preview, notebookPageContent, delegate
        {
            StartExpeditionDiceRoll(grandpa, reward, risk, result);
            MarkNotebookDirty();
        });
    }

    private Grandpa PendingExpeditionNarrativeGrandpa()
    {
        if (!ExpeditionNarrativeAutoTriggerEnabled)
        {
            return null;
        }

        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            if (grandpa.IsOnExpedition && !grandpa.ExpeditionLeaving && !grandpa.ExpeditionNarrativeResolved)
            {
                return grandpa;
            }
        }

        return null;
    }

    private void AddNotebookExpeditionReturnNotes()
    {
        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            if (grandpa.IsOnExpedition)
            {
                AddNotebookText(grandpa.Name + " вернётся через " + Mathf.CeilToInt(grandpa.ExpeditionUntil - Time.time) + "с.", 15, FontStyle.Italic, 32f);
            }
        }
    }

    private string NotebookCostLine(ResourceStock cost, string good)
    {
        return CanAfford(cost)
            ? "<color=#226b23>" + good + ": " + cost.ShortText() + "</color>"
            : "<color=#9a2f1e>не хватает: " + MissingResourceText(cost) + "</color>";
    }
}
