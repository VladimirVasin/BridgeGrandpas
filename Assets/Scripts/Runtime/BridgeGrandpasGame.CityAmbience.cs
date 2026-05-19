using System.Collections.Generic;
using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const int CityAmbienceSeed = 23101998;
    private const float CityWindowUpdateInterval = 0.12f;
    private readonly List<CityWindowGlow> cityWindows = new List<CityWindowGlow>();
    private readonly List<CityTrafficGlow> cityTraffic = new List<CityTrafficGlow>();
    private readonly List<CityLampGlow> cityLamps = new List<CityLampGlow>();
    private Transform cityAmbienceRoot;
    private float nextCityWindowUpdateAt;

    private sealed class CityWindowGlow
    {
        public Material Material;
        public Color Color;
        public float Phase;
        public float Speed;
    }

    private sealed class CityTrafficGlow
    {
        public Transform Transform;
        public Material Material;
        public Vector3 Start;
        public Vector3 End;
        public Color Color;
        public float Phase;
        public float Speed;
    }

    private sealed class CityLampGlow
    {
        public Light Light;
        public float BaseIntensity;
        public float Phase;
    }

    private void CreateCityAmbience()
    {
        if (worldRoot == null || cityAmbienceRoot != null)
        {
            return;
        }

        cityAmbienceRoot = new GameObject("Distant City Ambience").transform;
        cityAmbienceRoot.SetParent(worldRoot, false);
        cityWindows.Clear();
        cityTraffic.Clear();
        cityLamps.Clear();

        Random.State randomState = Random.state;
        Random.InitState(CityAmbienceSeed);
        CreateCityGroundBands();
        CreateCitySkyline();
        CreateCityPrefabRing();
        CreateCityWindowField();
        CreateCityTraffic();
        CreateCitySmoke();
        Random.state = randomState;
    }

    private void CreateCityGroundBands()
    {
        Material wall = Mat("city_retaining_wall_deep", new Color(0.030f, 0.035f, 0.043f));
        Material curb = Mat("city_curb_damp", new Color(0.115f, 0.125f, 0.13f));

        CreateDecorBox("Back upper wall cap", new Vector3(0f, 1.85f, 4.25f), new Vector3(28f, 1.2f, 0.28f), wall);
        CreateDecorBox("Far wet road band", new Vector3(0f, 0.02f, 5.25f), new Vector3(30f, 0.035f, 1.35f), curb);

        for (int i = 0; i < 12; i++)
        {
            float x = -10.45f + i * 1.9f;
            CreateDecorBox("Back curb slab " + i, new Vector3(x, 0.02f, 3.92f), new Vector3(1.35f, 0.08f, 0.18f), curb);
        }
    }

    private void CreateCityPrefabRing()
    {
        CreateCitySupport(-8.8f);
        CreateCitySupport(8.8f);
        CreateCityBench(new Vector3(-6.55f, 0.02f, -2.8f), 28f);
        CreateCityConcreteBlock(new Vector3(6.45f, 0.06f, -2.65f), -18f);
        CreateCityLampPost(new Vector3(-8.9f, 0f, -0.9f), 8f);
        CreateCityLampPost(new Vector3(8.9f, 0f, -0.35f), -10f);
        CreateCityWallLamp(new Vector3(-7.7f, 1.45f, 4.02f));
        CreateCityWallLamp(new Vector3(7.3f, 1.35f, 4.02f));

        CreateCityBuildingBlock("Distant office block A", -10.0f, 5.95f, 1.5f, 2.9f, 0.65f);
        CreateCityBuildingBlock("Distant factory block", -7.45f, 6.15f, 1.75f, 2.05f, 0.7f);
        CreateCityBuildingBlock("Distant house block A", -4.8f, 5.75f, 1.15f, 1.75f, 0.55f);
        CreateCityBuildingBlock("Distant office block B", -1.7f, 6.05f, 1.55f, 2.65f, 0.66f);
        CreateCityBuildingBlock("Distant factory block B", 1.4f, 6.2f, 1.85f, 2.25f, 0.72f);
        CreateCityBuildingBlock("Distant house block B", 4.45f, 5.8f, 1.25f, 1.85f, 0.57f);
        CreateCityBuildingBlock("Distant tower block", 7.55f, 6.08f, 1.45f, 3.25f, 0.62f);
        CreateCityBuildingBlock("Distant narrow block", 10.2f, 5.82f, 1.05f, 2.15f, 0.55f);
        CreateCityChimney(new Vector3(4.15f, 0f, 4.95f));
    }

    private void CreateCitySupport(float x)
    {
        Material concrete = Mat("city_support_concrete", new Color(0.085f, 0.090f, 0.10f));
        CreateDecorBox("Distant highway support", new Vector3(x, 1.4f, 2.9f), new Vector3(0.42f, 2.8f, 0.38f), concrete);
        GameObject brace = CreateDecorBox("Distant highway brace", new Vector3(x * 0.98f, 2.25f, 3.15f), new Vector3(0.22f, 1.4f, 0.34f), concrete);
        brace.transform.localRotation = Quaternion.Euler(0f, 0f, x < 0f ? -18f : 18f);
    }

    private void CreateCityBench(Vector3 position, float yaw)
    {
        Material wood = Mat("city_bench_wood", new Color(0.16f, 0.12f, 0.09f));
        Material metal = Mat("city_bench_metal", new Color(0.08f, 0.09f, 0.10f));
        Transform root = CreateDecorRoot("Distant bench", position, yaw);
        CreateBoxWithoutCollider("Bench seat", root, new Vector3(0f, 0.32f, 0f), new Vector3(1.05f, 0.10f, 0.26f), wood);
        CreateBoxWithoutCollider("Bench back", root, new Vector3(0f, 0.58f, 0.13f), new Vector3(1.05f, 0.12f, 0.08f), wood);
        CreateBoxWithoutCollider("Bench leg L", root, new Vector3(-0.38f, 0.18f, 0f), new Vector3(0.08f, 0.28f, 0.08f), metal);
        CreateBoxWithoutCollider("Bench leg R", root, new Vector3(0.38f, 0.18f, 0f), new Vector3(0.08f, 0.28f, 0.08f), metal);
    }

    private void CreateCityConcreteBlock(Vector3 position, float yaw)
    {
        Material concrete = Mat("city_concrete_block", new Color(0.12f, 0.13f, 0.14f));
        GameObject block = CreateDecorBox("Distant concrete block", position, new Vector3(0.8f, 0.32f, 0.45f), concrete);
        block.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
    }

    private void CreateCityLampPost(Vector3 position, float yaw)
    {
        Material metal = Mat("city_lamp_post", new Color(0.10f, 0.10f, 0.095f));
        Transform root = CreateDecorRoot("Distant lamp post", position, yaw);
        CreateCylinderWithoutCollider("Lamp pole", root, new Vector3(0f, 0.72f, 0f), new Vector3(0.035f, 0.035f, 1.45f), metal);
        CreateBoxWithoutCollider("Lamp arm", root, new Vector3(0.18f, 1.42f, 0f), new Vector3(0.38f, 0.035f, 0.035f), metal);
        Light light = AddPointLight(root, "Distant lamp glow", new Vector3(0.38f, 1.36f, 0f), new Color(1f, 0.56f, 0.24f), 2.1f, 0.18f);
        light.shadows = LightShadows.None;
        cityLamps.Add(new CityLampGlow { Light = light, BaseIntensity = light.intensity, Phase = Random.Range(0f, 10f) });
    }

    private void CreateCityWallLamp(Vector3 position)
    {
        Material frame = Mat("city_wall_lamp_frame", new Color(0.11f, 0.09f, 0.07f));
        CreateDecorBox("Distant wall lamp frame", position, new Vector3(0.18f, 0.12f, 0.06f), frame);
        Light light = AddPointLight(cityAmbienceRoot, "Distant wall lamp glow", position + new Vector3(0f, -0.04f, -0.06f), new Color(1f, 0.52f, 0.20f), 1.7f, 0.13f);
        light.shadows = LightShadows.None;
        cityLamps.Add(new CityLampGlow { Light = light, BaseIntensity = light.intensity, Phase = Random.Range(0f, 10f) });
    }

    private void CreateCityBuildingBlock(string name, float x, float z, float width, float height, float depth)
    {
        Color color = new Color(0.036f + height * 0.004f, 0.043f + height * 0.004f, 0.055f + height * 0.005f);
        Material building = Mat(name + "_mat", color);
        CreateDecorBox(name, new Vector3(x, height * 0.5f, z), new Vector3(width, height, depth), building);
    }

    private void CreateCityChimney(Vector3 position)
    {
        Material brick = Mat("city_chimney_brick", new Color(0.15f, 0.09f, 0.065f));
        CreateCylinderWithoutCollider("Distant chimney", cityAmbienceRoot, position + new Vector3(0f, 0.9f, 0f), new Vector3(0.16f, 0.16f, 1.8f), brick);
    }

    private void CreateCityWindowField()
    {
        Color warm = new Color(1f, 0.50f, 0.20f);
        for (int i = 0; i < 42; i++)
        {
            float x = -11.4f + (i % 21) * 1.08f;
            float y = 1.15f + (i % 4) * 0.38f + Random.Range(-0.05f, 0.06f);
            float z = 4.05f + (i / 14) * 0.12f;
            if (Random.value < 0.22f)
            {
                continue;
            }

            CreateCityWindow(new Vector3(x, y, z), new Vector3(0.12f, 0.08f, 0.028f), warm);
        }
    }

    private void CreateCityWindow(Vector3 position, Vector3 scale, Color color)
    {
        Material material = EmissiveMat("city_window_" + cityWindows.Count, color, 0.24f);
        GameObject window = CreateDecorBox("Distant window", position, scale, material);
        cityWindows.Add(new CityWindowGlow
        {
            Material = window.GetComponent<Renderer>().sharedMaterial,
            Color = color,
            Phase = Random.Range(0f, 20f),
            Speed = Random.Range(0.18f, 0.52f)
        });
    }

    private void CreateCityTraffic()
    {
        CreateTrafficGlow(new Vector3(-12.5f, 3.55f, 0.92f), new Vector3(12.5f, 3.55f, 0.92f), new Color(1f, 0.86f, 0.55f), 0.045f);
        CreateTrafficGlow(new Vector3(12.5f, 3.58f, 2.26f), new Vector3(-12.5f, 3.58f, 2.26f), new Color(1f, 0.18f, 0.11f), 0.038f);
        CreateTrafficGlow(new Vector3(-11.2f, 3.62f, 2.62f), new Vector3(11.4f, 3.62f, 2.62f), new Color(1f, 0.72f, 0.24f), 0.031f);
    }

    private void CreateTrafficGlow(Vector3 start, Vector3 end, Color color, float speed)
    {
        Material material = EmissiveMat("city_traffic_" + cityTraffic.Count, color, 1.2f);
        GameObject glow = CreateDecorBox("Distant traffic glow", start, new Vector3(0.12f, 0.045f, 0.045f), material);
        cityTraffic.Add(new CityTrafficGlow
        {
            Transform = glow.transform,
            Material = material,
            Start = start,
            End = end,
            Color = color,
            Phase = Random.Range(0f, 1f),
            Speed = speed
        });
    }

    private void CreateCitySmoke()
    {
        GameObject anchor = new GameObject("Distant chimney haze");
        anchor.transform.SetParent(cityAmbienceRoot, false);
        anchor.transform.localPosition = new Vector3(4.15f, 2.2f, 4.95f);
        AddSmokeOrSteam(anchor.transform, "Distant city smoke", new Color(0.32f, 0.34f, 0.38f, 0.25f), 5f, 4.8f);
    }

    private GameObject CreateDecorBox(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject box = CreateBox(name, cityAmbienceRoot, position, scale, material);
        Collider collider = box.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        box.layer = 2;
        return box;
    }

    private Transform CreateDecorRoot(string name, Vector3 position, float yaw)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(cityAmbienceRoot, false);
        root.transform.localPosition = position;
        root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        root.layer = 2;
        return root.transform;
    }

    private GameObject CreateBoxWithoutCollider(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject box = CreateBox(name, parent, position, scale, material);
        Collider collider = box.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        box.layer = 2;
        return box;
    }

    private GameObject CreateCylinderWithoutCollider(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cylinder = CreateCylinder(name, parent, position, scale, material);
        Collider collider = cylinder.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        cylinder.layer = 2;
        return cylinder;
    }

    private void UpdateCityAmbience(float deltaTime)
    {
        if (cityAmbienceRoot == null)
        {
            return;
        }

        UpdateCityWindows();
        UpdateCityTraffic();
        UpdateCityLamps();
    }

    private void UpdateCityWindows()
    {
        if (Time.time < nextCityWindowUpdateAt)
        {
            return;
        }

        nextCityWindowUpdateAt = Time.time + CityWindowUpdateInterval;
        for (int i = 0; i < cityWindows.Count; i++)
        {
            CityWindowGlow window = cityWindows[i];
            float pulse = 0.18f + Mathf.PerlinNoise(window.Phase, Time.time * window.Speed) * 0.42f;
            window.Material.SetColor("_EmissionColor", window.Color * pulse);
        }
    }

    private void UpdateCityTraffic()
    {
        for (int i = 0; i < cityTraffic.Count; i++)
        {
            CityTrafficGlow traffic = cityTraffic[i];
            float t = Mathf.Repeat(Time.time * traffic.Speed + traffic.Phase, 1f);
            traffic.Transform.localPosition = Vector3.Lerp(traffic.Start, traffic.End, t);
            float pulse = 0.9f + Mathf.Sin(Time.time * 7f + traffic.Phase * 12f) * 0.16f;
            traffic.Transform.localScale = new Vector3(0.12f * pulse, 0.045f, 0.045f);
            traffic.Material.SetColor("_EmissionColor", traffic.Color * (0.9f + pulse * 0.35f));
        }
    }

    private void UpdateCityLamps()
    {
        for (int i = 0; i < cityLamps.Count; i++)
        {
            CityLampGlow lamp = cityLamps[i];
            if (lamp.Light == null)
            {
                continue;
            }

            float pulse = 0.88f + Mathf.PerlinNoise(lamp.Phase, Time.time * 0.7f) * 0.28f;
            lamp.Light.intensity = lamp.BaseIntensity * pulse;
        }
    }
}
