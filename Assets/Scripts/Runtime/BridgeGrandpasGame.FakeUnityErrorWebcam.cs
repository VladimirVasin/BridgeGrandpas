using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float FakeUnityErrorWebcamHoldSeconds = 0.76f;
    private const float FakeUnityErrorWebcamPhotoAtSeconds = 0.16f;
    private const float FakeUnityErrorWebcamPhotoVisibleSeconds = 0.34f;

    private RectTransform fakeUnityErrorWebcamRoot;
    private CanvasGroup fakeUnityErrorWebcamGroup;
    private Text fakeUnityErrorWebcamText;
    private RectTransform fakeUnityErrorWebcamPhotoRoot;
    private CanvasGroup fakeUnityErrorWebcamPhotoGroup;
    private Text fakeUnityErrorWebcamPhotoText;
    private bool fakeUnityErrorWebcamMenuActive;
    private bool fakeUnityErrorWebcamPhotoTaken;
    private float fakeUnityErrorWebcamStartedAt;
    private float fakeUnityErrorWebcamPhotoTakenAt;

    private bool TryBeginFakeUnityErrorWebcamMenuFromEscape()
    {
        if (!fakeUnityErrorGrandpasHidden || fakeUnityErrorWebcamMenuActive || fakeUnityErrorReturnGlitchActive)
        {
            return false;
        }

        BeginFakeUnityErrorWebcamMenu();
        return true;
    }

    private void BeginFakeUnityErrorWebcamMenu()
    {
        EnsureFakeUnityErrorVisuals();
        EnsureFakeUnityErrorWebcamVisuals();
        PauseGameForFakeUnityError();
        fakeUnityErrorWebcamMenuActive = true;
        fakeUnityErrorWebcamPhotoTaken = false;
        fakeUnityErrorWebcamStartedAt = Time.unscaledTime;

        if (fakeUnityErrorCanvas != null)
        {
            fakeUnityErrorCanvas.gameObject.SetActive(true);
        }

        if (fakeUnityErrorWebcamRoot != null)
        {
            fakeUnityErrorWebcamRoot.gameObject.SetActive(true);
            fakeUnityErrorWebcamRoot.SetAsLastSibling();
        }

        if (fakeUnityErrorWebcamGroup != null)
        {
            fakeUnityErrorWebcamGroup.alpha = 1f;
        }

        if (fakeUnityErrorWebcamText != null)
        {
            fakeUnityErrorWebcamText.text = "Заклей вебку";
        }

        if (fakeUnityErrorWebcamPhotoGroup != null)
        {
            fakeUnityErrorWebcamPhotoGroup.alpha = 0f;
        }

        PlayFakeUnityErrorWebcamSound();
        WriteDebugLog("FAKE_UNITY_ERROR", "Fake webcam menu opened from Escape while grandpas were hidden.");
    }

    private void UpdateFakeUnityErrorWebcamMenu(float deltaTime)
    {
        if (!fakeUnityErrorWebcamMenuActive)
        {
            return;
        }

        float elapsed = Time.unscaledTime - fakeUnityErrorWebcamStartedAt;
        if (!fakeUnityErrorWebcamPhotoTaken && elapsed >= FakeUnityErrorWebcamPhotoAtSeconds)
        {
            TriggerFakeUnityErrorWebcamPhoto();
        }

        ApplyFakeUnityErrorWebcamVisuals(elapsed);
        ApplyFakeUnityErrorWebcamPhotoVisuals(elapsed);
        if (elapsed >= FakeUnityErrorWebcamHoldSeconds && !fakeUnityErrorReturnGlitchActive)
        {
            BeginFakeUnityErrorReturnGlitch();
        }
    }

    private void ApplyFakeUnityErrorWebcamVisuals(float elapsed)
    {
        if (fakeUnityErrorWebcamText == null)
        {
            return;
        }

        int tick = Mathf.FloorToInt(Time.unscaledTime * 86f);
        float flash = tick % 3 == 0 ? 0.18f : tick % 7 == 0 ? 0.58f : 1f;
        float slam = 1f + Mathf.Exp(-elapsed / 0.055f) * 0.72f;
        float snap = Mathf.Clamp01(1f - elapsed / FakeUnityErrorWebcamHoldSeconds);
        float jitterX = FakeUnityErrorSignedNoise(tick, 201) * Mathf.Lerp(12f, 40f, snap);
        float jitterY = FakeUnityErrorSignedNoise(tick, 202) * Mathf.Lerp(5f, 18f, snap);

        fakeUnityErrorWebcamText.color = new Color(1f, 1f, 1f, flash);
        fakeUnityErrorWebcamText.rectTransform.anchoredPosition = new Vector2(jitterX, jitterY);
        fakeUnityErrorWebcamText.rectTransform.localScale = new Vector3(slam, slam, 1f);
        fakeUnityErrorWebcamText.rectTransform.localRotation =
            Quaternion.Euler(0f, 0f, FakeUnityErrorSignedNoise(tick, 203) * Mathf.Lerp(0.6f, 4.2f, snap));
    }

    private void TriggerFakeUnityErrorWebcamPhoto()
    {
        fakeUnityErrorWebcamPhotoTaken = true;
        fakeUnityErrorWebcamPhotoTakenAt = Time.unscaledTime;
        if (fakeUnityErrorWebcamPhotoText != null)
        {
            fakeUnityErrorWebcamPhotoText.text = "IMG_23101998_0004.JPG";
        }

        PlayFakeUnityErrorPhotoSound();
        WriteDebugLog("FAKE_UNITY_ERROR", "Fake webcam photo captured inside fake Escape menu.");
    }

    private void ApplyFakeUnityErrorWebcamPhotoVisuals(float elapsed)
    {
        if (!fakeUnityErrorWebcamPhotoTaken || fakeUnityErrorWebcamPhotoRoot == null || fakeUnityErrorWebcamPhotoGroup == null)
        {
            return;
        }

        float photoElapsed = Time.unscaledTime - fakeUnityErrorWebcamPhotoTakenAt;
        float inT = Mathf.Clamp01(photoElapsed / 0.035f);
        float outT = Mathf.Clamp01((photoElapsed - FakeUnityErrorWebcamPhotoVisibleSeconds + 0.12f) / 0.12f);
        float alpha = Mathf.SmoothStep(0f, 1f, inT) * (1f - outT);
        int tick = Mathf.FloorToInt(Time.unscaledTime * 62f);
        fakeUnityErrorWebcamPhotoGroup.alpha = alpha;
        fakeUnityErrorWebcamPhotoRoot.anchoredPosition = new Vector2(
            -70f + FakeUnityErrorSignedNoise(tick, 221) * 2.5f,
            58f + FakeUnityErrorSignedNoise(tick, 222) * 1.5f);
        float scale = 1f + Mathf.Exp(-photoElapsed / 0.045f) * 0.24f;
        fakeUnityErrorWebcamPhotoRoot.localScale = new Vector3(scale, scale, 1f);
        fakeUnityErrorWebcamPhotoRoot.localRotation =
            Quaternion.Euler(0f, 0f, FakeUnityErrorSignedNoise(tick, 223) * 0.8f * alpha);
    }

    private void CompleteFakeUnityErrorWebcamRecovery()
    {
        fakeUnityErrorWebcamMenuActive = false;
        if (fakeUnityErrorWebcamRoot != null)
        {
            fakeUnityErrorWebcamRoot.gameObject.SetActive(false);
        }

        ResumeGameFromFakeUnityError();
        RestoreBackgroundMusicImmediatelyAfterFakeUnityError();
        WriteDebugLog("FAKE_UNITY_ERROR", "Fake webcam menu closed. Grandpas and background music restored.");
    }

    private void EnsureFakeUnityErrorWebcamVisuals()
    {
        if (fakeUnityErrorWebcamRoot != null || fakeUnityErrorCanvas == null)
        {
            return;
        }

        fakeUnityErrorWebcamRoot = CreatePanel("Fake Unity Webcam Menu", fakeUnityErrorCanvas.transform, Color.black);
        fakeUnityErrorWebcamRoot.anchorMin = Vector2.zero;
        fakeUnityErrorWebcamRoot.anchorMax = Vector2.one;
        fakeUnityErrorWebcamRoot.offsetMin = Vector2.zero;
        fakeUnityErrorWebcamRoot.offsetMax = Vector2.zero;
        fakeUnityErrorWebcamRoot.GetComponent<Image>().raycastTarget = true;
        fakeUnityErrorWebcamGroup = fakeUnityErrorWebcamRoot.gameObject.AddComponent<CanvasGroup>();

        fakeUnityErrorWebcamText = CreateText("Fake Unity Webcam Text", fakeUnityErrorWebcamRoot, 86,
            FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        fakeUnityErrorWebcamText.rectTransform.anchorMin = Vector2.zero;
        fakeUnityErrorWebcamText.rectTransform.anchorMax = Vector2.one;
        fakeUnityErrorWebcamText.rectTransform.offsetMin = Vector2.zero;
        fakeUnityErrorWebcamText.rectTransform.offsetMax = Vector2.zero;
        fakeUnityErrorWebcamText.text = "Заклей вебку";
        fakeUnityErrorWebcamText.raycastTarget = false;

        CreateFakeUnityErrorWebcamPhotoPreview(fakeUnityErrorWebcamRoot);
        fakeUnityErrorWebcamRoot.gameObject.SetActive(false);
    }

    private void CreateFakeUnityErrorWebcamPhotoPreview(Transform parent)
    {
        fakeUnityErrorWebcamPhotoRoot = CreatePanel("Fake Unity Webcam Photo Preview", parent, new Color(0.88f, 0.86f, 0.80f, 0.98f));
        fakeUnityErrorWebcamPhotoRoot.anchorMin = new Vector2(1f, 0f);
        fakeUnityErrorWebcamPhotoRoot.anchorMax = new Vector2(1f, 0f);
        fakeUnityErrorWebcamPhotoRoot.pivot = new Vector2(1f, 0f);
        fakeUnityErrorWebcamPhotoRoot.anchoredPosition = new Vector2(-70f, 58f);
        fakeUnityErrorWebcamPhotoRoot.sizeDelta = new Vector2(258f, 178f);
        fakeUnityErrorWebcamPhotoRoot.GetComponent<Image>().raycastTarget = false;
        fakeUnityErrorWebcamPhotoGroup = fakeUnityErrorWebcamPhotoRoot.gameObject.AddComponent<CanvasGroup>();
        fakeUnityErrorWebcamPhotoGroup.interactable = false;
        fakeUnityErrorWebcamPhotoGroup.blocksRaycasts = false;
        fakeUnityErrorWebcamPhotoGroup.alpha = 0f;

        RectTransform image = CreatePanel("Fake Unity Webcam Photo Image", fakeUnityErrorWebcamPhotoRoot, new Color(0.015f, 0.015f, 0.014f, 1f));
        image.anchorMin = new Vector2(0f, 0f);
        image.anchorMax = new Vector2(1f, 1f);
        image.offsetMin = new Vector2(12f, 36f);
        image.offsetMax = new Vector2(-12f, -12f);
        image.GetComponent<Image>().raycastTarget = false;

        RectTransform line = CreatePanel("Fake Unity Webcam Photo Scanline", image, new Color(1f, 1f, 1f, 0.10f));
        line.anchorMin = new Vector2(0f, 0.52f);
        line.anchorMax = new Vector2(1f, 0.52f);
        line.sizeDelta = new Vector2(0f, 2f);
        line.GetComponent<Image>().raycastTarget = false;

        fakeUnityErrorWebcamPhotoText = CreateText("Fake Unity Webcam Photo File", fakeUnityErrorWebcamPhotoRoot, 13,
            FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.08f, 0.07f, 0.06f));
        fakeUnityErrorWebcamPhotoText.rectTransform.anchorMin = new Vector2(0f, 0f);
        fakeUnityErrorWebcamPhotoText.rectTransform.anchorMax = new Vector2(1f, 0f);
        fakeUnityErrorWebcamPhotoText.rectTransform.offsetMin = new Vector2(14f, 5f);
        fakeUnityErrorWebcamPhotoText.rectTransform.offsetMax = new Vector2(-14f, 31f);
        fakeUnityErrorWebcamPhotoText.text = "IMG_23101998_0004.JPG";
        fakeUnityErrorWebcamPhotoText.raycastTarget = false;
    }
}
