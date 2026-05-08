using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core data container for a single puzzle level.
/// Dictates grid dimensions, blocked cells, dynamic gate placements, initial block spawns, and win/loss conditions.
/// Acts as the primary configuration file for Level Designers to create levels via the Unity Inspector.
/// </summary>
[CreateAssetMenu(fileName = "Level_00", menuName = "RollicCase/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [Header("Grid Configuration")]
    [Tooltip("Grid dimensions (X: Columns, Y: Rows). Minimum is 2x2.")]
    public Vector2Int GridSize = new Vector2Int(6, 9);

    [Tooltip("List of grid coordinates that are blocked and cannot contain blocks.")]
    public List<Vector2Int> BlockedCellPositions = new List<Vector2Int>();

    [Header("Gate Configuration")]
    [Tooltip("Positions, colors, and exit directions of the gates on the grid edges.")]
    public List<GateData> ExitGates = new List<GateData>();

    [Header("Puzzle Design")]
    [Tooltip("Blocks to be spawned on the grid at the start of the level.")]
    public List<BlockSpawnData> InitialBlocks = new List<BlockSpawnData>();

    [Header("Win/Loss Conditions")]
    [Tooltip("Time limit in seconds to complete the level. Minimum is 10 seconds.")]
    public float EditableTimer = 60f;

    // Data validation to prevent Level Designers from entering invalid/crashing values
    private void OnValidate()
    {
        if (GridSize.x < 2) GridSize.x = 2;
        if (GridSize.y < 2) GridSize.y = 2;
    }
}

[Serializable]
public struct GateData
{
    public BlockColorType TargetColor;
    public Direction ExitDirection;
    public int WallIndex;

    public Vector2Int GetGridPosition(Vector2Int gridSize)
    {
        switch (ExitDirection)
        {
            case Direction.Up: return new Vector2Int(WallIndex, gridSize.y - 1);
            case Direction.Down: return new Vector2Int(WallIndex, 0);
            case Direction.Left: return new Vector2Int(0, WallIndex);
            case Direction.Right: return new Vector2Int(gridSize.x - 1, WallIndex);
            default: return Vector2Int.zero;
        }
    }
}

[Serializable]
public struct BlockSpawnData
{
    public Vector2Int RootPosition;
    public BlockShapeType ShapeType;
    public BlockColorType ColorType;
}

// Expanded to match the actual Color Block Jam mechanics from your reference
public enum BlockColorType { Red, Blue, Green, Yellow, Purple }

// Expanded to support advanced puzzle designs
public enum BlockShapeType
{
    Single,
    Horizontal1x2,
    Vertical1x2,
    Square2x2,
    LShape,
    Cross,
    Horizontal1x3,
    Vertical1x3
}

public enum Direction { Up, Down, Left, Right }