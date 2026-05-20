using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const int MaxGroundLitter = 44;
    private const float GroundLitterSpawnX = -24.5f;
    private const float GroundLitterRecycleX = 24.5f;
    private readonly List<GroundLitterItem> activeGroundLitter = new List<GroundLitterItem>();
    private readonly List<GroundLitterItem> pooledGroundLitter = new List<GroundLitterItem>();
    private Transform groundLitterRoot;
    private BoxCollider groundLitterCollider;
    private float nextGroundLitterAt;

    private enum GroundLitterKind
    {
        Can,
        Butt,
        Paper
    }

    private sealed class GroundLitterItem
    {
        public GroundLitterKind Kind;
        public GameObject Root;
        public Transform Visual;
        public Rigidbody Body;
        public Mesh PaperMesh;
        public Vector3[] PaperBaseVertices;
        public Vector3[] PaperDeformedVertices;
        public Vector2 PaperSize;
        public float Phase;
        public float Speed;
        public float Spin;
        public float SideSway;
        public float LaneBias;
        public float Yaw;
    }

    private void SetupGroundLitter()
    {
        if (worldRoot == null || groundLitterRoot != null)
        {
            return;
        }

        groundLitterRoot = new GameObject("Wind Carried Ground Litter").transform;
        groundLitterRoot.SetParent(worldRoot, false);
        EnsureGroundLitterCollider();
        nextGroundLitterAt = Time.time + UnityEngine.Random.Range(0.5f, 1.4f);

        for (int i = 0; i < 24; i++)
        {
            SpawnGroundLitter(UnityEngine.Random.Range(-18f, 18f), true);
        }
    }

    private void UpdateGroundLitter(float deltaTime)
    {
        if (groundLitterRoot == null)
        {
            return;
        }

        if (activeGroundLitter.Count < MaxGroundLitter && Time.time >= nextGroundLitterAt)
        {
            SpawnGroundLitter(GroundLitterSpawnX - UnityEngine.Random.Range(0f, 3.5f), false);
            ScheduleNextGroundLitter();
        }

        for (int i = activeGroundLitter.Count - 1; i >= 0; i--)
        {
            GroundLitterItem item = activeGroundLitter[i];
            if (item.Root == null || item.Body == null)
            {
                activeGroundLitter.RemoveAt(i);
                continue;
            }

            UpdateGroundLitterItem(item, deltaTime);
            Vector3 local = item.Root.transform.localPosition;
            if (local.x > GroundLitterRecycleX || local.y < -1.4f)
            {
                RecycleGroundLitter(item);
                activeGroundLitter.RemoveAt(i);
            }
        }
    }

    private void ScheduleNextGroundLitter()
    {
        float fullness = Mathf.InverseLerp(MaxGroundLitter * 0.55f, MaxGroundLitter, activeGroundLitter.Count);
        float delay = UnityEngine.Random.Range(0.45f, 1.35f) + fullness * UnityEngine.Random.Range(1.1f, 3.2f);
        if (underpassWindGust > 0.65f)
        {
            delay *= 0.55f;
        }

        nextGroundLitterAt = Time.time + delay;
    }

    private void SpawnGroundLitter(float x, bool initialScatter)
    {
        if (activeGroundLitter.Count >= MaxGroundLitter)
        {
            return;
        }

        GroundLitterKind kind = ChooseGroundLitterKind();
        GroundLitterItem item = AcquireGroundLitter(kind);
        float z = UnityEngine.Random.Range(-2.85f, 1.65f);
        item.Phase = UnityEngine.Random.Range(0f, 50f);
        item.Speed = GroundLitterBaseSpeed(kind) * UnityEngine.Random.Range(0.78f, 1.28f);
        item.Spin = UnityEngine.Random.Range(55f, 185f) * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
        item.SideSway = UnityEngine.Random.Range(0.04f, 0.18f);
        item.LaneBias = z < -0.45f ? -1f : 1f;
        item.Yaw = UnityEngine.Random.Range(0f, 360f);

        item.Root.transform.localPosition = new Vector3(x, GroundLitterSpawnHeight(kind), z);
        item.Root.transform.localRotation = Quaternion.Euler(0f, item.Yaw, 0f);
        item.Root.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.84f, 1.18f);
        item.Root.SetActive(true);
        ResetGroundLitterBody(item);
        if (item.Kind == GroundLitterKind.Paper)
        {
            ResetGroundPaper(item);
        }

        activeGroundLitter.Add(item);
        if (initialScatter)
        {
            UpdateGroundLitterItem(item, UnityEngine.Random.Range(0.05f, 0.35f));
        }
    }

    private GroundLitterKind ChooseGroundLitterKind()
    {
        float roll = UnityEngine.Random.value;
        if (roll < 0.48f)
        {
            return GroundLitterKind.Paper;
        }

        return roll < 0.72f ? GroundLitterKind.Butt : GroundLitterKind.Can;
    }

    private GroundLitterItem AcquireGroundLitter(GroundLitterKind kind)
    {
        for (int i = pooledGroundLitter.Count - 1; i >= 0; i--)
        {
            GroundLitterItem pooled = pooledGroundLitter[i];
            if (pooled.Kind == kind && pooled.Root != null)
            {
                pooledGroundLitter.RemoveAt(i);
                return pooled;
            }
        }

        return CreateGroundLitterObject(kind);
    }

    private GroundLitterItem CreateGroundLitterObject(GroundLitterKind kind)
    {
        GameObject root = new GameObject("Ground litter " + kind);
        root.transform.SetParent(groundLitterRoot, false);
        root.layer = 2;

        GroundLitterItem item = new GroundLitterItem { Kind = kind, Root = root };
        item.Body = root.AddComponent<Rigidbody>();
        ConfigureGroundLitterBody(item.Body, kind);
        if (kind == GroundLitterKind.Can)
        {
            Material material = GroundLitterMat("litter_dull_can", new Color(0.34f, 0.35f, 0.33f), 0.08f);
            GameObject can = CreateCylinder("Dull crushed can", root.transform, new Vector3(0f, 0.055f, 0f), new Vector3(0.055f, 0.055f, 0.16f), material);
            can.transform.localRotation = Quaternion.Euler(0f, 0f, 88f);
            StripDecorCollider(can);
            CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
            collider.direction = 0;
            collider.radius = 0.065f;
            collider.height = 0.24f;
            collider.center = new Vector3(0f, 0.07f, 0f);
            item.Visual = can.transform;
        }
        else if (kind == GroundLitterKind.Butt)
        {
            Material material = GroundLitterMat("litter_cigarette_butt", new Color(0.58f, 0.50f, 0.36f), 0.02f);
            GameObject butt = CreateCylinder("Wet cigarette butt", root.transform, new Vector3(0f, 0.026f, 0f), new Vector3(0.015f, 0.015f, 0.16f), material);
            butt.transform.localRotation = Quaternion.Euler(0f, 0f, 86f);
            StripDecorCollider(butt);
            CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
            collider.direction = 0;
            collider.radius = 0.018f;
            collider.height = 0.20f;
            collider.center = new Vector3(0f, 0.035f, 0f);
            item.Visual = butt.transform;
        }
        else
        {
            CreateGroundPaperObject(item, root.transform);
        }

        root.SetActive(false);
        return item;
    }

    private void ConfigureGroundLitterBody(Rigidbody body, GroundLitterKind kind)
    {
        body.useGravity = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        body.maxAngularVelocity = kind == GroundLitterKind.Paper ? 2.2f : 12f;
        body.mass = kind == GroundLitterKind.Can ? 0.16f : kind == GroundLitterKind.Butt ? 0.035f : 0.018f;
        body.linearDamping = kind == GroundLitterKind.Can ? 1.15f : kind == GroundLitterKind.Butt ? 1.8f : 2.7f;
        body.angularDamping = kind == GroundLitterKind.Can ? 0.55f : 1.35f;
        if (kind == GroundLitterKind.Paper)
        {
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void CreateGroundPaperObject(GroundLitterItem item, Transform parent)
    {
        GameObject paper = new GameObject("Wind-bent paper scrap");
        paper.transform.SetParent(parent, false);
        paper.transform.localPosition = new Vector3(0f, 0.018f, 0f);
        paper.layer = 2;

        item.PaperSize = new Vector2(UnityEngine.Random.Range(0.22f, 0.38f), UnityEngine.Random.Range(0.15f, 0.28f));
        Mesh mesh = BuildGroundPaperMesh(item.PaperSize.x, item.PaperSize.y, out item.PaperBaseVertices);
        item.PaperMesh = mesh;
        item.PaperDeformedVertices = new Vector3[item.PaperBaseVertices.Length];

        MeshFilter filter = paper.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = paper.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = GroundLitterMat("litter_old_paper", new Color(0.48f, 0.46f, 0.37f), 0.01f);
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = true;
        BoxCollider collider = parent.gameObject.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, 0.022f, 0f);
        collider.size = new Vector3(item.PaperSize.x, 0.035f, item.PaperSize.y);
        item.Visual = paper.transform;
    }

    private Mesh BuildGroundPaperMesh(float width, float depth, out Vector3[] baseVertices)
    {
        const int columns = 3;
        const int rows = 2;
        baseVertices = new Vector3[(columns + 1) * (rows + 1)];
        Vector2[] uv = new Vector2[baseVertices.Length];
        int index = 0;
        for (int y = 0; y <= rows; y++)
        {
            float v = y / (float)rows;
            for (int x = 0; x <= columns; x++)
            {
                float u = x / (float)columns;
                baseVertices[index] = new Vector3((u - 0.5f) * width, 0f, (v - 0.5f) * depth);
                uv[index] = new Vector2(u, v);
                index++;
            }
        }

        List<int> triangles = new List<int>(columns * rows * 6);
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int a = y * (columns + 1) + x;
                int b = a + 1;
                int c = a + columns + 1;
                int d = c + 1;
                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);
                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(d);
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "Ground Paper Litter Mesh";
        mesh.vertices = baseVertices;
        mesh.uv = uv;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void UpdateGroundLitterItem(GroundLitterItem item, float deltaTime)
    {
        float windPower = 0.72f + underpassWindStrength * 1.85f + underpassWindGust * 2.15f;
        Vector3 wind = new Vector3(Mathf.Max(0.28f, underpassWindDirection.x), 0f, underpassWindDirection.z * 0.42f).normalized;
        item.Body.AddForce(wind * item.Speed * windPower, ForceMode.Acceleration);
        item.Body.AddForce(Vector3.down * 0.45f, ForceMode.Acceleration);
        item.Body.AddForce(new Vector3(0f, 0f, Mathf.Sin(Time.time * 0.9f + item.Phase) * item.SideSway * windPower), ForceMode.Acceleration);
        SteerGroundLitter(item, deltaTime);

        if (item.Kind == GroundLitterKind.Paper)
        {
            UpdateGroundPaper(item, windPower);
        }
        else
        {
            float torque = item.Spin * 0.012f * windPower;
            item.Body.AddTorque(new Vector3(0f, 0.35f, 1f) * torque, ForceMode.Acceleration);
            if (item.Visual != null && item.Kind == GroundLitterKind.Butt)
            {
                item.Visual.localRotation = Quaternion.Euler(0f, 0f, 86f + Mathf.Sin(Time.time * 3f + item.Phase) * 8f);
            }
        }
    }

    private void SteerGroundLitter(GroundLitterItem item, float deltaTime)
    {
        Vector3 position = item.Root.transform.localPosition;
        Vector3 velocity = item.Body.linearVelocity;
        if (position.z < -3.08f && velocity.z < 0f)
        {
            item.Body.AddForce(Vector3.forward * Mathf.Abs(velocity.z) * 3.5f, ForceMode.Acceleration);
        }
        else if (position.z > 1.92f && velocity.z > 0f)
        {
            item.Body.AddForce(Vector3.back * Mathf.Abs(velocity.z) * 3.5f, ForceMode.Acceleration);
        }

        AvoidCommuneCore(item, position, deltaTime);
    }

    private void AvoidCommuneCore(GroundLitterItem item, Vector3 position, float deltaTime)
    {
        Vector2 center = new Vector2(0.1f, -0.15f);
        Vector2 planar = new Vector2(position.x, position.z);
        float distance = Vector2.Distance(planar, center);
        if (distance > 2.25f)
        {
            return;
        }

        float targetZ = item.LaneBias < 0f ? -2.35f : 1.46f;
        float strength = (1f - distance / 2.25f) * (1.15f + underpassWindGust);
        Vector3 steer = new Vector3(0.22f, 0f, targetZ - position.z) * strength;
        item.Body.AddForce(steer, ForceMode.Acceleration);
    }

    private void UpdateGroundPaper(GroundLitterItem item, float windPower)
    {
        if (item.PaperMesh == null || item.PaperBaseVertices == null || item.PaperDeformedVertices == null)
        {
            return;
        }

        float flutter = 0.35f + underpassWindStrength + underpassWindGust * 0.9f;
        for (int i = 0; i < item.PaperBaseVertices.Length; i++)
        {
            Vector3 vertex = item.PaperBaseVertices[i];
            float edge = Mathf.Clamp01(Mathf.Abs(vertex.x) * 4.5f + Mathf.Abs(vertex.z) * 6f);
            float wave = Mathf.Sin(Time.time * (2.2f + windPower) + item.Phase + vertex.x * 17f + vertex.z * 13f);
            vertex.y = 0.004f + edge * (0.008f + wave * 0.014f * flutter);
            item.PaperDeformedVertices[i] = vertex;
        }

        item.PaperMesh.vertices = item.PaperDeformedVertices;
        item.PaperMesh.RecalculateNormals();
        item.PaperMesh.RecalculateBounds();
        item.Body.AddTorque(Vector3.up * Mathf.Sin(Time.time * 0.8f + item.Phase) * 0.08f * windPower, ForceMode.Acceleration);
    }

    private void ResetGroundPaper(GroundLitterItem item)
    {
        if (item.PaperMesh == null || item.PaperBaseVertices == null)
        {
            return;
        }

        item.PaperMesh.vertices = item.PaperBaseVertices;
        item.PaperMesh.RecalculateNormals();
        item.PaperMesh.RecalculateBounds();
    }

    private void RecycleGroundLitter(GroundLitterItem item)
    {
        if (item.Body != null)
        {
            item.Body.linearVelocity = Vector3.zero;
            item.Body.angularVelocity = Vector3.zero;
        }

        item.Root.SetActive(false);
        pooledGroundLitter.Add(item);
    }

    private void ResetGroundLitterBody(GroundLitterItem item)
    {
        if (item.Body == null)
        {
            return;
        }

        item.Body.linearVelocity = new Vector3(UnityEngine.Random.Range(0.02f, 0.18f), 0f, UnityEngine.Random.Range(-0.04f, 0.04f));
        item.Body.angularVelocity = Vector3.zero;
        item.Body.WakeUp();
    }

    private float GroundLitterSpawnHeight(GroundLitterKind kind)
    {
        if (kind == GroundLitterKind.Can)
        {
            return 0.13f;
        }

        return kind == GroundLitterKind.Butt ? 0.07f : 0.055f;
    }

    private float GroundLitterBaseSpeed(GroundLitterKind kind)
    {
        if (kind == GroundLitterKind.Paper)
        {
            return 0.82f;
        }

        return kind == GroundLitterKind.Butt ? 0.56f : 0.32f;
    }

    private Material GroundLitterMat(string key, Color color, float smoothness)
    {
        Material material = Mat(key, color);
        SetFloat(material, "_Cull", (float)CullMode.Off);
        SetFloat(material, "_Smoothness", smoothness);
        SetFloat(material, "_Glossiness", smoothness);
        SetFloat(material, "_Metallic", 0f);
        SetFloat(material, "_SpecularHighlights", 0f);
        SetFloat(material, "_EnvironmentReflections", 0f);
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", Color.black);
        }

        material.DisableKeyword("_EMISSION");
        material.doubleSidedGI = false;
        return material;
    }

    private void EnsureGroundLitterCollider()
    {
        if (groundLitterCollider != null || worldRoot == null)
        {
            return;
        }

        GameObject floor = new GameObject("Invisible ground litter physics floor");
        floor.transform.SetParent(worldRoot, false);
        floor.transform.localPosition = new Vector3(0f, -0.065f, -0.55f);
        floor.layer = 2;
        groundLitterCollider = floor.AddComponent<BoxCollider>();
        groundLitterCollider.size = new Vector3(56f, 0.12f, 8.2f);
        groundLitterCollider.center = Vector3.zero;
    }
}
