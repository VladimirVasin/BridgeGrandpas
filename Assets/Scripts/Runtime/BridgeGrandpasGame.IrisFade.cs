using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float StartIrisFadeDuration = 2.65f;
    private const float StartIrisFadeHold = 0.18f;

    private Canvas irisCanvas;
    private RectTransform irisRect;
    private BridgeGrandpasIrisFadeGraphic irisGraphic;
    private bool startIrisActive;
    private float startIrisElapsed;
    private float startIrisFinalRadius;

    private void SetupStartIrisFade()
    {
        GameObject canvasObject = new GameObject("Old Film Iris Fade", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        irisCanvas = canvasObject.GetComponent<Canvas>();
        irisCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        irisCanvas.sortingOrder = 180;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject irisObject = new GameObject("Iris Fade Graphic", typeof(RectTransform), typeof(CanvasRenderer), typeof(BridgeGrandpasIrisFadeGraphic));
        irisObject.transform.SetParent(canvasObject.transform, false);
        irisRect = irisObject.GetComponent<RectTransform>();
        irisRect.anchorMin = Vector2.zero;
        irisRect.anchorMax = Vector2.one;
        irisRect.offsetMin = Vector2.zero;
        irisRect.offsetMax = Vector2.zero;

        irisGraphic = irisObject.GetComponent<BridgeGrandpasIrisFadeGraphic>();
        irisGraphic.color = Color.black;
        irisGraphic.raycastTarget = false;
        irisCanvas.gameObject.SetActive(false);
    }

    private void BeginStartIrisFade()
    {
        if (irisCanvas == null)
        {
            SetupStartIrisFade();
        }

        CenterCameraOnFireBarrel();
        irisCanvas.gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
        Vector2 center = StartIrisCenterLocal();
        startIrisFinalRadius = StartIrisFinalRadius(center);
        startIrisElapsed = 0f;
        startIrisActive = true;
        irisGraphic.SetIris(center, 0f);
    }

    private void UpdateStartIrisFade(float deltaTime)
    {
        if (!startIrisActive || irisGraphic == null)
        {
            return;
        }

        startIrisElapsed += deltaTime;
        Vector2 center = StartIrisCenterLocal();
        startIrisFinalRadius = Mathf.Max(startIrisFinalRadius, StartIrisFinalRadius(center));
        float t = Mathf.Clamp01((startIrisElapsed - StartIrisFadeHold) / StartIrisFadeDuration);
        float eased = t * t * (3f - 2f * t);
        irisGraphic.SetIris(center, Mathf.Lerp(0f, startIrisFinalRadius, eased));

        if (t >= 1f)
        {
            startIrisActive = false;
            irisCanvas.gameObject.SetActive(false);
        }
    }

    private void CenterCameraOnFireBarrel()
    {
        cameraPanOffset = Vector2.zero;
        cameraPanVelocity = Vector2.zero;
        cameraZoomTarget = CameraDefaultZoom;
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = cameraZoomTarget;
            ApplyCameraPose(true);
        }
    }

    private Vector2 StartIrisCenterLocal()
    {
        if (irisRect == null || mainCamera == null)
        {
            return Vector2.zero;
        }

        Vector3 screenPoint = mainCamera.WorldToScreenPoint(StartIrisWorldCenter());
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(irisRect, screenPoint, null, out local);
        return local;
    }

    private Vector3 StartIrisWorldCenter()
    {
        Building fire;
        if (buildings.TryGetValue(BuildingType.FireBarrel, out fire) && fire.Root != null)
        {
            return fire.Root.transform.position + new Vector3(0f, 0.82f, 0f);
        }

        return new Vector3(0f, 0.82f, -0.1f);
    }

    private float StartIrisFinalRadius(Vector2 center)
    {
        if (irisRect == null || irisGraphic == null)
        {
            return 1200f;
        }

        return irisGraphic.FarthestCornerDistance(irisRect.rect, center) + 120f;
    }
}
