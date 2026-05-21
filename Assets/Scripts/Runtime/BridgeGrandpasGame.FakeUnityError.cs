using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float FakeUnityErrorReturnGlitchSeconds = 1.35f;
    private const int FakeUnityErrorGlitchBandCount = 18;
    private const int FakeUnityErrorGlitchBlockCount = 26;

    private readonly List<GameObject> fakeUnityErrorHiddenGrandpaRoots = new List<GameObject>();
    private readonly List<bool> fakeUnityErrorHiddenGrandpaWasActive = new List<bool>();

    private Canvas fakeUnityErrorCanvas;
    private RectTransform fakeUnityErrorModalRoot;
    private RectTransform fakeUnityErrorDialogRoot;
    private RectTransform fakeUnityErrorGlitchRoot;
    private CanvasGroup fakeUnityErrorModalGroup;
    private CanvasGroup fakeUnityErrorGlitchGroup;
    private Image fakeUnityErrorGlitchBackImage;
    private RectTransform[] fakeUnityErrorGlitchBandRects;
    private Image[] fakeUnityErrorGlitchBandImages;
    private RectTransform[] fakeUnityErrorGlitchBlockRects;
    private Image[] fakeUnityErrorGlitchBlockImages;

    private bool fakeUnityErrorModalActive;
    private bool fakeUnityErrorGrandpasHidden;
    private bool fakeUnityErrorReturnGlitchActive;
    private bool fakeUnityErrorGrandpasRestoredThisGlitch;
    private bool fakeUnityErrorTimePaused;
    private float fakeUnityErrorPreviousTimeScale = 1f;
    private float fakeUnityErrorPreviousFixedDeltaTime = 0.02f;
    private float fakeUnityErrorReturnGlitchStartedAt;

    private bool UpdateFakeUnityError(float deltaTime)
    {
        if (WasFakeUnityErrorPressed())
        {
            BeginFakeUnityError();
        }

        if (fakeUnityErrorModalActive && WasFakeUnityErrorOkPressed())
        {
            ConfirmFakeUnityError();
        }

        if (fakeUnityErrorWebcamMenuActive)
        {
            UpdateFakeUnityErrorWebcamMenu(deltaTime);
        }

        if (fakeUnityErrorGrandpasHidden)
        {
            KeepGrandpasHiddenForFakeUnityError();
            if (WasEscapePressed() && TryBeginFakeUnityErrorWebcamMenuFromEscape())
            {
                return true;
            }
        }

        if (fakeUnityErrorReturnGlitchActive)
        {
            UpdateFakeUnityErrorReturnGlitch(deltaTime);
        }

        return fakeUnityErrorModalActive || fakeUnityErrorWebcamMenuActive || fakeUnityErrorReturnGlitchActive;
    }

    private void BeginFakeUnityError()
    {
        if (!gameStarted || escapeMenuOpen || fakeUnityErrorModalActive || fakeUnityErrorGrandpasHidden ||
            fakeUnityErrorWebcamMenuActive || fakeUnityErrorReturnGlitchActive)
        {
            return;
        }

        EnsureFakeUnityErrorVisuals();
        PauseGameForFakeUnityError();
        SuppressBackgroundMusicAfterFakeUnityError();
        fakeUnityErrorModalActive = true;

        if (fakeUnityErrorCanvas != null)
        {
            fakeUnityErrorCanvas.gameObject.SetActive(true);
        }

        if (fakeUnityErrorModalRoot != null)
        {
            fakeUnityErrorModalRoot.gameObject.SetActive(true);
            fakeUnityErrorModalRoot.SetAsLastSibling();
        }

        if (fakeUnityErrorModalGroup != null)
        {
            fakeUnityErrorModalGroup.alpha = 1f;
            fakeUnityErrorModalGroup.blocksRaycasts = true;
            fakeUnityErrorModalGroup.interactable = true;
        }

        if (fakeUnityErrorGlitchRoot != null)
        {
            fakeUnityErrorGlitchRoot.gameObject.SetActive(false);
        }

        PlayFakeUnityErrorPopupSound();
        WriteDebugLog("FAKE_UNITY_ERROR", "F2 fake Unity error opened. " + DebugStateSnapshot());
    }

    private void ConfirmFakeUnityError()
    {
        if (!fakeUnityErrorModalActive)
        {
            return;
        }

        fakeUnityErrorModalActive = false;
        ResumeGameFromFakeUnityError();

        if (fakeUnityErrorModalRoot != null)
        {
            fakeUnityErrorModalRoot.gameObject.SetActive(false);
        }

        if (fakeUnityErrorModalGroup != null)
        {
            fakeUnityErrorModalGroup.blocksRaycasts = false;
            fakeUnityErrorModalGroup.interactable = false;
        }

        if (fakeUnityErrorCanvas != null)
        {
            fakeUnityErrorCanvas.gameObject.SetActive(false);
        }

        CaptureAndHideGrandpasForFakeUnityError();
        fakeUnityErrorGrandpasHidden = true;
        WriteDebugLog("FAKE_UNITY_ERROR", "OK pressed. Grandpas hidden until fake Escape menu. hiddenRoots=" +
            fakeUnityErrorHiddenGrandpaRoots.Count);
    }

    private void CaptureAndHideGrandpasForFakeUnityError()
    {
        fakeUnityErrorHiddenGrandpaRoots.Clear();
        fakeUnityErrorHiddenGrandpaWasActive.Clear();
        KeepGrandpasHiddenForFakeUnityError();
        hoveredTarget = null;
    }

    private void KeepGrandpasHiddenForFakeUnityError()
    {
        for (int i = 0; i < grandpas.Count; i++)
        {
            GameObject root = grandpas[i].Root;
            if (root == null)
            {
                continue;
            }

            int index = fakeUnityErrorHiddenGrandpaRoots.IndexOf(root);
            if (index < 0)
            {
                fakeUnityErrorHiddenGrandpaRoots.Add(root);
                fakeUnityErrorHiddenGrandpaWasActive.Add(root.activeSelf);
            }

            root.SetActive(false);
        }
    }

    private void BeginFakeUnityErrorReturnGlitch()
    {
        if (!fakeUnityErrorGrandpasHidden)
        {
            return;
        }

        EnsureFakeUnityErrorVisuals();
        fakeUnityErrorGrandpasHidden = false;
        fakeUnityErrorReturnGlitchActive = true;
        fakeUnityErrorGrandpasRestoredThisGlitch = false;
        fakeUnityErrorReturnGlitchStartedAt = Time.unscaledTime;

        if (fakeUnityErrorCanvas != null)
        {
            fakeUnityErrorCanvas.gameObject.SetActive(true);
        }

        if (fakeUnityErrorGlitchRoot != null)
        {
            fakeUnityErrorGlitchRoot.gameObject.SetActive(true);
            fakeUnityErrorGlitchRoot.SetAsLastSibling();
        }

        if (fakeUnityErrorGlitchGroup != null)
        {
            fakeUnityErrorGlitchGroup.alpha = 1f;
        }

        UpdateFakeUnityErrorReturnGlitch(0f);
        WriteDebugLog("FAKE_UNITY_ERROR", "Return glitch started.");
    }

    private void UpdateFakeUnityErrorReturnGlitch(float deltaTime)
    {
        float elapsed = Time.unscaledTime - fakeUnityErrorReturnGlitchStartedAt;
        if (!fakeUnityErrorGrandpasRestoredThisGlitch && elapsed >= 0.34f)
        {
            RestoreGrandpasAfterFakeUnityError();
            fakeUnityErrorGrandpasRestoredThisGlitch = true;
        }

        float progress = Mathf.Clamp01(elapsed / FakeUnityErrorReturnGlitchSeconds);
        float intensity = Mathf.Sin(progress * Mathf.PI);
        int tick = Mathf.FloorToInt(Time.unscaledTime * 44f);

        if (fakeUnityErrorGlitchGroup != null)
        {
            fakeUnityErrorGlitchGroup.alpha = Mathf.Clamp01(0.28f + intensity * 0.95f);
        }

        if (fakeUnityErrorGlitchBackImage != null)
        {
            fakeUnityErrorGlitchBackImage.color = new Color(0f, 0f, 0f, 0.16f + intensity * 0.34f);
        }

        UpdateFakeUnityErrorGlitchBands(tick, intensity);
        UpdateFakeUnityErrorGlitchBlocks(tick, intensity);

        if (elapsed >= FakeUnityErrorReturnGlitchSeconds)
        {
            EndFakeUnityErrorReturnGlitch();
        }
    }

    private void RestoreGrandpasAfterFakeUnityError()
    {
        for (int i = 0; i < fakeUnityErrorHiddenGrandpaRoots.Count; i++)
        {
            GameObject root = fakeUnityErrorHiddenGrandpaRoots[i];
            if (root != null)
            {
                bool active = i < fakeUnityErrorHiddenGrandpaWasActive.Count && fakeUnityErrorHiddenGrandpaWasActive[i];
                root.SetActive(active);
            }
        }

        WriteDebugLog("FAKE_UNITY_ERROR", "Grandpas restored after fake Unity error. restoredRoots=" +
            fakeUnityErrorHiddenGrandpaRoots.Count);
        fakeUnityErrorHiddenGrandpaRoots.Clear();
        fakeUnityErrorHiddenGrandpaWasActive.Clear();
    }

    private void EndFakeUnityErrorReturnGlitch()
    {
        fakeUnityErrorReturnGlitchActive = false;
        if (!fakeUnityErrorGrandpasRestoredThisGlitch)
        {
            RestoreGrandpasAfterFakeUnityError();
            fakeUnityErrorGrandpasRestoredThisGlitch = true;
        }

        if (fakeUnityErrorGlitchRoot != null)
        {
            fakeUnityErrorGlitchRoot.gameObject.SetActive(false);
        }

        if (fakeUnityErrorCanvas != null)
        {
            fakeUnityErrorCanvas.gameObject.SetActive(false);
        }

        if (fakeUnityErrorWebcamMenuActive)
        {
            CompleteFakeUnityErrorWebcamRecovery();
        }

        WriteDebugLog("FAKE_UNITY_ERROR", "Return glitch completed.");
    }

    private void PauseGameForFakeUnityError()
    {
        if (fakeUnityErrorTimePaused)
        {
            return;
        }

        fakeUnityErrorPreviousTimeScale = Time.timeScale;
        fakeUnityErrorPreviousFixedDeltaTime = Time.fixedDeltaTime;
        Time.timeScale = 0f;
        fakeUnityErrorTimePaused = true;
    }

    private void ResumeGameFromFakeUnityError()
    {
        if (!fakeUnityErrorTimePaused)
        {
            return;
        }

        Time.timeScale = fakeUnityErrorPreviousTimeScale;
        Time.fixedDeltaTime = fakeUnityErrorPreviousFixedDeltaTime;
        fakeUnityErrorTimePaused = false;
    }

    private void RestoreFakeUnityErrorPauseForShutdown()
    {
        ResumeGameFromFakeUnityError();
    }
}
