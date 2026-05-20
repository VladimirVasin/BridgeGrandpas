using System.Collections.Generic;

public sealed partial class BridgeGrandpasGame
{
    private void SavePendingObservationCards(List<ObservationCardSaveData> saved)
    {
        if (saved == null)
        {
            return;
        }

        for (int i = 0; i < observationCards.Count; i++)
        {
            ObservationCard card = observationCards[i];
            if (card == null || card.Applying || string.IsNullOrWhiteSpace(card.Text))
            {
                continue;
            }

            saved.Add(new ObservationCardSaveData
            {
                Label = card.Label,
                Text = card.Text,
                CreatedAt = card.CreatedAt
            });
        }
    }

    private void RestorePendingObservationCards(List<ObservationCardSaveData> saved)
    {
        ClearObservationCards();
        if (saved == null)
        {
            return;
        }

        for (int i = 0; i < saved.Count; i++)
        {
            ObservationCardSaveData card = saved[i];
            if (card == null)
            {
                continue;
            }

            CreateSavedObservationCard(card.Label, card.Text, card.CreatedAt);
        }
    }
}
