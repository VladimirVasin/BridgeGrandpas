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
    private const float MaxSuspicion = 100f;
    private const float BuddingGoal = 100f;
    private const int VictoryGrandpas = 20;
    private const int VictoryBuildings = 5;
    private const int VictoryInspections = 3;

    private readonly List<Grandpa> grandpas = new List<Grandpa>();
    private readonly Dictionary<BuildingType, Building> buildings = new Dictionary<BuildingType, Building>();
    private readonly List<BridgeEvent> events = new List<BridgeEvent>();
    private readonly Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

    private Transform worldRoot;
    private Transform settlementRoot;
    private Transform cozyDecorRoot;
    private Camera mainCamera;
    private Font uiFont;
    private System.Random random;
    private Light fireBarrelCoreLight;
    private Light fireBarrelPoolLight;
    private Light fireBarrelFlickerLightA;
    private Light fireBarrelFlickerLightB;
    private Transform[] fireFlames = Array.Empty<Transform>();

    private ResourceStock stock;
    private float suspicion;
    private float nextEventIn;
    private float nextRadioWhisperIn;
    private int nextGrandpaId = 1;
    private int inspectionsSurvived;
    private int mutationsSinceRare;
    private int cozyDecorTier;
    private float cozyScore;
    private bool rareMutationSeen;
    private bool victoryShown;
    private string lastAlert = "Первый дедушка обживает пространство под мостом.";
    private float alertUntil;

    private Grandpa selectedGrandpa;
    private Building selectedBuilding;
    private BridgeEvent pendingEvent;
    private UiTab currentTab = UiTab.Build;

    private Canvas canvas;
    private Text topStatsText;
    private RectTransform detailPanel;
    private CanvasGroup detailPanelGroup;
    private Text detailText;
    private Text alertText;
    private Text trayTitleText;
    private RectTransform trayPanel;
    private RectTransform trayBody;
    private ScrollRect trayScroll;
    private RectTransform eventModal;
    private Text eventTitleText;
    private Text eventBodyText;
    private RectTransform eventChoicesRoot;
    private RectTransform expeditionModal;
    private Text expeditionTitleText;
    private Text expeditionBodyText;
    private RectTransform expeditionChoicesRoot;
    private RectTransform expeditionDicePanel;
    private Text expeditionDiceText;
    private Text expeditionDiceCaptionText;
    private RectTransform victoryModal;
    private Image suspicionFill;
    private GameObject selectionMarker;
    private GameObject hoverMarker;
    private BridgeGrandpasSelectionTarget hoveredTarget;
    private bool trayOpen;
    private Vector2 detailPanelShownOffsetMin;
    private Vector2 detailPanelShownOffsetMax;
    private Vector2 detailPanelHiddenOffsetMin;
    private Vector2 detailPanelHiddenOffsetMax;
    private float detailPanelSlide;
    private string microHudTitle;
    private string microHudBody;
    private float microHudUntil;
    private bool expeditionDiceRolling;
    private bool expeditionDiceResultVisible;
    private float expeditionDiceStart;
    private float expeditionDiceUntil;
    private float expeditionDiceCloseAt;
    private int expeditionDiceResult;
    private Grandpa expeditionDiceGrandpa;
    private float expeditionDiceRewardMultiplier;
    private float expeditionDiceRiskMultiplier;
    private string expeditionDiceResultText;
    private Canvas startMenuCanvas;
    private bool gameStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnSceneLoad()
    {
        if (FindAnyObjectByType<BridgeGrandpasGame>() != null)
        {
            return;
        }

        GameObject gameObject = new GameObject("Bridge Grandpas MVP Runtime");
        gameObject.AddComponent<BridgeGrandpasGame>();
    }

    private void Awake()
    {
        random = new System.Random(Environment.TickCount);
        stock = new ResourceStock(18f, 20f, 20f, 14f, 2f);
        suspicion = 8f;
        nextEventIn = 45f;
        nextRadioWhisperIn = 30f;
    }

    private void Start()
    {
        SetupScene();
        SetupCameraControls();
        SetupStartMenu();
    }

    private void StartNewGame()
    {
        if (gameStarted)
        {
            return;
        }

        gameStarted = true;
        StopMenuMusic();
        if (startMenuCanvas != null)
        {
            startMenuCanvas.gameObject.SetActive(false);
        }

        SetupBuildings();
        SetupEvents();
        SetupUi();
        SetupBackgroundMusic();
        SetupAmbience();
        BuildInitialState();
        SelectOverview();
        RefreshAllUi();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f)
        {
            return;
        }

        if (!gameStarted)
        {
            UpdateStartMenuAnimation(deltaTime);
            return;
        }

        SimulateResources(deltaTime);
        SimulateSuspicion(deltaTime);
        SimulateEvents(deltaTime);
        SimulateGrandpas(deltaTime);
        UpdateCameraControls(deltaTime);
        HandlePointer();
        UpdateMicroHudPanel(deltaTime);
        UpdateExpeditionDice(deltaTime);
        UpdateMarkers();
        UpdateBillboards();
        UpdateFireBarrelLighting();
        UpdateBuildingAnimations(deltaTime);
        UpdateAmbience(deltaTime);
        UpdateCityAmbience(deltaTime);
        UpdateAmbientUi();
    }

}
