using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private RawImage startMenuBackgroundImage;
    private Texture2D startMenuDefaultTexture;
    private Texture2D startMenuEscapeTexture;
    private Button startMenuPrimaryButton;
    private Button startMenuLoadButton;
    private Text startMenuPrimaryButtonText;
    private Text startMenuLoadButtonText;
    private bool escapeMenuOpen;

    private bool UpdateEscapeMenuFlow(float deltaTime)
    {
        if (WasEscapePressed())
        {
            if (saveSlotScreenOpen)
            {
                HideSaveSlotScreen();
                return true;
            }

            if (escapeMenuOpen)
            {
                CloseEscapeMenu();
                return true;
            }

            if (CloseActiveInteractionModeFromEscape())
            {
                return true;
            }

            OpenEscapeMenu();
            return true;
        }

        if (!escapeMenuOpen)
        {
            return false;
        }

        UpdateEscapeMenuHum();
        UpdateStartMenuAnimation(deltaTime);
        UpdateEscapeMenuMadness();
        return true;
    }

    private void OpenEscapeMenu()
    {
        if (escapeMenuOpen)
        {
            return;
        }

        escapeMenuOpen = true;
        startMenuLoading = false;
        PauseGameForEscapeMenu();
        SilenceEscapeMenuMusic();
        StartEscapeMenuHum();
        SetStartMenuFireLayersVisible(false);
        ApplyStartMenuBackground(true);
        ApplyStartMenuButtonMode(true);
        HideSaveSlotScreenOnly();
        SetStartMenuButtonsInteractable(true);
        if (startMenuButtonsGroup != null)
        {
            startMenuButtonsGroup.alpha = 1f;
        }

        if (startMenuContentGroup != null)
        {
            startMenuContentGroup.alpha = 1f;
        }

        if (startMenuLoadingRoot != null)
        {
            startMenuLoadingRoot.gameObject.SetActive(false);
        }

        if (startMenuCanvas != null)
        {
            startMenuCanvas.gameObject.SetActive(true);
        }

        BeginEscapeMenuMadness();
        CloseTray();
        RefreshInteractionMode();
        WriteDebugLog("ESCAPE_MENU", "Opened in-game menu with MenuEscape background.");
    }

    private void CloseEscapeMenu()
    {
        if (!escapeMenuOpen)
        {
            return;
        }

        escapeMenuOpen = false;
        startMenuLoading = false;
        HideSaveSlotScreenOnly();
        StopEscapeMenuHum();
        EndEscapeMenuMadness();
        ResumeEscapeMenuMusic();
        ResumeGameFromEscapeMenu();
        SetStartMenuFireLayersVisible(true);
        ApplyStartMenuBackground(false);
        ApplyStartMenuButtonMode(false);
        if (startMenuLoadingRoot != null)
        {
            startMenuLoadingRoot.gameObject.SetActive(false);
        }

        if (startMenuCanvas != null && gameStarted)
        {
            startMenuCanvas.gameObject.SetActive(false);
        }

        RefreshInteractionMode();
        WriteDebugLog("ESCAPE_MENU", "Closed in-game menu.");
    }

    private void SilenceEscapeMenuMusic()
    {
        StopMenuMusic();
    }

    private void ResumeEscapeMenuMusic()
    {
    }

    private void ApplyStartMenuBackground(bool useEscapeBackground)
    {
        if (startMenuBackgroundImage == null)
        {
            return;
        }

        Texture2D texture = useEscapeBackground && startMenuEscapeTexture != null ? startMenuEscapeTexture : startMenuDefaultTexture;
        startMenuBackgroundImage.texture = texture;
        startMenuBackgroundImage.color = texture == null ? new Color(0.025f, 0.020f, 0.018f, 1f) : Color.white;
    }

    private void ApplyStartMenuButtonMode(bool inGameMenu)
    {
        ConfigureStartMenuButton(
            startMenuPrimaryButton,
            startMenuPrimaryButtonText,
            inGameMenu ? "Продолжить" : "Новая игра",
            inGameMenu ? CloseEscapeMenu : BeginStartMenuLoading);

        ConfigureStartMenuButton(
            startMenuLoadButton,
            startMenuLoadButtonText,
            inGameMenu ? "Сохранить" : "Загрузить",
            inGameMenu ? BeginStartMenuSave : BeginStartMenuLoad);

        if (startMenuLoadButton != null)
        {
            startMenuLoadButton.interactable = inGameMenu || HasSavedGame();
        }
    }

    private void BeginStartMenuSave()
    {
        if (!gameStarted)
        {
            return;
        }

        ShowSaveSlotScreen(SaveSlotScreenMode.Save);
    }

    private void ConfigureStartMenuButton(Button button, Text label, string text, UnityEngine.Events.UnityAction action)
    {
        if (label != null)
        {
            label.text = text;
        }

        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private bool CloseActiveInteractionModeFromEscape()
    {
        if (CloseActiveDialogFromEscape())
        {
            return true;
        }

        if (vhsModeEnabled)
        {
            SetVhsMode(false);
            WriteDebugLog("ESCAPE", "Closed VHS mode.");
            return true;
        }

        if (notebookModeEnabled)
        {
            SetNotebookMode(false);
            WriteDebugLog("ESCAPE", "Closed notebook mode.");
            return true;
        }

        if (watchModeEnabled)
        {
            SetWatchMode(false);
            WriteDebugLog("ESCAPE", "Closed watch mode.");
            return true;
        }

        if (trayOpen)
        {
            CloseTray();
            WriteDebugLog("ESCAPE", "Closed tray.");
            return true;
        }

        return false;
    }

    private bool CloseActiveDialogFromEscape()
    {
        if (eventModal != null && eventModal.gameObject.activeSelf)
        {
            eventModal.gameObject.SetActive(false);
            WriteDebugLog("ESCAPE", "Closed event modal.");
            RefreshInteractionMode();
            return true;
        }

        if (expeditionModal != null && expeditionModal.gameObject.activeSelf)
        {
            expeditionModal.gameObject.SetActive(false);
            ResetExpeditionDice();
            WriteDebugLog("ESCAPE", "Closed expedition modal.");
            RefreshInteractionMode();
            return true;
        }

        if (victoryModal != null && victoryModal.gameObject.activeSelf)
        {
            victoryModal.gameObject.SetActive(false);
            WriteDebugLog("ESCAPE", "Closed victory modal.");
            RefreshInteractionMode();
            return true;
        }

        return false;
    }

    private bool WasEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current.escapeKey.wasPressedThisFrame;
        }
#endif
        return Input.GetKeyDown(KeyCode.Escape);
    }
}
