using UnityEngine;
using UnityEngine.Rendering;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private Transform cameraForegroundRoot;
    private Transform cameraForegroundPanel;
    private Transform cameraForegroundEdge;
    private Transform[] cameraForegroundStreaks = System.Array.Empty<Transform>();

    private void SetupCameraForeground()
    {
        if (mainCamera == null || cameraForegroundRoot != null)
        {
            return;
        }

        cameraForegroundRoot = new GameObject("Camera Wet Foreground Apron").transform;
        cameraForegroundRoot.SetParent(mainCamera.transform, false);
        Material baseMat = CameraForegroundMat("camera_foreground_dark_wet", new Color(0.018f, 0.028f, 0.040f, 0.98f));
        Material edgeMat = CameraForegroundMat("camera_foreground_top_edge", new Color(0.070f, 0.096f, 0.118f, 0.92f));
        Material streakMat = CameraForegroundMat("camera_foreground_wet_streak", new Color(0.052f, 0.086f, 0.112f, 0.44f));

        cameraForegroundPanel = CreateCameraForegroundQuad("Wet dark foreground fill", baseMat);
        cameraForegroundEdge = CreateCameraForegroundQuad("Wet foreground upper edge", edgeMat);
        cameraForegroundStreaks = new[]
        {
            CreateCameraForegroundQuad("Wet foreground streak L", streakMat),
            CreateCameraForegroundQuad("Wet foreground streak C", streakMat),
            CreateCameraForegroundQuad("Wet foreground streak R", streakMat)
        };
        UpdateCameraForeground();
    }

    private void UpdateCameraForeground()
    {
        if (mainCamera == null || cameraForegroundRoot == null || !gameStarted || vhsModeEnabled)
        {
            if (cameraForegroundRoot != null)
            {
                cameraForegroundRoot.gameObject.SetActive(false);
            }

            return;
        }

        cameraForegroundRoot.gameObject.SetActive(!notebookModeEnabled);
        if (notebookModeEnabled)
        {
            return;
        }

        float size = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        float width = size * aspect * 2.18f;
        float fillHeight = size * 0.56f;
        float centerY = -size + fillHeight * 0.48f;
        SetCameraForegroundQuad(cameraForegroundPanel, new Vector3(0f, centerY, 7.5f), new Vector2(width, fillHeight), 0f);
        SetCameraForegroundQuad(cameraForegroundEdge, new Vector3(0f, centerY + fillHeight * 0.50f, 7.48f), new Vector2(width, size * 0.030f), 0f);

        SetCameraForegroundQuad(cameraForegroundStreaks[0], new Vector3(-width * 0.27f, centerY + fillHeight * 0.18f, 7.47f), new Vector2(width * 0.30f, size * 0.020f), -2.5f);
        SetCameraForegroundQuad(cameraForegroundStreaks[1], new Vector3(width * 0.05f, centerY - fillHeight * 0.02f, 7.46f), new Vector2(width * 0.42f, size * 0.018f), 1.0f);
        SetCameraForegroundQuad(cameraForegroundStreaks[2], new Vector3(width * 0.32f, centerY + fillHeight * 0.26f, 7.45f), new Vector2(width * 0.34f, size * 0.022f), -1.2f);
    }

    private Transform CreateCameraForegroundQuad(string name, Material material)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        quad.transform.SetParent(cameraForegroundRoot, false);
        Renderer renderer = quad.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        Collider collider = quad.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        quad.layer = 2;
        return quad.transform;
    }

    private void SetCameraForegroundQuad(Transform quad, Vector3 position, Vector2 scale, float roll)
    {
        if (quad == null)
        {
            return;
        }

        quad.localPosition = position;
        quad.localRotation = Quaternion.Euler(0f, 0f, roll);
        quad.localScale = new Vector3(scale.x, scale.y, 1f);
    }

    private Material CameraForegroundMat(string key, Color color)
    {
        Material material;
        if (materialCache.TryGetValue(key, out material))
        {
            return material;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Transparent");
        }

        material = shader == null ? TransparentMat(key, color) : new Material(shader);
        ApplyColor(material, "_Color", color);
        ApplyColor(material, "_BaseColor", color);
        SetFloat(material, "_Surface", 1f);
        SetFloat(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
        SetFloat(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        SetFloat(material, "_ZWrite", 0f);
        material.renderQueue = (int)RenderQueue.Transparent;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        materialCache[key] = material;
        return material;
    }
}
