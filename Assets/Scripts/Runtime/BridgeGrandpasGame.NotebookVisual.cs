using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private Transform notebookWorldRoot;
    private Light notebookFlashlight;

    private void SetupNotebookWorldVisuals()
    {
        if (mainCamera == null)
        {
            return;
        }

        notebookWorldRoot = new GameObject("Observer Notebook Prop").transform;
        notebookWorldRoot.SetParent(mainCamera.transform, false);
        notebookWorldRoot.gameObject.AddComponent<BridgeGrandpasNotebookClickTarget>();
        BoxCollider clickArea = notebookWorldRoot.gameObject.AddComponent<BoxCollider>();
        clickArea.center = new Vector3(0f, 0f, -0.05f);
        clickArea.size = new Vector3(11.8f, 1.9f, 0.58f);
        Material cover = Mat("notebook_world_cover", new Color(0.20f, 0.10f, 0.045f));
        Material paper = Mat("notebook_world_paper", new Color(0.72f, 0.60f, 0.42f));
        Material edge = Mat("notebook_world_page_edges", new Color(0.46f, 0.36f, 0.24f));
        CreateNotebookWorldPart("Cover", new Vector3(0f, 0f, 0.00f), new Vector3(11.2f, 1.56f, 0.12f), cover);
        CreateNotebookWorldPart("Paper left", new Vector3(-2.78f, 0.05f, -0.08f), new Vector3(5.15f, 1.20f, 0.045f), paper);
        CreateNotebookWorldPart("Paper right", new Vector3(2.78f, 0.05f, -0.08f), new Vector3(5.15f, 1.20f, 0.045f), paper);
        CreateNotebookWorldPart("Page center fold", new Vector3(0f, 0.05f, -0.12f), new Vector3(0.08f, 1.22f, 0.055f), edge);
        CreateNotebookWorldPart("Page bottom edge", new Vector3(0f, -0.67f, -0.13f), new Vector3(10.6f, 0.08f, 0.055f), edge);

        GameObject lightObject = new GameObject("Observer Notebook Flashlight");
        lightObject.transform.SetParent(mainCamera.transform, false);
        lightObject.transform.localPosition = new Vector3(-2.2f, -2.25f, 2.2f);
        lightObject.transform.localRotation = Quaternion.Euler(42f, 14f, 0f);
        notebookFlashlight = lightObject.AddComponent<Light>();
        notebookFlashlight.type = LightType.Spot;
        notebookFlashlight.color = new Color(1f, 0.75f, 0.42f);
        notebookFlashlight.spotAngle = 58f;
        notebookFlashlight.range = 5.8f;
        notebookFlashlight.intensity = 0f;
        UpdateNotebookWorldVisuals(0f);
    }

    private void CreateNotebookWorldPart(string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = "Notebook " + name;
        part.transform.SetParent(notebookWorldRoot, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        Renderer renderer = part.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }

    private void UpdateNotebookWorldVisuals(float deltaTime)
    {
        if (notebookWorldRoot == null || mainCamera == null)
        {
            return;
        }

        bool visible = !vhsModeEnabled;
        notebookWorldRoot.gameObject.SetActive(visible);
        float t = Mathf.SmoothStep(0f, 1f, notebookOpenAmount);
        float y = -mainCamera.orthographicSize + Mathf.Lerp(0.85f, 3.15f, t);
        notebookWorldRoot.localPosition = new Vector3(0f, y, 2.55f);
        notebookWorldRoot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-1.2f, 0.35f, t));
        notebookWorldRoot.localScale = new Vector3(1f, Mathf.Lerp(0.86f, 1.18f, t), 1f);

        if (notebookFlashlight != null)
        {
            notebookFlashlight.enabled = visible && notebookOpenAmount > 0.02f;
            notebookFlashlight.intensity = Mathf.Lerp(0f, 2.35f, t);
        }
    }

    private void ApplyNotebookCameraPose(ref Vector3 position, ref Quaternion rotation)
    {
        if (vhsModeEnabled || notebookOpenAmount <= 0.001f)
        {
            return;
        }

        float t = Mathf.SmoothStep(0f, 1f, notebookOpenAmount);
        position += Vector3.down * (0.12f * t) + cameraGroundForward * (0.16f * t);
        rotation = rotation * Quaternion.Euler(7.5f * t, 0f, 0f);
    }
}

public sealed class BridgeGrandpasNotebookClickTarget : MonoBehaviour
{
}
