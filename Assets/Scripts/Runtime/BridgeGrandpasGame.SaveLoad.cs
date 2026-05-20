using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const string SaveKey = "BridgeGrandpas.Save.v1";

    [Serializable]
    private sealed class SaveData
    {
        public ResourceStock Stock;
        public float Suspicion;
        public float CozyScore;
        public int CozyDecorTier;
        public int NextGrandpaId;
        public int InspectionsSurvived;
        public int MutationsSinceRare;
        public bool RareMutationSeen;
        public bool VictoryShown;
        public float DayClockElapsedSeconds;
        public bool PlansOldMenFollowupResolved;
        public int PlansOldMenCollectorGrandpaId;
        public List<BuildingSaveData> Buildings = new List<BuildingSaveData>();
        public List<GrandpaSaveData> Grandpas = new List<GrandpaSaveData>();
        public List<JunkPileSaveData> JunkPiles = new List<JunkPileSaveData>();
        public List<ObservationSaveData> Observations = new List<ObservationSaveData>();
        public List<ObservationCardSaveData> PendingObservationCards = new List<ObservationCardSaveData>();
    }

    [Serializable]
    private sealed class BuildingSaveData
    {
        public int Type;
        public bool Built;
        public int Level;
        public float BlockedRemaining;
    }

    [Serializable]
    private sealed class GrandpaSaveData
    {
        public int Id;
        public string Name;
        public int Role;
        public int WorkMode;
        public float Budding;
        public Vector3 Position;
        public Vector3 Target;
    }

    [Serializable]
    private sealed class JunkPileSaveData
    {
        public int Id;
        public Vector3 Position;
        public float RemainingJunk;
        public float MaxJunk;
        public int Variant;
    }

    [Serializable]
    private sealed class ObservationSaveData
    {
        public int Day;
        public float Time;
        public string Text;
        public bool Written;
        public bool HasClock;
    }

    [Serializable]
    private sealed class ObservationCardSaveData
    {
        public string Label;
        public string Text;
        public float CreatedAt;
    }

    private bool TryLoadGame()
    {
        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        SaveData data;
        try
        {
            data = JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[BridgeGrandpas] Save load failed: " + exception.Message);
            return false;
        }

        if (data == null || data.Grandpas == null || data.Grandpas.Count == 0)
        {
            return false;
        }

        ClearPlayableStateForLoad();
        RestoreSaveData(data);
        return true;
    }

    private bool HasSavedGame()
    {
        return !string.IsNullOrEmpty(PlayerPrefs.GetString(SaveKey, ""));
    }

    private void SaveGameFromMenu()
    {
        if (!gameStarted)
        {
            return;
        }

        SaveGame();
        Notify("Сохранено: блокнот прижал страницу, чтобы её не унесло ветром.");
        MarkNotebookDirty();
        RefreshAllUi();
    }

    private void LoadGameFromMenu()
    {
        if (!HasSavedGame())
        {
            Notify("Загружать нечего: старых записей пока нет.");
            return;
        }

        if (!TryLoadGame())
        {
            Notify("Сохранение не прочиталось. Блокнот делает вид, что так и было.");
            return;
        }

        SelectOverview();
        BeginStartIrisFade();
        MarkNotebookDirty();
        RefreshAllUi();
        Notify("Загружено: дедовская коммуна вернулась к старым записям.");
    }

    private void ClearPlayableStateForLoad()
    {
        selectedGrandpa = null;
        selectedBuilding = null;
        hoveredTarget = null;
        pendingEvent = null;
        trayOpen = false;
        trayDirty = true;
        microHudUntil = 0f;
        SetWatchMode(false);
        ResetDayClock();

        if (settlementRoot != null)
        {
            ClearChildren(settlementRoot);
        }

        cozyDecorRoot = null;
        ClearJunkScene();
        grandpas.Clear();
        buildings.Clear();
        SetupBuildings();
        fireBarrelCoreLight = null;
        fireBarrelPoolLight = null;
        fireBarrelFlickerLightA = null;
        fireBarrelFlickerLightB = null;
        fireFlames = Array.Empty<Transform>();
        ResetNotebookObservations();
        ResetObservationLeads();
    }

    private void RestoreSaveData(SaveData data)
    {
        stock = data.Stock;
        stock.ClampNonNegative();
        suspicion = Mathf.Clamp(data.Suspicion, 0f, MaxSuspicion);
        cozyScore = Mathf.Max(0f, data.CozyScore);
        cozyDecorTier = Mathf.Max(0, data.CozyDecorTier);
        nextGrandpaId = Mathf.Max(1, data.NextGrandpaId);
        inspectionsSurvived = Mathf.Max(0, data.InspectionsSurvived);
        mutationsSinceRare = Mathf.Max(0, data.MutationsSinceRare);
        rareMutationSeen = data.RareMutationSeen;
        victoryShown = data.VictoryShown;
        dayClockElapsedSeconds = Mathf.Max(0f, data.DayClockElapsedSeconds);
        UpdateWatchTimeText();

        ResetNotebookObservations();
        RestoreBuildings(data.Buildings);
        RestoreJunkPiles(data.JunkPiles);
        RestoreGrandpas(data.Grandpas);
        RestoreNotebookObservations(data.Observations);
        plansOldMenFollowupResolved = data.PlansOldMenFollowupResolved || PlansOldMenFollowupAlreadyResolved();
        plansOldMenCollectorGrandpaId = data.PlansOldMenCollectorGrandpaId > 0 ? data.PlansOldMenCollectorGrandpaId : -1;
        plansOldMenFollowupOpen = false;
        RestorePendingObservationCards(data.PendingObservationCards);
        CreateStarterCommuneProps();
        RefreshCozyDecor();
    }

    private void RestoreBuildings(List<BuildingSaveData> savedBuildings)
    {
        if (savedBuildings == null)
        {
            TryBuild(BuildingType.FireBarrel, true);
            return;
        }

        for (int i = 0; i < savedBuildings.Count; i++)
        {
            BuildingSaveData saved = savedBuildings[i];
            BuildingType type = (BuildingType)saved.Type;
            Building building;
            if (!buildings.TryGetValue(type, out building) || !saved.Built)
            {
                continue;
            }

            building.Built = true;
            building.Level = Mathf.Max(1, saved.Level);
            building.BlockedUntil = saved.BlockedRemaining > 0f ? Time.time + saved.BlockedRemaining : 0f;
            building.Root = CreateBuildingVisual(building);
        }

        Building fire;
        if (buildings.TryGetValue(BuildingType.FireBarrel, out fire) && !fire.Built)
        {
            TryBuild(BuildingType.FireBarrel, true);
        }
    }

    private void RestoreGrandpas(List<GrandpaSaveData> savedGrandpas)
    {
        int maxId = 0;
        for (int i = 0; i < savedGrandpas.Count; i++)
        {
            GrandpaSaveData saved = savedGrandpas[i];
            nextGrandpaId = Mathf.Max(1, saved.Id);
            Grandpa grandpa = SpawnGrandpa((GrandpaRole)saved.Role, saved.Position);
            grandpa.Name = string.IsNullOrEmpty(saved.Name) ? grandpa.Name : UserFacingGrandpaText(saved.Name);
            grandpa.Budding = Mathf.Clamp(saved.Budding, 0f, BuddingGoal);
            grandpa.Target = saved.Target;
            grandpa.WorkMode = (GrandpaWorkMode)Mathf.Clamp(saved.WorkMode, 0, 1);
            grandpa.JunkState = JunkCollectorState.Idle;
            grandpa.JunkPileId = -1;
            grandpa.CarryingJunk = 0f;
            grandpa.Root.name = GrandpaTechnicalName(grandpa);
            maxId = Mathf.Max(maxId, grandpa.Id);
        }

        nextGrandpaId = Mathf.Max(nextGrandpaId, Mathf.Max(maxId + 1, 1));
    }

    private void RestoreNotebookObservations(List<ObservationSaveData> observations)
    {
        notebookObservations.Clear();
        ResetObservationLeads();
        if (observations == null)
        {
            EnsureArchiveObservations();
            return;
        }

        for (int i = 0; i < observations.Count && notebookObservations.Count < MaxNotebookObservations; i++)
        {
            ObservationSaveData saved = observations[i];
            if (string.IsNullOrWhiteSpace(saved.Text))
            {
                continue;
            }

            int day = saved.Day <= 0 ? CurrentObservationDay : saved.Day;
            bool hasClock = saved.Day <= 0 ? true : saved.HasClock;
            NotebookObservation note = new NotebookObservation(day, saved.Time, UserFacingGrandpaText(saved.Text), saved.Written, hasClock);
            notebookObservations.Add(note);
        }

        EnsureArchiveObservations();
        TrimNotebookObservations();
    }

    private void SaveGame()
    {
        if (!gameStarted)
        {
            return;
        }

        SaveData data = BuildSaveData();
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private SaveData BuildSaveData()
    {
        SaveData data = new SaveData
        {
            Stock = stock,
            Suspicion = suspicion,
            CozyScore = cozyScore,
            CozyDecorTier = cozyDecorTier,
            NextGrandpaId = nextGrandpaId,
            InspectionsSurvived = inspectionsSurvived,
            MutationsSinceRare = mutationsSinceRare,
            RareMutationSeen = rareMutationSeen,
            VictoryShown = victoryShown,
            DayClockElapsedSeconds = dayClockElapsedSeconds,
            PlansOldMenFollowupResolved = PlansOldMenFollowupAlreadyResolved(),
            PlansOldMenCollectorGrandpaId = plansOldMenCollectorGrandpaId
        };

        foreach (KeyValuePair<BuildingType, Building> pair in buildings)
        {
            data.Buildings.Add(new BuildingSaveData
            {
                Type = (int)pair.Key,
                Built = pair.Value.Built,
                Level = pair.Value.Level,
                BlockedRemaining = Mathf.Max(0f, pair.Value.BlockedUntil - Time.time)
            });
        }

        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            if (grandpa == null)
            {
                continue;
            }

            data.Grandpas.Add(new GrandpaSaveData
            {
                Id = grandpa.Id,
                Name = grandpa.Name,
                Role = (int)grandpa.Role,
                WorkMode = (int)grandpa.WorkMode,
                Budding = grandpa.Budding,
                Position = grandpa.Root != null ? grandpa.Root.transform.position : grandpa.Target,
                Target = grandpa.Target
            });
        }

        for (int i = 0; i < junkPiles.Count; i++)
        {
            JunkPile pile = junkPiles[i];
            data.JunkPiles.Add(new JunkPileSaveData
            {
                Id = pile.Id,
                Position = pile.Position,
                RemainingJunk = pile.RemainingJunk,
                MaxJunk = pile.MaxJunk,
                Variant = (int)pile.Variant
            });
        }

        for (int i = 0; i < notebookObservations.Count; i++)
        {
            NotebookObservation note = notebookObservations[i];
            data.Observations.Add(new ObservationSaveData
            {
                Day = note.Day,
                Time = note.Time,
                Text = note.Text,
                Written = note.Written,
                HasClock = note.HasClock
            });
        }

        SavePendingObservationCards(data.PendingObservationCards);
        return data;
    }

}
