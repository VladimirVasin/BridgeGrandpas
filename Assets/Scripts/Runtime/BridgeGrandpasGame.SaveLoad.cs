using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const string SaveKey = "BridgeGrandpas.Save.v1";
    private const float AutoSaveInterval = 12f;
    private float nextAutoSaveAt;

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
        public List<BuildingSaveData> Buildings = new List<BuildingSaveData>();
        public List<GrandpaSaveData> Grandpas = new List<GrandpaSaveData>();
        public List<ObservationSaveData> Observations = new List<ObservationSaveData>();
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
        public float Budding;
        public Vector3 Position;
        public Vector3 Target;
    }

    [Serializable]
    private sealed class ObservationSaveData
    {
        public float Time;
        public string Text;
        public bool Written;
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

        RestoreSaveData(data);
        Notify("Блокнот нашёл старые записи. Коммуна продолжает шуршать.");
        return true;
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

        ResetNotebookObservations();
        RestoreBuildings(data.Buildings);
        RestoreGrandpas(data.Grandpas);
        RestoreNotebookObservations(data.Observations);
        CreateStarterCommuneProps();
        RefreshCozyDecor();
        nextAutoSaveAt = Time.time + AutoSaveInterval;
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
            grandpa.Name = string.IsNullOrEmpty(saved.Name) ? grandpa.Name : saved.Name;
            grandpa.Budding = Mathf.Clamp(saved.Budding, 0f, BuddingGoal);
            grandpa.Target = saved.Target;
            grandpa.Root.name = grandpa.Name;
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
            return;
        }

        for (int i = 0; i < observations.Count && notebookObservations.Count < MaxNotebookObservations; i++)
        {
            ObservationSaveData saved = observations[i];
            if (string.IsNullOrWhiteSpace(saved.Text))
            {
                continue;
            }

            NotebookObservation note = new NotebookObservation(saved.Time, saved.Text);
            note.Written = saved.Written;
            notebookObservations.Add(note);
        }
    }

    private void UpdateAutoSave()
    {
        if (!gameStarted || Time.time < nextAutoSaveAt)
        {
            return;
        }

        SaveGame();
        nextAutoSaveAt = Time.time + AutoSaveInterval;
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
            VictoryShown = victoryShown
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
                Budding = grandpa.Budding,
                Position = grandpa.Root != null ? grandpa.Root.transform.position : grandpa.Target,
                Target = grandpa.Target
            });
        }

        for (int i = 0; i < notebookObservations.Count; i++)
        {
            NotebookObservation note = notebookObservations[i];
            data.Observations.Add(new ObservationSaveData
            {
                Time = note.Time,
                Text = note.Text,
                Written = note.Written
            });
        }

        return data;
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
