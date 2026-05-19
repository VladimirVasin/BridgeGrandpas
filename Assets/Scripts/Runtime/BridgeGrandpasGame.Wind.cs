using System.Collections.Generic;
using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private readonly List<WindParticleTarget> windParticleTargets = new List<WindParticleTarget>();
    private readonly List<WindSwayTarget> windSwayTargets = new List<WindSwayTarget>();
    private WindZone underpassWindZone;
    private ParticleSystem rainParticles;
    private float underpassWindStrength;
    private float underpassWindGust;
    private Vector3 underpassWindDirection = new Vector3(1f, 0.08f, -0.18f).normalized;

    private sealed class WindParticleTarget
    {
        public ParticleSystem Particles;
        public float Multiplier;
    }

    private sealed class WindSwayTarget
    {
        public Transform Target;
        public Quaternion BaseRotation;
        public Vector3 BasePosition;
        public float Angle;
        public float Shift;
        public float Phase;
    }

    private void SetupUnderpassWind()
    {
        if (worldRoot == null || underpassWindZone != null)
        {
            return;
        }

        GameObject windObject = new GameObject("Underpass Wind Zone");
        windObject.transform.SetParent(worldRoot, false);
        windObject.transform.localPosition = new Vector3(-7.5f, 1.4f, -1.6f);
        windObject.transform.rotation = Quaternion.LookRotation(underpassWindDirection, Vector3.up);
        underpassWindZone = windObject.AddComponent<WindZone>();
        underpassWindZone.mode = WindZoneMode.Directional;
        underpassWindZone.windMain = 0.18f;
        underpassWindZone.windTurbulence = 0.55f;
        underpassWindZone.windPulseMagnitude = 0.36f;
        underpassWindZone.windPulseFrequency = 0.18f;
        CreateWindTraceParticles();
    }

    private void UpdateUnderpassWind(float deltaTime)
    {
        if (underpassWindZone == null)
        {
            return;
        }

        float slow = Mathf.PerlinNoise(Time.time * 0.045f, 21.7f);
        float gust = Mathf.PerlinNoise(9.3f, Time.time * 0.30f);
        underpassWindGust = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.52f, 0.92f, gust));
        underpassWindStrength = 0.18f + slow * 0.26f + underpassWindGust * 0.74f;

        float yaw = -14f + Mathf.Sin(Time.time * 0.11f) * 7f + underpassWindGust * 12f;
        underpassWindDirection = Quaternion.Euler(0f, yaw, 0f) * new Vector3(1f, 0.06f, -0.18f).normalized;
        underpassWindZone.transform.rotation = Quaternion.LookRotation(underpassWindDirection, Vector3.up);
        underpassWindZone.windMain = underpassWindStrength;
        underpassWindZone.windTurbulence = 0.46f + underpassWindGust * 0.52f;
        underpassWindZone.windPulseMagnitude = 0.22f + underpassWindGust * 0.55f;
        UpdateRainWind();
        UpdateWindParticles();
        UpdateWindSway(deltaTime);
    }

    private void RegisterWindParticleSystem(ParticleSystem particles, float multiplier)
    {
        if (particles == null)
        {
            return;
        }

        ParticleSystem.ExternalForcesModule forces = particles.externalForces;
        forces.enabled = true;
        forces.multiplier = multiplier;
        windParticleTargets.Add(new WindParticleTarget
        {
            Particles = particles,
            Multiplier = multiplier
        });
    }

    private void RegisterRainWind(ParticleSystem particles)
    {
        rainParticles = particles;
        RegisterWindParticleSystem(particles, 1.55f);
    }

    private void RegisterWindSway(Transform target, float angle, float shift)
    {
        if (target == null)
        {
            return;
        }

        windSwayTargets.Add(new WindSwayTarget
        {
            Target = target,
            BaseRotation = target.localRotation,
            BasePosition = target.localPosition,
            Angle = angle,
            Shift = shift,
            Phase = Random.Range(0f, 20f)
        });
    }

    private void UpdateWindParticles()
    {
        for (int i = windParticleTargets.Count - 1; i >= 0; i--)
        {
            WindParticleTarget target = windParticleTargets[i];
            if (target.Particles == null)
            {
                windParticleTargets.RemoveAt(i);
                continue;
            }

            ParticleSystem.ExternalForcesModule forces = target.Particles.externalForces;
            forces.enabled = true;
            forces.multiplier = target.Multiplier * (0.55f + underpassWindStrength * 1.15f);
        }
    }

    private void UpdateRainWind()
    {
        if (rainParticles == null)
        {
            return;
        }

        ParticleSystem.VelocityOverLifetimeModule velocity = rainParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        float gustLean = underpassWindStrength * 2.65f + underpassWindGust * 1.95f;
        velocity.x = -0.45f + underpassWindDirection.x * gustLean;
        velocity.y = -5.15f - underpassWindGust * 0.38f;
        velocity.z = underpassWindDirection.z * gustLean * 0.72f;
    }

    private void UpdateWindSway(float deltaTime)
    {
        for (int i = windSwayTargets.Count - 1; i >= 0; i--)
        {
            WindSwayTarget target = windSwayTargets[i];
            if (target.Target == null)
            {
                windSwayTargets.RemoveAt(i);
                continue;
            }

            float wave = Mathf.Sin(Time.time * (1.4f + underpassWindGust * 2.1f) + target.Phase);
            float noise = Mathf.PerlinNoise(target.Phase, Time.time * 0.62f) - 0.5f;
            float power = underpassWindStrength + underpassWindGust * 0.35f;
            float angle = (wave + noise) * target.Angle * power;
            Vector3 shift = underpassWindDirection * (target.Shift * power * (0.55f + wave * 0.45f));
            target.Target.localRotation = target.BaseRotation * Quaternion.Euler(0f, angle * 0.22f, -angle);
            target.Target.localPosition = Vector3.Lerp(target.Target.localPosition, target.BasePosition + shift, deltaTime * 3.5f);
        }
    }

    private float CurrentUnderpassWindAudioBoost()
    {
        return underpassWindZone == null ? 1f : 0.62f + underpassWindStrength * 1.25f + underpassWindGust * 0.42f;
    }

    private void CreateWindTraceParticles()
    {
        GameObject particleObject = new GameObject("Visible Underpass Draft");
        particleObject.transform.SetParent(worldRoot, false);
        particleObject.transform.localPosition = new Vector3(-1.5f, 0.72f, -1.05f);
        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.8f, 4.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.12f, 0.42f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.11f);
        main.startColor = new Color(0.74f, 0.82f, 0.88f, 0.16f);
        main.maxParticles = 90;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 14f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(11f, 0.9f, 2.4f);

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = 0.18f;
        velocity.y = 0.02f;
        velocity.z = -0.03f;

        ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
        renderer.material = ParticleMat("visible_underpass_draft_mat", new Color(0.64f, 0.76f, 0.88f, 0.12f));
        renderer.sortingOrder = 1;
        RegisterWindParticleSystem(particles, 1.18f);
        particles.Play();
    }
}
