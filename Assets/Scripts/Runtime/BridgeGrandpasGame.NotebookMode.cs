using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private enum NotebookPage
    {
        Summary,
        Observations,
        Build,
        Grandpas,
        Events,
        Expeditions
    }

    private bool notebookModeEnabled;
    private float notebookOpenAmount;
    private float notebookTargetOpen;
    private float notebookPageFlip;
    private NotebookPage currentNotebookPage = NotebookPage.Summary;

    private void UpdateNotebookMode(float deltaTime)
    {
        if (WasNotebookTogglePressed())
        {
            SetNotebookMode(!notebookModeEnabled);
        }

        notebookTargetOpen = notebookModeEnabled ? 1f : 0f;
        notebookOpenAmount = Mathf.Lerp(notebookOpenAmount, notebookTargetOpen, 1f - Mathf.Exp(-deltaTime * 8.5f));
        notebookPageFlip = Mathf.MoveTowards(notebookPageFlip, 0f, deltaTime * 2.75f);
        ApplyNotebookUiPose();
        UpdateNotebookWorldVisuals(deltaTime);
    }

    private void SetNotebookMode(bool enabled)
    {
        if (enabled && vhsModeEnabled)
        {
            SetVhsMode(false);
        }

        if (notebookModeEnabled == enabled)
        {
            ApplyLegacyHudVisibility();
            return;
        }

        notebookModeEnabled = enabled;
        if (enabled)
        {
            CloseTray();
            RefreshNotebookUi();
        }
        else
        {
            MarkNotebookDirty();
        }

        ApplyLegacyHudVisibility();
        RefreshInteractionMode();
    }

    private void SetNotebookPage(NotebookPage page)
    {
        bool pageChanged = currentNotebookPage != page;
        if (pageChanged && notebookModeEnabled)
        {
            notebookPageFlip = 1f;
        }

        currentNotebookPage = page;
        if (notebookModeEnabled && pageChanged)
        {
            RefreshNotebookUi();
            return;
        }

        MarkNotebookDirty();
    }

    private void MarkNotebookDirty()
    {
    }

    private void ApplyLegacyHudVisibility()
    {
        if (hudCanvasGroup == null)
        {
            return;
        }

        hudCanvasGroup.alpha = 0f;
        hudCanvasGroup.interactable = false;
        hudCanvasGroup.blocksRaycasts = false;
    }

    private bool WasNotebookTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.N);
#endif
    }
}
