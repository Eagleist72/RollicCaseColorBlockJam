using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A static data repository defining the local grid offsets for all available block shapes.
/// Serves as the mathematical blueprint for block instantiation, rendering, and collision detection.
/// </summary>
public static class BlockShapeDatabase
{
    public static List<Vector2Int> GetShapeOffsets(BlockShapeType shapeType)
    {
        switch (shapeType)
        {
            case BlockShapeType.Single:
                return new List<Vector2Int> { Vector2Int.zero };

            case BlockShapeType.Horizontal1x2:
                return new List<Vector2Int> { Vector2Int.zero, new Vector2Int(1, 0) };

            case BlockShapeType.Vertical1x2:
                return new List<Vector2Int> { Vector2Int.zero, new Vector2Int(0, 1) };

            case BlockShapeType.Square2x2:
                return new List<Vector2Int> {
                    Vector2Int.zero, new Vector2Int(1, 0),
                    new Vector2Int(0, 1), new Vector2Int(1, 1)
                };

            case BlockShapeType.LShape:
                return new List<Vector2Int> {
                    Vector2Int.zero, new Vector2Int(0, 1),
                    new Vector2Int(0, 2), new Vector2Int(1, 0)
                };

            case BlockShapeType.Horizontal1x3:
                return new List<Vector2Int> {
                    Vector2Int.zero, new Vector2Int(1, 0), new Vector2Int(2, 0)
                };

            case BlockShapeType.Vertical1x3:
                return new List<Vector2Int> {
                    Vector2Int.zero, new Vector2Int(0, 1), new Vector2Int(0, 2)
                };

            case BlockShapeType.Cross:
                return new List<Vector2Int> {
                    Vector2Int.zero,
                    new Vector2Int(0, 1),
                    new Vector2Int(0, -1),
                    new Vector2Int(-1, 0),
                    new Vector2Int(1, 0)
                };

            default:
                return new List<Vector2Int> { Vector2Int.zero };
        }
    }
}
