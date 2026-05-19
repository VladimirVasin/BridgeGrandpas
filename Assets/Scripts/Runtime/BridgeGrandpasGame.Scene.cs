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
        RenderSettings.ambientLight = new Color(0.006f, 0.006f, 0.009f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.001f, 0.001f, 0.002f);
        RenderSettings.fogDensity = 0.038f;

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
        }

        mainCamera.orthographic = true;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.orthographicSize = 2.85f;
        mainCamera.transform.position = new Vector3(0f, 1.08f, -8.65f);
        mainCamera.transform.LookAt(new Vector3(0f, 0.62f, -0.1f));
        mainCamera.nearClipPlane = 0.1f;
        mainCamera.farClipPlane = 95f;
        mainCamera.backgroundColor = Color.black;
        EnsureCameraAudioListener();

        Light sun = FindAnyObjectByType<Light>();
        if (sun == null)
        {
            GameObject lightObject = new GameObject("Soft City Moon");
            sun = lightObject.AddComponent<Light>();
        }

        sun.type = LightType.Directional;
        sun.intensity = 0.018f;
        sun.color = new Color(0.18f, 0.22f, 0.30f);
        sun.transform.rotation = Quaternion.Euler(42f, -32f, 0f);

        CreateBridgeWorld();
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
        Material ground = AsphaltMat("wet_asphalt_ground", new Color(0.19f, 0.20f, 0.215f), new Vector2(22f, 18f), 0.48f);
        Material concrete = Mat("old_concrete", new Color(0.11f, 0.11f, 0.12f));
        Material darkConcrete = Mat("dark_concrete", new Color(0.045f, 0.050f, 0.060f));
        Material rail = Mat("rail", new Color(0.025f, 0.028f, 0.034f));
        Material warm = EmissiveMat("warm_window", new Color(1.0f, 0.48f, 0.18f), 0.35f);
        Material city = Mat("city_silhouette", new Color(0.020f, 0.024f, 0.032f));
        Material water = Mat("rain_puddle", new Color(0.025f, 0.032f, 0.038f));
        Material road = AsphaltMat("bridge_asphalt_road", new Color(0.46f, 0.45f, 0.43f), new Vector2(6.0f, 1.15f), 0.42f);
        const float bridgeLength = 24.5f;

        CreateGroundPlane("Continuous wet city asphalt", worldRoot, new Vector3(0f, -0.08f, -18f), new Vector2(260f, 220f), ground);
        CreateForegroundGroundCurtain();
        CreateBox("Back retaining wall", worldRoot, new Vector3(0f, 1.15f, 3.85f), new Vector3(bridgeLength + 1.2f, 2.3f, 0.25f), darkConcrete);
        CreateBox("Bridge deck", worldRoot, new Vector3(0f, 3.15f, 1.55f), new Vector3(bridgeLength, 0.52f, 3.15f), concrete);
        CreateBox("Road strip on bridge", worldRoot, new Vector3(0f, 3.45f, 1.55f), new Vector3(bridgeLength - 0.65f, 0.05f, 2.1f), road);
        CreateBox("Left bridge rail", worldRoot, new Vector3(0f, 3.85f, 0.05f), new Vector3(bridgeLength - 0.35f, 0.22f, 0.12f), rail);
        CreateBox("Right bridge rail", worldRoot, new Vector3(0f, 3.85f, 3.05f), new Vector3(bridgeLength - 0.35f, 0.22f, 0.12f), rail);

        for (int i = -3; i <= 3; i++)
        {
            float x = i * 3.55f;
            CreateBox("Bridge pillar " + i, worldRoot, new Vector3(x, 1.4f, 2.9f), new Vector3(0.55f, 2.8f, 0.45f), concrete);
            CreateBox("Bridge arch hint " + i, worldRoot, new Vector3(x + 1.72f, 2.05f, 2.86f), new Vector3(2.42f, 0.16f, 0.18f), concrete);
        }

        CreateBox("Puddle left", worldRoot, new Vector3(-4.8f, -0.01f, -2.8f), new Vector3(1.7f, 0.03f, 0.9f), water);
        CreateBox("Puddle right", worldRoot, new Vector3(4.3f, -0.01f, -2.35f), new Vector3(1.2f, 0.03f, 0.7f), water);

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
    }

    private void CreateForegroundGroundCurtain()
    {
        Material face = AsphaltMat(
            "foreground_asphalt_face",
            new Color(0.055f, 0.064f, 0.072f),
            new Vector2(18f, 3.4f),
            0.30f);
        Material lip = AsphaltMat(
            "foreground_asphalt_lip",
            new Color(0.12f, 0.135f, 0.145f),
            new Vector2(20f, 1.2f),
            0.38f);

        CreateBox("Foreground asphalt lip", worldRoot, new Vector3(0f, -0.075f, -3.35f), new Vector3(280f, 0.05f, 0.58f), lip);
        CreateBox("Foreground ground curtain", worldRoot, new Vector3(0f, -1.68f, -3.66f), new Vector3(280f, 3.18f, 0.26f), face);
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
        rainObject.transform.position = new Vector3(0f, 6.5f, 0f);
        ParticleSystem particles = rainObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.startLifetime = 2.6f;
        main.startSpeed = 4.8f;
        main.startSize = 0.045f;
        main.maxParticles = 420;
        main.startColor = new Color(0.55f, 0.68f, 0.85f, 0.42f);

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 95f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(16f, 0.6f, 12f);

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.y = -5.4f;
        velocity.x = -0.75f;

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
            fireBarrelCoreLight.intensity = 16.5f * flicker;
            fireBarrelCoreLight.range = (5.4f + pulseB * 1.15f) * rangeBoost;
        }

        if (fireBarrelPoolLight != null)
        {
            fireBarrelPoolLight.intensity = 11.2f * (0.88f + pulseA * 0.28f);
            fireBarrelPoolLight.range = (12.8f + pulseB * 2.1f) * rangeBoost;
        }

        if (fireBarrelFlickerLightA != null)
        {
            float sideA = Mathf.PerlinNoise(Time.time * 6.3f, 9.1f);
            fireBarrelFlickerLightA.transform.localPosition = new Vector3(0.38f + sideA * 0.22f, 0.86f + sideA * 0.24f, -0.28f);
            fireBarrelFlickerLightA.intensity = 3.2f + sideA * 6.8f;
            fireBarrelFlickerLightA.range = (2.5f + sideA * 1.45f) * rangeBoost;
        }

        if (fireBarrelFlickerLightB != null)
        {
            float sideB = Mathf.PerlinNoise(11.7f, Time.time * 7.4f);
            fireBarrelFlickerLightB.transform.localPosition = new Vector3(-0.42f, 0.96f + sideB * 0.30f, 0.18f + sideB * 0.28f);
            fireBarrelFlickerLightB.intensity = 2.8f + sideB * 5.7f;
            fireBarrelFlickerLightB.range = (2.3f + sideB * 1.25f) * rangeBoost;
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

