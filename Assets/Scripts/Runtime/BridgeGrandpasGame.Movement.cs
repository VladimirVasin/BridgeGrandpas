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
    private const float GrandpaPersonalRadius = 0.48f;

    private Vector3 MoveGrandpaWithAvoidance(Grandpa grandpa, Vector3 current, Vector3 target, float speed, float deltaTime)
    {
        Vector3 flatCurrent = Flat(current);
        Vector3 desired = Flat(target) - flatCurrent;
        Vector3 steer = desired.sqrMagnitude > 0.01f ? desired.normalized : Vector3.zero;
        steer += GrandpaSeparation(grandpa, flatCurrent) * 1.55f;
        steer += BuildingSeparation(flatCurrent) * 1.85f;

        if (steer.sqrMagnitude < 0.001f)
        {
            return flatCurrent;
        }

        Vector3 next = flatCurrent + steer.normalized * speed * deltaTime;
        next = ResolveBuildingBlocks(next);
        next = ResolveGrandpaOverlap(grandpa, next);
        next = ResolveBuildingBlocks(next);
        return ClampToCommune(next);
    }

    private Vector3 GrandpaSeparation(Grandpa grandpa, Vector3 current)
    {
        Vector3 push = Vector3.zero;
        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa other = grandpas[i];
            if (other == grandpa || other.Root == null)
            {
                continue;
            }

            Vector3 delta = current - Flat(other.Root.transform.position);
            float distance = Mathf.Max(delta.magnitude, 0.001f);
            float minDistance = GrandpaPersonalRadius * 2f;
            if (distance < minDistance)
            {
                push += delta.normalized * ((minDistance - distance) / minDistance);
            }
        }

        return push;
    }

    private Vector3 BuildingSeparation(Vector3 current)
    {
        Vector3 push = Vector3.zero;
        foreach (Building building in buildings.Values)
        {
            if (!building.Built)
            {
                continue;
            }

            Vector3 delta = current - Flat(building.Position);
            float blocked = BuildingBlockRadius(building.Type);
            float influence = blocked + 0.55f;
            float distance = Mathf.Max(delta.magnitude, 0.001f);
            if (distance < influence)
            {
                push += delta.normalized * ((influence - distance) / influence);
            }
        }

        return push;
    }

    private Vector3 ResolveBuildingBlocks(Vector3 position)
    {
        foreach (Building building in buildings.Values)
        {
            if (!building.Built)
            {
                continue;
            }

            Vector3 center = Flat(building.Position);
            Vector3 delta = position - center;
            float radius = BuildingBlockRadius(building.Type);
            float distance = Mathf.Max(delta.magnitude, 0.001f);
            if (distance < radius)
            {
                position = center + delta.normalized * radius;
            }
        }

        return position;
    }

    private Vector3 ResolveGrandpaOverlap(Grandpa grandpa, Vector3 position)
    {
        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa other = grandpas[i];
            if (other == grandpa || other.Root == null)
            {
                continue;
            }

            Vector3 delta = position - Flat(other.Root.transform.position);
            float minDistance = GrandpaPersonalRadius * 2f;
            float distance = Mathf.Max(delta.magnitude, 0.001f);
            if (distance < minDistance)
            {
                position += delta.normalized * (minDistance - distance);
            }
        }

        return position;
    }

    private Vector3 TargetNearBuilding(Grandpa grandpa, BuildingType type, float radius)
    {
        grandpa.HasInteraction = true;
        grandpa.InteractionType = type;

        Building building = buildings[type];
        float angle = Mathf.Repeat(grandpa.Id * 137.5f + UnityEngine.Random.Range(-24f, 24f), 360f) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        return ClampToCommune(Flat(building.Position) + offset);
    }

    private float BuildingBlockRadius(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.Bedroom:
                return 1.05f;
            case BuildingType.GrumbleBench:
                return 0.95f;
            case BuildingType.CarpetCurtain:
                return 0.85f;
            case BuildingType.Samovar:
                return 0.82f;
            case BuildingType.RadioMayak:
                return 0.78f;
            case BuildingType.FireBarrel:
                return 0.72f;
            default:
                return 0.8f;
        }
    }

    private Vector3 Flat(Vector3 value)
    {
        return new Vector3(value.x, 0f, value.z);
    }

    private Vector3 ClampToCommune(Vector3 value)
    {
        value.x = Mathf.Clamp(value.x, -18.45f, 18.45f);
        value.z = Mathf.Clamp(value.z, -3.35f, 1.35f);
        value.y = 0f;
        return value;
    }
}
