using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float FakeCreditsDurationSeconds = 180f;
    private const float FakeCreditsContentHeight = 7200f;

    private sealed class FakeCreditsAudioState
    {
        public float Volume;
        public bool WasPlaying;
    }

    private readonly Dictionary<AudioSource, FakeCreditsAudioState> fakeCreditsAudioStates =
        new Dictionary<AudioSource, FakeCreditsAudioState>();

    private Canvas fakeCreditsCanvas;
    private CanvasGroup fakeCreditsGroup;
    private RectTransform fakeCreditsScrollRoot;
    private Text fakeCreditsText;
    private AudioSource fakeCreditsMusicSource;
    private AudioClip fakeCreditsMusicClip;
    private bool fakeCreditsActive;
    private bool fakeCreditsPausedTime;
    private float fakeCreditsStartedAt;
    private float fakeCreditsPreviousTimeScale = 1f;
    private float fakeCreditsPreviousFixedDeltaTime = 0.02f;

    private bool UpdateFakeCredits(float deltaTime)
    {
        if (WasFakeCreditsPressed())
        {
            BeginFakeCreditsScene();
        }

        if (!fakeCreditsActive)
        {
            return false;
        }

        if (WasFakeCreditsCancelPressed())
        {
            EndFakeCreditsScene(true);
            return true;
        }

        float elapsed = Time.unscaledTime - fakeCreditsStartedAt;
        UpdateFakeCreditsVisuals(elapsed);
        if (elapsed >= FakeCreditsDurationSeconds)
        {
            EndFakeCreditsScene(false);
        }

        return fakeCreditsActive;
    }

    private void BeginFakeCreditsScene()
    {
        if (fakeCreditsActive || fakeUnityErrorModalActive || fakeUnityErrorGrandpasHidden || escapeMenuBsodActive)
        {
            return;
        }

        EnsureFakeCreditsVisuals();
        EnsureFakeCreditsAudio();
        CaptureFakeCreditsAudioSources();
        PauseGameForFakeCredits();
        fakeCreditsActive = true;
        fakeCreditsStartedAt = Time.unscaledTime;

        if (fakeCreditsCanvas != null)
        {
            fakeCreditsCanvas.gameObject.SetActive(true);
        }

        if (fakeCreditsGroup != null)
        {
            fakeCreditsGroup.alpha = 1f;
        }

        StartFakeCreditsMusic();
        UpdateFakeCreditsVisuals(0f);
        WriteDebugLog("FAKE_CREDITS", "F3 fake credits started. duration=" + FakeCreditsDurationSeconds + "s");
    }

    private void EndFakeCreditsScene(bool cancelled)
    {
        if (!fakeCreditsActive)
        {
            return;
        }

        fakeCreditsActive = false;
        StopFakeCreditsMusic();
        RestoreFakeCreditsAudioSources();
        ResumeGameFromFakeCredits();

        if (fakeCreditsCanvas != null)
        {
            fakeCreditsCanvas.gameObject.SetActive(false);
        }

        WriteDebugLog("FAKE_CREDITS", cancelled ? "Fake credits cancelled by Escape." : "Fake credits ended after 180 seconds.");
    }

    private void EnsureFakeCreditsVisuals()
    {
        if (fakeCreditsCanvas != null)
        {
            return;
        }

        EnsureUiFont();
        GameObject canvasObject = new GameObject("Fake Credits Scene", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler));
        fakeCreditsCanvas = canvasObject.GetComponent<Canvas>();
        fakeCreditsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fakeCreditsCanvas.sortingOrder = 380;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = CreatePanel("Fake Credits Blackout", canvasObject.transform, Color.black);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        root.GetComponent<Image>().raycastTarget = true;
        fakeCreditsGroup = root.gameObject.AddComponent<CanvasGroup>();

        fakeCreditsScrollRoot = CreatePanel("Fake Credits Scroll", root, new Color(0f, 0f, 0f, 0f));
        fakeCreditsScrollRoot.anchorMin = new Vector2(0.5f, 0.5f);
        fakeCreditsScrollRoot.anchorMax = new Vector2(0.5f, 0.5f);
        fakeCreditsScrollRoot.pivot = new Vector2(0.5f, 0.5f);
        fakeCreditsScrollRoot.sizeDelta = new Vector2(1120f, FakeCreditsContentHeight);
        fakeCreditsScrollRoot.GetComponent<Image>().raycastTarget = false;

        fakeCreditsText = CreateText("Fake Credits Text", fakeCreditsScrollRoot, 24, FontStyle.Normal,
            TextAnchor.UpperCenter, new Color(0.88f, 0.88f, 0.84f));
        fakeCreditsText.supportRichText = true;
        fakeCreditsText.lineSpacing = 1.18f;
        fakeCreditsText.horizontalOverflow = HorizontalWrapMode.Wrap;
        fakeCreditsText.verticalOverflow = VerticalWrapMode.Overflow;
        fakeCreditsText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        fakeCreditsText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        fakeCreditsText.rectTransform.pivot = new Vector2(0.5f, 1f);
        fakeCreditsText.rectTransform.anchoredPosition = Vector2.zero;
        fakeCreditsText.rectTransform.sizeDelta = new Vector2(1120f, FakeCreditsContentHeight);
        fakeCreditsText.text = BuildFakeCreditsText();

        fakeCreditsCanvas.gameObject.SetActive(false);
    }

    private void UpdateFakeCreditsVisuals(float elapsed)
    {
        if (fakeCreditsScrollRoot == null || fakeCreditsGroup == null)
        {
            return;
        }

        float progress = Mathf.Clamp01(elapsed / FakeCreditsDurationSeconds);
        float startY = -940f;
        float endY = FakeCreditsContentHeight + 940f;
        fakeCreditsScrollRoot.anchoredPosition = new Vector2(0f, Mathf.Lerp(startY, endY, progress));

        float fadeIn = Mathf.Clamp01(elapsed / 2.5f);
        float fadeOut = Mathf.Clamp01((FakeCreditsDurationSeconds - elapsed) / 3.5f);
        fakeCreditsGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Min(fadeIn, fadeOut));
    }

    private void EnsureFakeCreditsAudio()
    {
        if (fakeCreditsMusicSource != null)
        {
            return;
        }

        fakeCreditsMusicClip = Resources.Load<AudioClip>("Music/Titles");
        if (fakeCreditsMusicClip == null)
        {
            WriteDebugWarningLog("FAKE_CREDITS", "Titles clip not found at Resources/Music/Titles.");
        }

        GameObject audioObject = new GameObject("Fake Credits Titles Music");
        audioObject.transform.SetParent(transform, false);
        fakeCreditsMusicSource = audioObject.AddComponent<AudioSource>();
        fakeCreditsMusicSource.clip = fakeCreditsMusicClip;
        fakeCreditsMusicSource.loop = fakeCreditsMusicClip != null && fakeCreditsMusicClip.length < FakeCreditsDurationSeconds;
        fakeCreditsMusicSource.playOnAwake = false;
        fakeCreditsMusicSource.spatialBlend = 0f;
        fakeCreditsMusicSource.volume = 0.48f;
        fakeCreditsMusicSource.pitch = 1f;
        fakeCreditsMusicSource.priority = 0;
        RouteAudioSource(fakeCreditsMusicSource, BridgeAudioBus.Music);
    }

    private void StartFakeCreditsMusic()
    {
        if (fakeCreditsMusicSource == null || fakeCreditsMusicClip == null)
        {
            return;
        }

        fakeCreditsMusicSource.Stop();
        fakeCreditsMusicSource.time = 0f;
        fakeCreditsMusicSource.Play();
    }

    private void StopFakeCreditsMusic()
    {
        if (fakeCreditsMusicSource != null)
        {
            fakeCreditsMusicSource.Stop();
        }
    }

    private void CaptureFakeCreditsAudioSources()
    {
        fakeCreditsAudioStates.Clear();
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude);
        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = sources[i];
            if (source == null || source == fakeCreditsMusicSource)
            {
                continue;
            }

            FakeCreditsAudioState state = new FakeCreditsAudioState();
            state.Volume = source.volume;
            state.WasPlaying = source.isPlaying;
            fakeCreditsAudioStates[source] = state;
            if (state.WasPlaying)
            {
                source.Pause();
            }
        }
    }

    private void RestoreFakeCreditsAudioSources()
    {
        foreach (KeyValuePair<AudioSource, FakeCreditsAudioState> pair in fakeCreditsAudioStates)
        {
            if (pair.Key == null)
            {
                continue;
            }

            pair.Key.volume = pair.Value.Volume;
            if (pair.Value.WasPlaying)
            {
                pair.Key.UnPause();
            }
        }

        fakeCreditsAudioStates.Clear();
    }

    private void PauseGameForFakeCredits()
    {
        if (fakeCreditsPausedTime)
        {
            return;
        }

        fakeCreditsPreviousTimeScale = Time.timeScale;
        fakeCreditsPreviousFixedDeltaTime = Time.fixedDeltaTime;
        Time.timeScale = 0f;
        fakeCreditsPausedTime = true;
    }

    private void ResumeGameFromFakeCredits()
    {
        if (!fakeCreditsPausedTime)
        {
            return;
        }

        Time.timeScale = fakeCreditsPreviousTimeScale;
        Time.fixedDeltaTime = fakeCreditsPreviousFixedDeltaTime;
        fakeCreditsPausedTime = false;
    }

    private void RestoreFakeCreditsForShutdown()
    {
        if (fakeCreditsActive)
        {
            fakeCreditsActive = false;
            StopFakeCreditsMusic();
            RestoreFakeCreditsAudioSources();
        }

        ResumeGameFromFakeCredits();
    }

    private string BuildFakeCreditsText()
    {
        return string.Join("\n", new[]
        {
            "<size=46><b>Мостовые Дедушки</b></size>",
            "",
            "<size=24>fake end credits sequence</size>",
            "",
            "",
            "<b>DEVELOPED BY</b>",
            "Underpass Observation Unit",
            "",
            "<b>GAME DIRECTOR</b>",
            "Anton Karelin",
            "",
            "<b>CREATIVE DIRECTOR</b>",
            "Mira Sokolova",
            "",
            "<b>PRODUCER</b>",
            "Lev Moroz",
            "",
            "<b>LEAD PROGRAMMER</b>",
            "Daniil Vorontsov",
            "",
            "<b>GAMEPLAY PROGRAMMING</b>",
            "Ilya Belov",
            "Nikita Shevchenko",
            "Roman Kuleshov",
            "Elena Orlova",
            "",
            "<b>UI AND TOOLS PROGRAMMING</b>",
            "Kirill Maslov",
            "Anna Bezrukova",
            "Stepan Volgin",
            "",
            "<b>TECHNICAL ART</b>",
            "Polina Yartseva",
            "Maksim Rodin",
            "",
            "<b>ART DIRECTOR</b>",
            "Vera Lantsova",
            "",
            "<b>ENVIRONMENT ART</b>",
            "Oleg Mironov",
            "Sofia Zimina",
            "Timur Akhmetov",
            "Maria Kostina",
            "",
            "<b>CHARACTER ART</b>",
            "Yaroslav Denisov",
            "Alina Gromova",
            "Pavel Sorokin",
            "",
            "<b>ANIMATION</b>",
            "Daria Vetrova",
            "Gleb Arseniev",
            "Mikhail Sazonov",
            "",
            "<b>VHS AND GLITCH DESIGN</b>",
            "Nina Tumanova",
            "Sergey Makarov",
            "",
            "<b>NARRATIVE DESIGN</b>",
            "Artyom Lavrov",
            "Liza Melnichuk",
            "Grigory Osipov",
            "",
            "<b>ADDITIONAL WRITING</b>",
            "Ksenia Barkova",
            "Vadim Terentiev",
            "Yulia Rakitina",
            "",
            "<b>SOUND DIRECTOR</b>",
            "Mark Feldman",
            "",
            "<b>MUSIC</b>",
            "Nikolai Reutov",
            "Marina Zhuravleva",
            "",
            "<b>SOUND DESIGN</b>",
            "Egor Tikhonov",
            "Oksana Kravets",
            "Matvey Fomin",
            "",
            "<b>QUALITY ASSURANCE</b>",
            "Irina Belyaeva",
            "Vladislav Karpov",
            "Semyon Pak",
            "Natalia Rogova",
            "Dmitry Kuzmin",
            "",
            "<b>LOCALIZATION</b>",
            "Margarita Frolova",
            "Leonid Kiselev",
            "Taisiya Romanenko",
            "",
            "<b>BUILD ENGINEERING</b>",
            "Alexey Sviridov",
            "Stanislav Nekrasov",
            "",
            "<b>SPECIAL THANKS</b>",
            "The Night Shift",
            "Old Bridge Archive",
            "Camera Three Maintenance Team",
            "Everyone who kept watching",
            "",
            "",
            "<size=22>Made under the bridge.</size>",
            "",
            "<size=18>© 1998-2026 Underpass Observation Unit</size>"
        });
    }

    private bool WasFakeCreditsPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F3);
#endif
    }

    private bool WasFakeCreditsCancelPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
