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
    private static readonly bool EventAutoTriggerEnabled = false;

    private void SimulateResources(float deltaTime)
    {
        ResourceStock income = CurrentResourceIncomePerSecond();
        stock.Tea += deltaTime * income.Tea;
        stock.Heat += deltaTime * income.Heat;
        stock.Cardboard += deltaTime * income.Cardboard;
        stock.Grumble += deltaTime * income.Grumble;
        stock.Coins += deltaTime * income.Coins;
        stock.ClampNonNegative();
    }

    private void SimulateSuspicion(float deltaTime)
    {
        float growth = 0.012f;
        growth += grandpas.Count * 0.0048f;
        growth += BuiltCount() * 0.0065f;

        Building samovar;
        if (buildings.TryGetValue(BuildingType.Samovar, out samovar) && samovar.Built && !samovar.IsBlocked)
        {
            growth += 0.010f + samovar.Level * 0.002f;
        }

        Building fire;
        if (buildings.TryGetValue(BuildingType.FireBarrel, out fire) && fire.Built && !fire.IsBlocked)
        {
            growth += 0.006f;
        }

        growth -= CountRole(GrandpaRole.Guard) * 0.014f;

        Building curtain;
        if (buildings.TryGetValue(BuildingType.CarpetCurtain, out curtain) && curtain.Built && !curtain.IsBlocked)
        {
            growth -= 0.030f + curtain.Level * 0.015f;
        }

        suspicion += Mathf.Max(0.003f, growth) * deltaTime;
        suspicion = Mathf.Clamp(suspicion, 0f, MaxSuspicion);

        if (suspicion >= MaxSuspicion)
        {
            TriggerInspection();
        }
    }

    private void SimulateEvents(float deltaTime)
    {
        if (!EventAutoTriggerEnabled)
        {
            return;
        }

        if (pendingEvent != null)
        {
            return;
        }

        Building radio;
        bool radioBuilt = buildings.TryGetValue(BuildingType.RadioMayak, out radio) && radio.Built && !radio.IsBlocked;
        nextEventIn -= deltaTime;
        if (radioBuilt)
        {
            nextRadioWhisperIn -= deltaTime;
            if (nextRadioWhisperIn <= 0f)
            {
                nextRadioWhisperIn = UnityEngine.Random.Range(38f, 62f);
                stock.Grumble += 4f;
                Notify("Радио шепчет: город сегодня особенно любит смотреть вниз. +4 ворчания.");
            }
        }

        if (nextEventIn <= 0f)
        {
            TriggerRandomEvent();
            nextEventIn = radioBuilt ? UnityEngine.Random.Range(42f, 68f) : UnityEngine.Random.Range(72f, 105f);
        }
    }

    private void SimulateGrandpas(float deltaTime)
    {
        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            if (UpdateGrandpaExpedition(grandpa))
            {
                continue;
            }

            UpdateGrandpaBudding(grandpa, deltaTime);
            UpdateGrandpaMovement(grandpa, deltaTime);
            UpdateGrandpaThought(grandpa);
        }

        UpdateAutoBudding();
    }

    private void UpdateGrandpaBudding(Grandpa grandpa, float deltaTime)
    {
        float resourceSupport = 0.35f;
        resourceSupport += Mathf.Clamp01(stock.Tea / 30f) * 0.25f;
        resourceSupport += Mathf.Clamp01(stock.Heat / 30f) * 0.25f;
        resourceSupport += Mathf.Clamp01(stock.Grumble / 30f) * 0.25f;

        float roleBonus = grandpa.Role == GrandpaRole.Philosopher ? 0.12f : 0f;
        grandpa.Budding = Mathf.Min(BuddingGoal, grandpa.Budding + deltaTime * (0.72f + resourceSupport + roleBonus));
    }

    private void UpdateGrandpaMovement(Grandpa grandpa, float deltaTime)
    {
        float distanceToTarget = Vector3.Distance(Flat(grandpa.Root.transform.position), Flat(grandpa.Target));
        bool arrived = distanceToTarget < 0.08f;
        if (arrived && Time.time < grandpa.IdleActionUntil)
        {
            UpdateGrandpaInteractionPose(grandpa);
            return;
        }

        if (Time.time >= grandpa.NextMoveAt || arrived)
        {
            grandpa.Target = ChooseGrandpaTarget(grandpa);
            grandpa.NextMoveAt = Mathf.Max(grandpa.IdleActionUntil, Time.time + UnityEngine.Random.Range(2.8f, 6.4f));
        }

        Vector3 current = grandpa.Root.transform.position;
        float speed = 0.55f + grandpa.WalkJitter;
        Vector3 next = MoveGrandpaWithAvoidance(grandpa, current, grandpa.Target, speed, deltaTime);
        float bob = Mathf.Sin((Time.time + grandpa.BobSeed) * 5.5f) * 0.025f;
        grandpa.Root.transform.position = new Vector3(next.x, bob, next.z);

        Vector3 move = grandpa.Target - current;
        if (move.sqrMagnitude > 0.002f)
        {
            Quaternion look = Quaternion.LookRotation(new Vector3(move.x, 0f, move.z));
            grandpa.Root.transform.rotation = Quaternion.Slerp(grandpa.Root.transform.rotation, look, deltaTime * 5f);
        }

        UpdateGrandpaInteractionPose(grandpa);
    }

    private Vector3 ChooseGrandpaTarget(Grandpa grandpa)
    {
        return ChooseGrandpaIdleTarget(grandpa);
    }

    private void UpdateGrandpaThought(Grandpa grandpa)
    {
        if (Time.time < grandpa.ThoughtUntil)
        {
            return;
        }

        if (UnityEngine.Random.value > 0.004f)
        {
            return;
        }

        string thought = RandomThought(grandpa);
        ShowThought(grandpa, thought, UnityEngine.Random.Range(2.4f, 4.2f));
    }

}

