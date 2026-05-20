using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private enum StartDayIntroStyle
    {
        SimpleBlackTitle,
        NoeStrobe
    }

    private const float StartDayIntroDarkHold = 2f;
    private const float StartDayIntroTitleHold = 6f;
    private const float StartDayIntroStrobeInterval = 0.035f;
    private const float StartDayIntroFinalCropTime = 0.48f;
    private const float StartDayIntroMusicPitch = 0.6f;
    private const float StartDayIntroTitleY = 128f;
    private const float StartDayIntroSubtitleY = -192f;
    private const float StartDayIntroSubtitleLettersWidth = 1180f;
    private const float StartDayIntroLetterTickDuration = 0.20f;
    private const StartDayIntroStyle ActiveStartDayIntroStyle = StartDayIntroStyle.SimpleBlackTitle;

    private Canvas startDayIntroCanvas;
    private Image startDayIntroBlackout;
    private Text startDayIntroTitle;
    private Text startDayIntroSubtitle;
    private RectTransform startDayIntroSubtitleLettersRoot;
    private Text[] startDayIntroSubtitleLetters;
    private RectTransform[] startDayIntroSubtitleLetterRects;
    private AudioSource startDayIntroMusicSource;
    private bool startDayIntroActive;
    private bool startDayIntroMusicStarted;
    private float startDayIntroElapsed;

    private void SetupStartDayIntro()
    {
        if (startDayIntroCanvas != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("Start Day Intro", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        startDayIntroCanvas = canvasObject.GetComponent<Canvas>();
        startDayIntroCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        startDayIntroCanvas.sortingOrder = 240;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform blackout = CreatePanel("Day Intro Blackout", canvasObject.transform, Color.black);
        blackout.anchorMin = Vector2.zero;
        blackout.anchorMax = Vector2.one;
        blackout.offsetMin = Vector2.zero;
        blackout.offsetMax = Vector2.zero;
        startDayIntroBlackout = blackout.GetComponent<Image>();
        startDayIntroBlackout.raycastTarget = true;
        SetupStartDayIntroGlitch(blackout);

        startDayIntroTitle = CreateText("Day Intro Title", blackout, 430, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        startDayIntroTitle.horizontalOverflow = HorizontalWrapMode.Overflow;
        startDayIntroTitle.verticalOverflow = VerticalWrapMode.Overflow;
        startDayIntroTitle.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        startDayIntroTitle.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        startDayIntroTitle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        startDayIntroTitle.rectTransform.anchoredPosition = new Vector2(0f, StartDayIntroTitleY);
        startDayIntroTitle.rectTransform.sizeDelta = new Vector2(1380f, 430f);
        startDayIntroTitle.raycastTarget = false;
        startDayIntroTitle.gameObject.SetActive(false);
        SetupStartDayIntroTitleLetters(blackout);

        startDayIntroSubtitle = CreateText("Day Intro Subtitle", blackout, 44, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        startDayIntroSubtitle.horizontalOverflow = HorizontalWrapMode.Overflow;
        startDayIntroSubtitle.verticalOverflow = VerticalWrapMode.Overflow;
        startDayIntroSubtitle.text = "Я должен за ними наблюдать";
        startDayIntroSubtitle.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        startDayIntroSubtitle.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        startDayIntroSubtitle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        startDayIntroSubtitle.rectTransform.anchoredPosition = new Vector2(0f, StartDayIntroSubtitleY);
        startDayIntroSubtitle.rectTransform.sizeDelta = new Vector2(StartDayIntroSubtitleLettersWidth, 66f);
        startDayIntroSubtitle.rectTransform.localScale = new Vector3(1.42f, 1f, 1f);
        startDayIntroSubtitle.raycastTarget = false;
        startDayIntroSubtitle.gameObject.SetActive(false);

        startDayIntroSubtitleLettersRoot = CreateStartDayIntroLetterRoot(
            "Day Intro Subtitle Letters",
            blackout,
            StartDayIntroSubtitleLettersWidth,
            86f,
            StartDayIntroSubtitleY);
        CreateStartDayIntroLetterTexts(
            startDayIntroSubtitleLettersRoot,
            "Я должен за ними наблюдать",
            44,
            FontStyle.Bold,
            StartDayIntroSubtitleLettersWidth,
            86f,
            out startDayIntroSubtitleLetters,
            out startDayIntroSubtitleLetterRects);

        SetupStartDayIntroMusic(canvasObject.transform);

        startDayIntroCanvas.gameObject.SetActive(false);
    }

    private RectTransform CreateStartDayIntroLetterRoot(string name, Transform parent, float width, float height, float y)
    {
        RectTransform root = CreatePanel(name, parent, new Color(0f, 0f, 0f, 0f));
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = new Vector2(0f, y);
        root.sizeDelta = new Vector2(width, height);
        root.GetComponent<Image>().raycastTarget = false;
        root.gameObject.SetActive(false);
        return root;
    }

    private void CreateStartDayIntroLetterTexts(
        RectTransform root,
        string text,
        int size,
        FontStyle style,
        float lineWidth,
        float lineHeight,
        out Text[] letters,
        out RectTransform[] letterRects)
    {
        int count = Mathf.Max(1, text.Length);
        float step = count > 1 ? lineWidth / (count - 1) : 0f;
        float cellWidth = step + size * 0.62f;
        letters = new Text[count];
        letterRects = new RectTransform[count];

        for (int i = 0; i < count; i++)
        {
            Text letter = CreateText("Letter " + i, root, size, style, TextAnchor.MiddleCenter, Color.white);
            letter.horizontalOverflow = HorizontalWrapMode.Overflow;
            letter.verticalOverflow = VerticalWrapMode.Overflow;
            letter.text = i < text.Length && text[i] != ' ' ? text[i].ToString() : string.Empty;
            letter.raycastTarget = false;

            RectTransform rect = letter.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-lineWidth * 0.5f + i * step, 0f);
            rect.sizeDelta = new Vector2(cellWidth, lineHeight);

            letters[i] = letter;
            letterRects[i] = rect;
        }
    }

    private void SetupStartDayIntroMusic(Transform parent)
    {
        AudioClip interlude = Resources.Load<AudioClip>(MusicResourcesPath + "/InterludeNormal");
        if (interlude == null)
        {
            Debug.LogWarning("[BridgeGrandpas] InterludeNormal clip not found in Resources/Music.");
            return;
        }

        GameObject audioObject = new GameObject("Start Day Interlude Normal");
        audioObject.transform.SetParent(parent, false);
        startDayIntroMusicSource = audioObject.AddComponent<AudioSource>();
        startDayIntroMusicSource.clip = interlude;
        startDayIntroMusicSource.loop = true;
        startDayIntroMusicSource.playOnAwake = false;
        startDayIntroMusicSource.spatialBlend = 0f;
        startDayIntroMusicSource.volume = 1f;
        startDayIntroMusicSource.pitch = StartDayIntroMusicPitch;
        startDayIntroMusicSource.priority = 8;
        startDayIntroMusicSource.bypassEffects = false;
        startDayIntroMusicSource.bypassListenerEffects = true;
        startDayIntroMusicSource.bypassReverbZones = true;
        startDayIntroMusicDistortion = audioObject.AddComponent<AudioDistortionFilter>();
        startDayIntroMusicDistortion.distortionLevel = 0f;
    }

    private void BeginStartDayIntro()
    {
        SetupStartDayIntro();
        startDayIntroElapsed = 0f;
        startDayIntroActive = true;
        startDayIntroMusicStarted = false;
        startDayIntroTitle.text = "День " + CurrentObservationDay;
        SetStartDayIntroTitleLetterText("День " + CurrentObservationDay);
        ResetStartDayIntroTitlePose();
        startDayIntroTitle.gameObject.SetActive(false);
        startDayIntroSubtitle.gameObject.SetActive(false);
        SetStartDayIntroTitleLettersVisible(false);
        SetStartDayIntroSubtitleLettersVisible(false);
        ResetStartDayIntroGlitch();
        ResetStartDayIntroMusicDistortion();
        startDayIntroBlackout.color = Color.black;
        startDayIntroCanvas.gameObject.SetActive(true);
    }

    private bool UpdateStartDayIntro(float deltaTime)
    {
        if (!startDayIntroActive)
        {
            return false;
        }

        startDayIntroElapsed += deltaTime;
        float titleElapsed = startDayIntroElapsed - StartDayIntroDarkHold;
        bool titlePhase = titleElapsed >= 0f && titleElapsed < StartDayIntroTitleHold;
        int strobePulse = Mathf.Max(0, Mathf.FloorToInt(titleElapsed / (StartDayIntroStrobeInterval * 2f)));
        bool redFrame = titlePhase && Mathf.Repeat(titleElapsed, StartDayIntroStrobeInterval * 2f) < StartDayIntroStrobeInterval;
        bool useStrobe = ActiveStartDayIntroStyle == StartDayIntroStyle.NoeStrobe;
        if (titlePhase && !startDayIntroMusicStarted)
        {
            PlayStartDayIntroMusic();
        }

        if (startDayIntroBlackout != null)
        {
            startDayIntroBlackout.color = useStrobe && redFrame ? new Color(0.92f, 0f, 0f, 1f) : Color.black;
        }

        if (startDayIntroTitle != null)
        {
            if (useStrobe)
            {
                SetStartDayIntroGlitchVisible(false);
                SetStartDayIntroTitleLettersVisible(false);
                SetStartDayIntroSubtitleLettersVisible(false);
                startDayIntroTitle.gameObject.SetActive(redFrame);
                SetStartDayIntroSubtitleVisible(redFrame);
                if (redFrame)
                {
                    ApplyStartDayIntroTitlePulse(titleElapsed, strobePulse);
                }
            }
            else
            {
                UpdateStartDayIntroGlitch(titleElapsed, titlePhase);
                UpdateStartDayIntroMusicDistortion(titleElapsed, titlePhase);
                SetStartDayIntroSubtitleVisible(false);
                ResetStartDayIntroTitlePose();
                startDayIntroTitle.text = "День " + CurrentObservationDay;
                startDayIntroTitle.gameObject.SetActive(false);
                SetStartDayIntroTitleLettersVisible(titlePhase);
                SetStartDayIntroSubtitleLettersVisible(titlePhase);
                if (titlePhase)
                {
                    ApplyStartDayIntroMysteriousTitle(titleElapsed);
                }
            }
        }

        if (startDayIntroElapsed >= StartDayIntroDarkHold + StartDayIntroTitleHold)
        {
            CompleteStartDayIntro();
        }

        return true;
    }

    private void ApplyStartDayIntroTitlePulse(float titleElapsed, int pulse)
    {
        bool observeFlash = pulse % 31 == 7 || pulse % 43 == 19;
        bool finalCrop = titleElapsed > StartDayIntroTitleHold - StartDayIntroFinalCropTime;
        startDayIntroTitle.text = observeFlash && !finalCrop ? "НАБЛЮДАЙ" : "День " + CurrentObservationDay;

        float jitterX = StartDayIntroSignedNoise(pulse, 1) * (finalCrop ? 240f : 70f);
        float jitterY = StartDayIntroSignedNoise(pulse, 2) * (finalCrop ? 160f : 44f);
        float rotation = StartDayIntroSignedNoise(pulse, 3) * (finalCrop ? 8.5f : 3.5f);
        float scale = finalCrop
            ? 2.65f + StartDayIntroNoise01(pulse, 4) * 0.55f
            : 0.86f + StartDayIntroNoise01(pulse, 5) * 0.46f;

        if (!finalCrop && (pulse % 13 == 3 || pulse % 17 == 9))
        {
            scale += 0.55f;
            jitterX *= 1.85f;
            jitterY *= 1.65f;
        }

        RectTransform rect = startDayIntroTitle.rectTransform;
        rect.anchoredPosition = new Vector2(jitterX, jitterY);
        rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        rect.localScale = new Vector3(scale, scale, 1f);

        if (startDayIntroSubtitle != null)
        {
            RectTransform subtitleRect = startDayIntroSubtitle.rectTransform;
            subtitleRect.anchoredPosition = new Vector2(jitterX * 0.32f, StartDayIntroSubtitleY + jitterY * 0.18f);
            subtitleRect.localRotation = Quaternion.Euler(0f, 0f, rotation * 0.35f);
            subtitleRect.localScale = new Vector3(1.42f * Mathf.Clamp(scale, 0.92f, 1.35f), 1f, 1f);
        }
    }

    private void ResetStartDayIntroTitlePose()
    {
        if (startDayIntroTitle == null)
        {
            return;
        }

        RectTransform rect = startDayIntroTitle.rectTransform;
        rect.anchoredPosition = new Vector2(0f, StartDayIntroTitleY);
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        startDayIntroTitle.color = Color.white;

        if (startDayIntroSubtitleLettersRoot != null)
        {
            startDayIntroSubtitleLettersRoot.anchoredPosition = new Vector2(0f, StartDayIntroSubtitleY);
            startDayIntroSubtitleLettersRoot.localRotation = Quaternion.identity;
            startDayIntroSubtitleLettersRoot.localScale = Vector3.one;
        }

        ResetStartDayIntroTitleLettersPose();
        ResetStartDayIntroSubtitleLetterRects();
        SetStartDayIntroLetterAlpha(startDayIntroSubtitleLetters, 1f);

        if (startDayIntroSubtitle != null)
        {
            RectTransform subtitleRect = startDayIntroSubtitle.rectTransform;
            subtitleRect.anchoredPosition = new Vector2(0f, StartDayIntroSubtitleY);
            subtitleRect.localRotation = Quaternion.identity;
            subtitleRect.localScale = new Vector3(1.42f, 1f, 1f);
        }
    }

    private void SetStartDayIntroSubtitleVisible(bool visible)
    {
        if (startDayIntroSubtitle != null)
        {
            startDayIntroSubtitle.gameObject.SetActive(visible);
        }
    }

    private float StartDayIntroSignedNoise(int pulse, int salt)
    {
        return StartDayIntroNoise01(pulse, salt) * 2f - 1f;
    }

    private float StartDayIntroNoise01(int pulse, int salt)
    {
        return Mathf.Repeat(Mathf.Sin((pulse + 1) * (12.9898f + salt * 7.17f)) * 43758.5453f, 1f);
    }

    private void CompleteStartDayIntro()
    {
        startDayIntroActive = false;
        ResetStartDayIntroTitlePose();
        if (startDayIntroCanvas != null)
        {
            startDayIntroCanvas.gameObject.SetActive(false);
        }

        StopStartDayIntroMusic();
        BeginStartIrisFade();
        gameStarted = true;
    }

    private void PlayStartDayIntroMusic()
    {
        startDayIntroMusicStarted = true;
        if (startDayIntroMusicSource == null || startDayIntroMusicSource.clip == null)
        {
            return;
        }

        startDayIntroMusicSource.time = 0f;
        startDayIntroMusicSource.pitch = StartDayIntroMusicPitch;
        ResetStartDayIntroMusicDistortion();
        startDayIntroMusicSource.Play();
    }

    private void StopStartDayIntroMusic()
    {
        if (startDayIntroMusicSource != null)
        {
            startDayIntroMusicSource.Stop();
        }

        ResetStartDayIntroMusicDistortion();
    }
}
