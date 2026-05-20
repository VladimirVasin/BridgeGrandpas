using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const int JunkPileColumns = 8;
    private const int JunkPileRows = 2;
    private const float JunkPileMinX = -18.25f;
    private const float JunkPileMaxX = 18.25f;

    private void ResetJunkPilesForNewGame()
    {
        stock.Junk = 0f;
        junkPiles.Clear();
        EnsureJunkRoots();
        ClearChildren(junkPileRoot);
        CreateJunkDepot();

        int id = 1;
        for (int row = 0; row < JunkPileRows; row++)
        {
            for (int column = 0; column < JunkPileColumns; column++)
            {
                Vector3 position = EvenJunkPilePosition(column, row, JunkPileColumns, JunkPileMinX, JunkPileMaxX);
                const float maxJunk = 9f;
                AddJunkPile(id, position, maxJunk, (JunkPileVariant)((column + row * 2) % 5), maxJunk);
                id++;
            }
        }

        RefreshJunkDepotVisual();
        WriteDebugLog("JUNK_INIT", "Reset junk piles for new game. piles=" + junkPiles.Count +
            " columns=" + JunkPileColumns + " rows=" + JunkPileRows + " rangeX=" + JunkPileMinX + ".." + JunkPileMaxX);
    }

    private Vector3 EvenJunkPilePosition(int column, int row, int columns, float minX, float maxX)
    {
        float x = columns <= 1 ? (minX + maxX) * 0.5f : Mathf.Lerp(minX, maxX, column / (float)(columns - 1));
        float[] lanes = { -2.92f, 0.74f };
        float z = lanes[Mathf.Clamp(row, 0, lanes.Length - 1)];
        float stagger = row == 1 ? (maxX - minX) / Mathf.Max(1f, (columns - 1) * 2f) : 0f;
        x = Mathf.Clamp(x + stagger, minX, maxX);

        Vector3 depot = JunkDepotPosition();
        if (Mathf.Abs(x - depot.x) < 0.78f && Mathf.Abs(z - depot.z) < 0.90f)
        {
            z = row == 1 ? 1.08f : -2.62f;
        }

        return new Vector3(x, 0f, z);
    }

    private void RestoreJunkPiles(System.Collections.Generic.List<JunkPileSaveData> savedPiles)
    {
        junkPiles.Clear();
        EnsureJunkRoots();
        ClearChildren(junkPileRoot);
        CreateJunkDepot();

        if (savedPiles == null || savedPiles.Count == 0 || savedPiles.Count != JunkPileColumns * JunkPileRows)
        {
            float savedJunk = stock.Junk;
            WriteDebugWarningLog("JUNK_RESTORE", "Saved junk piles missing or wrong count. savedCount=" +
                (savedPiles == null ? 0 : savedPiles.Count) + " expected=" + (JunkPileColumns * JunkPileRows));
            ResetJunkPilesForNewGame();
            stock.Junk = savedJunk;
            RefreshJunkDepotVisual();
            return;
        }

        for (int i = 0; i < savedPiles.Count; i++)
        {
            JunkPileSaveData saved = savedPiles[i];
            AddJunkPile(
                saved.Id <= 0 ? i + 1 : saved.Id,
                saved.Position,
                Mathf.Max(1f, saved.MaxJunk),
                (JunkPileVariant)Mathf.Clamp(saved.Variant, 0, 4),
                Mathf.Clamp(saved.RemainingJunk, 0f, Mathf.Max(1f, saved.MaxJunk)));
        }

        RefreshJunkDepotVisual();
        WriteDebugLog("JUNK_RESTORE", "Restored junk piles. piles=" + junkPiles.Count + " stockJunk=" + RateF(stock.Junk));
    }

    private void ClearJunkScene()
    {
        junkPiles.Clear();
        junkDepotGrowthPieces = null;
        junkDepotRoot = null;
        if (junkRoot != null)
        {
            ClearChildren(junkRoot);
        }

        junkRoot = null;
        junkPileRoot = null;
    }

    private void EnsureJunkRoots()
    {
        if (junkRoot == null)
        {
            junkRoot = new GameObject("Underpass Junk System").transform;
            junkRoot.SetParent(settlementRoot, false);
        }

        if (junkPileRoot == null)
        {
            junkPileRoot = new GameObject("Scavenge Junk Piles").transform;
            junkPileRoot.SetParent(junkRoot, false);
        }
    }

    private void AddJunkPile(int id, Vector3 position, float maxJunk, JunkPileVariant variant, float remainingJunk)
    {
        JunkPile pile = new JunkPile
        {
            Id = id,
            Position = Flat(position),
            MaxJunk = Mathf.Max(1f, maxJunk),
            RemainingJunk = Mathf.Clamp(remainingJunk, 0f, Mathf.Max(1f, maxJunk)),
            Variant = variant
        };
        pile.Root = CreateJunkPileVisual(pile);
        junkPiles.Add(pile);
    }

    private GameObject CreateJunkPileVisual(JunkPile pile)
    {
        GameObject root = new GameObject("Junk Pile " + pile.Id);
        root.transform.SetParent(junkPileRoot, false);
        root.transform.position = pile.Position;
        BuildJunkPileParts(pile, root.transform);
        return root;
    }

    private void RefreshJunkPileVisual(JunkPile pile)
    {
        if (pile == null || pile.Root == null)
        {
            return;
        }

        ClearChildren(pile.Root.transform);
        BuildJunkPileParts(pile, pile.Root.transform);
    }

    private void BuildJunkPileParts(JunkPile pile, Transform root)
    {
        float fullness = Mathf.Clamp01(pile.RemainingJunk / Mathf.Max(1f, pile.MaxJunk));
        root.gameObject.SetActive(fullness > 0.02f);
        root.localScale = Vector3.one * Mathf.Lerp(0.54f, 1.05f, fullness);
        if (fullness <= 0.02f)
        {
            return;
        }

        Material darkBag = Mat("junk_dark_bag", new Color(0.035f, 0.038f, 0.040f));
        Material wood = Mat("junk_old_wood", new Color(0.34f, 0.22f, 0.13f));
        Material cardboard = Mat("junk_cardboard", new Color(0.53f, 0.36f, 0.18f));
        Material metal = Mat("junk_dull_metal", new Color(0.20f, 0.22f, 0.22f));

        switch (pile.Variant)
        {
            case JunkPileVariant.Cabinet:
                CreateJunkBox("Broken Cabinet", root, new Vector3(0f, 0.45f, 0f), new Vector3(0.95f, 0.82f, 0.34f), wood, -8f);
                CreateJunkBox("Cabinet Door", root, new Vector3(0.34f, 0.26f, -0.28f), new Vector3(0.44f, 0.08f, 0.58f), wood, 18f);
                CreateJunkBox("Metal Handle", root, new Vector3(-0.28f, 0.50f, -0.22f), new Vector3(0.06f, 0.18f, 0.04f), metal, 0f);
                break;
            case JunkPileVariant.TableAndChairs:
                CreateJunkBox("Table Top", root, new Vector3(0.02f, 0.38f, 0f), new Vector3(1.08f, 0.12f, 0.66f), wood, 11f);
                CreateJunkBox("Chair Back", root, new Vector3(-0.44f, 0.62f, 0.26f), new Vector3(0.12f, 0.74f, 0.50f), wood, -18f);
                CreateJunkBox("Chair Seat", root, new Vector3(0.42f, 0.20f, -0.18f), new Vector3(0.50f, 0.10f, 0.42f), wood, 22f);
                break;
            case JunkPileVariant.Crates:
                CreateJunkBox("Crate A", root, new Vector3(-0.28f, 0.24f, 0.08f), new Vector3(0.54f, 0.44f, 0.46f), cardboard, 0f);
                CreateJunkBox("Crate B", root, new Vector3(0.30f, 0.34f, -0.12f), new Vector3(0.62f, 0.52f, 0.38f), cardboard, -12f);
                CreateJunkBox("Pipe", root, new Vector3(0.0f, 0.12f, 0.34f), new Vector3(0.18f, 0.18f, 0.82f), metal, 88f);
                break;
            case JunkPileVariant.Mixed:
                CreateJunkBox("Mixed Board", root, new Vector3(-0.18f, 0.22f, -0.18f), new Vector3(1.24f, 0.08f, 0.28f), wood, -22f);
                CreateJunkSphere("Bag", root, new Vector3(0.34f, 0.20f, 0.12f), new Vector3(0.48f, 0.34f, 0.44f), darkBag);
                CreateJunkBox("Old Drawer", root, new Vector3(0.04f, 0.52f, -0.06f), new Vector3(0.58f, 0.36f, 0.42f), wood, 14f);
                break;
            default:
                CreateJunkSphere("Trash Bag A", root, new Vector3(-0.24f, 0.22f, 0.04f), new Vector3(0.50f, 0.42f, 0.46f), darkBag);
                CreateJunkSphere("Trash Bag B", root, new Vector3(0.26f, 0.18f, -0.10f), new Vector3(0.44f, 0.34f, 0.42f), darkBag);
                CreateJunkBox("Loose Plank", root, new Vector3(0.10f, 0.48f, 0.05f), new Vector3(0.92f, 0.08f, 0.20f), wood, 27f);
                break;
        }
    }

    private void CreateJunkDepot()
    {
        if (junkDepotRoot != null)
        {
            return;
        }

        junkDepotRoot = new GameObject("Central Junk Heap").transform;
        junkDepotRoot.SetParent(junkRoot, false);
        junkDepotRoot.position = JunkDepotPosition();
        junkDepotBaseScale = Vector3.one;

        Material wood = Mat("junk_depot_wood", new Color(0.36f, 0.23f, 0.13f));
        Material bag = Mat("junk_depot_bag", new Color(0.04f, 0.042f, 0.038f));
        Material cardboard = Mat("junk_depot_cardboard", new Color(0.57f, 0.39f, 0.18f));
        junkDepotGrowthPieces = new Transform[7];
        junkDepotGrowthPieces[0] = CreateJunkBox("Depot Base Bags", junkDepotRoot, new Vector3(0f, 0.18f, 0f), new Vector3(0.96f, 0.36f, 0.78f), bag, 0f).transform;
        junkDepotGrowthPieces[1] = CreateJunkBox("Depot Cardboard", junkDepotRoot, new Vector3(-0.32f, 0.46f, -0.08f), new Vector3(0.68f, 0.16f, 0.50f), cardboard, -13f).transform;
        junkDepotGrowthPieces[2] = CreateJunkBox("Depot Plank", junkDepotRoot, new Vector3(0.25f, 0.62f, 0.08f), new Vector3(1.04f, 0.09f, 0.18f), wood, 22f).transform;
        junkDepotGrowthPieces[3] = CreateJunkBox("Depot Drawer", junkDepotRoot, new Vector3(0.24f, 0.44f, -0.28f), new Vector3(0.42f, 0.34f, 0.36f), wood, 8f).transform;
        junkDepotGrowthPieces[4] = CreateJunkBox("Depot Chair Back", junkDepotRoot, new Vector3(-0.48f, 0.78f, 0.18f), new Vector3(0.13f, 0.72f, 0.50f), wood, -18f).transform;
        junkDepotGrowthPieces[5] = CreateJunkBox("Depot Cabinet Door", junkDepotRoot, new Vector3(0.52f, 0.82f, 0.08f), new Vector3(0.52f, 0.10f, 0.76f), wood, 36f).transform;
        junkDepotGrowthPieces[6] = CreateJunkBox("Depot Weird Box", junkDepotRoot, new Vector3(0.0f, 1.02f, -0.04f), new Vector3(0.52f, 0.38f, 0.40f), cardboard, -7f).transform;
    }

    private void UpdateJunkScene(float deltaTime)
    {
        if (junkDepotRoot == null)
        {
            return;
        }

        junkDepotPulse = Mathf.MoveTowards(junkDepotPulse, 0f, deltaTime * 2.8f);
        RefreshJunkDepotVisual();
    }

    private void RefreshJunkDepotVisual()
    {
        if (junkDepotRoot == null)
        {
            return;
        }

        float fullness = Mathf.Clamp01(stock.Junk / 120f);
        float pulse = Mathf.Sin(junkDepotPulse * Mathf.PI) * 0.12f;
        float scale = 0.76f + fullness * 0.72f + pulse;
        junkDepotRoot.localScale = junkDepotBaseScale * scale;

        if (junkDepotGrowthPieces == null)
        {
            return;
        }

        for (int i = 0; i < junkDepotGrowthPieces.Length; i++)
        {
            if (junkDepotGrowthPieces[i] != null)
            {
                junkDepotGrowthPieces[i].gameObject.SetActive(i == 0 || stock.Junk >= 8f + i * 14f);
            }
        }
    }

    private GameObject CreateJunkBox(string name, Transform parent, Vector3 position, Vector3 scale, Material material, float yRotation)
    {
        GameObject box = CreateBox(name, parent, position, scale, material);
        box.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        RemoveCollider(box);
        return box;
    }

    private GameObject CreateJunkSphere(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject sphere = CreateSphere(name, parent, position, scale, material);
        RemoveCollider(sphere);
        return sphere;
    }

    private void RemoveCollider(GameObject gameObject)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }
}
