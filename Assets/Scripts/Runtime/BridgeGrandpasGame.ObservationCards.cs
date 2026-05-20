using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float ObservationCardWidth = 246f;
    private const float ObservationCardHeight = 78f;
    private const float ObservationCardSpacing = 10f;
    private const int ObservationCardsPerRow = 4;

    private readonly List<ObservationCard> observationCards = new List<ObservationCard>();
    private Canvas observationCardCanvas;
    private CanvasGroup observationCardCanvasGroup;
    private RectTransform observationCardRoot;
    private RectTransform observationCardDock;
    private Text observationCardDockText;

    private sealed class ObservationCard
    {
        public ObservationLead Lead;
        public string Label;
        public string Text;
        public float CreatedAt;
        public RectTransform Root;
        public CanvasGroup Group;
        public Button Button;
        public Image Background;
        public Text LabelText;
        public Text BodyText;
        public Vector2 SpawnPosition;
        public bool Applying;
        public float ApplyStart;
        public Vector2 ApplyFrom;
    }

    private void SetupObservationCardInterface()
    {
        GameObject canvasObject = new GameObject("Observation Card UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        observationCardCanvas = canvasObject.GetComponent<Canvas>();
        observationCardCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        observationCardCanvas.sortingOrder = 96;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        observationCardCanvasGroup = canvasObject.AddComponent<CanvasGroup>();
        observationCardRoot = CreatePanel("Observation Card Root", canvasObject.transform, new Color(0f, 0f, 0f, 0f));
        observationCardRoot.anchorMin = Vector2.zero;
        observationCardRoot.anchorMax = Vector2.one;
        observationCardRoot.offsetMin = Vector2.zero;
        observationCardRoot.offsetMax = Vector2.zero;
        observationCardRoot.GetComponent<Image>().raycastTarget = false;

        observationCardDock = CreatePanel("Observation Card Dock", observationCardRoot, new Color(0.05f, 0.035f, 0.025f, 0.78f));
        observationCardDock.anchorMin = new Vector2(0.5f, 0f);
        observationCardDock.anchorMax = new Vector2(0.5f, 0f);
        observationCardDock.pivot = new Vector2(0.5f, 0f);
        observationCardDock.anchoredPosition = new Vector2(0f, 18f);
        observationCardDock.sizeDelta = new Vector2(1080f, 116f);
        observationCardDock.GetComponent<Image>().raycastTarget = false;

        observationCardDockText = CreateText("Observation Card Dock Label", observationCardDock, 14, FontStyle.BoldAndItalic,
            TextAnchor.UpperLeft, new Color(0.95f, 0.78f, 0.46f, 0.90f));
        observationCardDockText.rectTransform.anchorMin = new Vector2(0f, 1f);
        observationCardDockText.rectTransform.anchorMax = new Vector2(1f, 1f);
        observationCardDockText.rectTransform.pivot = new Vector2(0.5f, 1f);
        observationCardDockText.rectTransform.offsetMin = new Vector2(18f, -28f);
        observationCardDockText.rectTransform.offsetMax = new Vector2(-18f, -8f);
        observationCardDockText.raycastTarget = false;
        observationCardDockText.text = "Карточки наблюдений";

        ApplyObservationCardDockVisibility();
    }

    private void CreateObservationCard(ObservationLead lead)
    {
        if (lead == null || string.IsNullOrWhiteSpace(lead.Text) || ObservationCardAlreadyExists(lead.Text))
        {
            return;
        }

        ObservationCard card = new ObservationCard
        {
            Lead = lead,
            Label = lead.Label,
            Text = lead.Text,
            CreatedAt = Time.time,
            SpawnPosition = ObservationCardSpawnPosition(lead)
        };

        CreateObservationCardView(card);
        observationCards.Add(card);
        vhsTrackingPulse = Mathf.Max(vhsTrackingPulse, 0.9f);
        ApplyObservationCardDockVisibility();
    }

    private void CreateSavedObservationCard(string label, string text, float createdAt)
    {
        if (string.IsNullOrWhiteSpace(text) || ObservationCardAlreadyExists(text))
        {
            return;
        }

        ObservationCard card = new ObservationCard
        {
            Label = string.IsNullOrWhiteSpace(label) ? "наблюдение" : label,
            Text = text,
            CreatedAt = Time.time - 1f,
            SpawnPosition = ObservationCardDockPosition(observationCards.Count, observationCards.Count + 1)
        };

        CreateObservationCardView(card);
        observationCards.Add(card);
        ApplyObservationCardDockVisibility();
    }

    private void CreateObservationCardView(ObservationCard card)
    {
        if (observationCardRoot == null)
        {
            return;
        }

        RectTransform rect = CreatePanel("Observation Card - " + card.Label, observationCardRoot, new Color(0.78f, 0.67f, 0.48f, 0.98f));
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(ObservationCardWidth, ObservationCardHeight);
        rect.anchoredPosition = card.SpawnPosition;

        Image image = rect.GetComponent<Image>();
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(delegate { StartObservationCardApplication(card); });
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.78f, 0.67f, 0.48f, 0.98f);
        colors.highlightedColor = new Color(0.90f, 0.76f, 0.50f, 1f);
        colors.pressedColor = new Color(0.66f, 0.50f, 0.30f, 1f);
        colors.disabledColor = new Color(0.36f, 0.31f, 0.25f, 0.76f);
        button.colors = colors;

        Outline outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.14f, 0.08f, 0.03f, 0.60f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
        rect.gameObject.AddComponent<BridgeGrandpasHudButtonAudio>();

        Text label = CreateText("Card Label", rect, 14, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.15f, 0.075f, 0.035f));
        label.text = card.Label;
        label.rectTransform.anchorMin = new Vector2(0f, 1f);
        label.rectTransform.anchorMax = new Vector2(1f, 1f);
        label.rectTransform.offsetMin = new Vector2(12f, -27f);
        label.rectTransform.offsetMax = new Vector2(-12f, -6f);
        label.raycastTarget = false;

        Text body = CreateText("Card Body", rect, 12, FontStyle.Italic, TextAnchor.UpperLeft, new Color(0.18f, 0.095f, 0.04f));
        body.text = ShortObservationCardText(card.Text);
        body.rectTransform.anchorMin = Vector2.zero;
        body.rectTransform.anchorMax = Vector2.one;
        body.rectTransform.offsetMin = new Vector2(12f, 9f);
        body.rectTransform.offsetMax = new Vector2(-12f, -30f);
        body.raycastTarget = false;

        card.Root = rect;
        card.Background = image;
        card.LabelText = label;
        card.BodyText = body;
        card.Group = rect.gameObject.AddComponent<CanvasGroup>();
        card.Button = button;
    }

    private string ShortObservationCardText(string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= 82)
        {
            return text;
        }

        return text.Substring(0, 79) + "...";
    }

    private void UpdateObservationCards(float deltaTime)
    {
        UpdateObservationCardDock(deltaTime);
        UpdateObservationCardButtons();

        for (int i = 0; i < observationCards.Count; i++)
        {
            ObservationCard card = observationCards[i];
            if (card.Root == null)
            {
                continue;
            }

            if (card.Applying)
            {
                if (UpdateObservationCardApplication(card))
                {
                    CompleteObservationCardApplication(card);
                    i--;
                }

                continue;
            }

            Vector2 target = ObservationCardDockPosition(i, observationCards.Count);
            float age = Mathf.Clamp01((Time.time - card.CreatedAt) / 0.48f);
            float follow = age < 1f ? Mathf.SmoothStep(0f, 1f, age) : 1f - Mathf.Exp(-deltaTime * 12f);
            Vector2 from = age < 1f ? card.SpawnPosition : card.Root.anchoredPosition;
            card.Root.anchoredPosition = Vector2.Lerp(from, target, follow);
            card.Root.localScale = Vector3.one * Mathf.Lerp(0.86f, 1f, age);
            card.Group.alpha = 1f;
            ApplyObservationCardVisualState(card);
        }
    }

    private void UpdateObservationCardDock(float deltaTime)
    {
        if (observationCardDock == null)
        {
            return;
        }

        int rows = Mathf.Max(1, Mathf.CeilToInt(observationCards.Count / (float)ObservationCardsPerRow));
        float targetHeight = observationCards.Count == 0 ? 0f : 40f + rows * (ObservationCardHeight + ObservationCardSpacing);
        observationCardDock.sizeDelta = Vector2.Lerp(observationCardDock.sizeDelta, new Vector2(1080f, targetHeight), 1f - Mathf.Exp(-deltaTime * 12f));
        observationCardDock.anchoredPosition = Vector2.Lerp(
            observationCardDock.anchoredPosition,
            new Vector2(0f, vhsModeEnabled ? 108f : 18f),
            1f - Mathf.Exp(-deltaTime * 10f));
        ApplyObservationCardDockVisibility();
    }

    private void ApplyObservationCardDockVisibility()
    {
        bool hasCards = observationCards.Count > 0;
        if (observationCardCanvasGroup != null)
        {
            observationCardCanvasGroup.alpha = hasCards ? 1f : 0f;
            observationCardCanvasGroup.blocksRaycasts = hasCards;
            observationCardCanvasGroup.interactable = hasCards;
        }

        if (observationCardDockText != null)
        {
            observationCardDockText.text = notebookModeEnabled ? "Карточки наблюдений: вклеить в блокнот" : "Карточки наблюдений ждут блокнот";
            observationCardDockText.color = vhsModeEnabled
                ? new Color(1f, 0.86f, 0.48f, 1f)
                : new Color(0.95f, 0.78f, 0.46f, 0.90f);
        }
    }

    private void UpdateObservationCardButtons()
    {
        bool canClick = true;
        for (int i = 0; i < observationCards.Count; i++)
        {
            ObservationCard card = observationCards[i];
            if (card.Button != null)
            {
                card.Button.interactable = canClick && !card.Applying;
            }
        }
    }

    private Vector2 ObservationCardDockPosition(int index, int count)
    {
        int row = index / ObservationCardsPerRow;
        int column = index % ObservationCardsPerRow;
        int rowStart = row * ObservationCardsPerRow;
        int rowCount = Mathf.Min(ObservationCardsPerRow, Mathf.Max(1, count - rowStart));
        float step = ObservationCardWidth + ObservationCardSpacing;
        float x = (column - (rowCount - 1) * 0.5f) * step;
        float y = (vhsModeEnabled ? 70f : 64f) + row * (ObservationCardHeight + ObservationCardSpacing);
        return new Vector2(x, y);
    }

    private void ApplyObservationCardVisualState(ObservationCard card)
    {
        if (card == null || card.Applying)
        {
            return;
        }

        if (vhsModeEnabled && !notebookModeEnabled)
        {
            if (card.Background != null)
            {
                card.Background.color = new Color(0.92f, 0.75f, 0.42f, 0.98f);
            }

            if (card.LabelText != null)
            {
                card.LabelText.color = new Color(0.07f, 0.045f, 0.025f, 1f);
                card.LabelText.fontSize = 15;
                card.LabelText.fontStyle = FontStyle.Bold;
            }

            if (card.BodyText != null)
            {
                card.BodyText.color = new Color(0.09f, 0.055f, 0.025f, 1f);
                card.BodyText.fontSize = 13;
                card.BodyText.fontStyle = FontStyle.Bold;
            }

            return;
        }

        if (card.Background != null)
        {
            card.Background.color = new Color(0.78f, 0.67f, 0.48f, 0.98f);
        }

        if (card.LabelText != null)
        {
            card.LabelText.color = new Color(0.15f, 0.075f, 0.035f);
            card.LabelText.fontSize = 14;
            card.LabelText.fontStyle = FontStyle.Bold;
        }

        if (card.BodyText != null)
        {
            card.BodyText.color = new Color(0.18f, 0.095f, 0.04f);
            card.BodyText.fontSize = 12;
            card.BodyText.fontStyle = FontStyle.Italic;
        }
    }

    private Vector2 ObservationCardSpawnPosition(ObservationLead lead)
    {
        if (mainCamera == null || observationCardRoot == null || lead == null)
        {
            return new Vector2(0f, 72f);
        }

        Vector3 screen = mainCamera.WorldToScreenPoint(ObservationLeadPosition(lead));
        if (screen.z <= 0f)
        {
            return new Vector2(0f, 72f);
        }

        return ScreenToObservationCardPosition(screen);
    }

    private Vector2 ScreenToObservationCardPosition(Vector2 screen)
    {
        if (observationCardRoot == null)
        {
            return new Vector2(0f, 72f);
        }

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(observationCardRoot, screen, null, out local);
        return new Vector2(local.x, local.y + observationCardRoot.rect.height * 0.5f);
    }

    private void StartObservationCardApplication(ObservationCard card)
    {
        if (card == null || card.Applying || vhsModeEnabled)
        {
            return;
        }

        SetNotebookPage(NotebookPage.Observations);
        if (!notebookModeEnabled)
        {
            SetNotebookMode(true);
        }

        card.Applying = true;
        card.ApplyStart = Time.time;
        card.ApplyFrom = card.Root != null ? card.Root.anchoredPosition : Vector2.zero;
        if (card.Button != null)
        {
            card.Button.interactable = false;
        }
    }

    private bool UpdateObservationCardApplication(ObservationCard card)
    {
        float t = Mathf.Clamp01((Time.time - card.ApplyStart) / 0.52f);
        Vector2 target = ObservationCardNotebookTarget();
        float smooth = Mathf.SmoothStep(0f, 1f, t);
        card.Root.anchoredPosition = Vector2.Lerp(card.ApplyFrom, target, smooth);
        card.Root.localScale = Vector3.one * Mathf.Lerp(1f, 0.44f, smooth);
        card.Root.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -4f, smooth));
        card.Group.alpha = 1f - smooth * 0.92f;
        return t >= 1f;
    }

    private Vector2 ObservationCardNotebookTarget()
    {
        if (notebookRightPage != null)
        {
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, notebookRightPage.position);
            return ScreenToObservationCardPosition(screen);
        }

        return new Vector2(260f, 310f);
    }

    private void CompleteObservationCardApplication(ObservationCard card)
    {
        if (card == null)
        {
            return;
        }

        AddNotebookObservation(card.Text);
        if (card.Lead != null)
        {
            card.Lead.State = ObservationLeadState.Written;
            if (card.Lead.HighlightRoot != null)
            {
                card.Lead.HighlightRoot.gameObject.SetActive(false);
            }
        }

        if (card.Root != null)
        {
            Destroy(card.Root.gameObject);
        }

        observationCards.Remove(card);
        MarkNotebookDirty();
        if (notebookModeEnabled && currentNotebookPage == NotebookPage.Observations)
        {
            RefreshNotebookUi();
        }

        ApplyObservationCardDockVisibility();
    }

    private bool ObservationCardAlreadyExists(string text)
    {
        for (int i = 0; i < observationCards.Count; i++)
        {
            if (observationCards[i].Text == text)
            {
                return true;
            }
        }

        return false;
    }

    private int PendingObservationCardCount()
    {
        return observationCards.Count;
    }

    private void ClearObservationCards()
    {
        for (int i = observationCards.Count - 1; i >= 0; i--)
        {
            if (observationCards[i].Root != null)
            {
                Destroy(observationCards[i].Root.gameObject);
            }
        }

        observationCards.Clear();
        ApplyObservationCardDockVisibility();
    }

}
