public sealed partial class BridgeGrandpasGame
{
    private enum GameInteractionMode
    {
        Normal,
        Dialog,
        Notebook,
        VhsCamera,
        Watch
    }

    private GameInteractionMode interactionMode;

    private void RefreshInteractionMode()
    {
        interactionMode = ResolveInteractionMode();
    }

    private GameInteractionMode ResolveInteractionMode()
    {
        if (escapeMenuOpen)
        {
            return GameInteractionMode.Dialog;
        }

        if (vhsModeEnabled)
        {
            return GameInteractionMode.VhsCamera;
        }

        if (notebookModeEnabled)
        {
            return GameInteractionMode.Notebook;
        }

        if (watchModeEnabled)
        {
            return GameInteractionMode.Watch;
        }

        if (IsAnyDialogOpen())
        {
            return GameInteractionMode.Dialog;
        }

        return GameInteractionMode.Normal;
    }

    private bool IsAnyDialogOpen()
    {
        return IsRectActive(eventModal) || IsRectActive(expeditionModal) || IsRectActive(victoryModal);
    }

    private static bool IsRectActive(UnityEngine.RectTransform rect)
    {
        return rect != null && rect.gameObject.activeSelf;
    }
}
