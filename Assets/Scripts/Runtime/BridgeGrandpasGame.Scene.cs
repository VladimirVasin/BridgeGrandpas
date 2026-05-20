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
    private void SetupScene()
    {
        worldRoot = new GameObject("Bridge World Root").transform;
        settlementRoot = new GameObject("Under-Bridge Commune").transform;
        settlementRoot.SetParent(worldRoot, false);

        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.050f, 0.058f, 0.070f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.001f, 0.001f, 0.002f);
        RenderSettings.fogDensity = 0.022f;

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
        }

        mainCamera.orthographic = true;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.orthographicSize = CameraDefaultZoom;
        mainCamera.transform.position = new Vector3(1.15f, 1.05f, -7.25f);
        mainCamera.transform.LookAt(new Vector3(0f, 0.58f, -0.25f));
        mainCamera.nearClipPlane = 0.1f;
        mainCamera.farClipPlane = 95f;
        mainCamera.backgroundColor = Color.black;
        EnsureCameraAudioListener();
        SetupAtmospherePostProcessing();
        SetupCameraForeground();

        Light sun = FindAnyObjectByType<Light>();
        if (sun == null)
        {
            GameObject lightObject = new GameObject("Soft City Moon");
            sun = lightObject.AddComponent<Light>();
        }

        sun.type = LightType.Directional;
        sun.intensity = 0.075f;
        sun.color = new Color(0.26f, 0.32f, 0.43f);
        sun.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
        SetupSceneLighting(sun);

        CreateBridgeWorld();
        SetupUnderpassWind();
        SetupGroundLitter();
        selectionMarker = CreateMarker("Selection Marker", new Color(1f, 0.76f, 0.24f, 0.85f));
        hoverMarker = CreateMarker("Hover Marker", new Color(0.55f, 0.85f, 1f, 0.55f));
    }

    private void EnsureCameraAudioListener()
    {
        if (mainCamera == null)
        {
            return;
        }

        if (mainCamera.GetComponent<AudioListener>() == null)
        {
            mainCamera.gameObject.AddComponent<AudioListener>();
        }

        EnsureMasterAudioEffects();
    }

    private void CreateBridgeWorld()
    {
        Material ground = AsphaltMat("wet_asphalt_ground", new Color(0.14f, 0.155f, 0.17f), new Vector2(18f, 14f), 0.64f);
        Material concrete = Mat("old_concrete", new Color(0.12f, 0.125f, 0.13f));
        Material darkConcrete = Mat("dark_concrete", new Color(0.045f, 0.050f, 0.060f));
        Material rail = Mat("rail", new Color(0.025f, 0.028f, 0.034f));
        Material warm = EmissiveMat("warm_window", new Color(1.0f, 0.48f, 0.18f), 0.35f);
        Material city = Mat("city_silhouette", new Color(0.020f, 0.024f, 0.032f));
        Material water = Mat("rain_puddle", new Color(0.025f, 0.032f, 0.038f));
        Material road = AsphaltMat("bridge_asphalt_road", new Color(0.30f, 0.31f, 0.32f), new Vector2(7.5f, 1.35f), 0.48f);
        const float bridgeLength = 46f;

        CreateGroundPlane("Continuous wet city asphalt", worldRoot, new Vector3(0f, -0.08f, -18f), new Vector2(260f, 220f), ground);
        CreateForegroundGroundCurtain();
        CreateBox("Back retaining wall", worldRoot, new Vector3(0f, 1.28f, 3.85f), new Vector3(bridgeLength + 2.6f, 2.55f, 0.32f), darkConcrete);
        CreateBox("Bridge deck", worldRoot, new Vector3(0f, 3.08f, 1.55f), new Vector3(bridgeLength, 0.72f, 3.25f), concrete);
        CreateBox("Road strip on bridge", worldRoot, new Vector3(0f, 3.48f, 1.55f), new Vector3(bridgeLength - 0.65f, 0.06f, 2.15f), road);
        CreateBox("Left bridge rail", worldRoot, new Vector3(0f, 3.86f, 0.05f), new Vector3(bridgeLength - 0.35f, 0.26f, 0.14f), rail);
        CreateBox("Right bridge rail", worldRoot, new Vector3(0f, 3.86f, 3.05f), new Vector3(bridgeLength - 0.35f, 0.26f, 0.14f), rail);

        for (int i = -2; i <= 2; i++)
        {
            float x = i * 5.7f;
            CreateBox("Bridge pillar " + i, worldRoot, new Vector3(x, 1.42f, 2.76f), new Vector3(0.92f, 2.85f, 0.78f), concrete);
            CreateBox("Bridge underside mass " + i, worldRoot, new Vector3(x + 2.85f, 2.34f, 2.76f), new Vector3(3.5f, 0.28f, 0.42f), concrete);
        }

        CreateBridgeDetailLayer(bridgeLength, concrete, rail);
        CreateBridgeTraffic();

        CreateBox("Puddle left", worldRoot, new Vector3(-4.8f, -0.01f, -2.8f), new Vector3(1.7f, 0.03f, 0.9f), water);
        CreateBox("Puddle right", worldRoot, new Vector3(4.3f, -0.01f, -2.35f), new Vector3(1.2f, 0.03f, 0.7f), water);
        CreateSceneDecalLayer();

        for (int i = 0; i < 9; i++)
        {
            float x = -6.6f + i * 1.65f;
            float h = 1.2f + (i % 3) * 0.55f;
            CreateBox("Distant block " + i, worldRoot, new Vector3(x, h * 0.5f, 4.35f), new Vector3(1.05f, h, 0.35f), city);
            if (i % 2 == 0)
            {
                CreateBox("Distant lit window " + i, worldRoot, new Vector3(x - 0.18f, h * 0.55f, 4.16f), new Vector3(0.14f, 0.12f, 0.04f), warm);
            }
        }

        CreateCityAmbience();
        CreateRain();
        CreateSceneAtmosphereParticles();
    }

    private void CreateForegroundGroundCurtain()
    {
        Material apron = AsphaltMat(
            "foreground_asphalt_apron",
            new Color(0.095f, 0.112f, 0.128f),
            new Vector2(20f, 6.5f),
            0.52f);
        Material lip = AsphaltMat(
            "foreground_asphalt_lip",
            new Color(0.135f, 0.152f, 0.165f),
            new Vector2(20f, 1.2f),
            0.50f);
        Material wet = TransparentMat("foreground_wet_reflection", new Color(0.18f, 0.26f, 0.34f, 0.28f));

        CreateBox("Foreground asphalt lip", worldRoot, new Vector3(0f, -0.075f, -3.35f), new Vector3(280f, 0.05f, 0.58f), lip);
        CreateGroundPlane("Foreground visible wet asphalt", worldRoot, new Vector3(0f, -0.082f, -7.8f), new Vector2(280f, 9.2f), apron);
        CreateBox("Foreground fire reflection smear", worldRoot, new Vector3(0.15f, -0.047f, -5.05f), new Vector3(3.8f, 0.014f, 0.70f), wet);
        CreateBox("Foreground cold puddle smear L", worldRoot, new Vector3(-6.0f, -0.048f, -5.8f), new Vector3(2.4f, 0.012f, 0.44f), wet);
        CreateBox("Foreground cold puddle smear R", worldRoot, new Vector3(6.5f, -0.048f, -6.35f), new Vector3(2.8f, 0.012f, 0.48f), wet);
    }

    private GameObject CreateGroundPlane(string name, Transform parent, Vector3 localPosition, Vector2 size, Material material)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = name;
        plane.transform.SetParent(parent, false);
        plane.transform.localPosition = localPosition;
        plane.transform.localScale = new Vector3(size.x / 10f, 1f, size.y / 10f);
        Renderer renderer = plane.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        Collider collider = plane.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        return plane;
    }

    private void CreateRain()
    {
        GameObject rainObject = new GameObject("Soft Rain");
        rainObject.transform.SetParent(worldRoot, false);
        rainObject.transform.position = new Vector3(0f, 6.5f, -0.35f);
        ParticleSystem particles = rainObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2.4f, 3.4f);
        main.startSpeed = 4.8f;
        main.startSize = 0.045f;
        main.maxParticles = 1600;
        main.startColor = new Color(0.55f, 0.68f, 0.85f, 0.42f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 330f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(72f, 0.75f, 17f);

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.y = -5.4f;
        velocity.x = -0.75f;
        RegisterRainWind(particles);

        ParticleSystemRenderer renderer = rainObject.GetComponent<ParticleSystemRenderer>();
        renderer.material = Mat("rain_mat", new Color(0.55f, 0.68f, 0.85f, 0.42f));
        particles.Play();
    }

    private void UpdateFireBarrelLighting()
    {
        if (fireBarrelCoreLight == null && fireBarrelPoolLight == null)
        {
            return;
        }

        int level = CurrentFireBarrelLevel();
        float rangeBoost = FireBarrelLightRangeBoost(level);
        float pulseA = Mathf.PerlinNoise(Time.time * 2.7f, 0.23f);
        float pulseB = Mathf.PerlinNoise(5.1f, Time.time * 4.1f);
        float flicker = 0.82f + pulseA * 0.25f + pulseB * 0.10f;

        if (fireBarrelCoreLight != null)
        {
            fireBarrelCoreLight.intensity = 6.0f * flicker;
            fireBarrelCoreLight.range = (5.2f + pulseB * 1.10f) * rangeBoost;
        }

        if (fireBarrelPoolLight != null)
        {
            fireBarrelPoolLight.intensity = 4.4f * (0.88f + pulseA * 0.28f);
            fireBarrelPoolLight.range = (8.2f + pulseB * 1.45f) * rangeBoost;
        }

        if (fireBarrelFlickerLightA != null)
        {
            float sideA = Mathf.PerlinNoise(Time.time * 6.3f, 9.1f);
            fireBarrelFlickerLightA.transform.localPosition = new Vector3(0.38f + sideA * 0.22f, 0.86f + sideA * 0.24f, -0.28f);
            fireBarrelFlickerLightA.intensity = 1.4f + sideA * 2.7f;
            fireBarrelFlickerLightA.range = (2.1f + sideA * 0.95f) * rangeBoost;
        }

        if (fireBarrelFlickerLightB != null)
        {
            float sideB = Mathf.PerlinNoise(11.7f, Time.time * 7.4f);
            fireBarrelFlickerLightB.transform.localPosition = new Vector3(-0.42f, 0.96f + sideB * 0.30f, 0.18f + sideB * 0.28f);
            fireBarrelFlickerLightB.intensity = 1.2f + sideB * 2.3f;
            fireBarrelFlickerLightB.range = (2.0f + sideB * 0.85f) * rangeBoost;
        }

        UpdateFireFlames(pulseA, pulseB);
    }

    private int CurrentFireBarrelLevel()
    {
        Building fire;
        if (buildings.TryGetValue(BuildingType.FireBarrel, out fire) && fire.Built)
        {
            return Mathf.Max(1, fire.Level);
        }

        return 1;
    }

    private float FireBarrelLightRangeBoost(int level)
    {
        return 1f + Mathf.Max(0, level - 1) * 0.14f;
    }

    private void UpdateFireFlames(float pulseA, float pulseB)
    {
        if (fireFlames == null)
        {
            return;
        }

        for (int i = 0; i < fireFlames.Length; i++)
        {
            Transform flame = fireFlames[i];
            if (flame == null)
            {
                continue;
            }

            float wave = Mathf.PerlinNoise(Time.time * (6.2f + i), 1.7f + i * 4.1f);
            float angle = i * 2.1f + pulseB * 0.7f;
            flame.localPosition = new Vector3(Mathf.Sin(angle) * 0.13f, 0.80f + wave * 0.18f, Mathf.Cos(angle) * 0.10f);
            flame.localScale = new Vector3(0.08f + wave * 0.04f, 0.30f + wave * 0.24f, 0.07f + pulseA * 0.04f);
            flame.localRotation = Quaternion.Euler(-12f + wave * 22f, Time.time * (68f + i * 11f), -18f + wave * 34f);
        }
    }

}

