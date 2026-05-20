using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private Vector2[] CaptureStartDayIntroLetterPositions(RectTransform[] rects)
    {
        if (rects == null)
        {
            return null;
        }

        Vector2[] positions = new Vector2[rects.Length];
        for (int i = 0; i < rects.Length; i++)
        {
            positions[i] = rects[i] != null ? rects[i].anchoredPosition : Vector2.zero;
        }

        return positions;
    }

    private void ApplyStartDayIntroSubtitleLetterGlitch(float titleElapsed, float baseAlpha)
    {
        if (startDayIntroSubtitleLettersRoot == null || startDayIntroSubtitleLetters == null || startDayIntroSubtitleLetterRects == null)
        {
            return;
        }

        if (startDayIntroSubtitleLetterBasePositions == null ||
            startDayIntroSubtitleLetterBasePositions.Length != startDayIntroSubtitleLetterRects.Length)
        {
            startDayIntroSubtitleLetterBasePositions = CaptureStartDayIntroLetterPositions(startDayIntroSubtitleLetterRects);
        }

        int tick = Mathf.Max(0, Mathf.FloorToInt(titleElapsed * 31f));
        float reveal = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(titleElapsed / 0.74f));
        float zoom = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(titleElapsed / StartDayIntroTitleHold));
        float burst = StartDayIntroNoise01(tick, 151) > 0.58f ? StartDayIntroNoise01(tick, 152) : 0f;
        float smear = StartDayIntroNoise01(tick, 153) > 0.76f ? StartDayIntroNoise01(tick, 154) : 0f;

        float rootX = StartDayIntroSignedNoise(tick, 161) * (burst * 13f + smear * 32f);
        float rootY = StartDayIntroSubtitleY + Mathf.Sin(titleElapsed * 0.43f + 0.8f) * 2.5f +
            StartDayIntroSignedNoise(tick, 162) * burst * 4.5f;
        startDayIntroSubtitleLettersRoot.anchoredPosition = new Vector2(rootX, rootY);
        startDayIntroSubtitleLettersRoot.localRotation = Quaternion.Euler(0f, 0f, StartDayIntroSignedNoise(tick, 163) * burst * 0.85f);
        startDayIntroSubtitleLettersRoot.localScale = new Vector3(1f + zoom * 0.026f + smear * 0.018f, 1f + burst * 0.010f, 1f);

        int count = Mathf.Min(startDayIntroSubtitleLetters.Length, startDayIntroSubtitleLetterRects.Length);
        for (int i = 0; i < count; i++)
        {
            Text letter = startDayIntroSubtitleLetters[i];
            RectTransform rect = startDayIntroSubtitleLetterRects[i];
            if (letter == null || rect == null || string.IsNullOrEmpty(letter.text))
            {
                continue;
            }

            Vector2 basePosition = i < startDayIntroSubtitleLetterBasePositions.Length
                ? startDayIntroSubtitleLetterBasePositions[i]
                : rect.anchoredPosition;

            float delay = StartDayIntroNoise01(i + 37, 164) * 0.42f;
            float letterReveal = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((titleElapsed - delay) / 0.34f));
            float dropout = StartDayIntroNoise01(tick + i * 13, 165) > 0.84f ? 0.18f : 1f;
            float blink = StartDayIntroNoise01(tick + i * 17, 166) > 0.93f ? 0f : 1f;
            Color color = letter.color;
            color.a = Mathf.Clamp01(baseAlpha * reveal * letterReveal * dropout * blink);
            letter.color = color;

            float localSmear = StartDayIntroNoise01(tick + i * 7, 167) > 0.78f ? smear : 0f;
            float xKick = StartDayIntroSignedNoise(tick + i, 168) * (burst * 12f + localSmear * 22f);
            float yKick = StartDayIntroSignedNoise(tick + i, 169) * (burst * 4.5f + localSmear * 2.5f);
            rect.anchoredPosition = basePosition + new Vector2(xKick, yKick);

            float scaleX = 1f + burst * 0.026f + localSmear * 0.070f + (1f - letterReveal) * 0.030f;
            float scaleY = 1f + burst * 0.014f;
            rect.localScale = new Vector3(scaleX, scaleY, 1f);
            rect.localRotation = Quaternion.Euler(0f, 0f, StartDayIntroSignedNoise(tick + i, 170) * (burst * 2.2f + localSmear * 1.4f));
        }

        ApplyStartDayIntroLetterTickLine(startDayIntroSubtitleLetters, startDayIntroSubtitleLetterRects, titleElapsed, 7, 9.5f, 2.0f);
    }

    private void ResetStartDayIntroSubtitleLetterPositions()
    {
        if (startDayIntroSubtitleLetterRects == null || startDayIntroSubtitleLetterBasePositions == null)
        {
            return;
        }

        int count = Mathf.Min(startDayIntroSubtitleLetterRects.Length, startDayIntroSubtitleLetterBasePositions.Length);
        for (int i = 0; i < count; i++)
        {
            if (startDayIntroSubtitleLetterRects[i] != null)
            {
                startDayIntroSubtitleLetterRects[i].anchoredPosition = startDayIntroSubtitleLetterBasePositions[i];
            }
        }
    }
}
