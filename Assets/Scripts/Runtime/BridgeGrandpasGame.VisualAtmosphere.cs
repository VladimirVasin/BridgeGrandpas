using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private Volume atmosphereVolume;
    private ColorAdjustments atmosphereColor;
    private Vignette atmosphereVignette;
    private FilmGrain atmosphereFilmGrain;
    private ChromaticAberration vhsChromaticAberration;
    private LensDistortion vhsLensDistortion;

    private void SetupAtmospherePostProcessing()
    {
        if (mainCamera == null || atmosphereVolume != null)
        {
            return;
        }

        UniversalAdditionalCameraData cameraData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null)
        {
            cameraData = mainCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        }

        cameraData.renderPostProcessing = true;
        cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        cameraData.antialiasingQuality = AntialiasingQuality.Medium;
        cameraData.requiresDepthTexture = true;
        cameraData.requiresColorTexture = true;
        cameraData.dithering = true;
        cameraData.volumeLayerMask = ~0;

        GameObject volumeObject = new GameObject("Bridge Grandpas Runtime Look");
        volumeObject.transform.SetParent(transform, false);
        atmosphereVolume = volumeObject.AddComponent<Volume>();
        atmosphereVolume.isGlobal = true;
        atmosphereVolume.priority = 8f;
        atmosphereVolume.sharedProfile = CreateAtmosphereVolumeProfile();
    }

    private VolumeProfile CreateAtmosphereVolumeProfile()
    {
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "Bridge Grandpas Warm Underpass";

        Bloom bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(1.18f);
        bloom.intensity.Override(0.30f);
        bloom.scatter.Override(0.48f);
        bloom.tint.Override(new Color(1f, 0.68f, 0.38f));
        bloom.highQualityFiltering.Override(true);

        atmosphereColor = profile.Add<ColorAdjustments>(true);
        atmosphereColor.postExposure.Override(0.22f);
        atmosphereColor.contrast.Override(4f);
        atmosphereColor.saturation.Override(3f);
        atmosphereColor.colorFilter.Override(new Color(0.98f, 0.99f, 1.03f));

        WhiteBalance balance = profile.Add<WhiteBalance>(true);
        balance.temperature.Override(-14f);
        balance.tint.Override(6f);

        atmosphereVignette = profile.Add<Vignette>(true);
        atmosphereVignette.color.Override(new Color(0f, 0f, 0.02f));
        atmosphereVignette.center.Override(new Vector2(0.5f, 0.48f));
        atmosphereVignette.intensity.Override(0.10f);
        atmosphereVignette.smoothness.Override(0.72f);

        atmosphereFilmGrain = profile.Add<FilmGrain>(true);
        atmosphereFilmGrain.type.Override(FilmGrainLookup.Thin1);
        atmosphereFilmGrain.intensity.Override(0.12f);
        atmosphereFilmGrain.response.Override(0.72f);

        vhsChromaticAberration = profile.Add<ChromaticAberration>(true);
        vhsChromaticAberration.intensity.Override(0f);

        vhsLensDistortion = profile.Add<LensDistortion>(true);
        vhsLensDistortion.intensity.Override(0f);
        vhsLensDistortion.scale.Override(1.02f);

        Tonemapping tone = profile.Add<Tonemapping>(true);
        tone.mode.Override(TonemappingMode.ACES);
        return profile;
    }

    private void ApplyVhsPostEffects(bool active, float zoomPulse, float trackingPulse)
    {
        if (atmosphereFilmGrain != null)
        {
            atmosphereFilmGrain.intensity.Override(active ? 0.42f + zoomPulse * 0.24f + trackingPulse * 0.12f : 0.12f);
        }

        if (atmosphereColor != null)
        {
            atmosphereColor.postExposure.Override(active ? 0.20f + zoomPulse * 0.08f + trackingPulse * 0.04f : 0.22f);
            atmosphereColor.contrast.Override(active ? 12f + trackingPulse * 8f : 4f);
            atmosphereColor.saturation.Override(active ? -18f : 3f);
        }

        if (atmosphereVignette != null)
        {
            atmosphereVignette.intensity.Override(active ? 0.28f + trackingPulse * 0.10f : 0.10f);
        }

        if (vhsChromaticAberration != null)
        {
            float amount = active ? 0.13f + zoomPulse * 0.24f + trackingPulse * 0.12f : 0f;
            vhsChromaticAberration.intensity.Override(Mathf.Clamp01(amount));
        }

        if (vhsLensDistortion != null)
        {
            vhsLensDistortion.intensity.Override(active ? -0.085f - zoomPulse * 0.055f - trackingPulse * 0.025f : 0f);
        }
    }

    private void SetupSceneLighting(Light moon)
    {
        if (moon != null)
        {
            moon.shadows = LightShadows.Soft;
            moon.shadowStrength = 0.22f;
            moon.shadowBias = 0.05f;
            moon.shadowNormalBias = 0.28f;
        }

        CreateAtmosphereSpot(
            "Cold city rim wash",
            new Vector3(0f, 2.25f, -4.6f),
            new Vector3(0f, 0.75f, 0.15f),
            new Color(0.30f, 0.42f, 0.65f),
            18f,
            1.15f,
            56f,
            false);
        CreateAtmosphereSpot(
            "Warm barrel bounce wash",
            new Vector3(0f, 0.42f, -2.7f),
            new Vector3(0f, 0.24f, -0.05f),
            new Color(1f, 0.46f, 0.18f),
            7.1f,
            0.76f,
            78f,
            true);
        CreateAtmosphereSpot(
            "Soft underpass readability fill",
            new Vector3(0f, 1.65f, -3.9f),
            new Vector3(0f, 0.70f, 1.1f),
            new Color(0.43f, 0.54f, 0.68f),
            16f,
            0.52f,
            62f,
            false);
        CreateCameraReadabilityLights();
    }

    private void CreateCameraReadabilityLights()
    {
        if (mainCamera == null || worldRoot == null)
        {
            return;
        }

        GameObject directionalObject = new GameObject("Camera Readability Directional Fill");
        directionalObject.transform.SetParent(worldRoot, false);
        directionalObject.transform.rotation = mainCamera.transform.rotation;
        Light directional = directionalObject.AddComponent<Light>();
        directional.type = LightType.Directional;
        directional.color = new Color(0.48f, 0.58f, 0.76f);
        directional.intensity = 0.34f;
        directional.shadows = LightShadows.None;

        GameObject coneObject = new GameObject("Camera Soft Cone Fill");
        coneObject.transform.SetParent(mainCamera.transform, false);
        coneObject.transform.localPosition = Vector3.zero;
        coneObject.transform.localRotation = Quaternion.identity;
        Light cone = coneObject.AddComponent<Light>();
        cone.type = LightType.Spot;
        cone.color = new Color(0.52f, 0.62f, 0.78f);
        cone.range = 28f;
        cone.intensity = 1.05f;
        cone.spotAngle = 92f;
        cone.shadows = LightShadows.None;
    }

    private void CreateAtmosphereSpot(string name, Vector3 position, Vector3 target, Color color, float range, float intensity, float angle, bool shadows)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(worldRoot, false);
        lightObject.transform.localPosition = position;
        lightObject.transform.LookAt(target);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = color;
        light.range = range;
        light.intensity = intensity;
        light.spotAngle = angle;
        light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
        light.shadowStrength = 0.35f;
        light.shadowBias = 0.04f;
        light.shadowNormalBias = 0.22f;
    }

    private void CreateBridgeDetailLayer(float bridgeLength, Material concrete, Material rail)
    {
        Material deep = Mat("bridge_deep_shadow_detail", new Color(0.020f, 0.024f, 0.030f));
        Material pipe = Mat("bridge_old_pipe", new Color(0.050f, 0.055f, 0.058f));
        Material rust = Mat("bridge_rust_marks", new Color(0.30f, 0.13f, 0.055f));

        CreateWorldDecorBox("Bridge front lower beam", new Vector3(0f, 2.70f, -0.05f), new Vector3(bridgeLength + 0.9f, 0.34f, 0.18f), deep);
        CreateWorldDecorBox("Bridge rear lower beam", new Vector3(0f, 2.66f, 3.16f), new Vector3(bridgeLength + 0.6f, 0.28f, 0.20f), deep);
        CreateWorldDecorBox("Bridge underside shadow slab", new Vector3(0f, 2.47f, 1.48f), new Vector3(bridgeLength - 1.2f, 0.22f, 2.62f), deep);

        for (int i = -3; i <= 3; i++)
        {
            float x = i * 4.8f;
            CreateWorldDecorBox("Bridge heavy rib " + i, new Vector3(x, 2.60f, 1.52f), new Vector3(0.26f, 0.42f, 3.05f), concrete);
            CreateWorldDecorBox("Bridge rust patch " + i, new Vector3(x + 0.55f, 2.24f, 2.74f), new Vector3(0.58f, 0.24f, 0.035f), rust);
        }

        Transform frontPipe = CreateWorldDecorCylinder("Long front drain pipe", new Vector3(0f, 2.44f, -0.18f), new Vector3(0.035f, 0.035f, bridgeLength - 1.0f), pipe).transform;
        frontPipe.localRotation = Quaternion.Euler(0f, 0f, 90f);
        Transform rearPipe = CreateWorldDecorCylinder("Long rear cable pipe", new Vector3(0f, 2.18f, 3.02f), new Vector3(0.028f, 0.028f, bridgeLength - 2.5f), pipe).transform;
        rearPipe.localRotation = Quaternion.Euler(0f, 0f, 90f);

        for (int i = 0; i < 5; i++)
        {
            float x = -10.0f + i * 5.0f;
            CreateWorldDecorBox("Road worn dash " + i, new Vector3(x, 3.49f, 1.55f), new Vector3(0.78f, 0.018f, 0.055f), rail);
        }
    }

    private void CreateSceneDecalLayer()
    {
        Material soot = TransparentMat("decal_soot", new Color(0.01f, 0.005f, 0.002f, 0.58f));
        Material wet = TransparentMat("decal_wet_glint", new Color(0.24f, 0.33f, 0.42f, 0.36f));
        Material crack = Mat("decal_dark_crack", new Color(0.014f, 0.016f, 0.018f));
        Material chalk = TransparentMat("decal_chalk_scuffs", new Color(0.65f, 0.69f, 0.70f, 0.22f));

        CreateWorldDecorBox("Soot bloom behind barrel", new Vector3(0f, 1.05f, 3.715f), new Vector3(4.4f, 1.55f, 0.028f), soot);
        CreateWorldDecorBox("Wet fire reflection", new Vector3(0f, -0.045f, -1.25f), new Vector3(3.1f, 0.018f, 0.72f), wet);
        CreateWorldDecorBox("Wet long orange reflection", new Vector3(0f, -0.046f, -2.05f), new Vector3(1.25f, 0.016f, 1.15f), wet);
        CreateWorldDecorBox("Wet back reflection", new Vector3(0.2f, -0.044f, 0.72f), new Vector3(2.35f, 0.016f, 0.38f), wet);
        CreateWorldDecorBox("Wet side reflection L", new Vector3(-3.2f, -0.042f, -2.45f), new Vector3(1.6f, 0.016f, 0.28f), wet);
        CreateWorldDecorBox("Wet side reflection R", new Vector3(3.4f, -0.042f, -2.05f), new Vector3(1.85f, 0.016f, 0.30f), wet);

        for (int i = 0; i < 9; i++)
        {
            float x = -5.8f + i * 1.45f;
            float z = -1.65f + Mathf.Sin(i * 1.7f) * 0.85f;
            GameObject line = CreateWorldDecorBox("Hairline asphalt crack " + i, new Vector3(x, -0.035f, z), new Vector3(0.72f, 0.014f, 0.025f), crack);
            line.transform.localRotation = Quaternion.Euler(0f, 18f + i * 17f, 0f);
        }

        CreateWorldDecorBox("Old chalk mark A", new Vector3(-6.0f, -0.034f, -1.05f), new Vector3(0.55f, 0.012f, 0.026f), chalk);
        CreateWorldDecorBox("Old chalk mark B", new Vector3(5.7f, -0.034f, -0.92f), new Vector3(0.62f, 0.012f, 0.026f), chalk);
    }

    private void CreateSceneAtmosphereParticles()
    {
        CreateAtmosphereParticles("Warm ember dust", new Vector3(0f, 0.78f, -0.15f), new Vector3(3.2f, 1.2f, 1.8f), new Color(1f, 0.42f, 0.10f, 0.34f), 18f, 2.4f, 0.042f, new Vector3(0.05f, 0.32f, -0.02f));
        CreateAtmosphereParticles("Cold underpass haze", new Vector3(0f, 0.22f, -1.35f), new Vector3(18f, 0.35f, 2.9f), new Color(0.18f, 0.25f, 0.34f, 0.14f), 6f, 8.5f, 0.62f, new Vector3(0.03f, 0.015f, 0.02f));
        CreateAtmosphereParticles("Ceiling dust in firelight", new Vector3(0f, 1.85f, 0.95f), new Vector3(11f, 1.3f, 2.2f), new Color(1f, 0.64f, 0.28f, 0.18f), 9f, 5.6f, 0.075f, new Vector3(-0.02f, 0.05f, -0.03f));

        Material shaft = TransparentMat("warm_light_shaft", new Color(1f, 0.32f, 0.08f, 0.075f));
        CreateWorldDecorBox("Soft fire light veil", new Vector3(0f, 1.18f, 1.30f), new Vector3(5.0f, 2.15f, 0.035f), shaft);
    }

    private void CreateAtmosphereParticles(string name, Vector3 position, Vector3 shapeScale, Color color, float rate, float lifetime, float size, Vector3 velocity)
    {
        GameObject particleObject = new GameObject(name);
        particleObject.transform.SetParent(worldRoot, false);
        particleObject.transform.localPosition = position;
        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.startLifetime = lifetime;
        main.startSpeed = 0.05f;
        main.startSize = size;
        main.startColor = color;
        main.maxParticles = 180;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = rate;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = shapeScale;

        ParticleSystem.VelocityOverLifetimeModule vel = particles.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = velocity.x;
        vel.y = velocity.y;
        vel.z = velocity.z;

        ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
        renderer.material = ParticleMat(name + "_mat", color);
        renderer.sortingOrder = 2;
        particles.Play();
    }

    private GameObject CreateWorldDecorBox(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject item = CreateBox(name, worldRoot, position, scale, material);
        StripDecorCollider(item);
        return item;
    }

    private GameObject CreateWorldDecorCylinder(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject item = CreateCylinder(name, worldRoot, position, scale, material);
        StripDecorCollider(item);
        return item;
    }

    private static void StripDecorCollider(GameObject item)
    {
        Collider collider = item.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        item.layer = 2;
    }
}
