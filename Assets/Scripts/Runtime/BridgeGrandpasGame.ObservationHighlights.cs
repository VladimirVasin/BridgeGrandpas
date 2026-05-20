using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private RectTransform vhsObservationHighlightRoot;

    private void CreateVhsObservationHighlightLayer()
    {
        if (vhsFrameRoot == null || vhsObservationHighlightRoot != null)
        {
            return;
        }

        vhsObservationHighlightRoot = CreateVhsPanel("VHS Observation Highlights", vhsFrameRoot, new Color(0f, 0f, 0f, 0f));
        vhsObservationHighlightRoot.anchorMin = Vector2.zero;
        vhsObservationHighlightRoot.anchorMax = Vector2.one;
        vhsObservationHighlightRoot.offsetMin = Vector2.zero;
        vhsObservationHighlightRoot.offsetMax = Vector2.zero;
        vhsObservationHighlightRoot.SetAsFirstSibling();
    }

    private void UpdateVhsObservationHighlights()
    {
        if (vhsObservationHighlightRoot == null || mainCamera == null)
        {
            return;
        }

        ObservationLead highlightedLead = FindSingleHighlightedObservationLead();
        for (int i = 0; i < observationLeads.Count; i++)
        {
            ObservationLead lead = observationLeads[i];
            EnsureObservationHighlight(lead);
            Vector2 center = Vector2.zero;
            Vector2 size = Vector2.zero;
            bool visible = lead == highlightedLead &&
                TryGetObservationHighlightRect(lead, out center, out size);

            float targetAlpha = visible ? (lead == activeObservationLead ? 1f : 0.72f) : 0f;
            lead.HighlightGroup.alpha = targetAlpha;
            lead.HighlightRoot.gameObject.SetActive(visible);
            SetObservationObjectOutlineVisible(lead, visible, targetAlpha);
            if (!visible)
            {
                continue;
            }

            Vector2 jitter = new Vector2(
                Mathf.Sin(Time.time * 17f + lead.Id) * 1.4f,
                Mathf.Cos(Time.time * 13f + lead.Id) * 1.0f);
            lead.HighlightRoot.anchoredPosition = center + jitter + new Vector2(0f, size.y * 0.5f + 16f);
            lead.HighlightRoot.sizeDelta = new Vector2(180f, 24f);
            lead.HighlightRoot.localRotation = Quaternion.identity;
            lead.HighlightText.text = lead.Label;
        }
    }

    private ObservationLead FindSingleHighlightedObservationLead()
    {
        if (!vhsModeEnabled || mainCamera == null)
        {
            return null;
        }

        if (activeObservationLead != null && activeObservationLead.State == ObservationLeadState.Queued)
        {
            return activeObservationLead;
        }

        ObservationLead orderedLead = NextOrderedObservationLead();
        if (orderedLead == null)
        {
            return null;
        }

        Vector3 viewport = mainCamera.WorldToViewportPoint(ObservationLeadPosition(orderedLead));
        if (viewport.z <= 0f || viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f)
        {
            return null;
        }

        return orderedLead;
    }

    private void EnsureObservationHighlight(ObservationLead lead)
    {
        if (lead.HighlightRoot != null || vhsObservationHighlightRoot == null)
        {
            return;
        }

        RectTransform root = CreateVhsPanel("VHS Observation Label " + lead.Id, vhsObservationHighlightRoot, new Color(0f, 0f, 0f, 0f));
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(180f, 24f);

        CanvasGroup group = root.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        Text label = CreateVhsText("VHS Observation Label", root, 13, TextAnchor.MiddleCenter, new Color(0.72f, 1f, 0.80f, 0.96f));
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
        label.text = lead.Label;

        lead.HighlightRoot = root;
        lead.HighlightGroup = group;
        lead.HighlightText = label;
        root.gameObject.SetActive(false);
    }

    private void SetObservationObjectOutlineVisible(ObservationLead lead, bool visible, float alpha)
    {
        if (visible)
        {
            EnsureObservationObjectOutline(lead);
        }

        for (int i = lead.HighlightObjectParts.Count - 1; i >= 0; i--)
        {
            GameObject part = lead.HighlightObjectParts[i];
            if (part == null)
            {
                lead.HighlightObjectParts.RemoveAt(i);
                continue;
            }

            part.SetActive(visible);
        }

        if (visible)
        {
            ApplyObservationOutlineColor(alpha);
        }
    }

    private void EnsureObservationObjectOutline(ObservationLead lead)
    {
        if (lead == null || lead.Target == null || lead.HighlightObjectParts.Count > 0)
        {
            return;
        }

        MeshRenderer[] renderers = lead.Target.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer source = renderers[i];
            if (IsObservationOutlinePart(source))
            {
                continue;
            }

            MeshFilter filter = source.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
            {
                continue;
            }

            GameObject part = new GameObject("VHS Object Contour");
            part.transform.SetParent(source.transform, false);
            part.transform.localPosition = Vector3.zero;
            part.transform.localRotation = Quaternion.identity;
            part.transform.localScale = Vector3.one * 1.055f;
            part.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
            MeshRenderer outline = part.AddComponent<MeshRenderer>();
            ConfigureObservationOutlineRenderer(outline);
            lead.HighlightObjectParts.Add(part);
            part.SetActive(false);
        }

        SkinnedMeshRenderer[] skinned = lead.Target.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < skinned.Length; i++)
        {
            SkinnedMeshRenderer source = skinned[i];
            if (IsObservationOutlinePart(source) || source.sharedMesh == null)
            {
                continue;
            }

            GameObject part = new GameObject("VHS Object Contour");
            part.transform.SetParent(source.transform, false);
            part.transform.localPosition = Vector3.zero;
            part.transform.localRotation = Quaternion.identity;
            part.transform.localScale = Vector3.one * 1.045f;
            SkinnedMeshRenderer outline = part.AddComponent<SkinnedMeshRenderer>();
            outline.sharedMesh = source.sharedMesh;
            outline.bones = source.bones;
            outline.rootBone = source.rootBone;
            outline.updateWhenOffscreen = true;
            ConfigureObservationOutlineRenderer(outline);
            lead.HighlightObjectParts.Add(part);
            part.SetActive(false);
        }
    }

    private bool IsObservationOutlinePart(Renderer renderer)
    {
        return renderer != null && renderer.gameObject.name == "VHS Object Contour";
    }

    private void ConfigureObservationOutlineRenderer(Renderer renderer)
    {
        renderer.sharedMaterial = ObservationObjectOutlineMaterial();
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private Material ObservationObjectOutlineMaterial()
    {
        Material material = TransparentGlowMat("vhs_object_contour", new Color(0.60f, 1f, 0.72f, 0.46f));
        SetFloat(material, "_Cull", (float)CullMode.Front);
        SetFloat(material, "_ZWrite", 0f);
        material.renderQueue = (int)RenderQueue.Transparent + 12;
        return material;
    }

    private void ApplyObservationOutlineColor(float alpha)
    {
        Color color = new Color(0.60f, 1f, 0.72f, Mathf.Lerp(0.30f, 0.62f, alpha));
        Material material = ObservationObjectOutlineMaterial();
        ApplyColor(material, "_Color", color);
        ApplyColor(material, "_BaseColor", color);
    }

    private bool TryGetObservationHighlightRect(ObservationLead lead, out Vector2 center, out Vector2 size)
    {
        center = Vector2.zero;
        size = Vector2.zero;
        if (lead == null || vhsFrameRoot == null || mainCamera == null)
        {
            return false;
        }

        Bounds bounds = ObservationLeadBounds(lead);
        Vector3 viewport = mainCamera.WorldToViewportPoint(bounds.center);
        if (viewport.z <= 0f || viewport.x < -0.08f || viewport.x > 1.08f || viewport.y < -0.08f || viewport.y > 1.08f)
        {
            return false;
        }

        Vector3 extents = bounds.extents;
        Vector3 min = bounds.center - extents;
        Vector3 max = bounds.center + extents;
        Vector3[] corners =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        };

        Vector2 localMin = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 localMax = new Vector2(float.MinValue, float.MinValue);
        bool hasPoint = false;
        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 screen = mainCamera.WorldToScreenPoint(corners[i]);
            if (screen.z <= 0f)
            {
                continue;
            }

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(vhsFrameRoot, screen, null, out local);
            localMin = Vector2.Min(localMin, local);
            localMax = Vector2.Max(localMax, local);
            hasPoint = true;
        }

        if (!hasPoint)
        {
            return false;
        }

        center = (localMin + localMax) * 0.5f;
        size = localMax - localMin + new Vector2(28f, 24f);
        size.x = Mathf.Clamp(size.x, 54f, 360f);
        size.y = Mathf.Clamp(size.y, 42f, 260f);
        return true;
    }

    private Bounds ObservationLeadBounds(ObservationLead lead)
    {
        if (lead.Target != null && lead.Target.gameObject.activeInHierarchy)
        {
            Renderer[] renderers = lead.Target.GetComponentsInChildren<Renderer>();
            bool hasRenderer = false;
            Bounds bounds = new Bounds(lead.Target.position, Vector3.one);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (!renderers[i].enabled || IsObservationOutlinePart(renderers[i]))
                {
                    continue;
                }

                if (!hasRenderer)
                {
                    bounds = renderers[i].bounds;
                    hasRenderer = true;
                }
                else
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            if (hasRenderer)
            {
                return bounds;
            }

            Collider collider = lead.Target.GetComponentInChildren<Collider>();
            if (collider != null)
            {
                return collider.bounds;
            }
        }

        return new Bounds(lead.FallbackPosition + new Vector3(0f, 0.7f, 0f), new Vector3(1.15f, 1.35f, 1.15f));
    }

    private void ClearObservationHighlights()
    {
        for (int i = 0; i < observationLeads.Count; i++)
        {
            ObservationLead lead = observationLeads[i];
            if (lead.HighlightRoot != null)
            {
                Destroy(lead.HighlightRoot.gameObject);
                lead.HighlightRoot = null;
                lead.HighlightGroup = null;
                lead.HighlightText = null;
            }

            for (int j = lead.HighlightObjectParts.Count - 1; j >= 0; j--)
            {
                if (lead.HighlightObjectParts[j] != null)
                {
                    Destroy(lead.HighlightObjectParts[j]);
                }
            }

            lead.HighlightObjectParts.Clear();
        }
    }

    private void HideAllObservationHighlights()
    {
        for (int i = 0; i < observationLeads.Count; i++)
        {
            ObservationLead lead = observationLeads[i];
            if (lead.HighlightGroup != null)
            {
                lead.HighlightGroup.alpha = 0f;
            }

            if (lead.HighlightRoot != null)
            {
                lead.HighlightRoot.gameObject.SetActive(false);
            }

            SetObservationObjectOutlineVisible(lead, false, 0f);
        }
    }
}
