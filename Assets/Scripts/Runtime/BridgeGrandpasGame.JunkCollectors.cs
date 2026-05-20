using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private bool UpdateGrandpaJunkCollector(Grandpa grandpa, float deltaTime)
    {
        if (grandpa == null || grandpa.WorkMode != GrandpaWorkMode.JunkCollector || grandpa.Root == null)
        {
            return false;
        }

        switch (grandpa.JunkState)
        {
            case JunkCollectorState.GoingToPile:
                return UpdateCollectorGoingToPile(grandpa, deltaTime);
            case JunkCollectorState.Collecting:
                return UpdateCollectorCollecting(grandpa);
            case JunkCollectorState.ReturningToDepot:
                return UpdateCollectorReturning(grandpa, deltaTime);
            case JunkCollectorState.Depositing:
                return UpdateCollectorDepositing(grandpa);
            case JunkCollectorState.NoAvailablePiles:
                return UpdateCollectorWaiting(grandpa, deltaTime);
            default:
                return BeginCollectorNextTask(grandpa, deltaTime);
        }
    }

    private bool BeginCollectorNextTask(Grandpa grandpa, float deltaTime)
    {
        JunkPile pile = FindClosestAvailableJunkPile(grandpa);
        if (pile == null)
        {
            grandpa.JunkState = JunkCollectorState.NoAvailablePiles;
            grandpa.JunkWorkUntil = Time.time + 3.0f;
            grandpa.Target = ClampToCommune(JunkDepotPosition() + new Vector3(0.85f, 0f, 0.42f));
            grandpa.IdleAction = GrandpaIdleAction.Wandering;
            return MoveGrandpaForCollector(grandpa, 0.48f, deltaTime);
        }

        ReserveJunkPile(grandpa, pile);
        grandpa.JunkState = JunkCollectorState.GoingToPile;
        grandpa.Target = JunkPileWorkPoint(pile);
        grandpa.IdleAction = GrandpaIdleAction.Wandering;
        grandpa.HasInteraction = false;
        return MoveGrandpaForCollector(grandpa, 0.62f, deltaTime);
    }

    private bool UpdateCollectorGoingToPile(Grandpa grandpa, float deltaTime)
    {
        JunkPile pile = FindJunkPile(grandpa.JunkPileId);
        if (pile == null || pile.RemainingJunk <= 0.05f)
        {
            ReleaseGrandpaJunkReservation(grandpa);
            grandpa.JunkState = JunkCollectorState.Idle;
            return true;
        }

        grandpa.Target = JunkPileWorkPoint(pile);
        if (DistanceToTarget(grandpa) <= 0.14f)
        {
            grandpa.JunkState = JunkCollectorState.Collecting;
            grandpa.JunkWorkUntil = Time.time + UnityEngine.Random.Range(1.65f, 2.65f);
            grandpa.IdleAction = GrandpaIdleAction.CollectingJunk;
            grandpa.IdleActionUntil = grandpa.JunkWorkUntil;
            ShowThought(grandpa, "тут что-то есть", 1.6f);
            FacePoint(grandpa, pile.Position);
            UpdateGrandpaInteractionPose(grandpa);
            return true;
        }

        return MoveGrandpaForCollector(grandpa, 0.62f, deltaTime);
    }

    private bool UpdateCollectorCollecting(Grandpa grandpa)
    {
        JunkPile pile = FindJunkPile(grandpa.JunkPileId);
        if (pile == null)
        {
            grandpa.JunkState = JunkCollectorState.Idle;
            return true;
        }

        FacePoint(grandpa, pile.Position);
        grandpa.Target = JunkPileWorkPoint(pile);
        grandpa.IdleAction = GrandpaIdleAction.CollectingJunk;
        grandpa.IdleActionUntil = Mathf.Max(grandpa.IdleActionUntil, Time.time + 0.1f);
        UpdateGrandpaInteractionPose(grandpa);

        if (Time.time < grandpa.JunkWorkUntil)
        {
            return true;
        }

        float taken = Mathf.Min(pile.RemainingJunk, UnityEngine.Random.Range(2.4f, 4.8f));
        pile.RemainingJunk = Mathf.Max(0f, pile.RemainingJunk - taken);
        pile.Pulse = 1f;
        grandpa.CarryingJunk = taken;
        RefreshJunkPileVisual(pile);
        ReleaseGrandpaJunkReservation(grandpa);

        grandpa.JunkState = JunkCollectorState.ReturningToDepot;
        grandpa.Target = JunkDepotDropPoint(grandpa);
        grandpa.IdleAction = GrandpaIdleAction.CarryingJunk;
        grandpa.IdleActionUntil = 0f;
        return true;
    }

    private bool UpdateCollectorReturning(Grandpa grandpa, float deltaTime)
    {
        grandpa.Target = JunkDepotDropPoint(grandpa);
        grandpa.IdleAction = GrandpaIdleAction.CarryingJunk;
        if (DistanceToTarget(grandpa) <= 0.14f)
        {
            grandpa.JunkState = JunkCollectorState.Depositing;
            grandpa.JunkWorkUntil = Time.time + 0.72f;
            grandpa.IdleAction = GrandpaIdleAction.DepositingJunk;
            grandpa.IdleActionUntil = grandpa.JunkWorkUntil;
            FacePoint(grandpa, JunkDepotPosition());
            UpdateGrandpaInteractionPose(grandpa);
            return true;
        }

        return MoveGrandpaForCollector(grandpa, 0.50f, deltaTime);
    }

    private bool UpdateCollectorDepositing(Grandpa grandpa)
    {
        FacePoint(grandpa, JunkDepotPosition());
        grandpa.IdleAction = GrandpaIdleAction.DepositingJunk;
        grandpa.IdleActionUntil = Mathf.Max(grandpa.IdleActionUntil, Time.time + 0.1f);
        UpdateGrandpaInteractionPose(grandpa);

        if (Time.time < grandpa.JunkWorkUntil)
        {
            return true;
        }

        if (grandpa.CarryingJunk > 0f)
        {
            float delivered = grandpa.CarryingJunk;
            stock.Junk += delivered;
            grandpa.CarryingJunk = 0f;
            junkDepotPulse = 1f;
            RefreshJunkDepotVisual();
            Notify(grandpa.Name + " притащил хлам к центральной куче. +" + Mathf.CeilToInt(delivered) + " хлама.");
        }

        grandpa.JunkState = JunkCollectorState.Idle;
        grandpa.IdleAction = GrandpaIdleAction.Wandering;
        grandpa.NextMoveAt = Time.time;
        RefreshAllUi();
        return true;
    }

    private bool UpdateCollectorWaiting(Grandpa grandpa, float deltaTime)
    {
        grandpa.IdleAction = GrandpaIdleAction.Wandering;
        if (Time.time >= grandpa.JunkWorkUntil)
        {
            grandpa.JunkState = JunkCollectorState.Idle;
        }

        return MoveGrandpaForCollector(grandpa, 0.46f, deltaTime);
    }

    private void SetGrandpaWorkMode(Grandpa grandpa, GrandpaWorkMode mode)
    {
        if (grandpa == null || grandpa.IsOnExpedition)
        {
            return;
        }

        ApplyGrandpaWorkModeState(grandpa, mode);
        Notify(mode == GrandpaWorkMode.JunkCollector
            ? grandpa.Name + " назначен собирать хлам до конца дня."
            : grandpa.Name + " возвращён к обычному дедовскому быту.");
        MarkNotebookDirty();
        RefreshAllUi();
    }

    private void ApplyGrandpaWorkModeState(Grandpa grandpa, GrandpaWorkMode mode)
    {
        ReleaseGrandpaJunkReservation(grandpa);
        grandpa.WorkMode = mode;
        grandpa.JunkState = JunkCollectorState.Idle;
        grandpa.JunkPileId = -1;
        grandpa.CarryingJunk = 0f;
        grandpa.JunkWorkUntil = 0f;
        grandpa.IdleActionUntil = 0f;
        grandpa.NextMoveAt = Time.time;
        grandpa.IdleAction = GrandpaIdleAction.Wandering;
    }

    private void EndGrandpaWorkModesAtDayEnd()
    {
        bool changed = false;
        for (int i = 0; i < grandpas.Count; i++)
        {
            Grandpa grandpa = grandpas[i];
            if (grandpa == null || grandpa.WorkMode != GrandpaWorkMode.JunkCollector)
            {
                continue;
            }

            ApplyGrandpaWorkModeState(grandpa, GrandpaWorkMode.None);
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        Notify("Полночь: сборщики хлама вернулись к обычному дедовскому быту.");
        MarkNotebookDirty();
        RefreshAllUi();
    }

    private JunkPile FindClosestAvailableJunkPile(Grandpa grandpa)
    {
        JunkPile best = null;
        float bestDistance = float.MaxValue;
        Vector3 position = Flat(grandpa.Root.transform.position);
        for (int i = 0; i < junkPiles.Count; i++)
        {
            JunkPile pile = junkPiles[i];
            if (pile.RemainingJunk <= 0.05f || (pile.ReservedByGrandpaId > 0 && pile.ReservedByGrandpaId != grandpa.Id))
            {
                continue;
            }

            float distance = Vector3.Distance(position, pile.Position);
            if (distance < bestDistance)
            {
                best = pile;
                bestDistance = distance;
            }
        }

        return best;
    }

    private JunkPile FindJunkPile(int id)
    {
        for (int i = 0; i < junkPiles.Count; i++)
        {
            if (junkPiles[i].Id == id)
            {
                return junkPiles[i];
            }
        }

        return null;
    }

    private void ReserveJunkPile(Grandpa grandpa, JunkPile pile)
    {
        ReleaseGrandpaJunkReservation(grandpa);
        pile.ReservedByGrandpaId = grandpa.Id;
        grandpa.JunkPileId = pile.Id;
    }

    private void ReleaseGrandpaJunkReservation(Grandpa grandpa)
    {
        JunkPile pile = FindJunkPile(grandpa.JunkPileId);
        if (pile != null && pile.ReservedByGrandpaId == grandpa.Id)
        {
            pile.ReservedByGrandpaId = -1;
        }
    }

    private Vector3 JunkPileWorkPoint(JunkPile pile)
    {
        Vector3 direction = JunkDepotPosition() - pile.Position;
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Vector3.forward;
        }

        return ClampToCommune(pile.Position + direction.normalized * 0.62f);
    }

    private Vector3 JunkDepotDropPoint(Grandpa grandpa)
    {
        float angle = Mathf.Repeat(grandpa.Id * 71.3f, 360f) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.78f;
        return ClampToCommune(JunkDepotPosition() + offset);
    }

    private bool MoveGrandpaForCollector(Grandpa grandpa, float speed, float deltaTime)
    {
        Vector3 current = grandpa.Root.transform.position;
        Vector3 next = MoveGrandpaWithAvoidance(grandpa, current, grandpa.Target, speed + grandpa.WalkJitter * 0.35f, deltaTime);
        float bob = Mathf.Sin((Time.time + grandpa.BobSeed) * 5.5f) * 0.025f;
        grandpa.Root.transform.position = new Vector3(next.x, bob, next.z);

        Vector3 move = grandpa.Target - current;
        if (move.sqrMagnitude > 0.002f)
        {
            Quaternion look = Quaternion.LookRotation(new Vector3(move.x, 0f, move.z));
            grandpa.Root.transform.rotation = Quaternion.Slerp(grandpa.Root.transform.rotation, look, deltaTime * 5f);
        }

        UpdateGrandpaInteractionPose(grandpa);
        return true;
    }

    private float DistanceToTarget(Grandpa grandpa)
    {
        return Vector3.Distance(Flat(grandpa.Root.transform.position), Flat(grandpa.Target));
    }

    private void FacePoint(Grandpa grandpa, Vector3 point)
    {
        Vector3 toPoint = Flat(point) - Flat(grandpa.Root.transform.position);
        if (toPoint.sqrMagnitude <= 0.01f)
        {
            return;
        }

        Quaternion look = Quaternion.LookRotation(toPoint.normalized);
        grandpa.Root.transform.rotation = Quaternion.Slerp(grandpa.Root.transform.rotation, look, Time.deltaTime * 9f);
    }

    private string GrandpaWorkText(Grandpa grandpa)
    {
        if (grandpa.WorkMode != GrandpaWorkMode.JunkCollector)
        {
            return "обычный быт";
        }

        switch (grandpa.JunkState)
        {
            case JunkCollectorState.GoingToPile:
                return "идёт к куче";
            case JunkCollectorState.Collecting:
                return "собирает хлам";
            case JunkCollectorState.ReturningToDepot:
                return "несёт хлам";
            case JunkCollectorState.Depositing:
                return "складывает хлам";
            case JunkCollectorState.NoAvailablePiles:
                return "ждёт новую кучу";
            default:
                return "собиратель";
        }
    }
}
