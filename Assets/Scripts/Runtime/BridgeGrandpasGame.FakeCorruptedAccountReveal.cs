using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float FakeCorruptedAccountRevealSeconds = 2.0f;

    private Canvas fakeCorruptedAccountCanvas;
    private CanvasGroup fakeCorruptedAccountGroup;
    private Image fakeCorruptedAccountArt;
    private Text fakeCorruptedAccountTitle;
    private Text fakeCorruptedAccountBody;
    private bool fakeCorruptedAccountRevealActive;
    private float fakeCorruptedAccountRevealStartedAt;

    private void BeginFakeCorruptedAccountCardReveal(ObservationCard card)
    {
        if (card == null || fakeCorruptedAccountRevealActive)
        {
            return;
        }

        EnsureFakeCorruptedAccountRevealUi();
        fakeCorruptedAccountRevealActive = true;
        fakeCorruptedAccountRevealStartedAt = Time.unscaledTime;

        SetVhsMode(false);
        SetNotebookMode(false);
        SetWatchMode(false);
        if (card.Button != null)
        {
            card.Button.interactable = false;
        }

        if (fakeCorruptedAccountCanvas != null)
        {
            fakeCorruptedAccountCanvas.gameObject.SetActive(true);
        }

        if (fakeCorruptedAccountGroup != null)
        {
            fakeCorruptedAccountGroup.alpha = 1f;
        }

        if (fakeCorruptedAccountArt != null)
        {
            Sprite sprite = LoadObservationCardArtByKey(FakeCorruptedCardArtName);
            fakeCorruptedAccountArt.sprite = sprite;
            fakeCorruptedAccountArt.color = sprite == null ? new Color(0.18f, 0.08f, 0.06f, 1f) : Color.white;
        }

        string account = FakeCorruptedUserName();
        if (fakeCorruptedAccountTitle != null)
        {
            fakeCorruptedAccountTitle.text = account;
        }

        if (fakeCorruptedAccountBody != null)
        {
            fakeCorruptedAccountBody.text = "наблюдатель записан";
        }

        WriteDebugLog("FAKE_CORRUPTED_CARD", "Corrupted account card clicked. quitIn=" + FakeCorruptedAccountRevealSeconds);
    }

    private bool UpdateFakeCorruptedAccountReveal(float deltaTime)
    {
        if (!fakeCorruptedAccountRevealActive)
        {
            return false;
        }

        float elapsed = Time.unscaledTime - fakeCorruptedAccountRevealStartedAt;
        if (fakeCorruptedAccountGroup != null)
        {
            fakeCorruptedAccountGroup.alpha = Mathf.Clamp01(elapsed / 0.12f);
        }

        if (elapsed >= FakeCorruptedAccountRevealSeconds)
        {
            QuitFromFakeCorruptedAccountCard();
        }

        return true;
    }

    private void EnsureFakeCorruptedAccountRevealUi()
    {
        if (fakeCorruptedAccountCanvas != null)
        {
            return;
        }

        EnsureUiFont();
        GameObject canvasObject = new GameObject("Fake Corrupted Account Card Reveal", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        fakeCorruptedAccountCanvas = canvasObject.GetComponent<Canvas>();
        fakeCorruptedAccountCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fakeCorruptedAccountCanvas.sortingOrder = 420;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        fakeCorruptedAccountGroup = canvasObject.AddComponent<CanvasGroup>();
        RectTransform root = CreatePanel("Corrupted Account Root", canvasObject.transform, new Color(0f, 0f, 0f, 0.96f));
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        RectTransform card = CreatePanel("Corrupted Account Card", root, new Color(0.12f, 0.085f, 0.045f, 1f));
        card.anchorMin = new Vector2(0.5f, 0.5f);
        card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(360f, 560f);
        Outline outline = card.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.86f, 0.08f, 0.04f, 0.85f);
        outline.effectDistance = new Vector2(3f, -3f);

        RectTransform artFrame = CreatePanel("Corrupted Account Art Frame", card, new Color(0.015f, 0.011f, 0.008f, 1f));
        artFrame.anchorMin = new Vector2(0f, 0.26f);
        artFrame.anchorMax = new Vector2(1f, 1f);
        artFrame.offsetMin = new Vector2(22f, 24f);
        artFrame.offsetMax = new Vector2(-22f, -22f);

        fakeCorruptedAccountArt = CreatePanel("Corrupted Account Art", artFrame, Color.black).GetComponent<Image>();
        fakeCorruptedAccountArt.rectTransform.anchorMin = Vector2.zero;
        fakeCorruptedAccountArt.rectTransform.anchorMax = Vector2.one;
        fakeCorruptedAccountArt.rectTransform.offsetMin = new Vector2(6f, 6f);
        fakeCorruptedAccountArt.rectTransform.offsetMax = new Vector2(-6f, -6f);
        fakeCorruptedAccountArt.preserveAspect = true;
        fakeCorruptedAccountArt.raycastTarget = false;

        fakeCorruptedAccountTitle = CreateText("Corrupted Account Title", card, 28, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.70f));
        fakeCorruptedAccountTitle.rectTransform.anchorMin = new Vector2(0f, 0.12f);
        fakeCorruptedAccountTitle.rectTransform.anchorMax = new Vector2(1f, 0.24f);
        fakeCorruptedAccountTitle.rectTransform.offsetMin = new Vector2(22f, 0f);
        fakeCorruptedAccountTitle.rectTransform.offsetMax = new Vector2(-22f, 0f);

        fakeCorruptedAccountBody = CreateText("Corrupted Account Body", card, 17, FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.86f, 0.23f, 0.18f));
        fakeCorruptedAccountBody.rectTransform.anchorMin = new Vector2(0f, 0.02f);
        fakeCorruptedAccountBody.rectTransform.anchorMax = new Vector2(1f, 0.12f);
        fakeCorruptedAccountBody.rectTransform.offsetMin = new Vector2(22f, 0f);
        fakeCorruptedAccountBody.rectTransform.offsetMax = new Vector2(-22f, 0f);

        fakeCorruptedAccountCanvas.gameObject.SetActive(false);
    }

    private void QuitFromFakeCorruptedAccountCard()
    {
        fakeCorruptedAccountRevealActive = false;
        WriteDebugLog("FAKE_CORRUPTED_CARD", "Closing application after corrupted account card.");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
