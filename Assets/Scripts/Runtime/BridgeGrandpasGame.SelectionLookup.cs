public sealed partial class BridgeGrandpasGame
{
    private Grandpa GrandpaFromTarget(BridgeGrandpasSelectionTarget target)
    {
        if (target == null || target.Kind != SelectionKind.Grandpa)
        {
            return null;
        }

        for (int i = 0; i < grandpas.Count; i++)
        {
            if (grandpas[i].Id == target.GrandpaId)
            {
                return grandpas[i];
            }
        }

        return null;
    }

    private Building BuildingFromTarget(BridgeGrandpasSelectionTarget target)
    {
        if (target == null || target.Kind != SelectionKind.Building)
        {
            return null;
        }

        BuildingType type = (BuildingType)target.BuildingTypeValue;
        Building building;
        return buildings.TryGetValue(type, out building) ? building : null;
    }
}
