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
    private GameObject CreateGrandpaVisual(Grandpa grandpa, Vector3 position)
    {
        GameObject root = new GameObject(grandpa.Name);
        root.transform.SetParent(settlementRoot, false);
        root.transform.position = position;

        Color coatColor = RoleColor(grandpa.Role);
        Material coat = Mat("coat_" + grandpa.Role, coatColor);
        Material skin = Mat("skin", new Color(0.79f, 0.62f, 0.48f));
        Material beard = Mat("beard", new Color(0.82f, 0.82f, 0.76f));
        Material dark = Mat("grandpa_dark", new Color(0.09f, 0.08f, 0.08f));
        Material scarf = Mat("scarf_" + grandpa.Role, Color.Lerp(coatColor, Color.white, 0.35f));

        bool imported = TryCreateImportedGrandpaVisual(grandpa, root.transform);
        if (!imported)
        {
            GameObject body = CreateCylinder("Body", root.transform, new Vector3(0f, 0.42f, 0f), new Vector3(0.42f, 0.42f, 0.62f), coat);
            grandpa.Body = body.transform;
            GameObject head = CreateSphere("Head", root.transform, new Vector3(0f, 0.93f, 0f), new Vector3(0.38f, 0.34f, 0.36f), skin);
            GameObject beardPart = CreateBox("Beard", root.transform, new Vector3(0f, 0.78f, -0.24f), new Vector3(0.34f, 0.22f, 0.08f), beard);
            GameObject hat = CreateCylinder("Hat", root.transform, new Vector3(0f, 1.22f, 0f), new Vector3(0.34f, 0.34f, 0.18f), dark);
            CreateBox("Scarf", root.transform, new Vector3(0f, 0.68f, -0.27f), new Vector3(0.48f, 0.08f, 0.08f), scarf);
            Transform leftArm = CreateBox("CTRL_ARM_L", root.transform, new Vector3(-0.34f, 0.52f, -0.02f), new Vector3(0.12f, 0.42f, 0.13f), coat).transform;
            Transform rightArm = CreateBox("CTRL_ARM_R", root.transform, new Vector3(0.34f, 0.52f, -0.02f), new Vector3(0.12f, 0.42f, 0.13f), coat).transform;
            Transform leftLeg = CreateBox("CTRL_LEG_L", root.transform, new Vector3(-0.14f, 0.13f, 0f), new Vector3(0.13f, 0.36f, 0.15f), dark).transform;
            Transform rightLeg = CreateBox("CTRL_LEG_R", root.transform, new Vector3(0.14f, 0.13f, 0f), new Vector3(0.13f, 0.36f, 0.15f), dark).transform;
            CacheProceduralGrandpaParts(grandpa, head.transform, hat.transform, beardPart.transform, leftArm, rightArm, leftLeg, rightLeg);

            if (grandpa.Role == GrandpaRole.Cardboarder)
            {
                CreateBox("Carried Cardboard", root.transform, new Vector3(0.42f, 0.55f, 0.05f), new Vector3(0.08f, 0.42f, 0.36f), Mat("cardboard", new Color(0.58f, 0.40f, 0.22f)));
            }
            else if (grandpa.Role == GrandpaRole.RadioReceiver)
            {
                CreateBox("Antenna Radio", root.transform, new Vector3(0.35f, 0.95f, 0f), new Vector3(0.18f, 0.18f, 0.18f), Mat("radio", new Color(0.08f, 0.11f, 0.10f)));
                CreateBox("Antenna", root.transform, new Vector3(0.48f, 1.22f, 0f), new Vector3(0.025f, 0.48f, 0.025f), dark);
            }
        }

        GameObject prop = CreateBox("Interaction Prop", root.transform, new Vector3(0f, 0.62f, -0.42f), new Vector3(0.16f, 0.16f, 0.16f), Mat("tea_prop", new Color(0.95f, 0.78f, 0.36f)));
        grandpa.InteractionProp = prop.transform;
        grandpa.InteractionPropRenderer = prop.GetComponent<Renderer>();
        prop.SetActive(false);

        BridgeGrandpasSelectionTarget target = root.AddComponent<BridgeGrandpasSelectionTarget>();
        target.Kind = SelectionKind.Grandpa;
        target.GrandpaId = grandpa.Id;

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, 0.65f, 0f);
        collider.size = new Vector3(0.9f, 1.35f, 0.9f);
        AttachGrandpaFootstepSource(grandpa);

        GameObject thoughtObject = new GameObject("Thought Bubble");
        thoughtObject.transform.SetParent(root.transform, false);
        TextMesh textMesh = thoughtObject.AddComponent<TextMesh>();
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = 34;
        textMesh.characterSize = 0.045f;
        textMesh.color = new Color(1f, 0.95f, 0.78f);
        textMesh.text = "";
        thoughtObject.SetActive(false);
        grandpa.ThoughtText = textMesh;

        return root;
    }

    private void CacheProceduralGrandpaParts(
        Grandpa grandpa,
        Transform head,
        Transform hat,
        Transform beard,
        Transform leftArm,
        Transform rightArm,
        Transform leftLeg,
        Transform rightLeg)
    {
        grandpa.ImportedHead = head;
        grandpa.ImportedHat = hat;
        grandpa.ImportedBeard = beard;
        grandpa.ImportedBodyControl = grandpa.Body;
        grandpa.ImportedArms = new[] { leftArm, rightArm };
        grandpa.ImportedLegs = new[] { leftLeg, rightLeg };
        grandpa.ImportedArmPhaseSigns = new[] { 1f, -1f };
        grandpa.ImportedLegPhaseSigns = new[] { 1f, -1f };
        grandpa.ImportedArmBaseRotations = CaptureBaseRotations(grandpa.ImportedArms);
        grandpa.ImportedLegBaseRotations = CaptureBaseRotations(grandpa.ImportedLegs);
        grandpa.ImportedArmBasePositions = CaptureBasePositions(grandpa.ImportedArms);
        grandpa.ImportedLegBasePositions = CaptureBasePositions(grandpa.ImportedLegs);
        grandpa.ImportedBodyBasePosition = grandpa.Body.localPosition;
        grandpa.ImportedBodyBaseRotation = grandpa.Body.localRotation;
        grandpa.ImportedHeadBaseRotation = head.localRotation;
        grandpa.ImportedHatBaseRotation = hat.localRotation;
        grandpa.ImportedBeardBaseRotation = beard.localRotation;
    }

    private GameObject CreateBuildingVisual(Building building)
    {
        GameObject root = new GameObject(building.Name);
        root.transform.SetParent(settlementRoot, false);
        root.transform.position = building.Position;

        BridgeGrandpasSelectionTarget target = root.AddComponent<BridgeGrandpasSelectionTarget>();
        target.Kind = SelectionKind.Building;
        target.BuildingTypeValue = (int)building.Type;

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, 0.45f, 0f);
        collider.size = new Vector3(1.6f, 1.1f, 1.6f);

        BuildBuildingParts(building, root.transform);
        return root;
    }

    private void RefreshBuildingVisual(Building building)
    {
        if (building.Root == null)
        {
            return;
        }

        ClearChildren(building.Root.transform);
        BuildBuildingParts(building, building.Root.transform);
    }

    private void BuildBuildingParts(Building building, Transform root)
    {
        ResetBuildingAnimationState(building);
        float levelScale = 1f + (building.Level - 1) * 0.12f;
        switch (building.Type)
        {
            case BuildingType.FireBarrel:
                CreateCylinder("Barrel", root, new Vector3(0f, 0.36f, 0f), new Vector3(0.58f, 0.58f, 0.58f), Mat("barrel", new Color(0.11f, 0.075f, 0.055f)));
                fireFlames = CreateFireFlames(root);
                float lightRange = FireBarrelLightRangeBoost(building.Level);
                fireBarrelCoreLight = AddPointLight(root, "Fire Core Light", new Vector3(0f, 0.96f, 0f), new Color(1f, 0.42f, 0.12f), 5.2f * lightRange, 6.2f);
                fireBarrelPoolLight = AddPointLight(root, "Fire Warm Pool Light", new Vector3(0f, 0.70f, 0f), new Color(1f, 0.50f, 0.18f), 8.2f * lightRange, 4.6f);
                fireBarrelFlickerLightA = AddPointLight(root, "Fire Side Flicker A", new Vector3(0.48f, 0.92f, -0.20f), new Color(1f, 0.30f, 0.06f), 2.4f * lightRange, 2.4f);
                fireBarrelFlickerLightB = AddPointLight(root, "Fire Side Flicker B", new Vector3(-0.34f, 1.02f, 0.28f), new Color(1f, 0.62f, 0.18f), 2.2f * lightRange, 2.1f);
                AddSmokeOrSteam(root, "Fire Sparks", new Color(1f, 0.38f, 0.05f, 0.82f), 46f, 1.05f);
                break;
            case BuildingType.Samovar:
                CreateBox("Samovar Table", root, new Vector3(0f, 0.18f, 0f), new Vector3(1.1f, 0.16f, 0.82f), Mat("wood", new Color(0.42f, 0.26f, 0.13f)));
                if (!TryCreateImportedSamovarVisual(building, root, levelScale))
                {
                    CreateCylinder("Samovar Body", root, new Vector3(0f, 0.46f, 0f), new Vector3(0.55f * levelScale, 0.55f * levelScale, 0.7f * levelScale), Mat("samovar_gold", new Color(0.86f, 0.62f, 0.24f)));
                    CreateSphere("Samovar Lid", root, new Vector3(0f, 0.86f * levelScale, 0f), new Vector3(0.38f, 0.14f, 0.38f), Mat("samovar_lid", new Color(0.95f, 0.78f, 0.36f)));
                }

                CreateSamovarCartoonEffects(building, root, levelScale);
                break;
            case BuildingType.Bedroom:
                CreateBox("Cardboard Bed Base", root, new Vector3(0f, 0.14f, 0f), new Vector3(1.45f * levelScale, 0.22f, 1.0f * levelScale), Mat("cardboard", new Color(0.58f, 0.40f, 0.22f)));
                CreateBox("Blanket", root, new Vector3(-0.16f, 0.32f, -0.08f), new Vector3(0.9f * levelScale, 0.12f, 0.64f), Mat("blanket", new Color(0.38f, 0.18f, 0.22f)));
                CreateBox("Cardboard Wall", root, new Vector3(0.55f, 0.64f, 0.38f), new Vector3(0.12f, 0.9f, 0.82f), Mat("cardboard_dark", new Color(0.42f, 0.29f, 0.16f)));
                break;
            case BuildingType.GrumbleBench:
                CreateBox("Bench Seat", root, new Vector3(0f, 0.36f, 0f), new Vector3(1.55f * levelScale, 0.16f, 0.42f), Mat("bench_wood", new Color(0.36f, 0.22f, 0.12f)));
                CreateBox("Bench Back", root, new Vector3(0f, 0.68f, 0.22f), new Vector3(1.55f * levelScale, 0.16f, 0.14f), Mat("bench_wood", new Color(0.36f, 0.22f, 0.12f)));
                CreateBox("Sign Grumble", root, new Vector3(0f, 0.95f, 0.26f), new Vector3(0.9f, 0.28f, 0.06f), Mat("old_sign", new Color(0.27f, 0.20f, 0.14f)));
                break;
            case BuildingType.CarpetCurtain:
                CreateBox("Carpet Rod", root, new Vector3(0f, 1.1f, 0f), new Vector3(1.7f * levelScale, 0.08f, 0.08f), Mat("rail", new Color(0.10f, 0.11f, 0.12f)));
                CreateBox("Red Carpet", root, new Vector3(-0.36f, 0.62f, 0f), new Vector3(0.62f, 1.05f, 0.08f), Mat("red_carpet", new Color(0.46f, 0.10f, 0.12f)));
                CreateBox("Blue Carpet", root, new Vector3(0.36f, 0.58f, 0f), new Vector3(0.62f, 0.98f, 0.08f), Mat("blue_carpet", new Color(0.11f, 0.22f, 0.40f)));
                break;
            case BuildingType.RadioMayak:
                CreateBox("Radio Crate", root, new Vector3(0f, 0.38f, 0f), new Vector3(0.92f * levelScale, 0.62f, 0.56f), Mat("radio", new Color(0.08f, 0.11f, 0.10f)));
                CreateSphere("Radio Dial", root, new Vector3(-0.24f, 0.42f, -0.31f), new Vector3(0.16f, 0.16f, 0.04f), Mat("dial", new Color(0.75f, 0.85f, 0.70f)));
                CreateBox("Radio Antenna", root, new Vector3(0.34f, 0.92f, 0f), new Vector3(0.035f, 0.85f * levelScale, 0.035f), Mat("antenna", new Color(0.75f, 0.75f, 0.68f)));
                AddPointLight(root, "Radio Green Light", new Vector3(-0.2f, 0.72f, -0.3f), new Color(0.35f, 1f, 0.55f), 1.2f, 0.45f);
                break;
        }
    }

    private Transform[] CreateFireFlames(Transform root)
    {
        Material orange = EmissiveMat("fire_flame_orange", new Color(1f, 0.34f, 0.05f), 1.9f);
        Material yellow = EmissiveMat("fire_flame_yellow", new Color(1f, 0.58f, 0.12f), 2.2f);
        return new[]
        {
            CreateFireFlame("Fire Flame A", root, new Vector3(-0.12f, 0.86f, -0.03f), yellow, -14f),
            CreateFireFlame("Fire Flame B", root, new Vector3(0.10f, 0.82f, 0.05f), orange, 18f),
            CreateFireFlame("Fire Flame C", root, new Vector3(0.02f, 0.94f, -0.10f), yellow, 4f),
            CreateFireFlame("Fire Flame D", root, new Vector3(0.16f, 0.88f, -0.02f), orange, -28f)
        };
    }

    private Transform CreateFireFlame(string name, Transform root, Vector3 position, Material material, float zRotation)
    {
        GameObject flame = CreateBox(name, root, position, new Vector3(0.10f, 0.38f, 0.08f), material);
        flame.transform.localRotation = Quaternion.Euler(-8f, 0f, zRotation);
        Collider collider = flame.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        return flame.transform;
    }


    private Material Mat(string key, Color color)
    {
        Material material;
        if (materialCache.TryGetValue(key, out material))
        {
            return material;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        material = new Material(shader);
        material.color = color;
        materialCache[key] = material;
        return material;
    }

    private Material EmissiveMat(string key, Color color, float intensity)
    {
        Material material = Mat(key, color);
        Color emission = color * intensity;
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", emission);
            material.EnableKeyword("_EMISSION");
        }

        return material;
    }

    private GameObject CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = localPosition;
        box.transform.localScale = localScale;
        Renderer renderer = box.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;
        return box;
    }

    private GameObject CreateSphere(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.SetParent(parent, false);
        sphere.transform.localPosition = localPosition;
        sphere.transform.localScale = localScale;
        Renderer renderer = sphere.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;
        return sphere;
    }

    private GameObject CreateCylinder(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name;
        cylinder.transform.SetParent(parent, false);
        cylinder.transform.localPosition = localPosition;
        cylinder.transform.localScale = new Vector3(localScale.x, localScale.z, localScale.y);
        Renderer renderer = cylinder.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;
        return cylinder;
    }

    private Light AddPointLight(Transform parent, string name, Vector3 localPosition, Color color, float range, float intensity)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.localPosition = localPosition;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = range;
        light.intensity = intensity;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.45f;
        light.shadowBias = 0.035f;
        light.shadowNormalBias = 0.20f;
        light.shadowNearPlane = 0.05f;
        return light;
    }

    private void AddSmokeOrSteam(Transform parent, string name, Color color, float rate, float lifetime)
    {
        GameObject particleObject = new GameObject(name);
        particleObject.transform.SetParent(parent, false);
        particleObject.transform.localPosition = new Vector3(0f, 1f, 0f);
        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.startLifetime = lifetime;
        main.startSpeed = 0.55f;
        main.startSize = 0.09f;
        main.startColor = color;
        main.maxParticles = 80;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = rate;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 14f;
        shape.radius = 0.18f;

        ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
        renderer.material = Mat(name + "_mat", color);
        particles.Play();
    }

    private GameObject CreateMarker(string name, Color color)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = name;
        marker.transform.SetParent(worldRoot, false);
        marker.transform.localScale = new Vector3(0.72f, 0.012f, 0.72f);
        marker.GetComponent<Renderer>().sharedMaterial = Mat(name, color);
        Collider collider = marker.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        marker.SetActive(false);
        return marker;
    }

    private RectTransform CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        Image image = panel.GetComponent<Image>();
        image.color = color;
        return panel.GetComponent<RectTransform>();
    }

    private Text CreateText(string name, Transform parent, int size, FontStyle style, TextAnchor anchor, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.font = uiFont;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = anchor;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private RectTransform CreateButton(string label, Transform parent, UnityEngine.Events.UnityAction action)
    {
        RectTransform rect = CreatePanel("Button - " + label, parent, new Color(0.075f, 0.085f, 0.095f, 0.82f));
        rect.sizeDelta = new Vector2(0f, 48f);

        LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = 42f;
        layout.preferredHeight = 48f;
        layout.flexibleWidth = 1f;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.onClick.AddListener(action);

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.075f, 0.085f, 0.095f, 0.82f);
        colors.highlightedColor = new Color(0.18f, 0.22f, 0.23f, 0.96f);
        colors.pressedColor = new Color(0.95f, 0.61f, 0.25f, 1f);
        colors.disabledColor = new Color(0.12f, 0.12f, 0.13f, 0.62f);
        button.colors = colors;

        Text text = CreateText("Label", rect, 15, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        text.supportRichText = true;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(8f, 2f);
        text.rectTransform.offsetMax = new Vector2(-8f, -2f);
        text.text = label;
        text.raycastTarget = false;

        Outline outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.76f, 0.54f, 0.22f, 0.25f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        BridgeGrandpasButtonVisual visual = rect.gameObject.AddComponent<BridgeGrandpasButtonVisual>();
        visual.Setup(button, rect.GetComponent<Image>(), text, outline);
        rect.gameObject.AddComponent<BridgeGrandpasHudButtonAudio>();

        return rect;
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}

