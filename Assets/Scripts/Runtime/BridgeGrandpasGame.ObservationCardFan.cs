using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float ObservationCardFanSpacing = 78f;
    private const float ObservationCardFanMaxSpread = 760f;
    private const float ObservationCardHiddenTop = 62f;
    private const float ObservationCardHiddenTopVhs = 108f;
    private const float ObservationCardHoverTop = 258f;
    private const float ObservationCardHoverTopVhs = 294f;

    private ObservationCard hoveredObservationCard;

    private ObservationCard HoveredObservationCard()
    {
        if (observationCards.Count == 0 || observationCardCanvasGroup == null || !observationCardCanvasGroup.blocksRaycasts)
        {
            hoveredObservationCard = null;
            return null;
        }

        Vector2 pointer = GetPointerPosition();
        if (ObservationCardContainsPointer(hoveredObservationCard, pointer))
        {
            return hoveredObservationCard;
        }

        for (int i = observationCards.Count - 1; i >= 0; i--)
        {
            ObservationCard card = observationCards[i];
            if (ObservationCardContainsPointer(card, pointer))
            {
                hoveredObservationCard = card;
                return card;
            }
        }

        hoveredObservationCard = null;
        return null;
    }

    private bool ObservationCardContainsPointer(ObservationCard card, Vector2 pointer)
    {
        return card != null &&
            card.Root != null &&
            !card.Applying &&
            RectTransformUtility.RectangleContainsScreenPoint(card.Root, pointer, null);
    }

    private void UpdateObservationCardSiblingOrder(ObservationCard hoveredCard)
    {
        for (int i = 0; i < observationCards.Count; i++)
        {
            ObservationCard card = observationCards[i];
            if (card != null && card.Root != null && card != hoveredCard)
            {
                card.Root.SetSiblingIndex(i + 1);
            }
        }

        if (hoveredCard != null && hoveredCard.Root != null)
        {
            hoveredCard.Root.SetAsLastSibling();
        }
    }

    private Vector2 ObservationCardDockPosition(int index, int count)
    {
        return ObservationCardDockPosition(index, count, 0f);
    }

    private Vector2 ObservationCardDockPosition(int index, int count, float hoverAmount)
    {
        float normalized = ObservationCardHandT(index, count);
        float spread = count <= 1 ? 0f : Mathf.Min(ObservationCardFanSpacing, ObservationCardFanMaxSpread / (count - 1));
        float x = (index - (count - 1) * 0.5f) * spread;
        x = Mathf.Lerp(x, x * 0.82f, hoverAmount);

        float closedTop = vhsModeEnabled ? ObservationCardHiddenTopVhs : ObservationCardHiddenTop;
        float hoverTop = vhsModeEnabled ? ObservationCardHoverTopVhs : ObservationCardHoverTop;
        float arc = (1f - normalized * normalized) * 16f - Mathf.Abs(normalized) * 9f;
        float top = Mathf.Lerp(closedTop, hoverTop, hoverAmount) + arc;
        float y = top - ObservationCardHeight * 0.5f;
        return new Vector2(x, y);
    }

    private float ObservationCardDockRotation(int index, int count, float hoverAmount)
    {
        float normalized = ObservationCardHandT(index, count);
        float closed = -normalized * 10.5f;
        float open = -normalized * 2.4f;
        return Mathf.Lerp(closed, open, hoverAmount);
    }

    private float ObservationCardDockScale(int index, int count, float hoverAmount)
    {
        float normalized = Mathf.Abs(ObservationCardHandT(index, count));
        return 1f - normalized * 0.035f + hoverAmount * 0.13f;
    }

    private float ObservationCardHandT(int index, int count)
    {
        if (count <= 1)
        {
            return 0f;
        }

        return Mathf.Clamp((index - (count - 1) * 0.5f) / ((count - 1) * 0.5f), -1f, 1f);
    }
}
