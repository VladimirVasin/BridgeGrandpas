using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private static readonly string[] WarmIdleThoughts =
    {
        "Кости наконец-то договорились",
        "У бочки мысли мягче",
        "Тепло держит дедовский строй"
    };

    private static readonly string[] TeaIdleThoughts =
    {
        "Чай делает вечер законным",
        "Кружка знает своё дело",
        "Самовар сегодня почти родня"
    };

    private static readonly string[] CozyIdleThoughts =
    {
        "Коврик держит цивилизацию",
        "Поправлю уют, пока он не убежал",
        "Тут уже можно стареть спокойно"
    };

    private Vector3 ChooseGrandpaIdleTarget(Grandpa grandpa)
    {
        GrandpaIdleAction action = RollGrandpaIdleAction(grandpa);
        grandpa.IdleAction = action;
        grandpa.HasInteraction = false;
        float cozy = Mathf.InverseLerp(0f, 70f, cozyScore);
        grandpa.IdleActionUntil = Time.time + UnityEngine.Random.Range(3.4f, 6.6f) * (1f + cozy * 0.35f);
        MaybeShowIdleActionThought(grandpa, action);

        switch (action)
        {
            case GrandpaIdleAction.DrinkingTea:
                return TargetForBuilt(grandpa, BuildingType.Samovar, 1.05f, BuildingType.FireBarrel, 1.15f);
            case GrandpaIdleAction.Grumbling:
                return TargetForBuilt(grandpa, BuildingType.GrumbleBench, 1.05f, BuildingType.FireBarrel, 1.2f);
            case GrandpaIdleAction.Resting:
                return TargetForBuilt(grandpa, BuildingType.Bedroom, 1.18f, BuildingType.FireBarrel, 1.25f);
            case GrandpaIdleAction.WorkingCardboard:
                return CardboardIdlePoint();
            case GrandpaIdleAction.Guarding:
                return TargetForBuilt(grandpa, BuildingType.CarpetCurtain, 1.12f, BuildingType.FireBarrel, 1.35f);
            case GrandpaIdleAction.ListeningRadio:
                return TargetForBuilt(grandpa, BuildingType.RadioMayak, 0.95f, BuildingType.FireBarrel, 1.2f);
            case GrandpaIdleAction.AdmiringCozyDecor:
                return CozyDecorPoint();
            case GrandpaIdleAction.Warming:
                return TargetNearBuilding(grandpa, BuildingType.FireBarrel, 1.16f);
            default:
                return RandomSpawnPosition();
        }
    }

    private GrandpaIdleAction RollGrandpaIdleAction(Grandpa grandpa)
    {
        float cozy = Mathf.InverseLerp(0f, 70f, cozyScore);
        float wandering = Mathf.Lerp(3.1f, 1.5f, cozy);
        float warming = 2.5f;
        float tea = buildings[BuildingType.Samovar].Built ? 1.1f + cozy * 1.6f : 0f;
        float grumble = buildings[BuildingType.GrumbleBench].Built ? 1.0f : 0.3f;
        float resting = buildings[BuildingType.Bedroom].Built ? 0.8f + cozy * 1.3f : 0f;
        float cardboard = grandpa.Role == GrandpaRole.Cardboarder ? 3.2f : 0.55f;
        float guarding = buildings[BuildingType.CarpetCurtain].Built ? 0.45f : 0f;
        float radio = buildings[BuildingType.RadioMayak].Built ? 0.55f : 0f;
        float decor = cozyDecorTier > 0 ? cozy * 2.8f : 0f;

        if (grandpa.Role == GrandpaRole.SamovarKeeper) tea += 3.0f;
        if (grandpa.Role == GrandpaRole.Mutterer) grumble += 3.0f;
        if (grandpa.Role == GrandpaRole.Guard) guarding += 3.5f;
        if (grandpa.Role == GrandpaRole.RadioReceiver) radio += 3.0f;
        if (grandpa.Role == GrandpaRole.Philosopher) decor += 1.0f;

        float total = wandering + warming + tea + grumble + resting + cardboard + guarding + radio + decor;
        float roll = UnityEngine.Random.value * total;
        if ((roll -= tea) < 0f) return GrandpaIdleAction.DrinkingTea;
        if ((roll -= grumble) < 0f) return GrandpaIdleAction.Grumbling;
        if ((roll -= resting) < 0f) return GrandpaIdleAction.Resting;
        if ((roll -= cardboard) < 0f) return GrandpaIdleAction.WorkingCardboard;
        if ((roll -= guarding) < 0f) return GrandpaIdleAction.Guarding;
        if ((roll -= radio) < 0f) return GrandpaIdleAction.ListeningRadio;
        if ((roll -= decor) < 0f) return GrandpaIdleAction.AdmiringCozyDecor;
        if ((roll -= warming) < 0f) return GrandpaIdleAction.Warming;
        return GrandpaIdleAction.Wandering;
    }

    private Vector3 TargetForBuilt(Grandpa grandpa, BuildingType type, float radius, BuildingType fallback, float fallbackRadius)
    {
        if (buildings[type].Built && !buildings[type].IsBlocked)
        {
            return TargetNearBuilding(grandpa, type, radius);
        }

        return TargetNearBuilding(grandpa, fallback, fallbackRadius);
    }

    private Vector3 CardboardIdlePoint()
    {
        return ClampToCommune(new Vector3(UnityEngine.Random.Range(-5.1f, 5.1f), 0f, UnityEngine.Random.Range(-3.2f, -2.55f)));
    }

    private Vector3 CozyDecorPoint()
    {
        Vector3[] points =
        {
            new Vector3(0.88f, 0f, -0.80f),
            new Vector3(-1.15f, 0f, -1.10f),
            new Vector3(-1.35f, 0f, 0.70f),
            new Vector3(1.65f, 0f, 0.55f),
            new Vector3(-1.55f, 0f, -1.00f)
        };

        return ClampToCommune(points[UnityEngine.Random.Range(0, points.Length)] + Jitter(Vector3.zero, 0.22f));
    }

    private bool IdleActionUsesPose(GrandpaIdleAction action)
    {
        return action != GrandpaIdleAction.Wandering;
    }

    private string IdleActionName(GrandpaIdleAction action)
    {
        switch (action)
        {
            case GrandpaIdleAction.Warming:
                return "греется у бочки";
            case GrandpaIdleAction.DrinkingTea:
                return "пьёт чай";
            case GrandpaIdleAction.Grumbling:
                return "ворчит по расписанию";
            case GrandpaIdleAction.Resting:
                return "дремлет";
            case GrandpaIdleAction.WorkingCardboard:
                return "тащит картон";
            case GrandpaIdleAction.Guarding:
                return "сторожит тишину";
            case GrandpaIdleAction.ListeningRadio:
                return "слушает радио";
            case GrandpaIdleAction.AdmiringCozyDecor:
                return "поправляет уют";
            case GrandpaIdleAction.CollectingJunk:
                return "роется в хламе";
            case GrandpaIdleAction.CarryingJunk:
                return "несёт хлам";
            case GrandpaIdleAction.DepositingJunk:
                return "складывает хлам";
            default:
                return "бродит под мостом";
        }
    }

    private void MaybeShowIdleActionThought(Grandpa grandpa, GrandpaIdleAction action)
    {
        if (Time.time < grandpa.ThoughtUntil || !Roll(0.36f))
        {
            return;
        }

        if (action == GrandpaIdleAction.Warming)
        {
            ShowThought(grandpa, PickThought(WarmIdleThoughts), 3f);
        }
        else if (action == GrandpaIdleAction.DrinkingTea)
        {
            ShowThought(grandpa, PickThought(TeaIdleThoughts), 3f);
        }
        else if (action == GrandpaIdleAction.AdmiringCozyDecor)
        {
            ShowThought(grandpa, PickThought(CozyIdleThoughts), 3f);
        }
    }
}
