using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private Text vhsObservationText;
    private RectTransform vhsFocusTrack;
    private RectTransform vhsFocusFill;

    private void CreateVhsObservationReadout()
    {
        vhsObservationText = CreateVhsText("VHS Observation", vhsFrameRoot, 16, TextAnchor.MiddleCenter, new Color(0.82f, 0.96f, 0.90f));
        PlaceVhsText(vhsObservationText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -82f), new Vector2(620f, 28f));
        vhsFocusTrack = CreateVhsPanel("VHS Focus Track", vhsFrameRoot, new Color(0.10f, 0.22f, 0.18f, 0.56f));
        vhsFocusTrack.anchorMin = new Vector2(0.5f, 0f);
        vhsFocusTrack.anchorMax = new Vector2(0.5f, 0f);
        vhsFocusTrack.pivot = new Vector2(0.5f, 0.5f);
        vhsFocusTrack.anchoredPosition = new Vector2(0f, 68f);
        vhsFocusTrack.sizeDelta = new Vector2(240f, 8f);
        vhsFocusFill = CreateVhsPanel("VHS Focus Fill", vhsFocusTrack, new Color(0.72f, 1f, 0.78f, 0.88f));
        vhsFocusFill.anchorMin = new Vector2(0f, 0f);
        vhsFocusFill.anchorMax = new Vector2(0f, 1f);
        vhsFocusFill.pivot = new Vector2(0f, 0.5f);
        vhsFocusFill.sizeDelta = new Vector2(0f, 0f);
    }

    private void UpdateVhsObservationReadout()
    {
        if (vhsObservationText != null)
        {
            vhsObservationText.text = BuildVhsObservationReadout();
        }

        if (vhsFocusFill != null)
        {
            vhsFocusFill.sizeDelta = new Vector2(Mathf.Lerp(0f, 240f, VhsObservationProgress01()), 0f);
        }
    }
}
