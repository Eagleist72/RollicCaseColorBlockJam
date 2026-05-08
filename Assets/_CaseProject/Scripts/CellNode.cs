using UnityEngine;

/// <summary>
/// Represents a single tile/node within the level's grid matrix.
/// Acts as the foundational data container that tracks its specific coordinates, 
/// static obstacle status (blocked), potential exit gate definitions, 
/// and dynamic block occupancy for collision and movement logic.
/// </summary>

public class CellNode
{
    public Vector2Int GridPosition { get; private set; }
    public bool IsBlocked { get; private set; }
    public GateData? GateInfo { get; set; }

    public BlockActor OccupyingBlock { get; private set; }

    public bool IsOccupied => OccupyingBlock != null || IsBlocked;

    public CellNode(Vector2Int position, bool isBlocked)
    {
        GridPosition = position;
        IsBlocked = isBlocked;
        OccupyingBlock = null;
        GateInfo = null;
    }

    public void SetGate(GateData gateData)
    {
        GateInfo = gateData;
    }

    public void SetOccupant(BlockActor block)
    {
        OccupyingBlock = block;
    }

    public void ClearOccupant()
    {
        OccupyingBlock = null;
    }
}