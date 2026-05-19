using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private float CozyForBuild(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.FireBarrel:
                return 4f;
            case BuildingType.Samovar:
                return 3.5f;
            case BuildingType.Bedroom:
                return 4.5f;
            case BuildingType.GrumbleBench:
                return 3f;
            case BuildingType.CarpetCurtain:
                return 3.8f;
            case BuildingType.RadioMayak:
                return 4.2f;
            default:
                return 1f;
        }
    }

    private string GainCozy(float amount)
    {
        if (amount <= 0f)
        {
            return "";
        }

        int previous = cozyDecorTier;
        cozyScore = Mathf.Max(0f, cozyScore + amount);
        RefreshCozyDecor();
        if (cozyDecorTier <= previous)
        {
            return "";
        }

        return " Открылся декор: " + CozyDecorName(cozyDecorTier) + ".";
    }

    private string CozyStat()
    {
        float bonus = (CozyIncomeMultiplier() - 1f) * 100f;
        string text = bonus < 0.5f ? "" : " <color=#ffcf7a>+" + RateF(bonus) + "%</color>";
        string level = cozyDecorTier > 0 ? " ур." + cozyDecorTier : "";
        return "Уют " + F(cozyScore) + level + text;
    }

    private float CozyIncomeMultiplier()
    {
        return 1f + Mathf.Min(0.18f, Mathf.Floor(cozyScore / 10f) * 0.01f);
    }

    private void RefreshCozyDecor()
    {
        int tier = CozyTierForScore();
        EnsureCozyDecorRoot();
        if (cozyDecorRoot == null || tier == cozyDecorTier)
        {
            return;
        }

        cozyDecorTier = tier;
        ClearChildren(cozyDecorRoot);
        if (tier >= 1) CreateCozyRug();
        if (tier >= 2) CreateCozyStringLights();
        if (tier >= 3) CreateCozyCups();
        if (tier >= 4) CreateCozyNewspaperStack();
        if (tier >= 5) CreateCozyOldPlaid();
        if (tier >= 6) CreateCozySockLine();
        if (tier >= 7) CreateCozyJarShelf();
        if (tier >= 8) CreateCozyCanopy();
        if (tier >= 9) CreateCozyWelcomeSign();
    }

    private int CozyTierForScore()
    {
        if (cozyScore >= 76f) return 9;
        if (cozyScore >= 62f) return 8;
        if (cozyScore >= 50f) return 7;
        if (cozyScore >= 38f) return 6;
        if (cozyScore >= 28f) return 5;
        if (cozyScore >= 20f) return 4;
        if (cozyScore >= 13f) return 3;
        if (cozyScore >= 8f) return 2;
        if (cozyScore >= 4f) return 1;
        return 0;
    }

    private string CozyDecorName(int tier)
    {
        switch (tier)
        {
            case 1: return "чайный коврик";
            case 2: return "гирлянда между столбами";
            case 3: return "чайные кружки";
            case 4: return "стопка газет";
            case 5: return "старый плед";
            case 6: return "сушилка с носками";
            case 7: return "полка с банками";
            case 8: return "маленький навес";
            case 9: return "табличка тишины";
            default: return "уютная мелочь";
        }
    }

    private void EnsureCozyDecorRoot()
    {
        if (cozyDecorRoot != null || settlementRoot == null)
        {
            return;
        }

        cozyDecorRoot = new GameObject("Cozy Idle Decor").transform;
        cozyDecorRoot.SetParent(settlementRoot, false);
    }

    private void CreateCozyRug()
    {
        Material rug = Mat("cozy_rug", new Color(0.42f, 0.15f, 0.13f));
        CozyBox("Cozy rug", new Vector3(0.85f, 0.012f, -0.78f), new Vector3(1.05f, 0.025f, 0.62f), rug);
    }

    private void CreateCozyCups()
    {
        Material cup = Mat("cozy_cup", new Color(0.92f, 0.83f, 0.62f));
        CozyCylinder("Cup A", new Vector3(0.48f, 0.09f, -0.56f), new Vector3(0.10f, 0.10f, 0.16f), cup);
        CozyCylinder("Cup B", new Vector3(0.74f, 0.09f, -0.92f), new Vector3(0.09f, 0.09f, 0.15f), cup);
    }

    private void CreateCozyStringLights()
    {
        Material wire = Mat("cozy_wire", new Color(0.05f, 0.04f, 0.035f));
        CozyBox("Bulb wire", new Vector3(0f, 2.16f, 2.68f), new Vector3(6.35f, 0.035f, 0.035f), wire);
        CozyBox("Left pillar tie", new Vector3(-3.18f, 2.08f, 2.68f), new Vector3(0.10f, 0.20f, 0.055f), wire);
        CozyBox("Right pillar tie", new Vector3(3.18f, 2.08f, 2.68f), new Vector3(0.10f, 0.20f, 0.055f), wire);
        Material bulb = EmissiveMat("cozy_bulb", new Color(1f, 0.58f, 0.22f), 1.8f);
        for (int i = 0; i < 7; i++)
        {
            float x = -2.52f + i * 0.84f;
            float sag = i == 3 ? -0.10f : Mathf.Abs(i - 3) == 1 ? -0.06f : -0.02f;
            CozyBox("Bulb drop " + i, new Vector3(x, 2.05f + sag, 2.68f), new Vector3(0.025f, 0.16f, 0.025f), wire);
            CozySphere("Warm bulb " + i, new Vector3(x, 1.93f + sag, 2.68f), new Vector3(0.13f, 0.13f, 0.13f), bulb);
        }

        AddPointLight(cozyDecorRoot, "Cozy bulbs glow", new Vector3(0f, 1.90f, 2.38f), new Color(1f, 0.54f, 0.22f), 4.8f, 0.55f);
    }

    private void CreateCozyNewspaperStack()
    {
        Material paper = Mat("cozy_newspaper", new Color(0.52f, 0.50f, 0.43f));
        CozyBox("Newspaper stack A", new Vector3(-1.02f, 0.045f, -1.18f), new Vector3(0.55f, 0.035f, 0.38f), paper);
        CozyBox("Newspaper stack B", new Vector3(-1.18f, 0.09f, -1.02f), new Vector3(0.45f, 0.035f, 0.34f), paper);
        CozyBox("Newspaper stack C", new Vector3(-0.88f, 0.13f, -1.00f), new Vector3(0.42f, 0.035f, 0.30f), paper);
    }

    private void CreateCozyOldPlaid()
    {
        Material plaid = Mat("cozy_old_plaid", new Color(0.30f, 0.12f, 0.20f));
        CozyBox("Old plaid base", new Vector3(-0.62f, 0.105f, -1.28f), new Vector3(0.78f, 0.075f, 0.52f), plaid);
        Material stripe = Mat("cozy_plaid_stripes", new Color(0.66f, 0.45f, 0.28f));
        CozyBox("Old plaid stripe A", new Vector3(-0.62f, 0.15f, -1.28f), new Vector3(0.08f, 0.035f, 0.54f), stripe);
        CozyBox("Old plaid stripe B", new Vector3(-0.62f, 0.155f, -1.28f), new Vector3(0.80f, 0.035f, 0.06f), stripe);
    }

    private void CreateCozySockLine()
    {
        Material line = Mat("cozy_sock_line", new Color(0.07f, 0.055f, 0.04f));
        CozyBox("Sock line", new Vector3(-1.35f, 1.08f, 0.9f), new Vector3(2.1f, 0.035f, 0.035f), line);
        Material sockA = Mat("cozy_sock_red", new Color(0.46f, 0.12f, 0.14f));
        Material sockB = Mat("cozy_sock_blue", new Color(0.12f, 0.22f, 0.38f));
        CozyBox("Sock A", new Vector3(-1.95f, 0.82f, 0.9f), new Vector3(0.16f, 0.34f, 0.035f), sockA);
        CozyBox("Sock B", new Vector3(-1.28f, 0.80f, 0.9f), new Vector3(0.16f, 0.30f, 0.035f), sockB);
        CozyBox("Sock C", new Vector3(-0.55f, 0.81f, 0.9f), new Vector3(0.16f, 0.32f, 0.035f), sockA);
    }

    private void CreateCozyJarShelf()
    {
        Material wood = Mat("cozy_shelf_wood", new Color(0.32f, 0.19f, 0.10f));
        CozyBox("Jar shelf", new Vector3(1.7f, 0.72f, 0.72f), new Vector3(1.0f, 0.10f, 0.20f), wood);
        Material jar = Mat("cozy_jars", new Color(0.58f, 0.67f, 0.66f));
        for (int i = 0; i < 4; i++)
        {
            CozyCylinder("Jar " + i, new Vector3(1.28f + i * 0.24f, 0.88f, 0.72f), new Vector3(0.08f, 0.08f, 0.22f), jar);
        }
    }

    private void CreateCozyCanopy()
    {
        Material pole = Mat("cozy_canopy_pole", new Color(0.22f, 0.14f, 0.08f));
        CozyBox("Canopy pole L", new Vector3(-2.35f, 0.92f, -0.72f), new Vector3(0.08f, 1.7f, 0.08f), pole);
        CozyBox("Canopy pole R", new Vector3(-0.88f, 0.92f, -0.72f), new Vector3(0.08f, 1.7f, 0.08f), pole);
        Material cloth = Mat("cozy_canopy_cloth", new Color(0.30f, 0.16f, 0.10f));
        CozyBox("Small canopy cloth", new Vector3(-1.62f, 1.76f, -0.72f), new Vector3(1.72f, 0.10f, 1.05f), cloth);
        Material edge = Mat("cozy_canopy_edge", new Color(0.52f, 0.30f, 0.14f));
        CozyBox("Small canopy front edge", new Vector3(-1.62f, 1.66f, -1.25f), new Vector3(1.76f, 0.12f, 0.06f), edge);
    }

    private void CreateCozyWelcomeSign()
    {
        Material sign = Mat("cozy_quiet_sign", new Color(0.36f, 0.23f, 0.12f));
        CozyBox("Quiet sign", new Vector3(0.15f, 0.78f, 1.23f), new Vector3(1.15f, 0.42f, 0.07f), sign);
        Material chalk = Mat("cozy_chalk_marks", new Color(0.86f, 0.80f, 0.65f));
        CozyBox("Quiet sign mark A", new Vector3(-0.12f, 0.82f, 1.18f), new Vector3(0.42f, 0.045f, 0.035f), chalk);
        CozyBox("Quiet sign mark B", new Vector3(0.28f, 0.72f, 1.18f), new Vector3(0.30f, 0.045f, 0.035f), chalk);
    }

    private GameObject CozyBox(string name, Vector3 position, Vector3 scale, Material material)
    {
        return MakeCozyDecor(CreateBox(name, cozyDecorRoot, position, scale, material));
    }

    private GameObject CozySphere(string name, Vector3 position, Vector3 scale, Material material)
    {
        return MakeCozyDecor(CreateSphere(name, cozyDecorRoot, position, scale, material));
    }

    private GameObject CozyCylinder(string name, Vector3 position, Vector3 scale, Material material)
    {
        return MakeCozyDecor(CreateCylinder(name, cozyDecorRoot, position, scale, material));
    }

    private GameObject MakeCozyDecor(GameObject item)
    {
        Collider collider = item.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        item.layer = 2;
        return item;
    }
}
