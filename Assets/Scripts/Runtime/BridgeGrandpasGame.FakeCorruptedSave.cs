using System;
using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float FakeCorruptedScanSeconds = 7.8f;
    private const string FakeCorruptedCardArtName = "685a2880-4327-4748-a5bd-d25a39679662";

    private bool fakeCorruptedSaveActive;
    private bool startMenuLoadCorruptedSave;
    private Transform fakeCorruptedRoot;
    private Transform fakeCorruptedGraveRoot;
    private AudioSource fakeCorruptedHissSource;
    private AudioClip fakeCorruptedHissClip;
    private float fakeCorruptedNextAudioCleanupAt;

    private bool HasFakeCorruptedSaveSlot()
    {
        return true;
    }

    private void SelectFakeCorruptedSaveSlot()
    {
        HideSaveSlotScreenOnly();
        WriteDebugLog("FAKE_CORRUPTED_SAVE", "Corrupted save slot selected. gameStarted=" + gameStarted);

        if (gameStarted)
        {
            CloseEscapeMenu();
            ClearPlayableStateForLoad();
            BuildInitialState();
            BeginFakeCorruptedSaveScene();
            return;
        }

        startMenuLoadCorruptedSave = true;
        BeginStartMenuLoading(true);
    }

    private void BeginFakeCorruptedSaveScene()
    {
        fakeCorruptedSaveActive = true;
        fakeCorruptedNextAudioCleanupAt = 0f;
        selectedGrandpa = null;
        selectedBuilding = null;
        hoveredTarget = null;
        trayOpen = false;
        trayDirty = true;
        microHudUntil = 0f;
        lastAlert = "";
        alertUntil = 0f;

        SetVhsMode(false);
        SetNotebookMode(false);
        SetWatchMode(false);
        CloseTray();
        ResetObservationLeads();
        ClearObservationCards();
        notebookObservations.Clear();
        observationSpreadStartDay = 0;

        ApplyFakeCorruptedWorldState();
        EnsureFakeCorruptedHissAudio();
        StopAllAudioExceptCorruptedHiss();
        QueueFakeCorruptedAccountObservation();
        SelectOverview();
        RefreshAllUi();
        UpdateWatchTimeText();

        WriteDebugLog("FAKE_CORRUPTED_SAVE", "Corrupted save scene started. account=" + FakeCorruptedUserName());
    }

    private void UpdateFakeCorruptedSave(float deltaTime)
    {
        if (!fakeCorruptedSaveActive)
        {
            return;
        }

        if (vhsModeEnabled || Time.unscaledTime >= fakeCorruptedNextAudioCleanupAt)
        {
            StopAllAudioExceptCorruptedHiss();
            fakeCorruptedNextAudioCleanupAt = Time.unscaledTime + 0.35f;
        }

        if (fakeCorruptedHissSource != null && !fakeCorruptedHissSource.isPlaying)
        {
            fakeCorruptedHissSource.Play();
        }
    }

    private void ApplyFakeCorruptedWorldState()
    {
        ClearFakeCorruptedWorldState();
        HideFakeCorruptedSceneNoise();

        if (settlementRoot != null)
        {
            ClearChildren(settlementRoot);
        }

        grandpas.Clear();
        foreach (Building building in buildings.Values)
        {
            building.Built = false;
            building.Root = null;
            building.Level = 0;
        }

        fireBarrelCoreLight = null;
        fireBarrelPoolLight = null;
        fireBarrelFlickerLightA = null;
        fireBarrelFlickerLightB = null;
        fireFlames = Array.Empty<Transform>();

        fakeCorruptedRoot = new GameObject("Fake Corrupted Save Scene").transform;
        fakeCorruptedRoot.SetParent(worldRoot, false);
        CreateFakeCorruptedGrave();
    }

    private void HideFakeCorruptedSceneNoise()
    {
        if (selectionMarker != null)
        {
            selectionMarker.SetActive(false);
        }

        if (hoverMarker != null)
        {
            hoverMarker.SetActive(false);
        }

        if (bridgeTrafficRoot != null)
        {
            for (int i = bridgeCars.Count - 1; i >= 0; i--)
            {
                if (bridgeCars[i] != null && bridgeCars[i].Root != null)
                {
                    Destroy(bridgeCars[i].Root.gameObject);
                }
            }

            bridgeCars.Clear();
            bridgeTrafficRoot.gameObject.SetActive(false);
        }

        if (cityAmbienceRoot != null)
        {
            cityAmbienceRoot.gameObject.SetActive(false);
        }

        if (groundLitterRoot != null)
        {
            groundLitterRoot.gameObject.SetActive(false);
        }

        ParticleSystem[] particles = worldRoot == null
            ? Array.Empty<ParticleSystem>()
            : worldRoot.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] == null)
            {
                continue;
            }

            particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles[i].gameObject.SetActive(false);
        }
    }

    private void ClearFakeCorruptedWorldState()
    {
        if (fakeCorruptedRoot != null)
        {
            Destroy(fakeCorruptedRoot.gameObject);
            fakeCorruptedRoot = null;
            fakeCorruptedGraveRoot = null;
        }
    }

    private void CreateFakeCorruptedGrave()
    {
        if (fakeCorruptedRoot == null)
        {
            return;
        }

        fakeCorruptedGraveRoot = new GameObject("Grave Instead Of Barrel").transform;
        fakeCorruptedGraveRoot.SetParent(fakeCorruptedRoot, false);
        fakeCorruptedGraveRoot.localPosition = new Vector3(0f, 0f, -0.1f);

        Material stone = Mat("fake_corrupted_grave_stone", new Color(0.20f, 0.22f, 0.24f));
        Material darkStone = Mat("fake_corrupted_grave_dark", new Color(0.055f, 0.060f, 0.068f));
        Material dirt = Mat("fake_corrupted_grave_dirt", new Color(0.105f, 0.075f, 0.050f));

        CreateBox("Fresh grave mound", fakeCorruptedGraveRoot, new Vector3(0f, 0.09f, 0.05f), new Vector3(1.68f, 0.18f, 0.92f), dirt);
        CreateBox("Cold grave slab", fakeCorruptedGraveRoot, new Vector3(0f, 0.28f, -0.05f), new Vector3(1.08f, 0.18f, 0.62f), darkStone);
        CreateBox("Cross vertical", fakeCorruptedGraveRoot, new Vector3(0f, 0.98f, -0.18f), new Vector3(0.17f, 1.38f, 0.13f), stone);
        CreateBox("Cross horizontal", fakeCorruptedGraveRoot, new Vector3(0f, 1.20f, -0.18f), new Vector3(0.72f, 0.14f, 0.13f), stone);
        AddPointLight(fakeCorruptedGraveRoot, "Cold grave light", new Vector3(0f, 1.15f, -0.75f), new Color(0.47f, 0.62f, 0.82f), 4.2f, 2.35f);
    }

    private void QueueFakeCorruptedAccountObservation()
    {
        int before = observationLeads.Count;
        string account = FakeCorruptedUserName();
        QueueObservationLead(
            "учётная запись",
            "Наблюдатель найден: " + account + "\nПапка сохранения смотрит в ответ.",
            fakeCorruptedGraveRoot,
            new Vector3(0f, 0.76f, -0.1f),
            0.22f);

        if (observationLeads.Count <= before)
        {
            return;
        }

        ObservationLead lead = observationLeads[observationLeads.Count - 1];
        lead.ScanSeconds = FakeCorruptedScanSeconds;
        lead.CorruptedAccount = true;
        WriteDebugLog("FAKE_CORRUPTED_SAVE", "Queued corrupted account observation id=" + lead.Id);
    }

    private string FakeCorruptedUserName()
    {
        try
        {
            string name = Environment.UserName;
            return string.IsNullOrWhiteSpace(name) ? "observer" : name.Trim();
        }
        catch (Exception)
        {
            return "observer";
        }
    }
}
