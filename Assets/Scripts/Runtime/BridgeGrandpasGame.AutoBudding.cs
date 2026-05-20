using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float AutoBuddingCheckInterval = 1.25f;
    private const float AutoBuddingSuccessDelay = 3.25f;
    private const float AutoBuddingThoughtInterval = 9f;
    private const float BuddingAnimationSeconds = 3f;
    private const float BuddingSettleSeconds = 0.35f;

    private float nextAutoBuddingAt;
    private float nextAutoBuddingThoughtAt;

    private void UpdateAutoBudding()
    {
        if (Time.time < nextAutoBuddingAt)
        {
            return;
        }

        nextAutoBuddingAt = Time.time + AutoBuddingCheckInterval;
        Grandpa candidate = FindBuddingCandidate(null);
        if (candidate == null)
        {
            return;
        }

        if (grandpas.Count >= PopulationCap())
        {
            ShowAutoBuddingWait(candidate, "Негде почковаться");
            return;
        }

        if (!CanAfford(BuddingCost()))
        {
            ShowAutoBuddingWait(candidate, "Для почки нужен чай и ворчание");
            return;
        }

        if (TryBudGrandpa(candidate, true))
        {
            nextAutoBuddingAt = Time.time + AutoBuddingSuccessDelay;
            RefreshAllUi();
        }
    }

    private Grandpa FindBuddingCandidate(Grandpa preferred)
    {
        if (preferred != null && !preferred.IsOnExpedition && preferred.Budding >= BuddingGoal)
        {
            return preferred;
        }

        for (int i = 0; i < grandpas.Count; i++)
        {
            if (!grandpas[i].IsOnExpedition && grandpas[i].Budding >= BuddingGoal)
            {
                return grandpas[i];
            }
        }

        return null;
    }

    private Grandpa FindClosestBuddingGrandpa()
    {
        Grandpa closest = null;
        for (int i = 0; i < grandpas.Count; i++)
        {
            if (grandpas[i].IsOnExpedition)
            {
                continue;
            }

            if (closest == null || grandpas[i].Budding > closest.Budding)
            {
                closest = grandpas[i];
            }
        }

        return closest;
    }

    private bool TryBudGrandpa(Grandpa candidate, bool automatic)
    {
        if (candidate == null)
        {
            return false;
        }

        if (grandpas.Count >= PopulationCap())
        {
            if (!automatic)
            {
                Notify("Нужна Картонная спальня или её улучшение: лимит дедушек достигнут.");
            }

            return false;
        }

        if (!Spend(BuddingCost()))
        {
            if (!automatic)
            {
                Notify("Для почкования нужны чай, тепло и ворчание.");
            }

            return false;
        }

        candidate.Budding = 0f;
        GrandpaRole role = RollMutationRole();
        Grandpa child = SpawnGrandpa(role, Jitter(candidate.Root.transform.position, 0.75f));
        child.Budding = UnityEngine.Random.Range(0f, 18f);
        child.BirthAnimStart = Time.time;
        child.BirthAnimUntil = Time.time + BuddingAnimationSeconds;
        child.BudBurstUntil = Time.time + BuddingAnimationSeconds + BuddingSettleSeconds;
        candidate.BudBurstUntil = Time.time + BuddingAnimationSeconds;
        CreateBuddingBurst(candidate.Root.transform.position);
        suspicion += 7f;
        ShowThought(candidate, automatic ? "Сам почкуюсь!" : "Почкуется!", 3f);
        ShowThought(child, "Я новый дед", 3.5f);
        if (!automatic)
        {
            SelectGrandpa(child);
        }

        string cozy = GainCozy(0.8f);
        string prefix = automatic ? "Автопочкование: " : "";
        Notify(prefix + candidate.Name + " почковался. Появился " + child.Name + " (" + RoleName(role) + "). Уют +0.8." + cozy);
        QueueObservationLead("новый дед", "Зафиксировано почкование: от " + candidate.Name + " появился " +
            child.Name + " (" + RoleName(role) + ").", child.Root != null ? child.Root.transform : null, child.Target, 0.20f);
        WriteDebugLog("BUDDING", "Success automatic=" + automatic + " parent=" + DebugGrandpaSnapshot(candidate) +
            " child=" + DebugGrandpaSnapshot(child) + " " + DebugStateSnapshot());
        return true;
    }

    private void ShowAutoBuddingWait(Grandpa candidate, string thought)
    {
        if (Time.time < nextAutoBuddingThoughtAt)
        {
            return;
        }

        nextAutoBuddingThoughtAt = Time.time + AutoBuddingThoughtInterval;
        ShowThought(candidate, thought, 2.7f);
    }
}
