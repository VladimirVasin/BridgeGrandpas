using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const int StartDayIntroTitleMaxLetters = 8;
    private const int StartDayIntroGlitchScanlineCount = 58;
    private const int StartDayIntroGlitchBandCount = 24;
    private const int StartDayIntroGlitchBlockCount = 46;
    private const float StartDayIntroTitleLetterHeight = 430f;

    private RectTransform startDayIntroGlitchRoot;
    private RectTransform startDayIntroTitleLettersRoot;
    private CanvasGroup startDayIntroGlitchGroup;
    private Text[] startDayIntroTitleLetters;
    private RectTransform[] startDayIntroTitleLetterRects;
    private Vector2[] startDayIntroTitleLetterBasePositions;
    private Image[] startDayIntroGlitchScanlineImages;
    private Image[] startDayIntroGlitchBandImages;
    private Image[] startDayIntroGlitchBlockImages;
    private RectTransform[] startDayIntroGlitchBandRects;
    private RectTransform[] startDayIntroGlitchBlockRects;
    private AudioDistortionFilter startDayIntroMusicDistortion;

    private void SetupStartDayIntroGlitch(Transform parent)
    {
        startDayIntroGlitchRoot = CreatePanel("Day Intro VHS Glitch", parent, new Color(0f, 0f, 0f, 0f));
        startDayIntroGlitchRoot.anchorMin = Vector2.zero;
        startDayIntroGlitchRoot.anchorMax = Vector2.one;
        startDayIntroGlitchRoot.offsetMin = Vector2.zero;
        startDayIntroGlitchRoot.offsetMax = Vector2.zero;
        startDayIntroGlitchRoot.SetAsFirstSibling();
        startDayIntroGlitchRoot.GetComponent<Image>().raycastTarget = false;
        startDayIntroGlitchGroup = startDayIntroGlitchRoot.gameObject.AddComponent<CanvasGroup>();
        startDayIntroGlitchGroup.interactable = false;
        startDayIntroGlitchGroup.blocksRaycasts = false;

        startDayIntroGlitchScanlineImages = new Image[StartDayIntroGlitchScanlineCount];
        for (int i = 0; i < startDayIntroGlitchScanlineImages.Length; i++)
        {
            RectTransform line = CreatePanel("Glitch Scanline " + i, startDayIntroGlitchRoot, new Color(1f, 1f, 1f, 0.04f));
            line.anchorMin = new Vector2(0f, i / (float)StartDayIntroGlitchScanlineCount);
            line.anchorMax = new Vector2(1f, line.anchorMin.y);
            line.pivot = new Vector2(0.5f, 0.5f);
            line.anchoredPosition = Vector2.zero;
            line.sizeDelta = new Vector2(0f, 1f + i % 3 * 0.55f);
            line.GetComponent<Image>().raycastTarget = false;
            startDayIntroGlitchScanlineImages[i] = line.GetComponent<Image>();
        }

        startDayIntroGlitchBandRects = new RectTransform[StartDayIntroGlitchBandCount];
        startDayIntroGlitchBandImages = new Image[StartDayIntroGlitchBandCount];
        for (int i = 0; i < startDayIntroGlitchBandRects.Length; i++)
        {
            RectTransform band = CreatePanel("Glitch Tear Band " + i, startDayIntroGlitchRoot, new Color(0.9f, 0.96f, 1f, 0f));
            band.anchorMin = new Vector2(0f, 0.5f);
            band.anchorMax = new Vector2(1f, 0.5f);
            band.pivot = new Vector2(0.5f, 0.5f);
            band.GetComponent<Image>().raycastTarget = false;
            startDayIntroGlitchBandRects[i] = band;
            startDayIntroGlitchBandImages[i] = band.GetComponent<Image>();
        }

        startDayIntroGlitchBlockRects = new RectTransform[StartDayIntroGlitchBlockCount];
        startDayIntroGlitchBlockImages = new Image[StartDayIntroGlitchBlockCount];
        for (int i = 0; i < startDayIntroGlitchBlockRects.Length; i++)
        {
            RectTransform block = CreatePanel("Glitch Dropout " + i, startDayIntroGlitchRoot, new Color(1f, 1f, 1f, 0f));
            block.anchorMin = new Vector2(0.5f, 0.5f);
            block.anchorMax = new Vector2(0.5f, 0.5f);
            block.pivot = new Vector2(0.5f, 0.5f);
            block.GetComponent<Image>().raycastTarget = false;
            startDayIntroGlitchBlockRects[i] = block;
            startDayIntroGlitchBlockImages[i] = block.GetComponent<Image>();
        }

        SetStartDayIntroGlitchVisible(false);
    }

    private void SetupStartDayIntroTitleLetters(Transform parent)
    {
        startDayIntroTitleLettersRoot = CreateStartDayIntroLetterRoot(
            "Day Intro Title Letters",
            parent,
            1460f,
            StartDayIntroTitleLetterHeight,
            StartDayIntroTitleY);

        startDayIntroTitleLetters = new Text[StartDayIntroTitleMaxLetters];
        startDayIntroTitleLetterRects = new RectTransform[StartDayIntroTitleMaxLetters];
        startDayIntroTitleLetterBasePositions = new Vector2[StartDayIntroTitleMaxLetters];
        for (int i = 0; i < StartDayIntroTitleMaxLetters; i++)
        {
            Text letter = CreateText("Day Intro Title Letter " + i, startDayIntroTitleLettersRoot, 430, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            letter.horizontalOverflow = HorizontalWrapMode.Overflow;
            letter.verticalOverflow = VerticalWrapMode.Overflow;
            letter.raycastTarget = false;
            RectTransform rect = letter.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(320f, StartDayIntroTitleLetterHeight);
            startDayIntroTitleLetters[i] = letter;
            startDayIntroTitleLetterRects[i] = rect;
        }

        SetStartDayIntroTitleLetterText("День 4");
        SetStartDayIntroTitleLettersVisible(false);
    }

    private void SetStartDayIntroTitleLetterText(string text)
    {
        if (startDayIntroTitleLetters == null || startDayIntroTitleLetterRects == null)
        {
            return;
        }

        float totalWidth = StartDayIntroTitleTextWidth(text);
        float cursor = -totalWidth * 0.5f;
        for (int i = 0; i < startDayIntroTitleLetters.Length; i++)
        {
            Text letter = startDayIntroTitleLetters[i];
            RectTransform rect = startDayIntroTitleLetterRects[i];
            if (letter == null || rect == null)
            {
                continue;
            }

            if (i >= text.Length)
            {
                letter.text = "";
                rect.gameObject.SetActive(false);
                continue;
            }

            char c = text[i];
            float width = StartDayIntroTitleCharWidth(c);
            letter.text = c == ' ' ? "" : c.ToString();
            rect.gameObject.SetActive(true);
            rect.sizeDelta = new Vector2(width + 160f, StartDayIntroTitleLetterHeight);
            rect.anchoredPosition = new Vector2(cursor + width * 0.5f, 0f);
            startDayIntroTitleLetterBasePositions[i] = rect.anchoredPosition;
            cursor += width;
        }
    }

    private float StartDayIntroTitleTextWidth(string text)
    {
        float width = 0f;
        for (int i = 0; i < text.Length; i++)
        {
            width += StartDayIntroTitleCharWidth(text[i]);
        }

        return width;
    }

    private float StartDayIntroTitleCharWidth(char c)
    {
        if (c == ' ')
        {
            return 138f;
        }

        if (char.IsDigit(c))
        {
            return 300f;
        }

        if (c == 'Д')
        {
            return 284f;
        }

        return c == 'ь' ? 216f : 242f;
    }

    private void UpdateStartDayIntroGlitch(float titleElapsed, bool titlePhase)
    {
        SetStartDayIntroGlitchVisible(titlePhase);
        if (!titlePhase || startDayIntroGlitchRoot == null)
        {
            return;
        }

        int tick = Mathf.Max(0, Mathf.FloorToInt(titleElapsed * 28f));
        float surge = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(titleElapsed / StartDayIntroTitleHold));
        float burst = StartDayIntroNoise01(tick, 93) > 0.62f ? StartDayIntroNoise01(tick, 94) : 0f;
        startDayIntroGlitchGroup.alpha = 0.72f + burst * 0.28f;

        UpdateStartDayIntroGlitchScanlines(tick, surge, burst);
        UpdateStartDayIntroGlitchBands(tick, surge, burst);
        UpdateStartDayIntroGlitchBlocks(tick, surge, burst);
    }

    private void UpdateStartDayIntroGlitchScanlines(int tick, float surge, float burst)
    {
        for (int i = 0; i < startDayIntroGlitchScanlineImages.Length; i++)
        {
            float n = StartDayIntroNoise01(tick + i * 3, 31);
            float alpha = 0.05f + n * 0.10f + burst * 0.13f + surge * 0.04f;
            startDayIntroGlitchScanlineImages[i].color = new Color(0.80f, 0.96f, 1f, alpha);
        }
    }

    private void UpdateStartDayIntroGlitchBands(int tick, float surge, float burst)
    {
        float height = startDayIntroGlitchRoot.rect.height;
        for (int i = 0; i < startDayIntroGlitchBandRects.Length; i++)
        {
            float n = StartDayIntroNoise01(tick + i * 11, 41);
            RectTransform band = startDayIntroGlitchBandRects[i];
            band.anchoredPosition = new Vector2(StartDayIntroSignedNoise(tick + i, 42) * (18f + burst * 70f),
                Mathf.Lerp(-height * 0.47f, height * 0.47f, StartDayIntroNoise01(tick + i * 5, 43)));
            band.sizeDelta = new Vector2(0f, Mathf.Lerp(2f, 34f, n) + burst * 22f);
            float alpha = n > 0.28f ? 0.08f + n * 0.30f + surge * 0.14f + burst * 0.26f : 0f;
            startDayIntroGlitchBandImages[i].color = StartDayIntroGlitchColor(i, alpha);
        }
    }

    private void UpdateStartDayIntroGlitchBlocks(int tick, float surge, float burst)
    {
        float width = startDayIntroGlitchRoot.rect.width;
        float height = startDayIntroGlitchRoot.rect.height;
        for (int i = 0; i < startDayIntroGlitchBlockRects.Length; i++)
        {
            float n = StartDayIntroNoise01(tick + i * 17, 57);
            RectTransform block = startDayIntroGlitchBlockRects[i];
            block.anchoredPosition = new Vector2(
                Mathf.Lerp(-width * 0.50f, width * 0.50f, StartDayIntroNoise01(tick + i * 7, 58)),
                Mathf.Lerp(-height * 0.46f, height * 0.46f, StartDayIntroNoise01(tick + i * 13, 59)));
            block.sizeDelta = new Vector2(Mathf.Lerp(8f, 150f, n) + burst * 90f, Mathf.Lerp(2f, 24f, StartDayIntroNoise01(tick + i, 60)));
            float alpha = n > 0.46f ? 0.09f + (n - 0.46f) * 0.80f + surge * 0.08f + burst * 0.18f : 0f;
            startDayIntroGlitchBlockImages[i].color = StartDayIntroGlitchColor(i + 3, alpha);
        }
    }

    private Color StartDayIntroGlitchColor(int index, float alpha)
    {
        if (index % 5 == 0)
        {
            return new Color(1f, 0.16f, 0.10f, alpha * 0.72f);
        }

        if (index % 3 == 0)
        {
            return new Color(0.16f, 0.92f, 1f, alpha * 0.80f);
        }

        return new Color(0.92f, 0.96f, 1f, alpha);
    }

    private void ApplyStartDayIntroMysteriousTitle(float titleElapsed)
    {
        float fade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(titleElapsed / 0.62f));
        float subtitleAlpha = fade * (0.68f + Mathf.Sin(titleElapsed * 0.48f + 1.2f) * 0.08f);
        ApplyStartDayIntroTitleLetterReveal(titleElapsed, fade);
        ApplyStartDayIntroSubtitleLetterGlitch(titleElapsed, subtitleAlpha);
    }

    private void ApplyStartDayIntroTitleLetterReveal(float titleElapsed, float baseFade)
    {
        if (startDayIntroTitleLettersRoot == null || startDayIntroTitleLetters == null || startDayIntroTitleLetterRects == null)
        {
            return;
        }

        int tick = Mathf.Max(0, Mathf.FloorToInt(titleElapsed * 24f));
        float zoom = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(titleElapsed / StartDayIntroTitleHold));
        float burst = StartDayIntroNoise01(tick, 73) > 0.70f ? StartDayIntroNoise01(tick, 74) : 0f;
        startDayIntroTitleLettersRoot.anchoredPosition = new Vector2(StartDayIntroSignedNoise(tick, 75) * burst * 18f,
            StartDayIntroTitleY + Mathf.Sin(titleElapsed * 0.35f) * 3.5f);
        float rootScale = Mathf.Lerp(0.985f, 1.085f, zoom);
        startDayIntroTitleLettersRoot.localScale = new Vector3(rootScale, rootScale, 1f);

        for (int i = 0; i < startDayIntroTitleLetters.Length; i++)
        {
            Text letter = startDayIntroTitleLetters[i];
            RectTransform rect = startDayIntroTitleLetterRects[i];
            if (letter == null || rect == null || string.IsNullOrEmpty(letter.text))
            {
                continue;
            }

            float delay = StartDayIntroNoise01(i + 3, 80) * 0.34f + (i % 2) * 0.04f;
            float fade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((titleElapsed - delay) / 0.32f));
            float dropout = StartDayIntroNoise01(tick + i * 19, 81) > 0.88f ? 0.30f : 1f;
            float alpha = baseFade * fade * dropout;
            letter.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));

            Vector2 basePosition = startDayIntroTitleLetterBasePositions[i];
            float xKick = StartDayIntroSignedNoise(tick + i, 82) * burst * (10f + i * 1.5f);
            float yKick = StartDayIntroSignedNoise(tick + i, 83) * burst * 4f;
            rect.anchoredPosition = basePosition + new Vector2(xKick, yKick);
            float scale = 1f + zoom * 0.026f + (1f - fade) * 0.018f + burst * 0.012f;
            rect.localScale = new Vector3(scale, scale, 1f);
            rect.localRotation = Quaternion.Euler(0f, 0f, StartDayIntroSignedNoise(tick + i, 84) * burst * 0.65f);
        }
    }

    private void ApplyStartDayIntroLetterTickLine(Text[] letters, RectTransform[] rects, float titleElapsed, int salt, float degrees, float basePeriod)
    {
        if (letters == null || rects == null)
        {
            return;
        }

        int count = Mathf.Min(letters.Length, rects.Length);
        for (int i = 0; i < count; i++)
        {
            Text letter = letters[i];
            RectTransform rect = rects[i];
            if (letter == null || rect == null || string.IsNullOrEmpty(letter.text) || !ShouldStartDayIntroLetterTick(i, salt))
            {
                continue;
            }

            float period = basePeriod + StartDayIntroNoise01(i + 17, salt) * 1.1f;
            float phase = Mathf.Repeat(titleElapsed + StartDayIntroNoise01(i + 31, salt + 5) * period, period);
            if (phase > StartDayIntroLetterTickDuration)
            {
                continue;
            }

            float progress = phase / StartDayIntroLetterTickDuration;
            float snap = progress < 0.28f
                ? Mathf.Lerp(0f, -degrees, progress / 0.28f)
                : Mathf.Lerp(-degrees, 0f, (progress - 0.28f) / 0.72f);
            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.028f;
            rect.localRotation = Quaternion.Euler(0f, 0f, snap);
            rect.localScale = new Vector3(scale, scale, 1f);
        }
    }

    private bool ShouldStartDayIntroLetterTick(int index, int salt)
    {
        return (index + salt) % 11 == 0 || (index * 7 + salt) % 23 == 0;
    }

    private void SetStartDayIntroGlitchVisible(bool visible)
    {
        if (startDayIntroGlitchRoot != null)
        {
            startDayIntroGlitchRoot.gameObject.SetActive(visible);
        }
    }

    private void ResetStartDayIntroGlitch()
    {
        SetStartDayIntroGlitchVisible(false);
        if (startDayIntroGlitchGroup != null)
        {
            startDayIntroGlitchGroup.alpha = 0f;
        }
    }

    private void ResetStartDayIntroTitleLettersPose()
    {
        if (startDayIntroTitleLettersRoot != null)
        {
            startDayIntroTitleLettersRoot.anchoredPosition = new Vector2(0f, StartDayIntroTitleY);
            startDayIntroTitleLettersRoot.localRotation = Quaternion.identity;
            startDayIntroTitleLettersRoot.localScale = Vector3.one;
        }

        ResetStartDayIntroLetterRects(startDayIntroTitleLetterRects);
        SetStartDayIntroLetterAlpha(startDayIntroTitleLetters, 1f);
    }

    private void ResetStartDayIntroSubtitleLetterRects()
    {
        ResetStartDayIntroLetterRects(startDayIntroSubtitleLetterRects);
        ResetStartDayIntroSubtitleLetterPositions();
    }

    private void ResetStartDayIntroLetterRects(RectTransform[] rects)
    {
        if (rects == null)
        {
            return;
        }

        for (int i = 0; i < rects.Length; i++)
        {
            if (rects[i] == null)
            {
                continue;
            }

            rects[i].localRotation = Quaternion.identity;
            rects[i].localScale = Vector3.one;
        }
    }

    private void SetStartDayIntroLetterAlpha(Text[] letters, float alpha)
    {
        if (letters == null)
        {
            return;
        }

        for (int i = 0; i < letters.Length; i++)
        {
            if (letters[i] == null)
            {
                continue;
            }

            Color color = letters[i].color;
            color.a = alpha;
            letters[i].color = color;
        }
    }

    private void SetStartDayIntroTitleLettersVisible(bool visible)
    {
        if (startDayIntroTitleLettersRoot != null)
        {
            startDayIntroTitleLettersRoot.gameObject.SetActive(visible);
        }
    }

    private void SetStartDayIntroSubtitleLettersVisible(bool visible)
    {
        if (startDayIntroSubtitleLettersRoot != null)
        {
            startDayIntroSubtitleLettersRoot.gameObject.SetActive(visible);
        }
    }

    private void UpdateStartDayIntroMusicDistortion(float titleElapsed, bool titlePhase)
    {
        if (startDayIntroMusicDistortion == null)
        {
            return;
        }

        if (!titlePhase)
        {
            startDayIntroMusicDistortion.distortionLevel = 0f;
            return;
        }

        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(titleElapsed / StartDayIntroTitleHold));
        float crackle = StartDayIntroNoise01(Mathf.FloorToInt(titleElapsed * 14f), 101) * 0.08f;
        startDayIntroMusicDistortion.distortionLevel = Mathf.Clamp01(Mathf.Lerp(0.05f, 0.78f, t) + crackle);
    }

    private void ResetStartDayIntroMusicDistortion()
    {
        if (startDayIntroMusicDistortion != null)
        {
            startDayIntroMusicDistortion.distortionLevel = 0f;
        }
    }
}
