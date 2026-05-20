using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private static readonly string[] EscapeMenuButtonNightmareTexts =
    {
        "НЕ ВЫХОДИ",
        "НАБЛЮДАЙ",
        "ОСТАНЬСЯ",
        "ДЕНЬ 4",
        "ОНИ СЛЫШАТ",
        "СОХРАНИ СЕБЯ",
        "НАЗАД ПОД МОСТ",
        "НЕ ЗАКРЫВАЙ ГЛАЗА"
    };

    private static readonly string[] EscapeMenuTitleNightmareTexts =
    {
        "ОНИ УЖЕ ЗДЕСЬ",
        "МОСТ ПОМНИТ",
        "ДЕДЫ СЧИТАЮТ",
        "НЕ СМОТРИ НАЗАД",
        "ТЫ ТОЖЕ ЗАПИСАН"
    };

    private static readonly string[] EscapeMenuSubtitleNightmareTexts =
    {
        "камера пишет даже когда ты моргаешь",
        "под мостом считают твои вдохи",
        "блокнот помнит страницы, которых не было",
        "если закрыть меню, они станут ближе",
        "у костра уже приготовили место"
    };

    private string escapeMenuMadnessOriginalTitleText;
    private string escapeMenuMadnessOriginalSubtitleText;
    private bool escapeMenuMadnessTitleCaptured;
    private bool escapeMenuMadnessSubtitleCaptured;

    private void ApplyEscapeMenuMadnessToButtons(int tick, float intensity, float burst)
    {
        if (startMenuCanvas == null)
        {
            return;
        }

        Button[] buttons = startMenuCanvas.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
            {
                continue;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            CanvasGroup group = button.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = button.gameObject.AddComponent<CanvasGroup>();
            }

            Text label = button.GetComponentInChildren<Text>(true);
            CaptureEscapeMenuMadnessOriginalText(label);
            if (intensity <= 0f)
            {
                group.alpha = 1f;
                if (rect != null)
                {
                    rect.localRotation = Quaternion.identity;
                    rect.localScale = Vector3.one;
                }

                RestoreEscapeMenuMadnessButtonText(label);
                continue;
            }

            float dropoutNoise = EscapeMenuMadnessNoise01(tick + i * 23, 81);
            float dropoutThreshold = Mathf.Lerp(0.996f, 0.76f, intensity);
            group.alpha = dropoutNoise > dropoutThreshold ? Mathf.Lerp(0.08f, 0.36f, EscapeMenuMadnessNoise01(tick + i, 82)) : 1f;

            if (rect != null)
            {
                float angle = EscapeMenuMadnessSignedNoise(tick + i, 83) * (intensity * 1.8f + burst * 4.5f);
                float scaleX = 1f + EscapeMenuMadnessSignedNoise(tick + i, 84) * (intensity * 0.030f + burst * 0.070f);
                float scaleY = 1f + EscapeMenuMadnessSignedNoise(tick + i, 85) * (intensity * 0.018f + burst * 0.045f);
                rect.localRotation = Quaternion.Euler(0f, 0f, angle);
                rect.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            if (label != null)
            {
                float corruptNoise = EscapeMenuMadnessNoise01(tick + i * 19, 86);
                bool corrupt = corruptNoise > Mathf.Lerp(0.995f, 0.82f, intensity);
                string original = escapeMenuMadnessOriginalTexts[label];
                label.text = corrupt ? PickEscapeMenuMadnessText(EscapeMenuButtonNightmareTexts, tick + i, 91, original) : original;
                RectTransform labelRect = label.rectTransform;
                labelRect.anchoredPosition = new Vector2(
                    EscapeMenuMadnessSignedNoise(tick + i, 87) * (intensity * 5f + burst * 12f),
                    EscapeMenuMadnessSignedNoise(tick + i, 88) * (intensity * 2f + burst * 5f));
                label.color = EscapeMenuMadnessButtonTextColor(i, intensity, burst, group.alpha);
            }
        }
    }

    private void ApplyEscapeMenuMadnessToMenuText(int tick, float intensity, float burst)
    {
        if (intensity <= 0f)
        {
            RestoreEscapeMenuMadnessMenuText();
            return;
        }

        ApplyEscapeMenuMadnessToMenuLabel(startMenuTitleRect, true, tick + 121, intensity, burst);
        ApplyEscapeMenuMadnessToMenuLabel(startMenuSubtitleRect, false, tick + 137, intensity, burst);
    }

    private void ApplyEscapeMenuMadnessToMenuLabel(RectTransform rect, bool title, int tick, float intensity, float burst)
    {
        if (rect == null)
        {
            return;
        }

        Text label = rect.GetComponent<Text>();
        if (label == null)
        {
            return;
        }

        CaptureEscapeMenuMadnessMenuText(label, title);
        int slowTick = Mathf.FloorToInt((Time.unscaledTime - escapeMenuMadnessOpenedAt) * Mathf.Lerp(0.7f, 4.6f, intensity));
        float replaceThreshold = Mathf.Lerp(title ? 0.998f : 0.996f, title ? 0.70f : 0.62f, intensity);
        bool replace = EscapeMenuMadnessNoise01(slowTick + tick, title ? 141 : 151) > replaceThreshold;
        if (burst > 0.10f && EscapeMenuMadnessNoise01(tick, title ? 142 : 152) > 0.38f)
        {
            replace = true;
        }

        string original = title ? escapeMenuMadnessOriginalTitleText : escapeMenuMadnessOriginalSubtitleText;
        string[] variants = title ? EscapeMenuTitleNightmareTexts : EscapeMenuSubtitleNightmareTexts;
        label.text = replace ? PickEscapeMenuMadnessText(variants, slowTick + tick, title ? 143 : 153, original) : original;
    }

    private void CaptureEscapeMenuMadnessMenuText(Text label, bool title)
    {
        if (title)
        {
            if (!escapeMenuMadnessTitleCaptured)
            {
                escapeMenuMadnessOriginalTitleText = label.text;
                escapeMenuMadnessTitleCaptured = true;
            }

            return;
        }

        if (!escapeMenuMadnessSubtitleCaptured)
        {
            escapeMenuMadnessOriginalSubtitleText = label.text;
            escapeMenuMadnessSubtitleCaptured = true;
        }
    }

    private void RestoreEscapeMenuMadnessMenuText()
    {
        if (escapeMenuMadnessTitleCaptured && startMenuTitleRect != null)
        {
            Text title = startMenuTitleRect.GetComponent<Text>();
            if (title != null)
            {
                title.text = escapeMenuMadnessOriginalTitleText;
            }
        }

        if (escapeMenuMadnessSubtitleCaptured && startMenuSubtitleRect != null)
        {
            Text subtitle = startMenuSubtitleRect.GetComponent<Text>();
            if (subtitle != null)
            {
                subtitle.text = escapeMenuMadnessOriginalSubtitleText;
            }
        }
    }

    private void CaptureEscapeMenuMadnessOriginalText(Text label)
    {
        if (label != null && !escapeMenuMadnessOriginalTexts.ContainsKey(label))
        {
            escapeMenuMadnessOriginalTexts.Add(label, label.text);
        }
    }

    private void RestoreEscapeMenuMadnessButtonText(Text label)
    {
        if (label == null)
        {
            return;
        }

        string original;
        if (escapeMenuMadnessOriginalTexts.TryGetValue(label, out original))
        {
            label.text = original;
        }

        label.rectTransform.anchoredPosition = Vector2.zero;
        label.color = Color.white;
    }

    private void ResetEscapeMenuMadnessButtonVisuals()
    {
        RestoreEscapeMenuMadnessMenuText();
        foreach (KeyValuePair<Text, string> pair in escapeMenuMadnessOriginalTexts)
        {
            if (pair.Key != null)
            {
                pair.Key.text = pair.Value;
                pair.Key.rectTransform.anchoredPosition = Vector2.zero;
                pair.Key.color = Color.white;
            }
        }

        if (startMenuCanvas == null)
        {
            return;
        }

        Button[] buttons = startMenuCanvas.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null)
            {
                continue;
            }

            RectTransform rect = buttons[i].GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
            }

            CanvasGroup group = buttons[i].GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
            }
        }
    }

    private string PickEscapeMenuMadnessText(string[] variants, int tick, int salt, string fallback)
    {
        if (variants == null || variants.Length == 0)
        {
            return fallback;
        }

        int index = Mathf.FloorToInt(EscapeMenuMadnessNoise01(tick, salt) * variants.Length);
        return variants[Mathf.Clamp(index, 0, variants.Length - 1)];
    }
}
