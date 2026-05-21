using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float ObservationCardWidth = 172f;
    private const float ObservationCardHeight = 218f;
    private const float ObservationCardSpacing = 14f;
    private const int ObservationCardsPerRow = 5;

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
        public bool CorruptedAccount;
        public RectTransform Root;
        public CanvasGroup Group;
        public Button Button;
        public Image Background;
        public Text BodyText;
        public RectTransform ArtFrame;
        public Image ArtImage;
        public Vector2 SpawnPosition;
        public float HoverAmount;
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
        observationCardDock.sizeDelta = new Vector2(1120f, 266f);
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
            CorruptedAccount = lead.CorruptedAccount,
            SpawnPosition = ObservationCardSpawnPosition(lead)
        };

        CreateObservationCardView(card);
        observationCards.Add(card);
        WriteDebugLog("OBS_CARD", "Created card label=" + card.Label + " text=" + card.Text + " cards=" + observationCards.Count);
        vhsTrackingPulse = Mathf.Max(vhsTrackingPulse, 0.9f);
        ApplyObservationCardDockVisibility();
    }

    private void CreateSavedObservationCard(string label, string text, float createdAt)
    {
        label = UserFacingGrandpaText(label);
        text = UserFacingGrandpaText(text);
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
        WriteDebugLog("OBS_CARD_RESTORE", "Restored pending card label=" + card.Label + " text=" + card.Text + " cards=" + observationCards.Count);
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

        RectTransform artFrame = CreatePanel("Card Pixel Art Frame", rect, new Color(0.11f, 0.075f, 0.045f, 1f));
        artFrame.anchorMin = Vector2.zero;
        artFrame.anchorMax = Vector2.one;
        artFrame.offsetMin = new Vector2(12f, 14f);
        artFrame.offsetMax = new Vector2(-12f, -58f);
        artFrame.GetComponent<Image>().raycastTarget = false;

        Image artImage = CreatePanel("Card Pixel Art", artFrame, new Color(0.20f, 0.16f, 0.11f, 1f)).GetComponent<Image>();
        RectTransform artRect = artImage.rectTransform;
        artRect.anchorMin = Vector2.zero;
        artRect.anchorMax = Vector2.one;
        artRect.offsetMin = new Vector2(4f, 4f);
        artRect.offsetMax = new Vector2(-4f, -4f);
        artImage.raycastTarget = false;
        artImage.preserveAspect = true;
        Sprite sprite = ObservationCardArtSprite(card.Label);
        if (sprite != null)
        {
            artImage.sprite = sprite;
            artImage.color = Color.white;
        }
        else
        {
            CreateObservationCardPlaceholderArt(artRect, card.Label);
        }

        Text caption = CreateText("Card Title", rect, 15, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.12f, 0.065f, 0.032f));
        caption.text = ObservationCardCaption(card);
        caption.rectTransform.anchorMin = new Vector2(0f, 1f);
        caption.rectTransform.anchorMax = Vector2.one;
        caption.rectTransform.offsetMin = new Vector2(10f, -52f);
        caption.rectTransform.offsetMax = new Vector2(-10f, -8f);
        caption.raycastTarget = false;
        caption.horizontalOverflow = HorizontalWrapMode.Wrap;
        caption.verticalOverflow = VerticalWrapMode.Truncate;

        card.Root = rect;
        card.Background = image;
        card.ArtFrame = artFrame;
        card.ArtImage = artImage;
        card.BodyText = caption;
        card.Group = rect.gameObject.AddComponent<CanvasGroup>();
        card.Button = button;
    }

    private void UpdateObservationCards(float deltaTime)
    {
        UpdateObservationCardDock(deltaTime);
        UpdateObservationCardButtons();
        ObservationCard hoveredCard = HoveredObservationCard();
        UpdateObservationCardSiblingOrder(hoveredCard);

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

            float targetHover = card == hoveredCard ? 1f : 0f;
            card.HoverAmount = Mathf.Lerp(card.HoverAmount, targetHover, 1f - Mathf.Exp(-deltaTime * 16f));
            Vector2 target = ObservationCardDockPosition(i, observationCards.Count, card.HoverAmount);
            float age = Mathf.Clamp01((Time.time - card.CreatedAt) / 0.48f);
            float follow = age < 1f ? Mathf.SmoothStep(0f, 1f, age) : 1f - Mathf.Exp(-deltaTime * 12f);
            Vector2 from = age < 1f ? card.SpawnPosition : card.Root.anchoredPosition;
            card.Root.anchoredPosition = Vector2.Lerp(from, target, follow);
            float angle = Mathf.LerpAngle(card.Root.localEulerAngles.z, ObservationCardDockRotation(i, observationCards.Count, card.HoverAmount), 1f - Mathf.Exp(-deltaTime * 14f));
            card.Root.localRotation = Quaternion.Euler(0f, 0f, angle);
            card.Root.localScale = Vector3.one * Mathf.Lerp(0.86f, ObservationCardDockScale(i, observationCards.Count, card.HoverAmount), age);
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

        float targetHeight = observationCards.Count == 0 ? 0f : vhsModeEnabled ? 124f : 94f;
        observationCardDock.sizeDelta = Vector2.Lerp(observationCardDock.sizeDelta, new Vector2(1120f, targetHeight), 1f - Mathf.Exp(-deltaTime * 12f));
        observationCardDock.anchoredPosition = Vector2.Lerp(
            observationCardDock.anchoredPosition,
            new Vector2(0f, vhsModeEnabled ? 68f : 0f),
            1f - Mathf.Exp(-deltaTime * 10f));
        ApplyObservationCardDockVisibility();
    }

    private void ApplyObservationCardDockVisibility()
    {
        bool hasCards = observationCards.Count > 0 && !watchModeEnabled;
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

            if (card.BodyText != null)
            {
                card.BodyText.color = new Color(0.09f, 0.055f, 0.025f, 1f);
                card.BodyText.fontSize = 16;
                card.BodyText.fontStyle = FontStyle.Bold;
            }

            if (card.ArtImage != null && card.ArtImage.sprite != null)
            {
                card.ArtImage.color = Color.white;
            }

            return;
        }

        if (card.Background != null)
        {
            card.Background.color = new Color(0.78f, 0.67f, 0.48f, 0.98f);
        }

        if (card.BodyText != null)
        {
            card.BodyText.color = new Color(0.18f, 0.095f, 0.04f);
            card.BodyText.fontSize = 15;
            card.BodyText.fontStyle = FontStyle.Bold;
        }

        if (card.ArtImage != null && card.ArtImage.sprite != null)
        {
            card.ArtImage.color = Color.white;
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
        if (card == null || card.Applying)
        {
            return;
        }

        if (card.CorruptedAccount)
        {
            BeginFakeCorruptedAccountCardReveal(card);
            return;
        }

        if (vhsModeEnabled)
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
            WriteDebugLog("OBS_APPLY", "Card applied and lead written id=" + card.Lead.Id + " label=" + card.Label +
                " text=" + card.Text);
            if (card.Lead.HighlightRoot != null)
            {
                card.Lead.HighlightRoot.gameObject.SetActive(false);
            }
        }
        else
        {
            WriteDebugLog("OBS_APPLY", "Saved card applied label=" + card.Label + " text=" + card.Text);
        }

        if (card.Root != null)
        {
            Destroy(card.Root.gameObject);
        }

        if (hoveredObservationCard == card)
        {
            hoveredObservationCard = null;
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

        hoveredObservationCard = null;
        observationCards.Clear();
        ApplyObservationCardDockVisibility();
    }

}
