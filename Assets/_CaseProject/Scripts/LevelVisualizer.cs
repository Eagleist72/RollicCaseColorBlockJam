using UnityEngine;


/// <summary>
/// An editor utility script that renders a holographic preview of the level data using Unity Gizmos.
/// Operates in Edit Mode ([ExecuteAlways]) to allow Level Designers to instantly visualize 
/// grid bounds, exit gates, blocked cells, and block placements without needing to enter Play Mode.
/// </summary>
[ExecuteAlways]
public class LevelVisualizer : MonoBehaviour
{
    [Header("Görselleþtirilecek Bölüm")]
    public LevelData levelDataToPreview;

    [Header("Ayarlar")]
    public float cellSize = 1f;
    public Vector3 gridOrigin = Vector3.zero;

    private void OnDrawGizmos()
    {
        if (levelDataToPreview == null) return;

        DrawGrid();
        DrawBlockedCells();
        DrawVisualizerBorders();
        DrawBlocks();
    }

    private void DrawGrid()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        for (int x = 0; x < levelDataToPreview.GridSize.x; x++)
        {
            for (int y = 0; y < levelDataToPreview.GridSize.y; y++)
            {
                Vector3 pos = GetWorldPosition(x, y);
                Gizmos.DrawWireCube(pos, new Vector3(cellSize, 0.1f, cellSize));
            }
        }
    }

    private void DrawBlockedCells()
    {
        if (levelDataToPreview.BlockedCellPositions == null) return;
        Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Siyah
        foreach (var pos in levelDataToPreview.BlockedCellPositions)
        {
            Gizmos.DrawCube(GetWorldPosition(pos.x, pos.y), new Vector3(cellSize, 0.2f, cellSize));
        }
    }

    private void DrawVisualizerBorders()
    {
        Vector2Int size = levelDataToPreview.GridSize;
        Color wallColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        Gizmos.color = wallColor;

        for (int x = -1; x <= size.x; x++)
        {
            DrawSingleBorderCube(x, -1);
            DrawSingleBorderCube(x, size.y);
        }

        for (int y = 0; y < size.y; y++)
        {
            DrawSingleBorderCube(-1, y);
            DrawSingleBorderCube(size.x, y);
        }
    }

    private void DrawSingleBorderCube(int x, int y)
    {
        Vector2Int size = levelDataToPreview.GridSize;
        Color finalColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        bool isGate = false;

        if (levelDataToPreview.ExitGates != null)
        {
            foreach (var gate in levelDataToPreview.ExitGates)
            {

                bool isMatch = false;
                Vector2Int gatePos = gate.GetGridPosition(levelDataToPreview.GridSize);
                switch (gate.ExitDirection)
                {
                    case Direction.Up:
                        if (x == gatePos.x && y == size.y && x >= 0 && x < size.x) isMatch = true;
                        break;
                    case Direction.Down:
                        if (x == gatePos.x && y == -1 && x >= 0 && x < size.x) isMatch = true;
                        break;
                    case Direction.Left:
                        if (x == -1 && y == gatePos.y && y >= 0 && y < size.y) isMatch = true;
                        break;
                    case Direction.Right:
                        if (x == size.x && y == gatePos.y && y >= 0 && y < size.y) isMatch = true;
                        break;
                }

                if (isMatch)
                {
                    finalColor = GetGizmoColor(gate.TargetColor);
                    isGate = true;
                    break;
                }
            }
        }

        Gizmos.color = finalColor;
        Vector3 pos = GetWorldPosition(x, y);
        float blockHeight = isGate ? 0.6f : 0.5f;
        Gizmos.DrawCube(pos + Vector3.up * 0.25f, new Vector3(cellSize * 0.95f, 0.5f, cellSize * 0.95f));
    }

    private void DrawBlocks()
    {
        if (levelDataToPreview.InitialBlocks == null) return;
        foreach (var block in levelDataToPreview.InitialBlocks)
        {
            Gizmos.color = GetGizmoColor(block.ColorType);

            var offsets = BlockShapeDatabase.GetShapeOffsets(block.ShapeType);

            foreach (var offset in offsets)
            {
                int finalX = block.RootPosition.x + offset.x;
                int finalY = block.RootPosition.y + offset.y;

                Vector3 pos = GetWorldPosition(finalX, finalY);
                Gizmos.DrawCube(pos + Vector3.up * 0.25f, new Vector3(cellSize * 0.9f, 0.5f, cellSize * 0.9f));
            }
        }
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return gridOrigin + new Vector3(x * cellSize, 0, y * cellSize);
    }

    private Color GetGizmoColor(BlockColorType colorType)
    {
        switch (colorType)
        {
            case BlockColorType.Red: return Color.red;
            case BlockColorType.Blue: return Color.blue;
            case BlockColorType.Green: return Color.green;
            case BlockColorType.Yellow: return Color.yellow;
            case BlockColorType.Purple: return new Color(0.5f, 0f, 0.5f); // Mor
            default: return Color.white;
        }
    }
}
