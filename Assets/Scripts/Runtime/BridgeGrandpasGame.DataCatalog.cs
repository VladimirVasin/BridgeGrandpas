public sealed partial class BridgeGrandpasGame
{
    private const string MusicResourcesPath = "Music";
    private const string FootstepsWalkResourcesPath = "Footsteps - Essentials/Footsteps_Grass/Footsteps_Grass_Walk";
    private const string FootstepsRootResourcesPath = "Footsteps - Essentials";
    private const string CameraBreathingResourcePath = "Sfx/CameraBreathing";
    private const string CameraBreathingFolderPath = "Sfx";
    private const string SamovarResourcePath = "Buildable Objects/Samovar";
    private const string SamovarLegacyResourcePath = "Buildable Objects/samovar";

    private string ImportedGrandpaResourcePath(GrandpaRole role)
    {
        switch (role)
        {
            case GrandpaRole.Common:
                return "Grandpas/Common";
            case GrandpaRole.SamovarKeeper:
                return "Grandpas/Teapot";
            case GrandpaRole.Cardboarder:
                return "Grandpas/Cardboard";
            case GrandpaRole.Mutterer:
                return "Grandpas/Grumpy";
            case GrandpaRole.Guard:
                return "Grandpas/Guard";
            case GrandpaRole.Philosopher:
                return "Grandpas/Philosopher";
            case GrandpaRole.RadioReceiver:
                return "Grandpas/Radio";
            default:
                return null;
        }
    }
}
