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
    private bool TryBuild(BuildingType type, bool free)
    {
        Building building = buildings[type];
        if (building.Built)
        {
            return false;
        }

        if (!free && !Spend(building.BuildCost))
        {
            Notify("Ресурсов не хватает. Дедушки смотрят на смету с уважением.");
            return false;
        }

        building.Built = true;
        building.Level = 1;
        building.Root = CreateBuildingVisual(building);
        suspicion += type == BuildingType.CarpetCurtain ? 1f : 4f;

        if (type == BuildingType.RadioMayak)
        {
            nextEventIn = Mathf.Min(nextEventIn, 8f);
        }

        string cozy = GainCozy(CozyForBuild(type));
        Notify("Построено: " + building.Name + ". Уют +" + RateF(CozyForBuild(type)) + "." + cozy);
        string observationText = type == BuildingType.FireBarrel
            ? "Была разведена бочка с огнём. Под мостом стало явно уютнее"
            : NotebookBuildPhrase(type) + ". Уют под мостом вырос на " + RateF(CozyForBuild(type)) + ".";
        QueueObservationLead(NotebookObjectName(type), observationText,
            building.Root != null ? building.Root.transform : null, building.Position, 0.12f);
        return true;
    }

    private void TryUpgrade(Building building)
    {
        if (!building.Built)
        {
            Notify("Сначала надо построить объект.");
            return;
        }

        ResourceStock cost = UpgradeCost(building);
        if (!Spend(cost))
        {
            Notify("На улучшение не хватает ресурсов.");
            return;
        }

        building.Level++;
        suspicion += building.Type == BuildingType.CarpetCurtain ? 0.5f : 3f;
        RefreshBuildingVisual(building);
        float cozyGain = 1.2f + building.Level * 0.55f;
        string cozy = GainCozy(cozyGain);
        Notify("Улучшено: " + building.Name + " до уровня " + building.Level + ". Уют +" + RateF(cozyGain) + "." + cozy);
        QueueObservationLead(NotebookObjectName(building.Type), NotebookObjectName(building.Type) +
            " укреплён до уровня " + building.Level + ". Дедушки делают вид, что так было всегда.",
            building.Root != null ? building.Root.transform : null, building.Position, 0.18f);
    }

    private void TryBudSelected()
    {
        Grandpa candidate = FindBuddingCandidate(selectedGrandpa);
        if (candidate == null)
        {
            Grandpa closest = FindClosestBuddingGrandpa();
            Notify(closest == null ? "Пока некому почковаться." : "Самый готовый дедушка: " + Mathf.FloorToInt(closest.Budding) + "%.");
            return;
        }

        TryBudGrandpa(candidate, false);
    }

    private Grandpa SpawnGrandpa(GrandpaRole role, Vector3 position)
    {
        Grandpa grandpa = new Grandpa
        {
            Id = nextGrandpaId++,
            Name = RandomGrandpaName(),
            Role = role,
            Budding = UnityEngine.Random.Range(6f, 24f),
            Target = position,
            BobSeed = UnityEngine.Random.Range(0f, 100f),
            WalkJitter = UnityEngine.Random.Range(-0.08f, 0.12f),
            ActionSeed = UnityEngine.Random.Range(0f, 100f),
            FootstepCyclePhase = -1f
        };

        grandpa.Root = CreateGrandpaVisual(grandpa, position);
        grandpas.Add(grandpa);

        if (role == GrandpaRole.Philosopher || role == GrandpaRole.RadioReceiver)
        {
            rareMutationSeen = true;
            mutationsSinceRare = 0;
        }

        return grandpa;
    }

    private GrandpaRole RollMutationRole()
    {
        mutationsSinceRare++;
        int rareBonus = mutationsSinceRare >= 10 ? 4 : 0;
        int roll = random.Next(0, 100 + rareBonus);

        if (roll < 45)
        {
            return GrandpaRole.Common;
        }

        if (roll < 60)
        {
            return GrandpaRole.SamovarKeeper;
        }

        if (roll < 75)
        {
            return GrandpaRole.Cardboarder;
        }

        if (roll < 85)
        {
            return GrandpaRole.Mutterer;
        }

        if (roll < 95)
        {
            return GrandpaRole.Guard;
        }

        if (roll < 99 + rareBonus)
        {
            return GrandpaRole.Philosopher;
        }

        return GrandpaRole.RadioReceiver;
    }

}

