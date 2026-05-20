using System.Collections.Generic;
using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private enum GrandpaWorkMode
    {
        None,
        JunkCollector
    }

    private enum JunkCollectorState
    {
        Idle,
        GoingToPile,
        Collecting,
        ReturningToDepot,
        Depositing,
        NoAvailablePiles
    }

    private enum JunkPileVariant
    {
        TrashBags,
        Cabinet,
        TableAndChairs,
        Crates,
        Mixed
    }

    private sealed class JunkPile
    {
        public int Id;
        public Vector3 Position;
        public float RemainingJunk;
        public float MaxJunk;
        public int ReservedByGrandpaId = -1;
        public JunkPileVariant Variant;
        public GameObject Root;
        public float Pulse;
    }

    private readonly List<JunkPile> junkPiles = new List<JunkPile>();
    private Transform junkRoot;
    private Transform junkPileRoot;
    private Transform junkDepotRoot;
    private Transform[] junkDepotGrowthPieces;
    private Vector3 junkDepotBaseScale = Vector3.one;
    private float junkDepotPulse;

    private Vector3 JunkDepotPosition()
    {
        return new Vector3(-1.45f, 0f, -0.78f);
    }
}
