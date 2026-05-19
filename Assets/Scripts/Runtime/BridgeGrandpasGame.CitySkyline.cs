using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private void CreateCitySkyline()
    {
        CreateSkylineSkyBackplate();
        CreateUpperSkylineLayer();
        CreateSkylineBackLayer();
        CreateSkylineMiddleLayer();
        CreateSkylineStreetLayer();
    }

    private void CreateSkylineSkyBackplate()
    {
        Material sky = Mat("skyline_night_backplate", new Color(0.004f, 0.009f, 0.018f));
        Material lowerSky = TransparentMat("skyline_lower_blue_haze", new Color(0.055f, 0.082f, 0.125f, 0.18f));
        Material topHaze = TransparentMat("skyline_top_mist", new Color(0.035f, 0.050f, 0.078f, 0.13f));
        CreateDecorBox("Night sky backplate", new Vector3(0f, 5.65f, 13.05f), new Vector3(36f, 6.4f, 0.05f), sky);
        CreateDecorBox("Blue city haze upper", new Vector3(0f, 4.15f, 12.78f), new Vector3(34f, 1.15f, 0.04f), lowerSky);
        CreateDecorBox("Blue city haze top", new Vector3(0f, 6.45f, 12.72f), new Vector3(34f, 1.55f, 0.04f), topHaze);
        CreateSkylineSkyDots();
    }

    private void CreateSkylineSkyDots()
    {
        for (int i = 0; i < 24; i++)
        {
            float x = Random.Range(-16.5f, 16.5f);
            float y = Random.Range(4.65f, 8.15f);
            float z = Random.Range(12.55f, 12.68f);
            float size = Random.Range(0.018f, 0.045f);
            Color color = new Color(0.25f, 0.42f, 0.78f);
            CreateDecorBox("Cold sky speck " + i, new Vector3(x, y, z), new Vector3(size, size, 0.012f), EmissiveMat("cold_sky_speck_" + i, color, 0.10f));
        }
    }

    private void CreateUpperSkylineLayer()
    {
        CreateUpperSkylineBuilding("Upper micro block west edge", -14.2f, 11.7f, 1.0f, 1.55f, 0.48f, 4, 2, 0.11f);
        CreateUpperSkylineBuilding("Upper micro panel west", -12.55f, 11.45f, 1.35f, 2.05f, 0.52f, 5, 3, 0.12f);
        CreateUpperSkylineBuilding("Upper micro tower west", -10.35f, 11.95f, 0.8f, 2.45f, 0.42f, 6, 2, 0.10f);
        CreateUpperSkylineBuilding("Upper distant slab west", -8.45f, 11.55f, 1.65f, 1.62f, 0.5f, 4, 4, 0.09f);
        CreateUpperSkylineBuilding("Upper tiny offices west", -5.95f, 12.15f, 1.25f, 2.18f, 0.46f, 5, 3, 0.10f);
        CreateUpperSkylineBuilding("Upper sleeping ridge", -3.85f, 11.65f, 1.75f, 1.85f, 0.52f, 4, 4, 0.12f);
        CreateUpperSkylineBuilding("Upper thin tower center", -1.25f, 12.05f, 0.75f, 2.65f, 0.42f, 6, 2, 0.08f);
        CreateUpperSkylineBuilding("Upper municipal speck", 0.45f, 11.62f, 1.42f, 1.72f, 0.48f, 4, 3, 0.10f);
        CreateUpperSkylineBuilding("Upper block center east", 2.65f, 11.88f, 1.18f, 2.28f, 0.46f, 5, 3, 0.09f);
        CreateUpperSkylineBuilding("Upper slab east", 4.55f, 11.50f, 1.78f, 1.58f, 0.54f, 4, 4, 0.11f);
        CreateUpperSkylineBuilding("Upper narrow east", 6.95f, 12.05f, 0.9f, 2.35f, 0.44f, 6, 2, 0.08f);
        CreateUpperSkylineBuilding("Upper office east", 8.82f, 11.68f, 1.45f, 1.95f, 0.5f, 5, 3, 0.10f);
        CreateUpperSkylineBuilding("Upper far house east", 11.05f, 11.45f, 1.6f, 1.48f, 0.5f, 4, 4, 0.09f);
        CreateUpperSkylineBuilding("Upper micro block east edge", 13.25f, 11.86f, 1.05f, 2.08f, 0.45f, 5, 2, 0.10f);
    }

    private void CreateSkylineBackLayer()
    {
        CreateSkylineBuilding("Far tower block west", -12.8f, 8.4f, 1.75f, 4.8f, 0.68f, 9, 3, 0.22f);
        CreateSkylineBuilding("Far slab block west", -9.8f, 8.15f, 2.45f, 3.6f, 0.72f, 7, 5, 0.18f);
        CreateSkylineBuilding("Far narrow block", -6.4f, 8.55f, 1.25f, 5.7f, 0.62f, 11, 2, 0.20f);
        CreateSkylineBuilding("Far office mass", -3.35f, 8.25f, 2.8f, 4.25f, 0.76f, 8, 5, 0.16f);
        CreateSkylineBuilding("Far sleeping house", 0.25f, 8.7f, 2.2f, 5.15f, 0.66f, 10, 4, 0.24f);
        CreateSkylineBuilding("Far factory silhouette", 3.85f, 8.35f, 3.1f, 3.35f, 0.82f, 6, 6, 0.14f);
        CreateSkylineBuilding("Far tower block east", 7.45f, 8.65f, 1.65f, 5.9f, 0.66f, 11, 3, 0.18f);
        CreateSkylineBuilding("Far slab block east", 10.75f, 8.2f, 2.7f, 4.05f, 0.78f, 8, 5, 0.20f);
        CreateSkylineBuilding("Far edge tower", 13.6f, 8.45f, 1.35f, 4.7f, 0.62f, 9, 2, 0.16f);
    }

    private void CreateSkylineMiddleLayer()
    {
        CreateSkylineBuilding("Mid panel house west", -11.0f, 6.55f, 2.2f, 3.95f, 0.82f, 7, 4, 0.31f);
        CreateSkylineBuilding("Mid old office west", -7.8f, 6.35f, 2.0f, 3.15f, 0.86f, 6, 4, 0.26f);
        CreateSkylineBuilding("Mid brick tower", -4.8f, 6.70f, 1.55f, 4.65f, 0.78f, 9, 3, 0.28f);
        CreateSkylineBuilding("Mid municipal block", -1.35f, 6.50f, 3.1f, 3.85f, 0.92f, 7, 6, 0.23f);
        CreateSkylineBuilding("Mid sleeping tower", 2.55f, 6.65f, 1.75f, 4.95f, 0.78f, 10, 3, 0.30f);
        CreateSkylineBuilding("Mid factory house", 5.9f, 6.38f, 2.65f, 3.25f, 0.95f, 6, 5, 0.21f);
        CreateSkylineBuilding("Mid right block", 9.25f, 6.55f, 2.35f, 4.25f, 0.86f, 8, 4, 0.27f);
    }

    private void CreateSkylineStreetLayer()
    {
        Material road = AsphaltMat("distant_city_wet_road", new Color(0.055f, 0.065f, 0.074f), new Vector2(18f, 1.8f), 0.58f);
        Material glow = TransparentMat("distant_lamp_pool", new Color(1f, 0.55f, 0.20f, 0.22f));
        CreateDecorBox("Distant skyline road", new Vector3(0f, 0.018f, 6.05f), new Vector3(31f, 0.025f, 0.95f), road);

        for (int i = 0; i < 6; i++)
        {
            float x = -11.6f + i * 4.65f;
            CreateSkylineLamp(x, 5.72f, i % 2 == 0 ? 1f : -1f);
            CreateDecorBox("Distant lamp reflection " + i, new Vector3(x + 0.25f, 0.026f, 5.48f), new Vector3(0.7f, 0.012f, 0.16f), glow);
        }

        CreateTrafficGlow(new Vector3(-14.0f, 0.35f, 5.68f), new Vector3(14.0f, 0.35f, 5.68f), new Color(1f, 0.83f, 0.45f), 0.026f);
        CreateTrafficGlow(new Vector3(14.0f, 0.42f, 5.92f), new Vector3(-14.0f, 0.42f, 5.92f), new Color(1f, 0.16f, 0.10f), 0.022f);
    }

    private void CreateSkylineBuilding(string name, float x, float z, float width, float height, float depth, int floors, int columns, float litChance)
    {
        float tone = Mathf.InverseLerp(3.0f, 6.0f, height);
        Color bodyColor = Color.Lerp(new Color(0.018f, 0.024f, 0.034f), new Color(0.048f, 0.058f, 0.074f), tone);
        Material body = Mat(name + "_body", bodyColor);
        Material dark = Mat(name + "_roof_dark", new Color(0.012f, 0.016f, 0.022f));
        CreateDecorBox(name, new Vector3(x, height * 0.5f, z), new Vector3(width, height, depth), body);
        CreateDecorBox(name + " roof cap", new Vector3(x, height + 0.08f, z), new Vector3(width + 0.16f, 0.16f, depth + 0.08f), dark);

        if (columns > 3)
        {
            CreateDecorBox(name + " roof shack", new Vector3(x - width * 0.23f, height + 0.34f, z - 0.06f), new Vector3(width * 0.30f, 0.38f, depth * 0.42f), body);
        }

        if (height > 4.2f)
        {
            CreateDecorBox(name + " antenna", new Vector3(x + width * 0.30f, height + 0.62f, z - depth * 0.12f), new Vector3(0.035f, 0.95f, 0.035f), dark);
        }

        if (Random.value > 0.56f)
        {
            Material tank = Mat(name + "_water_tank", new Color(0.050f, 0.056f, 0.060f));
            CreateCylinderWithoutCollider(name + " roof tank", cityAmbienceRoot, new Vector3(x + width * 0.18f, height + 0.42f, z + depth * 0.06f), new Vector3(0.17f, 0.17f, 0.28f), tank);
        }

        CreateSkylineWindows(name, x, z - depth * 0.53f, width, height, floors, columns, litChance);
    }

    private void CreateUpperSkylineBuilding(string name, float x, float z, float width, float height, float depth, int floors, int columns, float litChance)
    {
        float baseY = 3.85f + Random.Range(-0.12f, 0.12f);
        float tone = Mathf.InverseLerp(1.3f, 2.7f, height);
        Color bodyColor = Color.Lerp(new Color(0.006f, 0.012f, 0.022f), new Color(0.020f, 0.031f, 0.048f), tone);
        Material body = Mat(name + "_upper_body", bodyColor);
        Material dark = Mat(name + "_upper_roof", new Color(0.004f, 0.008f, 0.014f));
        CreateDecorBox(name, new Vector3(x, baseY + height * 0.5f, z), new Vector3(width, height, depth), body);
        CreateDecorBox(name + " top line", new Vector3(x, baseY + height + 0.045f, z), new Vector3(width + 0.08f, 0.09f, depth + 0.04f), dark);

        if (height > 2.1f)
        {
            CreateDecorBox(name + " tiny antenna", new Vector3(x + width * 0.28f, baseY + height + 0.34f, z - depth * 0.08f), new Vector3(0.018f, 0.58f, 0.018f), dark);
            CreateCityWindow(new Vector3(x + width * 0.28f, baseY + height + 0.66f, z - depth * 0.12f), new Vector3(0.045f, 0.045f, 0.018f), new Color(1f, 0.12f, 0.08f));
        }

        CreateUpperSkylineWindows(name, x, z - depth * 0.54f, baseY, width, height, floors, columns, litChance);
    }

    private void CreateSkylineWindows(string name, float x, float frontZ, float width, float height, int floors, int columns, float litChance)
    {
        float xStep = width / (columns + 1f);
        float yStep = Mathf.Max(0.24f, (height - 0.65f) / Mathf.Max(1, floors));
        for (int floor = 0; floor < floors; floor++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (Random.value > litChance)
                {
                    continue;
                }

                float wx = x - width * 0.5f + xStep * (col + 1f) + Random.Range(-0.025f, 0.025f);
                float wy = 0.42f + yStep * floor + Random.Range(-0.018f, 0.018f);
                Color color = SkylineWindowColor(floor, col);
                Vector3 scale = new Vector3(Random.Range(0.075f, 0.115f), Random.Range(0.085f, 0.14f), 0.024f);
                CreateCityWindow(new Vector3(wx, wy, frontZ), scale, color);
            }
        }

        if (Random.value > 0.45f)
        {
            Material stairLight = EmissiveMat(name + "_stair_light", new Color(0.55f, 0.68f, 1f), 0.22f);
            CreateDecorBox(name + " cold stairwell", new Vector3(x + width * 0.36f, height * 0.47f, frontZ - 0.01f), new Vector3(0.055f, height * 0.58f, 0.026f), stairLight);
        }
    }

    private void CreateUpperSkylineWindows(string name, float x, float frontZ, float baseY, float width, float height, int floors, int columns, float litChance)
    {
        float xStep = width / (columns + 1f);
        float yStep = Mathf.Max(0.20f, (height - 0.32f) / Mathf.Max(1, floors));
        for (int floor = 0; floor < floors; floor++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (Random.value > litChance)
                {
                    continue;
                }

                float wx = x - width * 0.5f + xStep * (col + 1f) + Random.Range(-0.012f, 0.012f);
                float wy = baseY + 0.22f + yStep * floor + Random.Range(-0.01f, 0.012f);
                Color color = UpperSkylineWindowColor(floor, col);
                Vector3 scale = new Vector3(Random.Range(0.045f, 0.075f), Random.Range(0.050f, 0.085f), 0.018f);
                CreateCityWindow(new Vector3(wx, wy, frontZ), scale, color);
            }
        }

        if (Random.value > 0.72f)
        {
            Material stairLight = EmissiveMat(name + "_upper_stair_light", new Color(0.42f, 0.58f, 0.90f), 0.10f);
            CreateDecorBox(name + " tiny cold stairwell", new Vector3(x + width * 0.34f, baseY + height * 0.48f, frontZ - 0.01f), new Vector3(0.026f, height * 0.50f, 0.018f), stairLight);
        }
    }

    private Color SkylineWindowColor(int floor, int col)
    {
        int variant = Mathf.Abs(floor * 7 + col * 11) % 9;
        if (variant == 0)
        {
            return new Color(0.58f, 0.70f, 1f);
        }

        if (variant == 1)
        {
            return new Color(1f, 0.33f, 0.16f);
        }

        return new Color(1f, Random.Range(0.48f, 0.64f), Random.Range(0.20f, 0.34f));
    }

    private Color UpperSkylineWindowColor(int floor, int col)
    {
        int variant = Mathf.Abs(floor * 5 + col * 13) % 8;
        if (variant == 0)
        {
            return new Color(0.40f, 0.55f, 0.90f);
        }

        if (variant == 1)
        {
            return new Color(1f, 0.22f, 0.12f);
        }

        return new Color(0.92f, Random.Range(0.42f, 0.53f), Random.Range(0.18f, 0.28f));
    }

    private void CreateSkylineLamp(float x, float z, float side)
    {
        Material metal = Mat("skyline_lamp_metal", new Color(0.055f, 0.056f, 0.052f));
        Transform root = CreateDecorRoot("Skyline street lamp", new Vector3(x, 0f, z), 0f);
        CreateCylinderWithoutCollider("Skyline lamp pole", root, new Vector3(0f, 0.72f, 0f), new Vector3(0.028f, 0.028f, 1.42f), metal);
        CreateBoxWithoutCollider("Skyline lamp arm", root, new Vector3(side * 0.18f, 1.38f, 0f), new Vector3(0.36f, 0.028f, 0.028f), metal);
        CreateBoxWithoutCollider("Skyline lamp head", root, new Vector3(side * 0.37f, 1.33f, 0f), new Vector3(0.13f, 0.07f, 0.09f), EmissiveMat("skyline_lamp_head", new Color(1f, 0.55f, 0.22f), 0.45f));
        Light light = AddPointLight(root, "Skyline lamp light", new Vector3(side * 0.37f, 1.28f, 0f), new Color(1f, 0.54f, 0.22f), 1.65f, 0.10f);
        light.shadows = LightShadows.None;
        cityLamps.Add(new CityLampGlow { Light = light, BaseIntensity = light.intensity, Phase = Random.Range(0f, 10f) });
    }
}
