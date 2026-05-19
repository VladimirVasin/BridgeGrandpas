using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private bool UpdateGrandpaExpedition(Grandpa grandpa)
    {
        if (!grandpa.IsOnExpedition)
        {
            return false;
        }

        if (grandpa.ExpeditionLeaving)
        {
            UpdateExpeditionDeparture(grandpa);
            return true;
        }

        if (!grandpa.ExpeditionNarrativeResolved)
        {
            ShowExpeditionNarrativeModal(grandpa);
        }

        if (grandpa.Root != null && grandpa.Root.activeSelf)
        {
            grandpa.Root.SetActive(false);
        }

        if (Time.time < grandpa.ExpeditionUntil)
        {
            return true;
        }

        CompleteGrandpaExpedition(grandpa);
        return false;
    }

    private void BuildExpeditionsTray()
    {
        trayTitleText.text = "Вылазки наверх";
        Grandpa chosen = selectedGrandpa != null && !selectedGrandpa.IsOnExpedition
            ? selectedGrandpa
            : FirstAvailableGrandpa();

        if (chosen == null)
        {
            AddTrayNote("Все дедушки сейчас наверху или коммуна пуста. Ждём шороха с лестницы.");
            AddExpeditionReturnNotes();
            return;
        }

        AddTrayNote("Выбран: " + chosen.Name + " (" + RoleName(chosen.Role) + ")");
        CreateExpeditionButton(chosen, ExpeditionType.CardboardRun);
        CreateExpeditionButton(chosen, ExpeditionType.CoinAdvice);
        CreateExpeditionButton(chosen, ExpeditionType.TeaSalvage);
        CreateExpeditionButton(chosen, ExpeditionType.CityRumors);
        AddTrayNote("Другие свободные дедушки:");

        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            if (grandpa == chosen || grandpa.IsOnExpedition)
            {
                continue;
            }

            CreateButton("Выбрать: " + grandpa.Name + " | " + RoleName(grandpa.Role), trayBody, delegate
            {
                SelectGrandpa(grandpa);
                RefreshAllUi();
            });
        }

        AddExpeditionReturnNotes();
    }

    private void CreateExpeditionButton(Grandpa grandpa, ExpeditionType type)
    {
        ResourceStock cost = ExpeditionCost(type);
        float duration = ExpeditionDuration(grandpa, type);
        string label = ExpeditionName(type) + "\n" + ExpeditionHint(type) +
            " | " + Mathf.CeilToInt(duration) + "с | " + cost.ShortText();
        RectTransform button = CreateButton(label, trayBody, delegate
        {
            TryStartGrandpaExpedition(grandpa, type);
        });
        button.GetComponent<Button>().interactable = CanAfford(cost);
    }

    private void TryStartGrandpaExpedition(Grandpa grandpa, ExpeditionType type)
    {
        if (grandpa == null || grandpa.IsOnExpedition)
        {
            Notify("Этот дедушка уже где-то между лестницей и судьбой.");
            return;
        }

        ResourceStock cost = ExpeditionCost(type);
        if (!Spend(cost))
        {
            Notify("На вылазку не хватает припасов.");
            RefreshAllUi();
            return;
        }

        grandpa.IsOnExpedition = true;
        grandpa.ExpeditionLeaving = true;
        grandpa.ExpeditionNarrativeResolved = false;
        grandpa.ExpeditionType = type;
        grandpa.ExpeditionDepartureUntil = Time.time + 3.6f;
        grandpa.ExpeditionUntil = grandpa.ExpeditionDepartureUntil + ExpeditionDuration(grandpa, type);
        grandpa.ExpeditionRewardMultiplier = 1f;
        grandpa.ExpeditionRiskMultiplier = 1f;
        grandpa.ExpeditionExitPosition = ExpeditionExitPosition(type);
        grandpa.ExpeditionReturnPosition = new Vector3(UnityEngine.Random.Range(-4.6f, 4.6f), 0f, -2.75f);
        grandpa.Target = grandpa.ExpeditionExitPosition;
        grandpa.NextMoveAt = grandpa.ExpeditionUntil;
        grandpa.HasInteraction = false;
        ShowThought(grandpa, "Пойду наверх", 2.4f);

        suspicion += 0.8f;
        Notify(grandpa.Name + " собирается наверх: " + ExpeditionName(type) + ".");
        QueueObservationLead("уход наверх", grandpa.Name + " ушёл наверх: " + ExpeditionName(type) +
            ". В блокноте оставлено место под возвращение.",
            grandpa.Root != null ? grandpa.Root.transform : null, grandpa.Target, 0.12f);
        RefreshAllUi();
    }

    private void CompleteGrandpaExpedition(Grandpa grandpa)
    {
        grandpa.IsOnExpedition = false;
        grandpa.ExpeditionLeaving = false;
        grandpa.ExpeditionNarrativeResolved = true;
        if (grandpa.Root != null)
        {
            grandpa.Root.SetActive(true);
            grandpa.Root.transform.position = grandpa.ExpeditionReturnPosition;
            grandpa.Target = Jitter(grandpa.ExpeditionReturnPosition, 0.5f);
            grandpa.NextMoveAt = Time.time + UnityEngine.Random.Range(1.2f, 2.4f);
        }

        ResourceStock reward = ExpeditionReward(grandpa, grandpa.ExpeditionType);
        reward.Tea *= grandpa.ExpeditionRewardMultiplier;
        reward.Heat *= grandpa.ExpeditionRewardMultiplier;
        reward.Cardboard *= grandpa.ExpeditionRewardMultiplier;
        reward.Grumble *= grandpa.ExpeditionRewardMultiplier;
        reward.Coins *= grandpa.ExpeditionRewardMultiplier;
        stock.Tea += reward.Tea;
        stock.Heat += reward.Heat;
        stock.Cardboard += reward.Cardboard;
        stock.Grumble += reward.Grumble;
        stock.Coins += reward.Coins;
        float risk = ExpeditionSuspicion(grandpa, grandpa.ExpeditionType) * grandpa.ExpeditionRiskMultiplier;
        suspicion = Mathf.Clamp(suspicion + risk, 0f, MaxSuspicion);

        if (grandpa.ExpeditionType == ExpeditionType.CityRumors)
        {
            nextEventIn = Mathf.Min(nextEventIn, UnityEngine.Random.Range(8f, 16f));
        }

        grandpa.ExpeditionRewardMultiplier = 1f;
        grandpa.ExpeditionRiskMultiplier = 1f;
        ShowThought(grandpa, "Я видел верх", 3.2f);
        Notify(grandpa.Name + " вернулся: +" + reward.ShortText() + ", подозрение +" + RateF(risk) + ".");
        QueueObservationLead("возвращение", grandpa.Name + " вернулся сверху: +" + reward.ShortText() +
            ", город насторожился на " + RateF(risk) + ".",
            grandpa.Root != null ? grandpa.Root.transform : null, grandpa.ExpeditionReturnPosition, 0.10f);
        RefreshAllUi();
    }

    private void UpdateExpeditionDeparture(Grandpa grandpa)
    {
        if (grandpa.Root == null)
        {
            grandpa.ExpeditionLeaving = false;
            return;
        }

        if (!grandpa.Root.activeSelf)
        {
            grandpa.Root.SetActive(true);
        }

        Vector3 current = grandpa.Root.transform.position;
        Vector3 next = MoveGrandpaWithAvoidance(grandpa, current, grandpa.ExpeditionExitPosition, 0.82f, Time.deltaTime);
        grandpa.Root.transform.position = next;
        Vector3 move = grandpa.ExpeditionExitPosition - current;
        if (move.sqrMagnitude > 0.002f)
        {
            grandpa.Root.transform.rotation = Quaternion.Slerp(grandpa.Root.transform.rotation,
                Quaternion.LookRotation(new Vector3(move.x, 0f, move.z)), Time.deltaTime * 7f);
        }

        grandpa.Target = grandpa.ExpeditionExitPosition;
        UpdateGrandpaInteractionPose(grandpa);
        bool arrived = Vector3.Distance(Flat(next), Flat(grandpa.ExpeditionExitPosition)) < 0.14f;
        if (Time.time < grandpa.ExpeditionDepartureUntil && !arrived)
        {
            return;
        }

        grandpa.ExpeditionLeaving = false;
        grandpa.Root.SetActive(false);
        ShowExpeditionNarrativeModal(grandpa);
    }

    private Vector3 ExpeditionExitPosition(ExpeditionType type)
    {
        switch (type)
        {
            case ExpeditionType.CityRumors:
                return new Vector3(4.9f, 0f, 1.55f);
            case ExpeditionType.TeaSalvage:
                return new Vector3(-4.8f, 0f, 1.35f);
            case ExpeditionType.CoinAdvice:
                return new Vector3(4.7f, 0f, -2.65f);
            default:
                return new Vector3(-4.9f, 0f, -2.75f);
        }
    }

    private string BuildExpeditionMicroHudText()
    {
        return "<b>Вылазки наверх</b>\n\n" +
            "Свободные дедушки: " + AvailableGrandpaCount() + "/" + grandpas.Count + "\n" +
            "Отправляй дедушек за ресурсами, но город от этого становится внимательнее.\n\n" +
            "Картонщики лучше носят коробки, радиодеды лучше ловят слухи, сторожа шумят тише.";
    }

    private void AddExpeditionReturnNotes()
    {
        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            if (grandpa.IsOnExpedition)
            {
                AddTrayNote(grandpa.Name + " вернётся через " + Mathf.CeilToInt(grandpa.ExpeditionUntil - Time.time) + "с.");
            }
        }
    }

    private Grandpa FirstAvailableGrandpa()
    {
        for (int i = 0; i < grandpas.Count; i++)
        {
            if (!grandpas[i].IsOnExpedition)
            {
                return grandpas[i];
            }
        }

        return null;
    }

    private int AvailableGrandpaCount()
    {
        int count = 0;
        for (int i = 0; i < grandpas.Count; i++)
        {
            if (!grandpas[i].IsOnExpedition)
            {
                count++;
            }
        }

        return count;
    }

    private ResourceStock ExpeditionCost(ExpeditionType type)
    {
        switch (type)
        {
            case ExpeditionType.CoinAdvice:
                return new ResourceStock(3f, 0f, 0f, 4f, 0f);
            case ExpeditionType.TeaSalvage:
                return new ResourceStock(0f, 4f, 0f, 1f, 0f);
            case ExpeditionType.CityRumors:
                return new ResourceStock(2f, 0f, 0f, 2f, 0f);
            default:
                return new ResourceStock(2f, 0f, 0f, 1f, 0f);
        }
    }

    private float ExpeditionDuration(Grandpa grandpa, ExpeditionType type)
    {
        float duration = type == ExpeditionType.CoinAdvice ? 34f : 27f;
        if (type == ExpeditionType.CityRumors)
        {
            duration = 24f;
        }

        if (grandpa.Role == GrandpaRole.RadioReceiver && type == ExpeditionType.CityRumors)
        {
            duration *= 0.72f;
        }
        else if (grandpa.Role == GrandpaRole.Cardboarder && type == ExpeditionType.CardboardRun)
        {
            duration *= 0.78f;
        }
        else if (grandpa.Role == GrandpaRole.Guard)
        {
            duration *= 0.92f;
        }

        return duration;
    }

    private ResourceStock ExpeditionReward(Grandpa grandpa, ExpeditionType type)
    {
        switch (type)
        {
            case ExpeditionType.CoinAdvice:
                float coins = UnityEngine.Random.Range(1f, 3.2f) + (grandpa.Role == GrandpaRole.Philosopher ? 1.3f : 0f);
                return new ResourceStock(0f, 0f, 0f, UnityEngine.Random.Range(1f, 4f), coins);
            case ExpeditionType.TeaSalvage:
                float tea = UnityEngine.Random.Range(10f, 19f) + (grandpa.Role == GrandpaRole.SamovarKeeper ? 7f : 0f);
                return new ResourceStock(tea, 0f, 0f, 0f, 0f);
            case ExpeditionType.CityRumors:
                float grumble = UnityEngine.Random.Range(8f, 15f) + (grandpa.Role == GrandpaRole.RadioReceiver ? 8f : 0f);
                return new ResourceStock(0f, 0f, 0f, grumble, UnityEngine.Random.Range(0f, 1.4f));
            default:
                float cardboard = UnityEngine.Random.Range(12f, 22f) + (grandpa.Role == GrandpaRole.Cardboarder ? 10f : 0f);
                return new ResourceStock(0f, 0f, cardboard, UnityEngine.Random.Range(0f, 3f), 0f);
        }
    }

    private float ExpeditionSuspicion(Grandpa grandpa, ExpeditionType type)
    {
        float risk;
        switch (type)
        {
            case ExpeditionType.CoinAdvice:
                risk = UnityEngine.Random.Range(3f, 7f);
                break;
            case ExpeditionType.CityRumors:
                risk = UnityEngine.Random.Range(1.5f, 4f);
                break;
            case ExpeditionType.TeaSalvage:
                risk = UnityEngine.Random.Range(2f, 5f);
                break;
            default:
                risk = UnityEngine.Random.Range(2.5f, 6f);
                break;
        }

        if (grandpa.Role == GrandpaRole.Guard)
        {
            risk *= 0.45f;
        }

        return risk;
    }

    private string ExpeditionName(ExpeditionType type)
    {
        switch (type)
        {
            case ExpeditionType.CoinAdvice:
                return "Дать советы прохожим";
            case ExpeditionType.TeaSalvage:
                return "Найти чайные остатки";
            case ExpeditionType.CityRumors:
                return "Послушать город";
            default:
                return "Стащить картон";
        }
    }

    private string ExpeditionHint(ExpeditionType type)
    {
        switch (type)
        {
            case ExpeditionType.CoinAdvice:
                return "+монетки, +ворчание, риск заметности";
            case ExpeditionType.TeaSalvage:
                return "+чай, тратит тепло";
            case ExpeditionType.CityRumors:
                return "+ворчание, ускоряет событие";
            default:
                return "+картон, небольшой шум";
        }
    }
}
