using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float EscapeMenuMadnessRampSeconds = 120f;
    private const float EscapeMenuMadnessVisualDelaySeconds = 20f;
    private const float EscapeMenuMadnessButtonDelaySeconds = 40f;
    private const int EscapeMenuMadnessScanlineCount = 42;
    private const int EscapeMenuMadnessBandCount = 18;
    private const int EscapeMenuMadnessBlockCount = 32;

    private RectTransform escapeMenuMadnessRoot;
    private CanvasGroup escapeMenuMadnessGroup;
    private Image escapeMenuMadnessBlackoutImage;
    private Image[] escapeMenuMadnessScanlineImages;
    private Image[] escapeMenuMadnessBandImages;
    private Image[] escapeMenuMadnessBlockImages;
    private RectTransform[] escapeMenuMadnessBandRects;
    private RectTransform[] escapeMenuMadnessBlockRects;
    private readonly Dictionary<Text, string> escapeMenuMadnessOriginalTexts = new Dictionary<Text, string>();
    private float escapeMenuMadnessOpenedAt;
    private float escapeMenuMadness;
    private bool escapeMenuMadnessDisabledThisOpen;

    private void BeginEscapeMenuMadness()
    {
        EnsureEscapeMenuMadnessVisuals();
        ResetEscapeMenuBsodForNewMenu();
        escapeMenuMadnessOpenedAt = Time.unscaledTime;
        escapeMenuMadness = 0f;
        escapeMenuMadnessDisabledThisOpen = false;
        escapeMenuMadnessOriginalTexts.Clear();
        if (escapeMenuMadnessRoot != null)
        {
            escapeMenuMadnessRoot.gameObject.SetActive(true);
            PlaceEscapeMenuMadnessBehindContent();
        }

        ResetEscapeMenuMadnessButtonVisuals();
        WriteDebugLog("ESCAPE_MADNESS", "Started delayed visual ramp. visualDelay=" + EscapeMenuMadnessVisualDelaySeconds +
            " buttonDelay=" + EscapeMenuMadnessButtonDelaySeconds + " rampSeconds=" + EscapeMenuMadnessRampSeconds);
    }

    private void UpdateEscapeMenuMadness()
    {
        if (!escapeMenuOpen || escapeMenuBsodActive || escapeMenuBsodTriggeredThisOpen || escapeMenuMadnessDisabledThisOpen)
        {
            return;
        }

        EnsureEscapeMenuMadnessVisuals();
        float elapsed = Time.unscaledTime - escapeMenuMadnessOpenedAt;
        escapeMenuMadness = SmoothEscapeMenuMadnessRamp(elapsed, EscapeMenuMadnessVisualDelaySeconds);
        float buttonMadness = SmoothEscapeMenuMadnessRamp(elapsed, EscapeMenuMadnessButtonDelaySeconds);
        int tick = Mathf.FloorToInt(Time.unscaledTime * Mathf.Lerp(6f, 46f, escapeMenuMadness));
        float intensity = escapeMenuMadness * escapeMenuMadness;
        float buttonIntensity = buttonMadness * buttonMadness;
        float burst = EscapeMenuMadnessNoise01(tick, 11) > Mathf.Lerp(0.995f, 0.58f, intensity)
            ? EscapeMenuMadnessNoise01(tick, 12) * intensity
            : 0f;

        UpdateEscapeMenuMadnessOverlay(tick, intensity, burst);
        ApplyEscapeMenuMadnessToLayout(tick, intensity, burst);
        ApplyEscapeMenuMadnessToButtons(tick, buttonIntensity, buttonIntensity <= 0f ? 0f : burst);
        ApplyEscapeMenuMadnessToMenuText(tick, buttonIntensity, buttonIntensity <= 0f ? 0f : burst);

        if (elapsed >= EscapeMenuMadnessRampSeconds)
        {
            BeginEscapeMenuBsod();
        }
    }

    private void EndEscapeMenuMadness()
    {
        ResetEscapeMenuMadnessButtonVisuals();
        if (escapeMenuMadnessRoot != null)
        {
            escapeMenuMadnessRoot.gameObject.SetActive(false);
        }

        if (escapeMenuMadnessGroup != null)
        {
            escapeMenuMadnessGroup.alpha = 0f;
        }

        if (escapeMenuMadnessBlackoutImage != null)
        {
            escapeMenuMadnessBlackoutImage.color = new Color(0f, 0f, 0f, 0f);
        }

        ResetEscapeMenuMadnessTransforms();
        EndEscapeMenuBsod();
        escapeMenuMadness = 0f;
        escapeMenuMadnessDisabledThisOpen = false;
        escapeMenuMadnessOriginalTexts.Clear();
    }

    private void DisableEscapeMenuMadnessAfterBsod()
    {
        escapeMenuMadnessDisabledThisOpen = true;
        escapeMenuMadness = 0f;
        ClearEscapeMenuMadnessOverlay();
        ResetEscapeMenuMadnessButtonVisuals();
        ResetEscapeMenuMadnessTransforms();
        if (escapeMenuMadnessGroup != null)
        {
            escapeMenuMadnessGroup.alpha = 0f;
        }

        if (escapeMenuMadnessBlackoutImage != null)
        {
            escapeMenuMadnessBlackoutImage.color = new Color(0f, 0f, 0f, 0f);
        }

        if (escapeMenuMadnessRoot != null)
        {
            escapeMenuMadnessRoot.gameObject.SetActive(false);
        }
    }

    private void EnsureEscapeMenuMadnessVisuals()
    {
        if (escapeMenuMadnessRoot != null || startMenuCanvas == null)
        {
            return;
        }

        escapeMenuMadnessRoot = CreatePanel("Escape Menu Madness", startMenuCanvas.transform, new Color(0f, 0f, 0f, 0f));
        escapeMenuMadnessRoot.anchorMin = Vector2.zero;
        escapeMenuMadnessRoot.anchorMax = Vector2.one;
        escapeMenuMadnessRoot.offsetMin = Vector2.zero;
        escapeMenuMadnessRoot.offsetMax = Vector2.zero;
        escapeMenuMadnessRoot.GetComponent<Image>().raycastTarget = false;
        escapeMenuMadnessGroup = escapeMenuMadnessRoot.gameObject.AddComponent<CanvasGroup>();
        escapeMenuMadnessGroup.interactable = false;
        escapeMenuMadnessGroup.blocksRaycasts = false;

        RectTransform blackout = CreatePanel("Escape Menu Blackout Pulse", escapeMenuMadnessRoot, new Color(0f, 0f, 0f, 0f));
        blackout.anchorMin = Vector2.zero;
        blackout.anchorMax = Vector2.one;
        blackout.offsetMin = Vector2.zero;
        blackout.offsetMax = Vector2.zero;
        blackout.GetComponent<Image>().raycastTarget = false;
        escapeMenuMadnessBlackoutImage = blackout.GetComponent<Image>();

        escapeMenuMadnessScanlineImages = new Image[EscapeMenuMadnessScanlineCount];
        for (int i = 0; i < escapeMenuMadnessScanlineImages.Length; i++)
        {
            RectTransform line = CreatePanel("Escape Madness Scanline " + i, escapeMenuMadnessRoot, new Color(0.7f, 0.95f, 1f, 0f));
            line.anchorMin = new Vector2(0f, i / (float)EscapeMenuMadnessScanlineCount);
            line.anchorMax = new Vector2(1f, line.anchorMin.y);
            line.pivot = new Vector2(0.5f, 0.5f);
            line.sizeDelta = new Vector2(0f, 1f + i % 4);
            line.GetComponent<Image>().raycastTarget = false;
            escapeMenuMadnessScanlineImages[i] = line.GetComponent<Image>();
        }

        escapeMenuMadnessBandRects = new RectTransform[EscapeMenuMadnessBandCount];
        escapeMenuMadnessBandImages = new Image[EscapeMenuMadnessBandCount];
        for (int i = 0; i < escapeMenuMadnessBandRects.Length; i++)
        {
            RectTransform band = CreatePanel("Escape Madness Tear " + i, escapeMenuMadnessRoot, new Color(1f, 1f, 1f, 0f));
            band.anchorMin = new Vector2(0f, 0.5f);
            band.anchorMax = new Vector2(1f, 0.5f);
            band.pivot = new Vector2(0.5f, 0.5f);
            band.GetComponent<Image>().raycastTarget = false;
            escapeMenuMadnessBandRects[i] = band;
            escapeMenuMadnessBandImages[i] = band.GetComponent<Image>();
        }

        escapeMenuMadnessBlockRects = new RectTransform[EscapeMenuMadnessBlockCount];
        escapeMenuMadnessBlockImages = new Image[EscapeMenuMadnessBlockCount];
        for (int i = 0; i < escapeMenuMadnessBlockRects.Length; i++)
        {
            RectTransform block = CreatePanel("Escape Madness Dropout " + i, escapeMenuMadnessRoot, new Color(1f, 1f, 1f, 0f));
            block.anchorMin = new Vector2(0.5f, 0.5f);
            block.anchorMax = new Vector2(0.5f, 0.5f);
            block.pivot = new Vector2(0.5f, 0.5f);
            block.GetComponent<Image>().raycastTarget = false;
            escapeMenuMadnessBlockRects[i] = block;
            escapeMenuMadnessBlockImages[i] = block.GetComponent<Image>();
        }

        escapeMenuMadnessRoot.gameObject.SetActive(false);
    }

    private void PlaceEscapeMenuMadnessBehindContent()
    {
        if (escapeMenuMadnessRoot == null)
        {
            return;
        }

        if (startMenuContentRoot != null && startMenuContentRoot.parent == escapeMenuMadnessRoot.parent)
        {
            escapeMenuMadnessRoot.SetSiblingIndex(startMenuContentRoot.GetSiblingIndex());
            return;
        }

        escapeMenuMadnessRoot.SetAsFirstSibling();
    }

    private void UpdateEscapeMenuMadnessOverlay(int tick, float intensity, float burst)
    {
        if (escapeMenuMadnessGroup == null || escapeMenuMadnessRoot == null)
        {
            return;
        }

        if (intensity <= 0f)
        {
            escapeMenuMadnessGroup.alpha = 0f;
            escapeMenuMadnessBlackoutImage.color = new Color(0f, 0f, 0f, 0f);
            ClearEscapeMenuMadnessOverlay();
            return;
        }

        escapeMenuMadnessGroup.alpha = Mathf.Clamp01(intensity * 0.78f + burst * 0.20f);
        float blackout = EscapeMenuMadnessNoise01(tick, 21) > Mathf.Lerp(0.998f, 0.90f, intensity)
            ? Mathf.Lerp(0.08f, 0.56f, intensity) + burst * 0.18f
            : 0f;
        escapeMenuMadnessBlackoutImage.color = new Color(0f, 0f, 0f, blackout);

        for (int i = 0; i < escapeMenuMadnessScanlineImages.Length; i++)
        {
            float n = EscapeMenuMadnessNoise01(tick + i * 3, 31);
            float alpha = Mathf.Lerp(0.025f, 0.18f, intensity) + n * intensity * 0.18f + burst * 0.12f;
            escapeMenuMadnessScanlineImages[i].color = new Color(0.74f, 0.97f, 1f, alpha);
        }

        float width = escapeMenuMadnessRoot.rect.width;
        float height = escapeMenuMadnessRoot.rect.height;
        for (int i = 0; i < escapeMenuMadnessBandRects.Length; i++)
        {
            float n = EscapeMenuMadnessNoise01(tick + i * 11, 41);
            RectTransform band = escapeMenuMadnessBandRects[i];
            band.anchoredPosition = new Vector2(EscapeMenuMadnessSignedNoise(tick + i, 42) * (16f + intensity * 80f + burst * 120f),
                Mathf.Lerp(-height * 0.48f, height * 0.48f, EscapeMenuMadnessNoise01(tick + i * 5, 43)));
            band.sizeDelta = new Vector2(0f, Mathf.Lerp(2f, 42f, n) + burst * 26f);
            float alpha = n > Mathf.Lerp(0.62f, 0.24f, intensity) ? Mathf.Lerp(0.04f, 0.34f, intensity) + n * 0.22f + burst * 0.28f : 0f;
            escapeMenuMadnessBandImages[i].color = EscapeMenuMadnessColor(i, alpha);
        }

        for (int i = 0; i < escapeMenuMadnessBlockRects.Length; i++)
        {
            float n = EscapeMenuMadnessNoise01(tick + i * 17, 57);
            RectTransform block = escapeMenuMadnessBlockRects[i];
            block.anchoredPosition = new Vector2(
                Mathf.Lerp(-width * 0.52f, width * 0.52f, EscapeMenuMadnessNoise01(tick + i * 7, 58)),
                Mathf.Lerp(-height * 0.48f, height * 0.48f, EscapeMenuMadnessNoise01(tick + i * 13, 59)));
            block.sizeDelta = new Vector2(Mathf.Lerp(14f, 250f, n) + burst * 110f, Mathf.Lerp(3f, 30f, EscapeMenuMadnessNoise01(tick + i, 60)));
            float alpha = n > Mathf.Lerp(0.80f, 0.38f, intensity) ? Mathf.Lerp(0.04f, 0.42f, intensity) + burst * 0.22f : 0f;
            escapeMenuMadnessBlockImages[i].color = EscapeMenuMadnessColor(i + 4, alpha);
        }
    }

    private float SmoothEscapeMenuMadnessRamp(float elapsed, float delay)
    {
        float raw = Mathf.Clamp01((elapsed - delay) / Mathf.Max(1f, EscapeMenuMadnessRampSeconds - delay));
        return Mathf.SmoothStep(0f, 1f, raw);
    }

    private void ClearEscapeMenuMadnessOverlay()
    {
        if (escapeMenuMadnessScanlineImages != null)
        {
            for (int i = 0; i < escapeMenuMadnessScanlineImages.Length; i++)
            {
                if (escapeMenuMadnessScanlineImages[i] != null)
                {
                    escapeMenuMadnessScanlineImages[i].color = new Color(0f, 0f, 0f, 0f);
                }
            }
        }

        if (escapeMenuMadnessBandImages != null)
        {
            for (int i = 0; i < escapeMenuMadnessBandImages.Length; i++)
            {
                if (escapeMenuMadnessBandImages[i] != null)
                {
                    escapeMenuMadnessBandImages[i].color = new Color(0f, 0f, 0f, 0f);
                }
            }
        }

        if (escapeMenuMadnessBlockImages != null)
        {
            for (int i = 0; i < escapeMenuMadnessBlockImages.Length; i++)
            {
                if (escapeMenuMadnessBlockImages[i] != null)
                {
                    escapeMenuMadnessBlockImages[i].color = new Color(0f, 0f, 0f, 0f);
                }
            }
        }
    }

    private void ApplyEscapeMenuMadnessToLayout(int tick, float intensity, float burst)
    {
        if (intensity <= 0f)
        {
            return;
        }

        float shake = intensity * 24f + burst * 42f;
        if (startMenuBackgroundRect != null)
        {
            startMenuBackgroundRect.anchoredPosition += new Vector2(
                EscapeMenuMadnessSignedNoise(tick, 71) * shake * 1.35f,
                EscapeMenuMadnessSignedNoise(tick, 72) * shake * 0.54f);
            float scale = 1f + intensity * 0.035f + burst * 0.045f;
            startMenuBackgroundRect.localScale = new Vector3(startMenuBackgroundRect.localScale.x * scale, startMenuBackgroundRect.localScale.y, 1f);
        }

        if (startMenuContentRoot != null)
        {
            startMenuContentRoot.anchoredPosition += new Vector2(
                EscapeMenuMadnessSignedNoise(tick, 73) * shake * 0.36f,
                EscapeMenuMadnessSignedNoise(tick, 74) * shake * 0.16f);
            startMenuContentRoot.localRotation = Quaternion.Euler(0f, 0f, EscapeMenuMadnessSignedNoise(tick, 75) * (intensity * 0.75f + burst * 2.4f));
        }

        if (startMenuButtonsRect != null && !saveSlotScreenOpen)
        {
            startMenuButtonsRect.anchoredPosition += new Vector2(
                EscapeMenuMadnessSignedNoise(tick, 76) * (intensity * 18f + burst * 36f),
                EscapeMenuMadnessSignedNoise(tick, 77) * (intensity * 8f + burst * 18f));
            float scale = 1f + EscapeMenuMadnessSignedNoise(tick, 78) * (intensity * 0.018f + burst * 0.040f);
            startMenuButtonsRect.localScale = new Vector3(scale, 1f + burst * 0.025f, 1f);
        }
    }

    private void ResetEscapeMenuMadnessTransforms()
    {
        if (startMenuContentRoot != null)
        {
            startMenuContentRoot.localRotation = Quaternion.identity;
            startMenuContentRoot.localScale = Vector3.one;
        }

        if (startMenuButtonsRect != null)
        {
            startMenuButtonsRect.localRotation = Quaternion.identity;
            startMenuButtonsRect.localScale = Vector3.one;
        }
    }

    private string EscapeMenuCorruptedButtonText(int tick, string original)
    {
        int variant = Mathf.FloorToInt(EscapeMenuMadnessNoise01(tick, 91) * 7f);
        switch (variant)
        {
            case 0:
                return "////";
            case 1:
                return "...";
            case 2:
                return "НАБЛЮДАЙ";
            case 3:
                return "НЕ ВЫХОДИ";
            case 4:
                return "00:00";
            case 5:
                return "????";
            default:
                return original;
        }
    }

    private Color EscapeMenuMadnessButtonTextColor(int index, float intensity, float burst, float alpha)
    {
        if (EscapeMenuMadnessNoise01(index + Mathf.FloorToInt(Time.unscaledTime * 31f), 101) > Mathf.Lerp(0.99f, 0.70f, intensity))
        {
            return index % 2 == 0
                ? new Color(0.45f, 1f, 1f, alpha)
                : new Color(1f, 0.26f, 0.18f, alpha);
        }

        float dim = 1f - intensity * 0.20f + burst * 0.12f;
        return new Color(dim, dim, dim, alpha);
    }

    private Color EscapeMenuMadnessColor(int index, float alpha)
    {
        if (index % 5 == 0)
        {
            return new Color(1f, 0.10f, 0.08f, alpha);
        }

        if (index % 3 == 0)
        {
            return new Color(0.10f, 0.92f, 1f, alpha);
        }

        return new Color(0.90f, 0.95f, 1f, alpha);
    }

    private float EscapeMenuMadnessSignedNoise(int tick, int salt)
    {
        return EscapeMenuMadnessNoise01(tick, salt) * 2f - 1f;
    }

    private float EscapeMenuMadnessNoise01(int tick, int salt)
    {
        return Mathf.Repeat(Mathf.Sin((tick + 1) * (14.371f + salt * 5.913f)) * 46340.753f, 1f);
    }
}
