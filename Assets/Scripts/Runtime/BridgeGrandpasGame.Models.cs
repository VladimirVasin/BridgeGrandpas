using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;
public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private enum UiTab
    {
        Build,
        Upgrade,
        Events,
        Grandpas,
        Expeditions
    }

    private enum BuildingType
    {
        FireBarrel,
        Samovar,
        Bedroom,
        GrumbleBench,
        CarpetCurtain,
        RadioMayak
    }

    private enum GrandpaRole
    {
        Common,
        SamovarKeeper,
        Cardboarder,
        Mutterer,
        Guard,
        Philosopher,
        RadioReceiver
    }

    private enum ExpeditionType
    {
        CardboardRun,
        CoinAdvice,
        TeaSalvage,
        CityRumors
    }

    private enum GrandpaIdleAction
    {
        Wandering,
        Warming,
        DrinkingTea,
        Grumbling,
        Resting,
        WorkingCardboard,
        Guarding,
        ListeningRadio,
        AdmiringCozyDecor
    }

    private sealed class Grandpa
    {
        public int Id;
        public string Name;
        public GrandpaRole Role;
        public float Budding;
        public GameObject Root;
        public Transform Body;
        public Transform ImportedModelRoot;
        public Transform ImportedBodyControl;
        public Transform ImportedHead;
        public Transform ImportedHat;
        public Transform ImportedBeard;
        public Transform[] ImportedArms;
        public Transform[] ImportedLegs;
        public float[] ImportedArmPhaseSigns;
        public float[] ImportedLegPhaseSigns;
        public Quaternion ImportedHeadBaseRotation;
        public Quaternion ImportedHatBaseRotation;
        public Quaternion ImportedBeardBaseRotation;
        public Quaternion ImportedBodyBaseRotation;
        public Quaternion[] ImportedArmBaseRotations;
        public Quaternion[] ImportedLegBaseRotations;
        public Vector3 ImportedBodyBasePosition;
        public Vector3[] ImportedArmBasePositions;
        public Vector3[] ImportedLegBasePositions;
        public Vector3 ImportedModelBaseScale;
        public Vector3 ImportedModelBasePosition;
        public bool UsesImportedModel;
        public Transform InteractionProp;
        public Renderer InteractionPropRenderer;
        public AudioSource FootstepSource;
        public Vector3 Target;
        public float NextMoveAt;
        public float ThoughtUntil;
        public TextMesh ThoughtText;
        public float BobSeed;
        public float WalkJitter;
        public bool HasInteraction;
        public BuildingType InteractionType;
        public GrandpaIdleAction IdleAction;
        public float IdleActionUntil;
        public float ActionSeed;
        public float FootstepCyclePhase;
        public float NextFootstepAt;
        public float BirthAnimStart;
        public float BirthAnimUntil;
        public float BudBurstUntil;
        public bool IsOnExpedition;
        public bool ExpeditionLeaving;
        public bool ExpeditionNarrativeResolved;
        public ExpeditionType ExpeditionType;
        public float ExpeditionUntil;
        public float ExpeditionDepartureUntil;
        public float ExpeditionRewardMultiplier;
        public float ExpeditionRiskMultiplier;
        public Vector3 ExpeditionExitPosition;
        public Vector3 ExpeditionReturnPosition;
    }

    private sealed class Building
    {
        public BuildingType Type;
        public string Name;
        public string Description;
        public Vector3 Position;
        public ResourceStock BuildCost;
        public bool Built;
        public int Level;
        public GameObject Root;
        public float BlockedUntil;
        public Transform AnimatedRoot;
        public Vector3 AnimatedBasePosition;
        public Quaternion AnimatedBaseRotation;
        public Vector3 AnimatedBaseScale;
        public float AnimatedSeed;
        public ParticleSystem SteamParticles;
        public Light AccentLight;

        public bool IsBlocked
        {
            get { return Time.time < BlockedUntil; }
        }
    }

    private sealed class BridgeEvent
    {
        public readonly string Title;
        public readonly string Body;
        public readonly EventChoice[] Choices;

        public BridgeEvent(string title, string body, params EventChoice[] choices)
        {
            Title = title;
            Body = body;
            Choices = choices;
        }
    }

    private sealed class EventChoice
    {
        public readonly string Label;
        public readonly string Preview;
        public readonly Action<BridgeGrandpasGame> Apply;

        public EventChoice(string label, string preview, Action<BridgeGrandpasGame> apply)
        {
            Label = label;
            Preview = preview;
            Apply = apply;
        }
    }

    private struct ResourceStock
    {
        public float Tea;
        public float Heat;
        public float Cardboard;
        public float Grumble;
        public float Coins;

        public ResourceStock(float tea, float heat, float cardboard, float grumble, float coins)
        {
            Tea = tea;
            Heat = heat;
            Cardboard = cardboard;
            Grumble = grumble;
            Coins = coins;
        }

        public void ClampNonNegative()
        {
            Tea = Mathf.Max(0f, Tea);
            Heat = Mathf.Max(0f, Heat);
            Cardboard = Mathf.Max(0f, Cardboard);
            Grumble = Mathf.Max(0f, Grumble);
            Coins = Mathf.Max(0f, Coins);
        }

        public string ShortText()
        {
            List<string> pieces = new List<string>();
            if (Tea > 0f)
            {
                pieces.Add("чай " + Mathf.CeilToInt(Tea));
            }

            if (Heat > 0f)
            {
                pieces.Add("тепло " + Mathf.CeilToInt(Heat));
            }

            if (Cardboard > 0f)
            {
                pieces.Add("картон " + Mathf.CeilToInt(Cardboard));
            }

            if (Grumble > 0f)
            {
                pieces.Add("ворчание " + Mathf.CeilToInt(Grumble));
            }

            if (Coins > 0f)
            {
                pieces.Add("монетки " + Mathf.CeilToInt(Coins));
            }

            return pieces.Count == 0 ? "бесплатно" : string.Join(", ", pieces.ToArray());
        }

        public string ColoredCost(ResourceStock stock)
        {
            List<string> pieces = new List<string>();
            AddCost(pieces, "Чай", Tea, stock.Tea);
            AddCost(pieces, "Тепло", Heat, stock.Heat);
            AddCost(pieces, "Картон", Cardboard, stock.Cardboard);
            AddCost(pieces, "Ворчание", Grumble, stock.Grumble);
            AddCost(pieces, "Монетки", Coins, stock.Coins);
            return pieces.Count == 0 ? "Бесплатно" : string.Join("\n", pieces.ToArray());
        }

        private static void AddCost(List<string> pieces, string label, float cost, float have)
        {
            if (cost <= 0f)
            {
                return;
            }

            string color = have >= cost ? "#9cff93" : "#ff8f7a";
            pieces.Add(label + ": <color=" + color + ">" + Mathf.CeilToInt(have) + "/" + Mathf.CeilToInt(cost) + "</color>");
        }
    }
}

public enum SelectionKind
{
    Grandpa,
    Building
}

public sealed class BridgeGrandpasSelectionTarget : MonoBehaviour
{
    public SelectionKind Kind;
    public object Grandpa;
    public object Building;
}

