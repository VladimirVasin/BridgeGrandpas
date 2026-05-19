using UnityEngine;
using UnityEngine.UI;

public sealed class BridgeGrandpasIrisFadeGraphic : Graphic
{
    public Vector2 CenterLocal;
    public float RadiusPixels;
    public float SoftEdgePixels = 86f;
    public int Segments = 96;

    public void SetIris(Vector2 centerLocal, float radiusPixels)
    {
        CenterLocal = centerLocal;
        RadiusPixels = radiusPixels;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();
        float outerRadius = FarthestCornerDistance(rect, CenterLocal) + SoftEdgePixels + 8f;
        float clearRadius = Mathf.Max(0f, RadiusPixels);
        if (clearRadius >= outerRadius)
        {
            return;
        }

        int segmentCount = Mathf.Max(24, Segments);
        float edgeRadius = Mathf.Min(outerRadius, clearRadius + Mathf.Max(1f, SoftEdgePixels));
        Color transparent = color;
        transparent.a = 0f;
        Color opaque = color;
        AddRing(vh, clearRadius, edgeRadius, transparent, opaque, segmentCount);
        AddRing(vh, edgeRadius, outerRadius, opaque, opaque, segmentCount);
    }

    private void AddRing(VertexHelper vh, float innerRadius, float outerRadius, Color innerColor, Color outerColor, int segmentCount)
    {
        if (outerRadius <= innerRadius + 0.01f)
        {
            return;
        }

        float step = Mathf.PI * 2f / segmentCount;
        for (int i = 0; i < segmentCount; i++)
        {
            float angleA = step * i;
            float angleB = step * (i + 1);
            Vector2 dirA = new Vector2(Mathf.Cos(angleA), Mathf.Sin(angleA));
            Vector2 dirB = new Vector2(Mathf.Cos(angleB), Mathf.Sin(angleB));
            int start = vh.currentVertCount;
            vh.AddVert(CenterLocal + dirA * outerRadius, outerColor, Vector2.zero);
            vh.AddVert(CenterLocal + dirA * innerRadius, innerColor, Vector2.zero);
            vh.AddVert(CenterLocal + dirB * innerRadius, innerColor, Vector2.zero);
            vh.AddVert(CenterLocal + dirB * outerRadius, outerColor, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }
    }

    public float FarthestCornerDistance(Rect rect, Vector2 center)
    {
        float max = 0f;
        max = Mathf.Max(max, Vector2.Distance(center, new Vector2(rect.xMin, rect.yMin)));
        max = Mathf.Max(max, Vector2.Distance(center, new Vector2(rect.xMin, rect.yMax)));
        max = Mathf.Max(max, Vector2.Distance(center, new Vector2(rect.xMax, rect.yMin)));
        max = Mathf.Max(max, Vector2.Distance(center, new Vector2(rect.xMax, rect.yMax)));
        return max;
    }
}
