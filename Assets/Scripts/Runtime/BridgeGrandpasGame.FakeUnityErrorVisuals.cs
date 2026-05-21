using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private void EnsureFakeUnityErrorVisuals()
    {
        if (fakeUnityErrorCanvas != null)
        {
            return;
        }

        EnsureUiFont();
        GameObject canvasObject = new GameObject("Fake Unity Error Overlay", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        fakeUnityErrorCanvas = canvasObject.GetComponent<Canvas>();
        fakeUnityErrorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fakeUnityErrorCanvas.sortingOrder = 360;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        CreateFakeUnityErrorModal(canvasObject.transform);
        CreateFakeUnityErrorGlitch(canvasObject.transform);
        fakeUnityErrorCanvas.gameObject.SetActive(false);
    }

    private void CreateFakeUnityErrorModal(Transform parent)
    {
        fakeUnityErrorModalRoot = CreatePanel("Fake Unity Error Modal Root", parent, new Color(0f, 0f, 0f, 0.12f));
        fakeUnityErrorModalRoot.anchorMin = Vector2.zero;
        fakeUnityErrorModalRoot.anchorMax = Vector2.one;
        fakeUnityErrorModalRoot.offsetMin = Vector2.zero;
        fakeUnityErrorModalRoot.offsetMax = Vector2.zero;
        fakeUnityErrorModalRoot.GetComponent<Image>().raycastTarget = true;
        fakeUnityErrorModalGroup = fakeUnityErrorModalRoot.gameObject.AddComponent<CanvasGroup>();

        fakeUnityErrorDialogRoot = CreatePanel("Unity Error Dialog", fakeUnityErrorModalRoot, new Color(0.925f, 0.925f, 0.925f, 1f));
        fakeUnityErrorDialogRoot.anchorMin = new Vector2(0.5f, 0.5f);
        fakeUnityErrorDialogRoot.anchorMax = new Vector2(0.5f, 0.5f);
        fakeUnityErrorDialogRoot.pivot = new Vector2(0.5f, 0.5f);
        fakeUnityErrorDialogRoot.sizeDelta = new Vector2(620f, 292f);

        Outline dialogOutline = fakeUnityErrorDialogRoot.gameObject.AddComponent<Outline>();
        dialogOutline.effectColor = new Color(0f, 0f, 0f, 0.48f);
        dialogOutline.effectDistance = new Vector2(2f, -2f);

        RectTransform titleBar = CreatePanel("Unity Error Title Bar", fakeUnityErrorDialogRoot, new Color(0.84f, 0.84f, 0.84f, 1f));
        titleBar.anchorMin = new Vector2(0f, 1f);
        titleBar.anchorMax = new Vector2(1f, 1f);
        titleBar.pivot = new Vector2(0.5f, 1f);
        titleBar.sizeDelta = new Vector2(0f, 36f);
        titleBar.anchoredPosition = Vector2.zero;

        Text title = CreateText("Unity Error Title", titleBar, 16, FontStyle.Bold, TextAnchor.MiddleLeft, Color.black);
        title.rectTransform.anchorMin = Vector2.zero;
        title.rectTransform.anchorMax = Vector2.one;
        title.rectTransform.offsetMin = new Vector2(14f, 0f);
        title.rectTransform.offsetMax = new Vector2(-14f, 0f);
        title.text = "Unity Error";

        RectTransform icon = CreatePanel("Unity Error Icon", fakeUnityErrorDialogRoot, new Color(0.78f, 0.12f, 0.08f, 1f));
        icon.anchorMin = new Vector2(0f, 1f);
        icon.anchorMax = new Vector2(0f, 1f);
        icon.pivot = new Vector2(0f, 1f);
        icon.anchoredPosition = new Vector2(32f, -74f);
        icon.sizeDelta = new Vector2(46f, 46f);

        Text mark = CreateText("Unity Error Mark", icon, 34, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        mark.rectTransform.anchorMin = Vector2.zero;
        mark.rectTransform.anchorMax = Vector2.one;
        mark.rectTransform.offsetMin = Vector2.zero;
        mark.rectTransform.offsetMax = Vector2.zero;
        mark.text = "!";
        mark.raycastTarget = false;

        Text body = CreateText("Unity Error Body", fakeUnityErrorDialogRoot, 16, FontStyle.Normal, TextAnchor.UpperLeft, Color.black);
        body.rectTransform.anchorMin = new Vector2(0f, 1f);
        body.rectTransform.anchorMax = new Vector2(1f, 1f);
        body.rectTransform.pivot = new Vector2(0f, 1f);
        body.rectTransform.offsetMin = new Vector2(96f, -208f);
        body.rectTransform.offsetMax = new Vector2(-32f, -58f);
        body.text =
            "UnityPlayer.dll caused an Access Violation (0xC0000005)\n" +
            "in module GrandpaRenderer at 00000000.\n\n" +
            "Object reference not set to an instance of an object:\n" +
            "BridgeGrandpas.Observer.Grandpa[]\n\n" +
            "Press OK to continue.";

        RectTransform okButton = CreateFakeUnityErrorButton(fakeUnityErrorDialogRoot);
        okButton.anchoredPosition = new Vector2(-32f, 28f);
    }

    private RectTransform CreateFakeUnityErrorButton(Transform parent)
    {
        RectTransform rect = CreatePanel("Unity Error OK Button", parent, new Color(0.86f, 0.86f, 0.86f, 1f));
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.sizeDelta = new Vector2(96f, 34f);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.onClick.AddListener(ConfirmFakeUnityError);

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.86f, 0.86f, 0.86f, 1f);
        colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
        colors.pressedColor = new Color(0.70f, 0.70f, 0.70f, 1f);
        button.colors = colors;

        Outline outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.42f);
        outline.effectDistance = new Vector2(1f, -1f);

        Text label = CreateText("Unity Error OK Label", rect, 15, FontStyle.Normal, TextAnchor.MiddleCenter, Color.black);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
        label.text = "OK";
        label.raycastTarget = false;

        return rect;
    }

    private void CreateFakeUnityErrorGlitch(Transform parent)
    {
        fakeUnityErrorGlitchRoot = CreatePanel("Fake Unity Error Return Glitch", parent, new Color(0f, 0f, 0f, 0f));
        fakeUnityErrorGlitchRoot.anchorMin = Vector2.zero;
        fakeUnityErrorGlitchRoot.anchorMax = Vector2.one;
        fakeUnityErrorGlitchRoot.offsetMin = Vector2.zero;
        fakeUnityErrorGlitchRoot.offsetMax = Vector2.zero;
        fakeUnityErrorGlitchRoot.GetComponent<Image>().raycastTarget = false;
        fakeUnityErrorGlitchGroup = fakeUnityErrorGlitchRoot.gameObject.AddComponent<CanvasGroup>();
        fakeUnityErrorGlitchGroup.blocksRaycasts = false;
        fakeUnityErrorGlitchGroup.interactable = false;

        RectTransform back = CreatePanel("Fake Unity Error Glitch Back", fakeUnityErrorGlitchRoot, new Color(0f, 0f, 0f, 0.2f));
        back.anchorMin = Vector2.zero;
        back.anchorMax = Vector2.one;
        back.offsetMin = Vector2.zero;
        back.offsetMax = Vector2.zero;
        back.GetComponent<Image>().raycastTarget = false;
        fakeUnityErrorGlitchBackImage = back.GetComponent<Image>();

        fakeUnityErrorGlitchBandRects = new RectTransform[FakeUnityErrorGlitchBandCount];
        fakeUnityErrorGlitchBandImages = new Image[FakeUnityErrorGlitchBandCount];
        for (int i = 0; i < FakeUnityErrorGlitchBandCount; i++)
        {
            RectTransform band = CreatePanel("Unity Error Return Band " + i, fakeUnityErrorGlitchRoot, Color.clear);
            band.anchorMin = new Vector2(0.5f, 0.5f);
            band.anchorMax = new Vector2(0.5f, 0.5f);
            band.pivot = new Vector2(0.5f, 0.5f);
            band.GetComponent<Image>().raycastTarget = false;
            fakeUnityErrorGlitchBandRects[i] = band;
            fakeUnityErrorGlitchBandImages[i] = band.GetComponent<Image>();
        }

        fakeUnityErrorGlitchBlockRects = new RectTransform[FakeUnityErrorGlitchBlockCount];
        fakeUnityErrorGlitchBlockImages = new Image[FakeUnityErrorGlitchBlockCount];
        for (int i = 0; i < FakeUnityErrorGlitchBlockCount; i++)
        {
            RectTransform block = CreatePanel("Unity Error Return Block " + i, fakeUnityErrorGlitchRoot, Color.clear);
            block.anchorMin = new Vector2(0.5f, 0.5f);
            block.anchorMax = new Vector2(0.5f, 0.5f);
            block.pivot = new Vector2(0.5f, 0.5f);
            block.GetComponent<Image>().raycastTarget = false;
            fakeUnityErrorGlitchBlockRects[i] = block;
            fakeUnityErrorGlitchBlockImages[i] = block.GetComponent<Image>();
        }

        fakeUnityErrorGlitchRoot.gameObject.SetActive(false);
    }

    private void UpdateFakeUnityErrorGlitchBands(int tick, float intensity)
    {
        if (fakeUnityErrorGlitchBandRects == null)
        {
            return;
        }

        float width = Screen.width <= 0 ? 1600f : Screen.width;
        float height = Screen.height <= 0 ? 900f : Screen.height;
        for (int i = 0; i < fakeUnityErrorGlitchBandRects.Length; i++)
        {
            float n = FakeUnityErrorNoise01(tick + i * 11, 17);
            RectTransform rect = fakeUnityErrorGlitchBandRects[i];
            Image image = fakeUnityErrorGlitchBandImages[i];
            rect.anchoredPosition = new Vector2(FakeUnityErrorSignedNoise(tick + i, 19) * (24f + intensity * 90f),
                Mathf.Lerp(-height * 0.5f, height * 0.5f, FakeUnityErrorNoise01(tick + i * 7, 23)));
            rect.sizeDelta = new Vector2(width * 1.25f, Mathf.Lerp(3f, 34f, n) + intensity * 24f);
            image.color = FakeUnityErrorGlitchColor(i, Mathf.Clamp01((0.08f + n * 0.34f) * intensity));
        }
    }

    private void UpdateFakeUnityErrorGlitchBlocks(int tick, float intensity)
    {
        if (fakeUnityErrorGlitchBlockRects == null)
        {
            return;
        }

        float width = Screen.width <= 0 ? 1600f : Screen.width;
        float height = Screen.height <= 0 ? 900f : Screen.height;
        for (int i = 0; i < fakeUnityErrorGlitchBlockRects.Length; i++)
        {
            float n = FakeUnityErrorNoise01(tick + i * 17, 41);
            RectTransform rect = fakeUnityErrorGlitchBlockRects[i];
            Image image = fakeUnityErrorGlitchBlockImages[i];
            rect.anchoredPosition = new Vector2(
                Mathf.Lerp(-width * 0.48f, width * 0.48f, FakeUnityErrorNoise01(tick + i * 5, 43)),
                Mathf.Lerp(-height * 0.46f, height * 0.46f, FakeUnityErrorNoise01(tick + i * 9, 47)));
            rect.sizeDelta = new Vector2(Mathf.Lerp(18f, 210f, n), Mathf.Lerp(4f, 28f, FakeUnityErrorNoise01(tick + i, 53)));
            image.color = FakeUnityErrorGlitchColor(i + 5, Mathf.Clamp01((0.04f + n * 0.46f) * intensity));
        }
    }

    private Color FakeUnityErrorGlitchColor(int index, float alpha)
    {
        switch (index % 4)
        {
            case 0:
                return new Color(0.72f, 0.96f, 1f, alpha);
            case 1:
                return new Color(1f, 0.10f, 0.06f, alpha * 0.88f);
            case 2:
                return new Color(1f, 1f, 1f, alpha * 0.76f);
            default:
                return new Color(0.22f, 0.06f, 0.02f, alpha * 0.92f);
        }
    }

    private float FakeUnityErrorSignedNoise(int seed, int salt)
    {
        return FakeUnityErrorNoise01(seed, salt) * 2f - 1f;
    }

    private float FakeUnityErrorNoise01(int seed, int salt)
    {
        return Mathf.Repeat(Mathf.Sin((seed + 1) * 12.9898f + salt * 78.233f) * 43758.5453f, 1f);
    }

    private bool WasFakeUnityErrorPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F2);
#endif
    }

    private bool WasFakeUnityErrorOkPressed()
    {
        if (!fakeUnityErrorModalActive)
        {
            return false;
        }

#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
    }
}
