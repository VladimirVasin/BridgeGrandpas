using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private void CreateStarterCommuneProps()
    {
        Transform root = new GameObject("Starter Cozy Commune Props").transform;
        root.SetParent(settlementRoot, false);

        Material wood = Mat("starter_old_wood", new Color(0.28f, 0.18f, 0.10f));
        Material cardboard = Mat("starter_cardboard", new Color(0.52f, 0.35f, 0.17f));
        Material blanket = Mat("starter_blanket", new Color(0.33f, 0.13f, 0.18f));
        Material metal = Mat("starter_dull_metal", new Color(0.28f, 0.24f, 0.18f));
        Material rug = Mat("starter_rug_red", new Color(0.48f, 0.12f, 0.11f));
        Material paper = Mat("starter_old_paper", new Color(0.55f, 0.52f, 0.42f));
        Material dark = Mat("starter_dark_junk", new Color(0.07f, 0.075f, 0.078f));
        Material cup = Mat("starter_cup", new Color(0.78f, 0.72f, 0.58f));

        DecorBox(root, "Small rug by fire", new Vector3(-0.15f, 0.02f, -0.82f), new Vector3(1.45f, 0.025f, 0.72f), rug, 2f);
        DecorBox(root, "Cardboard bed left", new Vector3(-1.45f, 0.13f, 0.56f), new Vector3(1.22f, 0.20f, 0.62f), cardboard, -7f);
        DecorBox(root, "Cardboard bed right", new Vector3(1.35f, 0.13f, 0.62f), new Vector3(1.10f, 0.20f, 0.58f), cardboard, 8f);
        DecorBox(root, "Old blanket left", new Vector3(-1.48f, 0.27f, 0.48f), new Vector3(0.86f, 0.08f, 0.44f), blanket, -7f);
        DecorBox(root, "Folded blanket right", new Vector3(1.28f, 0.27f, 0.54f), new Vector3(0.74f, 0.08f, 0.38f), blanket, 8f);

        DecorBox(root, "Tea stool", new Vector3(-1.65f, 0.20f, -0.52f), new Vector3(0.65f, 0.16f, 0.52f), wood, -5f);
        DecorCylinder(root, "Small kettle body", new Vector3(-1.65f, 0.50f, -0.52f), new Vector3(0.20f, 0.20f, 0.44f), metal, 0f);
        DecorSphere(root, "Small kettle lid", new Vector3(-1.65f, 0.78f, -0.52f), new Vector3(0.24f, 0.09f, 0.24f), metal);
        DecorBox(root, "Kettle spout", new Vector3(-1.36f, 0.55f, -0.52f), new Vector3(0.26f, 0.055f, 0.06f), metal, -10f);
        DecorCylinder(root, "Mug near kettle", new Vector3(-1.20f, 0.35f, -0.92f), new Vector3(0.09f, 0.09f, 0.16f), cup, 0f);
        DecorCylinder(root, "Mug by barrel", new Vector3(0.58f, 0.08f, -1.16f), new Vector3(0.075f, 0.075f, 0.12f), cup, 0f);

        Transform steam = new GameObject("Small kettle steam anchor").transform;
        steam.SetParent(root, false);
        steam.localPosition = new Vector3(-1.65f, 0.18f, -0.52f);
        AddSmokeOrSteam(steam, "Small kettle steam", new Color(0.70f, 0.76f, 0.82f, 0.28f), 4f, 3.2f);

        DecorBox(root, "Grumble bench seat", new Vector3(1.72f, 0.33f, -1.03f), new Vector3(1.22f, 0.12f, 0.32f), wood, 8f);
        DecorBox(root, "Grumble bench back", new Vector3(1.72f, 0.58f, -0.88f), new Vector3(1.22f, 0.12f, 0.10f), wood, 8f);
        DecorBox(root, "Radio crate prop", new Vector3(2.25f, 0.26f, -1.72f), new Vector3(0.56f, 0.46f, 0.38f), dark, -12f);
        DecorSphere(root, "Radio dial prop", new Vector3(2.06f, 0.30f, -1.93f), new Vector3(0.11f, 0.11f, 0.035f), cup);
        DecorBox(root, "Radio antenna prop", new Vector3(2.52f, 0.82f, -1.62f), new Vector3(0.028f, 0.78f, 0.028f), metal, -18f);

        DecorBox(root, "Cardboard screen back", new Vector3(-0.38f, 0.52f, 1.12f), new Vector3(0.88f, 0.96f, 0.08f), cardboard, -8f);
        DecorBox(root, "Carpet scrap back", new Vector3(0.62f, 0.58f, 1.16f), new Vector3(0.72f, 1.02f, 0.07f), rug, 8f);

        for (int i = 0; i < 8; i++)
        {
            float x = -2.45f + i * 0.68f;
            float z = -1.72f + Mathf.Sin(i * 1.4f) * 0.24f;
            Material board = i % 2 == 0 ? wood : cardboard;
            DecorBox(root, "Loose board " + i, new Vector3(x, 0.04f, z), new Vector3(0.54f, 0.045f, 0.10f), board, i * 17f);
        }

        for (int i = 0; i < 7; i++)
        {
            float x = -1.9f + i * 0.62f;
            float z = 1.08f + Mathf.Cos(i * 1.2f) * 0.22f;
            DecorBox(root, "Old newspaper " + i, new Vector3(x, 0.035f, z), new Vector3(0.34f, 0.018f, 0.22f), paper, -20f + i * 13f);
        }
    }

    private GameObject DecorBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material, float yaw)
    {
        GameObject item = CreateBox(name, parent, position, scale, material);
        item.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        StripDecorCollider(item);
        return item;
    }

    private GameObject DecorCylinder(Transform parent, string name, Vector3 position, Vector3 scale, Material material, float yaw)
    {
        GameObject item = CreateCylinder(name, parent, position, scale, material);
        item.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        StripDecorCollider(item);
        return item;
    }

    private GameObject DecorSphere(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject item = CreateSphere(name, parent, position, scale, material);
        StripDecorCollider(item);
        return item;
    }
}
