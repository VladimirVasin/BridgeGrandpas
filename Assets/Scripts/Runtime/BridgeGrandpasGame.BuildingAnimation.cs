using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float SamovarTableTopY = 0.27f;
    private const float SamovarTargetFootprint = 0.86f;
    private const float SamovarTargetHeight = 1.02f;

    private void ResetBuildingAnimationState(Building building)
    {
        building.AnimatedRoot = null;
        building.SteamParticles = null;
        building.AccentLight = null;
        building.AnimatedSeed = Random.Range(0f, 100f);
    }

    private bool TryCreateImportedSamovarVisual(Building building, Transform root, float levelScale)
    {
        GameObject asset = Resources.Load<GameObject>(SamovarResourcePath) ??
            Resources.Load<GameObject>(SamovarLegacyResourcePath);
        if (asset == null)
        {
            return false;
        }

        GameObject pivot = new GameObject("Samovar Animated Pivot");
        pivot.transform.SetParent(root, false);
        pivot.transform.localPosition = new Vector3(0f, SamovarTableTopY, 0f);

        GameObject instance = Instantiate(asset, pivot.transform);
        instance.name = "Imported Samovar Mesh";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        instance.transform.localScale = Vector3.one;
        RemoveImportedColliders(instance);
        RemoveImportedSceneExtras(instance);

        if (!FitSamovarModel(pivot.transform, instance.transform, levelScale))
        {
            Destroy(pivot);
            return false;
        }

        ApplySamovarMaterials(instance);
        building.AnimatedRoot = pivot.transform;
        building.AnimatedBasePosition = pivot.transform.localPosition;
        building.AnimatedBaseRotation = pivot.transform.localRotation;
        building.AnimatedBaseScale = pivot.transform.localScale;
        return true;
    }

    private bool FitSamovarModel(Transform pivot, Transform model, float levelScale)
    {
        Bounds bounds;
        if (!TryGetRendererBounds(model, out bounds))
        {
            return false;
        }

        Vector3 size = bounds.size;
        float horizontal = Mathf.Max(size.x, size.z);
        if (horizontal <= 0.001f || size.y <= 0.001f)
        {
            return false;
        }

        float byFootprint = SamovarTargetFootprint * levelScale / horizontal;
        float byHeight = SamovarTargetHeight * levelScale / size.y;
        float scale = Mathf.Clamp(Mathf.Min(byFootprint, byHeight), 0.00001f, 2.5f);
        model.localScale *= scale;
        if (!TryGetRendererBounds(model, out bounds))
        {
            return false;
        }

        Vector3 center = pivot.InverseTransformPoint(bounds.center);
        float minY = pivot.InverseTransformPoint(new Vector3(bounds.center.x, bounds.min.y, bounds.center.z)).y;
        model.localPosition += new Vector3(-center.x, -minY, -center.z);
        return true;
    }

    private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            bounds = new Bounds();
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return true;
    }

    private static void RemoveImportedColliders(GameObject instance)
    {
        Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Destroy(colliders[i]);
        }
    }

    private static void RemoveImportedSceneExtras(GameObject instance)
    {
        Light[] lights = instance.GetComponentsInChildren<Light>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].enabled = false;
            lights[i].intensity = 0f;
            lights[i].range = 0f;
        }

        Camera[] cameras = instance.GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].enabled = false;
            cameras[i].depth = -100f;
            cameras[i].cullingMask = 0;
            cameras[i].clearFlags = CameraClearFlags.Nothing;
        }
    }

    private void ApplySamovarMaterials(GameObject instance)
    {
        Material brass = Mat("imported_samovar_brass", new Color(0.62f, 0.38f, 0.14f));
        Material dark = Mat("imported_samovar_dark", new Color(0.12f, 0.075f, 0.045f));
        Material cream = Mat("imported_samovar_cream", new Color(0.78f, 0.64f, 0.42f));
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            string n = renderers[i].name.ToLowerInvariant();
            Material material = n.Contains("leg") || n.Contains("wood") || n.Contains("base") ? dark :
                n.Contains("lid") || n.Contains("cup") || n.Contains("plate") ? cream : brass;
            Material[] materials = renderers[i].sharedMaterials;
            for (int j = 0; j < materials.Length; j++)
            {
                materials[j] = material;
            }

            renderers[i].sharedMaterials = materials;
        }
    }

    private void CreateSamovarCartoonEffects(Building building, Transform root, float levelScale)
    {
        building.SteamParticles = CreateSamovarSteam(root, levelScale);
        building.AccentLight = AddPointLight(root, "Samovar Amber Pulse", new Vector3(0f, 0.78f, -0.10f),
            new Color(1f, 0.56f, 0.18f), 2.0f, 0.65f);
    }

    private ParticleSystem CreateSamovarSteam(Transform root, float levelScale)
    {
        GameObject steamObject = new GameObject("Samovar Cartoon Steam");
        steamObject.transform.SetParent(root, false);
        steamObject.transform.localPosition = new Vector3(0f, SamovarTableTopY + SamovarTargetHeight * levelScale, 0f);

        ParticleSystem particles = steamObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.25f, 2.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.28f, 0.62f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.17f);
        main.startColor = new Color(0.88f, 0.91f, 0.96f, 0.52f);
        main.maxParticles = 70;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 18f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 18f;
        shape.radius = 0.07f;

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.06f, 0.06f);
        velocity.y = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.035f, 0.035f);

        particles.GetComponent<ParticleSystemRenderer>().material =
            Mat("samovar_cartoon_steam_mat", new Color(0.78f, 0.82f, 0.90f, 0.42f));
        particles.Play();
        return particles;
    }

    private void UpdateBuildingAnimations(float deltaTime)
    {
        foreach (Building building in buildings.Values)
        {
            if (!building.Built || building.Type != BuildingType.Samovar)
            {
                continue;
            }

            UpdateSamovarAnimation(building);
        }
    }

    private void UpdateSamovarAnimation(Building building)
    {
        float t = Time.time + building.AnimatedSeed;
        float bounce = Mathf.Sin(t * 3.2f) * 0.018f + Mathf.PerlinNoise(t * 0.8f, 2.4f) * 0.018f;
        float wobble = Mathf.Sin(t * 4.8f) * 4.5f;

        if (building.AnimatedRoot != null)
        {
            float squash = Mathf.Sin(t * 3.2f) * 0.035f;
            building.AnimatedRoot.localPosition = building.AnimatedBasePosition + Vector3.up * bounce;
            building.AnimatedRoot.localRotation = building.AnimatedBaseRotation * Quaternion.Euler(0f, wobble, -wobble * 0.25f);
            building.AnimatedRoot.localScale = Vector3.Scale(building.AnimatedBaseScale, new Vector3(1f + squash, 1f - squash * 0.45f, 1f + squash));
        }

        if (building.SteamParticles != null)
        {
            ParticleSystem.EmissionModule emission = building.SteamParticles.emission;
            emission.rateOverTime = 16f + Mathf.PerlinNoise(6.1f, t * 1.7f) * 18f;
        }

        if (building.AccentLight != null)
        {
            building.AccentLight.intensity = 0.42f + Mathf.PerlinNoise(t * 1.5f, 9.2f) * 0.55f;
        }
    }
}
