using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private readonly List<WindClothTarget> windClothTargets = new List<WindClothTarget>();

    private sealed class WindClothTarget
    {
        public Cloth Cloth;
        public float Multiplier;
        public float Phase;
    }

    private GameObject CreateClothPanel(
        string name,
        Transform parent,
        Vector3 topCenter,
        Vector2 size,
        Material material,
        float windMultiplier)
    {
        const int columns = 8;
        const int rows = 10;
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        panel.transform.localPosition = topCenter;
        panel.transform.localRotation = Quaternion.identity;
        panel.layer = 2;

        Mesh mesh = BuildClothPanelMesh(name + " Mesh", size, columns, rows);
        ConfigureClothMaterial(material);
        SkinnedMeshRenderer renderer = panel.AddComponent<SkinnedMeshRenderer>();
        renderer.sharedMesh = mesh;
        renderer.sharedMaterial = material;
        renderer.updateWhenOffscreen = true;
        renderer.localBounds = new Bounds(new Vector3(0f, -size.y * 0.5f, 0f), new Vector3(size.x * 1.8f, size.y * 1.7f, 0.9f));

        Cloth cloth = panel.AddComponent<Cloth>();
        cloth.coefficients = ClothCoefficients(columns, rows);
        cloth.useGravity = true;
        cloth.stretchingStiffness = 0.72f;
        cloth.bendingStiffness = 0.28f;
        cloth.damping = 0.34f;
        cloth.worldVelocityScale = 0.22f;
        cloth.worldAccelerationScale = 0.34f;
        cloth.clothSolverFrequency = 90f;
        cloth.selfCollisionDistance = 0.025f;
        cloth.selfCollisionStiffness = 0.18f;
        RegisterWindCloth(cloth, windMultiplier);
        return panel;
    }

    private Mesh BuildClothPanelMesh(string name, Vector2 size, int columns, int rows)
    {
        int vertexCount = (columns + 1) * (rows + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        int index = 0;
        for (int y = 0; y <= rows; y++)
        {
            float v = y / (float)rows;
            for (int x = 0; x <= columns; x++)
            {
                float u = x / (float)columns;
                vertices[index] = new Vector3((u - 0.5f) * size.x, -v * size.y, 0f);
                uv[index] = new Vector2(u, v);
                index++;
            }
        }

        List<int> triangles = new List<int>(columns * rows * 12);
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int a = y * (columns + 1) + x;
                int b = a + 1;
                int c = a + columns + 1;
                int d = c + 1;
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(b);
                triangles.Add(d);
                triangles.Add(c);
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = name;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void ConfigureClothMaterial(Material material)
    {
        if (material == null)
        {
            return;
        }

        SetFloat(material, "_Cull", (float)CullMode.Off);
        SetFloat(material, "_Smoothness", 0.035f);
        SetFloat(material, "_Glossiness", 0.035f);
        SetFloat(material, "_Metallic", 0f);
        SetFloat(material, "_SpecularHighlights", 0f);
        SetFloat(material, "_EnvironmentReflections", 0f);
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", Color.black);
        }

        material.DisableKeyword("_EMISSION");
        material.doubleSidedGI = false;
    }

    private ClothSkinningCoefficient[] ClothCoefficients(int columns, int rows)
    {
        ClothSkinningCoefficient[] coefficients = new ClothSkinningCoefficient[(columns + 1) * (rows + 1)];
        int index = 0;
        for (int y = 0; y <= rows; y++)
        {
            float row = y / (float)rows;
            for (int x = 0; x <= columns; x++)
            {
                float edge = Mathf.Abs((x / (float)columns) - 0.5f) * 2f;
                coefficients[index].maxDistance = y == 0 ? 0f : Mathf.Lerp(0.06f, 0.42f, row) * Mathf.Lerp(0.82f, 1.1f, edge);
                coefficients[index].collisionSphereDistance = 0f;
                index++;
            }
        }

        return coefficients;
    }

    private void RegisterWindCloth(Cloth cloth, float multiplier)
    {
        if (cloth == null)
        {
            return;
        }

        windClothTargets.Add(new WindClothTarget
        {
            Cloth = cloth,
            Multiplier = multiplier,
            Phase = Random.Range(0f, 20f)
        });
    }

    private void UpdateWindCloth()
    {
        for (int i = windClothTargets.Count - 1; i >= 0; i--)
        {
            WindClothTarget target = windClothTargets[i];
            if (target.Cloth == null)
            {
                windClothTargets.RemoveAt(i);
                continue;
            }

            float flutter = Mathf.PerlinNoise(target.Phase, Time.time * 0.85f);
            float force = (4.5f + underpassWindStrength * 10.5f + underpassWindGust * 8.5f) * target.Multiplier;
            Vector3 main = underpassWindDirection * force;
            Vector3 random = new Vector3(
                (flutter - 0.5f) * 1.8f,
                0.25f + underpassWindGust * 0.7f,
                (Mathf.PerlinNoise(Time.time * 0.7f, target.Phase) - 0.5f) * 1.2f);
            target.Cloth.externalAcceleration = main;
            target.Cloth.randomAcceleration = random * (0.35f + underpassWindStrength);
        }
    }
}
