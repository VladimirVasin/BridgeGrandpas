using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float ObservationPageCornerSize = 84f;

    private void CreateNotebookObservationPageCorners()
    {
        notebookPreviousObservationCorner = CreateObservationPageCorner(
            "Observation Previous Page Corner",
            notebookLeftPage,
            false,
            delegate { TurnObservationPages(-1); });
        notebookNextObservationCorner = CreateObservationPageCorner(
            "Observation Next Page Corner",
            notebookRightPage,
            true,
            delegate { TurnObservationPages(1); });
        RefreshObservationPageCorners();
    }

    private RectTransform CreateObservationPageCorner(string name, RectTransform page, bool next, UnityEngine.Events.UnityAction action)
    {
        RectTransform hitArea = CreatePanel(name, page, new Color(0f, 0f, 0f, 0.01f));
        hitArea.anchorMin = new Vector2(next ? 1f : 0f, 0f);
        hitArea.anchorMax = hitArea.anchorMin;
        hitArea.pivot = new Vector2(next ? 1f : 0f, 0f);
        hitArea.anchoredPosition = new Vector2(next ? -12f : 12f, 12f);
        hitArea.sizeDelta = new Vector2(ObservationPageCornerSize, ObservationPageCornerSize);

        Button button = hitArea.gameObject.AddComponent<Button>();
        button.targetGraphic = hitArea.GetComponent<Image>();
        button.onClick.AddListener(action);
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0f, 0f, 0f, 0.01f);
        colors.highlightedColor = new Color(0f, 0f, 0f, 0.01f);
        colors.pressedColor = new Color(0f, 0f, 0f, 0.01f);
        colors.selectedColor = new Color(0f, 0f, 0f, 0.01f);
        colors.disabledColor = new Color(0f, 0f, 0f, 0f);
        button.colors = colors;
        hitArea.gameObject.AddComponent<BridgeGrandpasHudButtonAudio>();

        CreateObservationPageCornerFold(hitArea, next);
        hitArea.gameObject.SetActive(false);
        return hitArea;
    }

    private void CreateObservationPageCornerFold(RectTransform parent, bool next)
    {
        Vector2 anchor = new Vector2(next ? 1f : 0f, 0f);

        RectTransform shadow = CreateObservationPageCornerTriangle("Fold Shadow", parent, next, 62f, new Color(0.16f, 0.075f, 0.025f, 0.26f));
        shadow.anchoredPosition = new Vector2(next ? 4f : -4f, -4f);
        CreateObservationPageCornerTriangle("Fold Paper", parent, next, 56f, new Color(0.88f, 0.78f, 0.57f, 0.98f));

        RectTransform crease = CreatePanel("Fold Crease", parent, new Color(0.29f, 0.18f, 0.075f, 0.30f));
        crease.anchorMin = anchor;
        crease.anchorMax = anchor;
        crease.pivot = new Vector2(0.5f, 0.5f);
        crease.anchoredPosition = new Vector2(next ? -34f : 34f, 34f);
        crease.sizeDelta = new Vector2(56f, 2f);
        crease.localRotation = Quaternion.Euler(0f, 0f, next ? -45f : 45f);
        crease.GetComponent<Image>().raycastTarget = false;

        Text marker = CreateText("Fold Direction", parent, 19, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.18f, 0.09f, 0.035f, 0.72f));
        marker.text = next ? ">" : "<";
        marker.rectTransform.anchorMin = anchor;
        marker.rectTransform.anchorMax = anchor;
        marker.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        marker.rectTransform.anchoredPosition = new Vector2(next ? -20f : 20f, 20f);
        marker.rectTransform.sizeDelta = new Vector2(32f, 32f);
        marker.raycastTarget = false;
    }

    private RectTransform CreateObservationPageCornerTriangle(string name, RectTransform parent, bool next, float size, Color color)
    {
        GameObject triangleObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(BridgeGrandpasFoldedPageCornerGraphic));
        triangleObject.transform.SetParent(parent, false);
        RectTransform triangle = triangleObject.GetComponent<RectTransform>();
        triangle.anchorMin = new Vector2(next ? 1f : 0f, 0f);
        triangle.anchorMax = triangle.anchorMin;
        triangle.pivot = triangle.anchorMin;
        triangle.anchoredPosition = Vector2.zero;
        triangle.sizeDelta = new Vector2(size, size);
        BridgeGrandpasFoldedPageCornerGraphic graphic = triangleObject.GetComponent<BridgeGrandpasFoldedPageCornerGraphic>();
        graphic.Setup(next, color);
        graphic.raycastTarget = false;
        return triangle;
    }

    private void RefreshObservationPageCorners()
    {
        if (notebookPreviousObservationCorner == null || notebookNextObservationCorner == null)
        {
            return;
        }

        bool observationsOpen = currentNotebookPage == NotebookPage.Observations;
        if (observationsOpen)
        {
            EnsureObservationSpreadStartDay();
        }

        notebookPreviousObservationCorner.gameObject.SetActive(observationsOpen && CanTurnObservationPagesBackward());
        notebookNextObservationCorner.gameObject.SetActive(observationsOpen && CanTurnObservationPagesForward());
    }

    private void ResetObservationSpreadToLatest()
    {
        EnsureArchiveObservations();
        observationSpreadStartDay = LatestObservationSpreadStartDay();
    }

    private void EnsureObservationSpreadStartDay()
    {
        int latestStartDay = LatestObservationSpreadStartDay();
        if (observationSpreadStartDay <= 0)
        {
            observationSpreadStartDay = latestStartDay;
        }

        observationSpreadStartDay = Mathf.Clamp(observationSpreadStartDay, FirstArchiveObservationDay, latestStartDay);
    }

    private int LatestObservationSpreadStartDay()
    {
        int lastDay = LastNotebookObservationDay();
        int distanceFromFirstDay = Mathf.Max(0, lastDay - FirstArchiveObservationDay);
        return lastDay - distanceFromFirstDay % 2;
    }

    private int LastNotebookObservationDay()
    {
        int lastDay = CurrentObservationDay;
        for (int i = 0; i < notebookObservations.Count; i++)
        {
            lastDay = Mathf.Max(lastDay, notebookObservations[i].Day);
        }

        return Mathf.Max(FirstArchiveObservationDay, lastDay);
    }

    private bool CanTurnObservationPagesBackward()
    {
        return observationSpreadStartDay > FirstArchiveObservationDay;
    }

    private bool CanTurnObservationPagesForward()
    {
        return observationSpreadStartDay + 2 <= LastNotebookObservationDay();
    }

    private void TurnObservationPages(int direction)
    {
        EnsureObservationSpreadStartDay();
        if (direction < 0)
        {
            if (!CanTurnObservationPagesBackward())
            {
                return;
            }

            observationSpreadStartDay -= 2;
            notebookPageFlipDirection = -1;
        }
        else
        {
            if (!CanTurnObservationPagesForward())
            {
                return;
            }

            observationSpreadStartDay += 2;
            notebookPageFlipDirection = 1;
        }

        notebookPageFlip = 1f;
        RefreshNotebookUi();
    }
}

public sealed class BridgeGrandpasFoldedPageCornerGraphic : MaskableGraphic
{
    private bool nextCorner = true;

    public void Setup(bool next, Color tint)
    {
        nextCorner = next;
        color = tint;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Rect rect = rectTransform.rect;
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = nextCorner
            ? new Vector3(rect.xMax, rect.yMin, 0f)
            : new Vector3(rect.xMin, rect.yMin, 0f);
        vertexHelper.AddVert(vertex);

        vertex.position = nextCorner
            ? new Vector3(rect.xMin, rect.yMin, 0f)
            : new Vector3(rect.xMax, rect.yMin, 0f);
        vertexHelper.AddVert(vertex);

        vertex.position = nextCorner
            ? new Vector3(rect.xMax, rect.yMax, 0f)
            : new Vector3(rect.xMin, rect.yMax, 0f);
        vertexHelper.AddVert(vertex);

        vertexHelper.AddTriangle(0, 1, 2);
    }
}
