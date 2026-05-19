using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private bool TryCreateImportedGrandpaVisual(Grandpa grandpa, Transform parent)
    {
        string resourcePath = ImportedGrandpaResourcePath(grandpa.Role);
        if (string.IsNullOrEmpty(resourcePath))
        {
            return false;
        }

        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            return false;
        }

        GameObject model = Instantiate(prefab, parent);
        model.name = "Imported " + RoleName(grandpa.Role);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;
        DisableImportedRuntimeComponents(model);

        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            Destroy(model);
            return false;
        }

        FitImportedGrandpaModel(parent, model.transform, renderers, grandpa.Role);
        CacheImportedGrandpaParts(grandpa, model.transform);
        grandpa.UsesImportedModel = true;
        grandpa.ImportedModelRoot = model.transform;
        grandpa.Body = model.transform;
        grandpa.ImportedModelBaseScale = model.transform.localScale;
        grandpa.ImportedModelBasePosition = model.transform.localPosition;
        return true;
    }

    private void DisableImportedRuntimeComponents(GameObject model)
    {
        Collider[] colliders = model.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Camera[] cameras = model.GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].enabled = false;
        }

        Light[] lights = model.GetComponentsInChildren<Light>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].enabled = false;
        }

        Animator[] animators = model.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].enabled = false;
        }
    }

    private void FitImportedGrandpaModel(Transform parent, Transform model, Renderer[] renderers, GrandpaRole role)
    {
        Bounds bounds = CombinedBounds(renderers);
        float height = Mathf.Max(bounds.size.y, 0.001f);
        float width = Mathf.Max(bounds.size.x, bounds.size.z);
        Vector2 targetSize = ImportedGrandpaTargetSize(role);
        float scale = Mathf.Min(targetSize.y / height, targetSize.x / Mathf.Max(width, 0.001f));
        model.localScale = Vector3.one * scale;

        bounds = CombinedBounds(renderers);
        Vector3 offset = parent.position - bounds.center;
        offset.y = parent.position.y - bounds.min.y;
        model.position += offset;
    }

    private Vector2 ImportedGrandpaTargetSize(GrandpaRole role)
    {
        if (role == GrandpaRole.SamovarKeeper)
        {
            return new Vector2(0.98f, 1.34f);
        }

        if (role == GrandpaRole.Cardboarder)
        {
            return new Vector2(1.08f, 1.32f);
        }

        if (role == GrandpaRole.Mutterer)
        {
            return new Vector2(0.92f, 1.24f);
        }

        if (role == GrandpaRole.Guard)
        {
            return new Vector2(0.94f, 1.28f);
        }

        if (role == GrandpaRole.Philosopher)
        {
            return new Vector2(0.96f, 1.28f);
        }

        if (role == GrandpaRole.RadioReceiver)
        {
            return new Vector2(1.02f, 1.30f);
        }

        return new Vector2(0.78f, 1.25f);
    }

    private Bounds CombinedBounds(Renderer[] renderers)
    {
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private void CacheImportedGrandpaParts(Grandpa grandpa, Transform modelRoot)
    {
        List<Transform> arms = new List<Transform>();
        List<Transform> legs = new List<Transform>();
        List<float> armSigns = new List<float>();
        List<float> legSigns = new List<float>();
        Transform[] children = modelRoot.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            string name = child.name.ToLowerInvariant();
            if (grandpa.ImportedBodyControl == null && IsBodyControlName(name))
            {
                grandpa.ImportedBodyControl = child;
            }

            if (grandpa.ImportedHead == null && name.Contains("head"))
            {
                grandpa.ImportedHead = child;
            }
            else if (grandpa.ImportedHat == null && (name.Contains("hat") || name.Contains("cap")))
            {
                grandpa.ImportedHat = child;
            }
            else if (grandpa.ImportedBeard == null && (name.Contains("beard") || name.Contains("mustache")))
            {
                grandpa.ImportedBeard = child;
            }
        }

        CacheImportedLimbParts(children, arms, armSigns, "ctrl_arm_l", "ctrl_arm_r", "arm", "hand");
        CacheImportedLimbParts(children, legs, legSigns, "ctrl_leg_l", "ctrl_leg_r", "leg", "foot", "boot");

        grandpa.ImportedArms = arms.ToArray();
        grandpa.ImportedLegs = legs.ToArray();
        grandpa.ImportedArmPhaseSigns = armSigns.ToArray();
        grandpa.ImportedLegPhaseSigns = legSigns.ToArray();
        grandpa.ImportedArmBaseRotations = CaptureBaseRotations(grandpa.ImportedArms);
        grandpa.ImportedLegBaseRotations = CaptureBaseRotations(grandpa.ImportedLegs);
        grandpa.ImportedArmBasePositions = CaptureBasePositions(grandpa.ImportedArms);
        grandpa.ImportedLegBasePositions = CaptureBasePositions(grandpa.ImportedLegs);
        grandpa.ImportedBodyBasePosition = grandpa.ImportedBodyControl != null ? grandpa.ImportedBodyControl.localPosition : Vector3.zero;
        grandpa.ImportedBodyBaseRotation = grandpa.ImportedBodyControl != null ? grandpa.ImportedBodyControl.localRotation : Quaternion.identity;
        grandpa.ImportedHeadBaseRotation = grandpa.ImportedHead != null ? grandpa.ImportedHead.localRotation : Quaternion.identity;
        grandpa.ImportedHatBaseRotation = grandpa.ImportedHat != null ? grandpa.ImportedHat.localRotation : Quaternion.identity;
        grandpa.ImportedBeardBaseRotation = grandpa.ImportedBeard != null ? grandpa.ImportedBeard.localRotation : Quaternion.identity;
    }

    private void CacheImportedLimbParts(
        Transform[] children,
        List<Transform> parts,
        List<float> signs,
        string leftControl,
        string rightControl,
        params string[] tokens)
    {
        if (TryAddControlPair(children, parts, signs, leftControl, rightControl))
        {
            return;
        }

        List<Transform> candidates = new List<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            string name = child.name.ToLowerInvariant();
            if (!HasAnyToken(name, tokens) || IsLimbContainerName(name))
            {
                continue;
            }

            float sign = LimbPhaseSign(name);
            if (Mathf.Approximately(sign, 0f))
            {
                continue;
            }

            if (child.GetComponent<Renderer>() != null ||
                child.GetComponentInChildren<Renderer>(true) != null ||
                name.StartsWith("ctrl_", StringComparison.Ordinal))
            {
                candidates.Add(child);
            }
        }

        candidates.Sort(CompareLimbCandidatePriority);
        for (int i = 0; i < candidates.Count; i++)
        {
            Transform candidate = candidates[i];
            if (HasSelectedLimbRelative(candidate, parts))
            {
                continue;
            }

            parts.Add(candidate);
            signs.Add(LimbPhaseSign(candidate.name.ToLowerInvariant()));
        }
    }

    private bool TryAddControlPair(
        Transform[] children,
        List<Transform> parts,
        List<float> signs,
        string leftControl,
        string rightControl)
    {
        Transform left = FindExactNode(children, leftControl);
        Transform right = FindExactNode(children, rightControl);
        if (left == null || right == null)
        {
            return false;
        }

        parts.Add(left);
        signs.Add(1f);
        parts.Add(right);
        signs.Add(-1f);
        return true;
    }

    private Transform FindExactNode(Transform[] children, string target)
    {
        for (int i = 0; i < children.Length; i++)
        {
            string name = children[i].name;
            if (string.Equals(name, target, StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith(target + "_", StringComparison.OrdinalIgnoreCase))
            {
                return children[i];
            }
        }

        return null;
    }

    private bool HasAnyToken(string name, string[] tokens)
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            if (name.Contains(tokens[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsLimbContainerName(string name)
    {
        return name == "arms" ||
            name == "legs" ||
            name.Contains("root") ||
            name.Contains("body") ||
            name.Contains("action");
    }

    private bool IsBodyControlName(string name)
    {
        return name == "ctrl_body" ||
            name.StartsWith("ctrl_body_", StringComparison.Ordinal) ||
            name == "body";
    }

    private float LimbPhaseSign(string name)
    {
        string compact = name.Replace(' ', '_').Replace('.', '_').Replace('-', '_');
        if (compact.Contains("left") || compact.EndsWith("_l", StringComparison.Ordinal) || compact.Contains("_l_"))
        {
            return 1f;
        }

        if (compact.Contains("right") || compact.EndsWith("_r", StringComparison.Ordinal) || compact.Contains("_r_"))
        {
            return -1f;
        }

        return 0f;
    }

    private int CompareLimbCandidatePriority(Transform left, Transform right)
    {
        int result = LimbCandidatePriority(left).CompareTo(LimbCandidatePriority(right));
        if (result != 0)
        {
            return result;
        }

        return left.GetSiblingIndex().CompareTo(right.GetSiblingIndex());
    }

    private int LimbCandidatePriority(Transform item)
    {
        string name = item.name.ToLowerInvariant();
        if (name.StartsWith("ctrl_", StringComparison.Ordinal))
        {
            return 0;
        }

        if (item.GetComponent<Renderer>() != null)
        {
            return 1;
        }

        return 2;
    }

    private bool HasSelectedLimbRelative(Transform candidate, List<Transform> selected)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            if (candidate.IsChildOf(selected[i]) || selected[i].IsChildOf(candidate))
            {
                return true;
            }
        }

        return false;
    }

    private Quaternion[] CaptureBaseRotations(Transform[] transforms)
    {
        Quaternion[] rotations = new Quaternion[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            rotations[i] = transforms[i].localRotation;
        }

        return rotations;
    }

    private Vector3[] CaptureBasePositions(Transform[] transforms)
    {
        Vector3[] positions = new Vector3[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            positions[i] = transforms[i].localPosition;
        }

        return positions;
    }
}
