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
    private void UpdateGrandpaInteractionPose(Grandpa grandpa)
    {
        if (grandpa.Root == null)
        {
            return;
        }

        bool close = grandpa.HasInteraction && Vector3.Distance(Flat(grandpa.Root.transform.position), Flat(grandpa.Target)) < 0.28f;
        float action = close ? (Mathf.Sin((Time.time + grandpa.ActionSeed) * 5f) + 1f) * 0.5f : 0f;
        Vector3 scale = Vector3.one;

        if (Time.time < grandpa.BirthAnimUntil)
        {
            float t = Mathf.InverseLerp(grandpa.BirthAnimStart, grandpa.BirthAnimUntil, Time.time);
            float bounce = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t);
            scale *= Mathf.Lerp(0.08f, 1f, SmoothOutBack(t)) + bounce * 0.18f;
        }
        else if (Time.time < grandpa.BudBurstUntil)
        {
            float t = 1f - Mathf.InverseLerp(grandpa.BudBurstUntil - 0.75f, grandpa.BudBurstUntil, Time.time);
            scale = new Vector3(1f + t * 0.28f, 1f - t * 0.16f, 1f + t * 0.28f);
        }

        if (close)
        {
            scale = Vector3.Scale(scale, InteractionScale(grandpa.InteractionType, action));
            FaceInteractionBuilding(grandpa);
        }

        grandpa.Root.transform.localScale = Vector3.Lerp(grandpa.Root.transform.localScale, scale, Time.deltaTime * 12f);
        AnimateGrandpaModel(grandpa, close, action);
        UpdateInteractionProp(grandpa, close, action);
    }

    private void AnimateGrandpaModel(Grandpa grandpa, bool close, float action)
    {
        if (grandpa.Root == null)
        {
            return;
        }

        bool walking = Vector3.Distance(Flat(grandpa.Root.transform.position), Flat(grandpa.Target)) > 0.22f;
        float animTime = (Time.time + grandpa.ActionSeed) * (walking ? 9f : 2.4f);
        float stride = Mathf.Sin(animTime);

        if (grandpa.UsesImportedModel && grandpa.ImportedModelRoot != null)
        {
            float lift = walking ? Mathf.Abs(stride) * 0.006f : Mathf.Sin((Time.time + grandpa.ActionSeed) * 1.8f) * 0.006f;
            grandpa.ImportedModelRoot.localPosition = grandpa.ImportedModelBasePosition + Vector3.up * lift;
            grandpa.ImportedModelRoot.localScale = grandpa.ImportedModelBaseScale;
        }

        AnimateImportedParts(grandpa, stride, walking, close);
        MaybePlayGrandpaFootstep(grandpa, walking, animTime);
    }

    private void AnimateImportedParts(Grandpa grandpa, float stride, bool walking, bool close)
    {
        float idle = Mathf.Sin(Time.time * 2.2f + grandpa.ActionSeed);
        float armDegrees = walking ? ArmWalkDegrees(grandpa.Role) : 2f;
        ApplyWalkCycle(grandpa.ImportedLegs, grandpa.ImportedLegBaseRotations, grandpa.ImportedLegBasePositions,
            grandpa.ImportedLegPhaseSigns, walking ? stride : 0f, 7f);
        ApplyWalkCycle(grandpa.ImportedArms, grandpa.ImportedArmBaseRotations, grandpa.ImportedArmBasePositions,
            grandpa.ImportedArmPhaseSigns, walking ? -stride : idle, armDegrees);
        AnimateBodyControl(grandpa, stride, walking, close);

        if (grandpa.ImportedHead != null)
        {
            float nod = close ? Mathf.Sin(Time.time * 4f + grandpa.ActionSeed) * 4f : Mathf.Sin(Time.time * 1.5f + grandpa.ActionSeed) * 1.3f;
            grandpa.ImportedHead.localRotation = grandpa.ImportedHeadBaseRotation * Quaternion.Euler(nod, 0f, 0f);
        }

        if (grandpa.ImportedHat != null)
        {
            grandpa.ImportedHat.localRotation = grandpa.ImportedHatBaseRotation * Quaternion.Euler(0f, 0f, stride * 1.5f);
        }

        if (grandpa.ImportedBeard != null)
        {
            grandpa.ImportedBeard.localRotation = grandpa.ImportedBeardBaseRotation * Quaternion.Euler(Mathf.Abs(stride) * 1.8f, 0f, 0f);
        }
    }

    private void ApplyWalkCycle(
        Transform[] parts,
        Quaternion[] rotations,
        Vector3[] positions,
        float[] signs,
        float stride,
        float degrees)
    {
        if (parts == null || rotations == null)
        {
            return;
        }

        for (int i = 0; i < parts.Length && i < rotations.Length; i++)
        {
            float sign = signs != null && i < signs.Length && !Mathf.Approximately(signs[i], 0f)
                ? signs[i]
                : (i % 2 == 1 ? -1f : 1f);
            float phase = stride * sign;
            parts[i].localRotation = rotations[i] * Quaternion.Euler(phase * degrees, 0f, 0f);
            if (positions != null && i < positions.Length)
            {
                parts[i].localPosition = positions[i];
            }
        }
    }

    private void AnimateBodyControl(Grandpa grandpa, float stride, bool walking, bool close)
    {
        if (grandpa.ImportedBodyControl == null)
        {
            return;
        }

        float sway = walking ? stride * 1.8f : Mathf.Sin(Time.time * 1.7f + grandpa.ActionSeed) * 0.8f;
        float nod = close ? Mathf.Sin(Time.time * 4f + grandpa.ActionSeed) * 1.4f : Mathf.Abs(stride) * 0.9f;
        grandpa.ImportedBodyControl.localPosition = grandpa.ImportedBodyBasePosition;
        grandpa.ImportedBodyControl.localRotation = grandpa.ImportedBodyBaseRotation * Quaternion.Euler(nod, 0f, -sway);
    }

    private float ArmWalkDegrees(GrandpaRole role)
    {
        switch (role)
        {
            case GrandpaRole.Common:
                return 9f;
            case GrandpaRole.Guard:
            case GrandpaRole.Mutterer:
                return 5f;
            default:
                return 2f;
        }
    }

    private Vector3 InteractionScale(BuildingType type, float action)
    {
        switch (type)
        {
            case BuildingType.GrumbleBench:
                return new Vector3(1.12f, 0.72f + action * 0.04f, 1.16f);
            case BuildingType.FireBarrel:
                return new Vector3(1f + action * 0.04f, 0.96f, 1f + action * 0.04f);
            case BuildingType.Bedroom:
                return new Vector3(1.18f, 0.58f + action * 0.03f, 1.10f);
            case BuildingType.Samovar:
                return new Vector3(1f, 1f + action * 0.08f, 1f);
            default:
                return new Vector3(1f, 1f + action * 0.035f, 1f);
        }
    }

    private void FaceInteractionBuilding(Grandpa grandpa)
    {
        if (!buildings.ContainsKey(grandpa.InteractionType))
        {
            return;
        }

        Vector3 toBuilding = Flat(buildings[grandpa.InteractionType].Position) - Flat(grandpa.Root.transform.position);
        if (toBuilding.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(toBuilding.normalized);
            grandpa.Root.transform.rotation = Quaternion.Slerp(grandpa.Root.transform.rotation, look, Time.deltaTime * 8f);
        }
    }

    private void UpdateInteractionProp(Grandpa grandpa, bool close, float action)
    {
        if (grandpa.InteractionProp == null)
        {
            return;
        }

        grandpa.InteractionProp.gameObject.SetActive(close);
        if (!close)
        {
            return;
        }

        Color color = new Color(0.95f, 0.78f, 0.36f);
        Vector3 scale = new Vector3(0.16f, 0.16f, 0.16f);
        Vector3 position = new Vector3(0f, 0.62f + action * 0.1f, -0.42f);

        switch (grandpa.InteractionType)
        {
            case BuildingType.Samovar:
                color = new Color(0.98f, 0.86f, 0.55f);
                scale = new Vector3(0.18f, 0.12f, 0.18f);
                break;
            case BuildingType.FireBarrel:
                color = new Color(1f, 0.44f, 0.08f);
                scale = new Vector3(0.12f + action * 0.06f, 0.12f, 0.12f);
                break;
            case BuildingType.Bedroom:
                color = new Color(0.58f, 0.40f, 0.22f);
                scale = new Vector3(0.30f, 0.06f, 0.18f);
                position = new Vector3(0f, 0.32f, -0.38f);
                break;
            case BuildingType.GrumbleBench:
                color = new Color(0.70f, 0.62f, 0.82f);
                scale = new Vector3(0.22f, 0.05f, 0.12f);
                position = new Vector3(0f, 0.38f, -0.42f);
                break;
            case BuildingType.CarpetCurtain:
                color = new Color(1f, 0.86f, 0.35f);
                scale = new Vector3(0.10f, 0.22f, 0.10f);
                break;
            case BuildingType.RadioMayak:
                color = new Color(0.35f, 1f, 0.55f);
                scale = new Vector3(0.14f, 0.20f + action * 0.08f, 0.10f);
                break;
        }

        grandpa.InteractionProp.localPosition = position;
        grandpa.InteractionProp.localScale = scale;
        if (grandpa.InteractionPropRenderer != null)
        {
            grandpa.InteractionPropRenderer.sharedMaterial = Mat("interaction_" + grandpa.InteractionType, color);
        }
    }

    private void CreateBuddingBurst(Vector3 position)
    {
        GameObject burst = new GameObject("Budding Cartoon Burst");
        burst.transform.SetParent(worldRoot, false);
        burst.transform.position = Flat(position) + Vector3.up * 0.65f;
        ParticleSystem particles = burst.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = 0.75f;
        main.startSpeed = 1.9f;
        main.startSize = 0.16f;
        main.startColor = new Color(1f, 0.78f, 0.32f, 0.92f);
        main.maxParticles = 32;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 28) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.28f;

        ParticleSystemRenderer renderer = burst.GetComponent<ParticleSystemRenderer>();
        renderer.material = Mat("budding_burst", new Color(1f, 0.72f, 0.26f, 0.95f));
        particles.Play();
        Destroy(burst, 1.4f);
    }

    private float SmoothOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
