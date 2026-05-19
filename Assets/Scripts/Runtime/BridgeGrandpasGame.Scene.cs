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
        worldRoot = new GameObject("MVP Diorama Root").transform;
        settlementRoot = new GameObject("Under-Bridge Commune").transform;
        settlementRoot.SetParent(worldRoot, false);

        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.006f, 0.006f, 0.009f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.001f, 0.001f, 0.002f);
        RenderSettings.fogDensity = 0.115f;

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
        }

        mainCamera.orthographic = true;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.orthographicSize = 6.2f;
        mainCamera.transform.position = new Vector3(8.2f, 2.05f, -8.85f);
        mainCamera.transform.rotation = Quaternion.Euler(16.5f, -39f, 0f);
        mainCamera.nearClipPlane = 0.1f;
        mainCamera.farClipPlane = 38f;
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

        CreateDiorama();
        selectionMarker = CreateMarker("Selection Marker", new Color(1f, 0.76f, 0.24f, 0.85f));
        hoverMarker = CreateMarker("Hover Marker", new Color(0.55f, 0.85f, 1f, 0.55f));
    }

    private void EnsureCameraAudioListener()
    {
        if (mainCamera == null || mainCamera.GetComponent<AudioListener>() != null)
        {
            return;
        }

        mainCamera.gameObject.AddComponent<AudioListener>();
    }

    private void CreateDiorama()
    {
        Material voidMat = Mat("outer_void", new Color(0.003f, 0.004f, 0.007f));
        Material ground = AsphaltMat("wet_asphalt_ground", new Color(0.34f, 0.34f, 0.35f), new Vector2(7.2f, 5.8f), 0.55f);
        Material concrete = Mat("old_concrete", new Color(0.11f, 0.11f, 0.12f));
        Material darkConcrete = Mat("dark_concrete", new Color(0.045f, 0.050f, 0.060f));
        Material rail = Mat("rail", new Color(0.025f, 0.028f, 0.034f));
        Material warm = EmissiveMat("warm_window", new Color(1.0f, 0.48f, 0.18f), 0.35f);
        Material city = Mat("city_silhouette", new Color(0.020f, 0.024f, 0.032f));
        Material water = Mat("rain_puddle", new Color(0.025f, 0.032f, 0.038f));
        Material road = AsphaltMat("bridge_asphalt_road", new Color(0.46f, 0.45f, 0.43f), new Vector2(6.0f, 1.15f), 0.42f);

        CreateBox("Outer darkness void", worldRoot, new Vector3(0f, -0.18f, 0f), new Vector3(70f, 0.06f, 50f), voidMat);
        CreateBox("Wet ground", worldRoot, new Vector3(0f, -0.08f, -3.8f), new Vector3(34f, 0.12f, 30f), ground);
        CreateBox("Back retaining wall", worldRoot, new Vector3(0f, 1.15f, 3.85f), new Vector3(15f, 2.3f, 0.25f), darkConcrete);
        CreateBox("Bridge deck", worldRoot, new Vector3(0f, 3.15f, 1.55f), new Vector3(15f, 0.52f, 3.15f), concrete);
        CreateBox("Road strip on bridge", worldRoot, new Vector3(0f, 3.45f, 1.55f), new Vector3(14.4f, 0.05f, 2.1f), road);
        CreateBox("Left bridge rail", worldRoot, new Vector3(0f, 3.85f, 0.05f), new Vector3(14.6f, 0.22f, 0.12f), rail);
        CreateBox("Right bridge rail", worldRoot, new Vector3(0f, 3.85f, 3.05f), new Vector3(14.6f, 0.22f, 0.12f), rail);

        for (int i = -2; i <= 2; i++)
        {
            float x = i * 3.2f;
            CreateBox("Bridge pillar " + i, worldRoot, new Vector3(x, 1.4f, 2.9f), new Vector3(0.55f, 2.8f, 0.45f), concrete);
            CreateBox("Bridge arch hint " + i, worldRoot, new Vector3(x + 1.55f, 2.05f, 2.86f), new Vector3(2.15f, 0.16f, 0.18f), concrete);
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

        float pulseA = Mathf.PerlinNoise(Time.time * 2.7f, 0.23f);
        float pulseB = Mathf.PerlinNoise(5.1f, Time.time * 4.1f);
        float flicker = 0.82f + pulseA * 0.25f + pulseB * 0.10f;

        if (fireBarrelCoreLight != null)
        {
            fireBarrelCoreLight.intensity = 10.0f * flicker;
            fireBarrelCoreLight.range = 3.7f + pulseB * 0.8f;
        }

        if (fireBarrelPoolLight != null)
        {
            fireBarrelPoolLight.intensity = 6.2f * (0.88f + pulseA * 0.28f);
            fireBarrelPoolLight.range = 7.4f + pulseB * 1.35f;
        }

        if (fireBarrelFlickerLightA != null)
        {
            float sideA = Mathf.PerlinNoise(Time.time * 6.3f, 9.1f);
            fireBarrelFlickerLightA.transform.localPosition = new Vector3(0.38f + sideA * 0.22f, 0.86f + sideA * 0.24f, -0.28f);
            fireBarrelFlickerLightA.intensity = 2.0f + sideA * 4.2f;
            fireBarrelFlickerLightA.range = 1.7f + sideA * 1.1f;
        }

        if (fireBarrelFlickerLightB != null)
        {
            float sideB = Mathf.PerlinNoise(11.7f, Time.time * 7.4f);
            fireBarrelFlickerLightB.transform.localPosition = new Vector3(-0.42f, 0.96f + sideB * 0.30f, 0.18f + sideB * 0.28f);
            fireBarrelFlickerLightB.intensity = 1.6f + sideB * 3.6f;
            fireBarrelFlickerLightB.range = 1.5f + sideB * 0.9f;
        }

        UpdateFireFlames(pulseA, pulseB);
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

