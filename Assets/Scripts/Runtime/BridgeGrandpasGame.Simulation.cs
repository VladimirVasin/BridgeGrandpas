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
    private void SimulateResources(float deltaTime)
    {
        float common = CountRole(GrandpaRole.Common);
        float samovar = CountRole(GrandpaRole.SamovarKeeper);
        float cardboarder = CountRole(GrandpaRole.Cardboarder);
        float mutterer = CountRole(GrandpaRole.Mutterer);
        float philosopher = CountRole(GrandpaRole.Philosopher);

        Building fire = buildings[BuildingType.FireBarrel];
        if (fire.Built && !fire.IsBlocked)
        {
            stock.Heat += deltaTime * (0.23f + fire.Level * 0.11f);
        }

        Building samovarBuilding = buildings[BuildingType.Samovar];
        if (samovarBuilding.Built && !samovarBuilding.IsBlocked)
        {
            stock.Tea += deltaTime * (0.16f + samovarBuilding.Level * 0.08f + samovar * 0.11f);
        }

        Building bench = buildings[BuildingType.GrumbleBench];
        if (bench.Built && !bench.IsBlocked)
        {
            stock.Grumble += deltaTime * (0.11f + bench.Level * 0.06f + mutterer * 0.14f + philosopher * 0.05f);
        }

        stock.Grumble += deltaTime * (common * 0.035f + mutterer * 0.05f);
        stock.Cardboard += deltaTime * (common * 0.025f + cardboarder * 0.12f);

        if (buildings[BuildingType.RadioMayak].Built && !buildings[BuildingType.RadioMayak].IsBlocked)
        {
            stock.Coins += deltaTime * CountRole(GrandpaRole.RadioReceiver) * 0.012f;
        }

        stock.ClampNonNegative();
    }

    private void SimulateSuspicion(float deltaTime)
    {
        float growth = 0.012f;
        growth += grandpas.Count * 0.0048f;
        growth += BuiltCount() * 0.0065f;

        if (buildings[BuildingType.Samovar].Built && !buildings[BuildingType.Samovar].IsBlocked)
        {
            growth += 0.010f + buildings[BuildingType.Samovar].Level * 0.002f;
        }

        if (buildings[BuildingType.FireBarrel].Built && !buildings[BuildingType.FireBarrel].IsBlocked)
        {
            growth += 0.006f;
        }

        growth -= CountRole(GrandpaRole.Guard) * 0.014f;

        Building curtain = buildings[BuildingType.CarpetCurtain];
        if (curtain.Built && !curtain.IsBlocked)
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
        if (pendingEvent != null)
        {
            return;
        }

        bool radioBuilt = buildings[BuildingType.RadioMayak].Built && !buildings[BuildingType.RadioMayak].IsBlocked;
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
            UpdateGrandpaBudding(grandpa, deltaTime);
            UpdateGrandpaMovement(grandpa, deltaTime);
            UpdateGrandpaThought(grandpa);
        }
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
        if (Time.time >= grandpa.NextMoveAt || Vector3.Distance(grandpa.Root.transform.position, grandpa.Target) < 0.06f)
        {
            grandpa.Target = ChooseGrandpaTarget(grandpa);
            grandpa.NextMoveAt = Time.time + UnityEngine.Random.Range(2.8f, 6.4f);
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
        if (grandpa.Role == GrandpaRole.SamovarKeeper && buildings[BuildingType.Samovar].Built)
        {
            return TargetNearBuilding(grandpa, BuildingType.Samovar, 1.1f);
        }

        if (grandpa.Role == GrandpaRole.Mutterer && buildings[BuildingType.GrumbleBench].Built)
        {
            return TargetNearBuilding(grandpa, BuildingType.GrumbleBench, 1.1f);
        }

        if (grandpa.Role == GrandpaRole.Guard && buildings[BuildingType.CarpetCurtain].Built)
        {
            return TargetNearBuilding(grandpa, BuildingType.CarpetCurtain, 1.15f);
        }

        if (grandpa.Role == GrandpaRole.Cardboarder)
        {
            return new Vector3(UnityEngine.Random.Range(-4.9f, 4.8f), 0f, UnityEngine.Random.Range(-3.1f, -2.45f));
        }

        if (grandpa.Role == GrandpaRole.RadioReceiver && buildings[BuildingType.RadioMayak].Built)
        {
            return TargetNearBuilding(grandpa, BuildingType.RadioMayak, 1.0f);
        }

        if (UnityEngine.Random.value < 0.38f)
        {
            return TargetNearBuilding(grandpa, BuildingType.FireBarrel, 1.2f);
        }

        if (buildings[BuildingType.Bedroom].Built && UnityEngine.Random.value < 0.25f)
        {
            return TargetNearBuilding(grandpa, BuildingType.Bedroom, 1.25f);
        }

        grandpa.HasInteraction = false;
        return RandomSpawnPosition();
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

    private string RandomThought(Grandpa grandpa)
    {
        if (grandpa.Budding > 86f)
        {
            return "Я почти готов почковаться";
        }

        switch (grandpa.Role)
        {
            case GrandpaRole.SamovarKeeper:
                return "Самовар - это сердце";
            case GrandpaRole.Cardboarder:
                return "Картон - основа цивилизации";
            case GrandpaRole.Mutterer:
                return "Пора ворчать";
            case GrandpaRole.Guard:
                return "Сверху шумят";
            case GrandpaRole.Philosopher:
                return "Тёплая мысль греет дважды";
            case GrandpaRole.RadioReceiver:
                return "Ш-ш-ш... комиссия...";
            default:
                return UnityEngine.Random.value < 0.5f ? "Хочу чаю" : "Тут можно жить";
        }
    }

}

