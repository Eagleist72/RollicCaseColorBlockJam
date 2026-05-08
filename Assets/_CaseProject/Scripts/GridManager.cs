using UnityEngine;

/// <summary>
/// Core manager responsible for generating and handling the physical 3D grid in the scene.
/// Instantiates cell/wall prefabs, sets up exit gates with appropriate materials, 
/// handles world-to-grid coordinate conversions, and automatically adjusts the camera to frame the level.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform gridParent;

    [Header("Wall Architecture")]
    [Tooltip("Standard wall prefab used to draw the boundary walls.")]
    [SerializeField] private GameObject wallPrefab;
    [Tooltip("Y-axis offset to ensure walls sit correctly on the ground.")]
    [SerializeField] private float wallYOffset = 0.5f;
    [Tooltip("Thickness of the wall. Used to snap exactly to the grid boundaries.")]
    [SerializeField] private float wallThickness = 0.2f;

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float cellSpacing = 0.1f;

    [Header("Gate Visuals")]
    [Tooltip("Assign color materials in the exact order of the BlockColorType enum.")]
    [SerializeField] private Material[] gateColorMaterials;

    private CellNode[,] _gridMap;
    private Vector2Int _currentGridSize;

    public Vector2Int GridSize => _currentGridSize;
    public float CellSize => cellSize;
    public float CellSpacing => cellSpacing;

    public void GenerateGrid(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("[GridManager] LevelData is null. Generation aborted.");
            return;
        }

        ClearGrid();

        _currentGridSize = levelData.GridSize;
        _gridMap = new CellNode[_currentGridSize.x, _currentGridSize.y];
        Vector3 startPosition = CalculateGridStartOffset(_currentGridSize);

        for (int x = 0; x < _currentGridSize.x; x++)
        {
            for (int y = 0; y < _currentGridSize.y; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);
                bool isBlocked = levelData.BlockedCellPositions.Contains(currentPos);

                _gridMap[x, y] = new CellNode(currentPos, isBlocked);

                if (!isBlocked)
                {
                    SpawnCellVisual(x, y, startPosition);
                }
            }
        }

        foreach (var gateData in levelData.ExitGates)
        {
            Vector2Int calculatedPos = gateData.GetGridPosition(_currentGridSize);
            if (IsPositionValid(calculatedPos))
            {
                _gridMap[calculatedPos.x, calculatedPos.y].SetGate(gateData);
            }
        }

        GeneratePerimeterWalls(startPosition);

        FrameCamera(_currentGridSize);
    }

    private void GeneratePerimeterWalls(Vector3 startPosition)
    {
        int width = _currentGridSize.x;
        int height = _currentGridSize.y;

        float offset = (cellSize / 2f) + (wallThickness / 2f) + (cellSpacing / 2f);

        for (int x = 0; x < width; x++)
        {
            Vector3 bottomCellPos = GetCellWorldPosition(x, 0, startPosition);
            Vector3 bottomWallPos = bottomCellPos + new Vector3(0, wallYOffset, -offset);
            CreateWallSegment(bottomWallPos, Quaternion.identity, new Vector2Int(x, 0), Direction.Down);

            Vector3 topCellPos = GetCellWorldPosition(x, height - 1, startPosition);
            Vector3 topWallPos = topCellPos + new Vector3(0, wallYOffset, offset);
            CreateWallSegment(topWallPos, Quaternion.identity, new Vector2Int(x, height - 1), Direction.Up);
        }

        for (int y = 0; y < height; y++)
        {
            Vector3 leftCellPos = GetCellWorldPosition(0, y, startPosition);
            Vector3 leftWallPos = leftCellPos + new Vector3(-offset, wallYOffset, 0);
            CreateWallSegment(leftWallPos, Quaternion.Euler(0, 90, 0), new Vector2Int(0, y), Direction.Left);

            Vector3 rightCellPos = GetCellWorldPosition(width - 1, y, startPosition);
            Vector3 rightWallPos = rightCellPos + new Vector3(offset, wallYOffset, 0);
            CreateWallSegment(rightWallPos, Quaternion.Euler(0, 90, 0), new Vector2Int(width - 1, y), Direction.Right);
        }
    }

    private void CreateWallSegment(Vector3 position, Quaternion rotation, Vector2Int adjacentPos, Direction requiredDirection)
    {
        GameObject wallObj = Instantiate(wallPrefab, position, rotation, gridParent);

        if (IsPositionValid(adjacentPos))
        {
            CellNode adjCell = _gridMap[adjacentPos.x, adjacentPos.y];
            if (adjCell != null && adjCell.GateInfo != null)
            {
                if (adjCell.GateInfo.Value.ExitDirection == requiredDirection)
                {
                    MeshRenderer wallRend = wallObj.GetComponentInChildren<MeshRenderer>();
                    if (wallRend != null)
                    {
                        int colorIndex = (int)adjCell.GateInfo.Value.TargetColor;
                        if (colorIndex >= 0 && colorIndex < gateColorMaterials.Length)
                        {
                            wallRend.material = gateColorMaterials[colorIndex];
                        }
                    }
                }
            }
        }
    }

    private Vector3 GetCellWorldPosition(int x, int y, Vector3 startPosition)
    {
        return startPosition + new Vector3(
            x * (cellSize + cellSpacing),
            0f,
            y * (cellSize + cellSpacing)
        );
    }
    private void FrameCamera(Vector2Int gridSize)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float cameraAngleX = 57f;
        cam.transform.rotation = Quaternion.Euler(cameraAngleX, 0f, 0f);


        cam.transform.position = new Vector3(0f, 20f, -15f);

        if (!cam.orthographic)
        {
            cam.orthographic = true;
        }

        float gridWidth = (gridSize.x * cellSize) + ((gridSize.x - 1) * cellSpacing);

        float apparentDepth = (gridSize.y * cellSize) * Mathf.Sin(cameraAngleX * Mathf.Deg2Rad);

        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = gridWidth / apparentDepth;

        float verticalPadding = 1.25f;

        if (screenRatio >= targetRatio)
        {
            cam.orthographicSize = (apparentDepth / 2f) + verticalPadding;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            cam.orthographicSize = ((apparentDepth / 2f) + verticalPadding) * differenceInSize;
        }
    }

    private Vector3 CalculateGridStartOffset(Vector2Int size)
    {
        float totalWidth = (size.x * cellSize) + ((size.x - 1) * cellSpacing);
        float totalDepth = (size.y * cellSize) + ((size.y - 1) * cellSpacing);
        float startX = -totalWidth / 2f + (cellSize / 2f);
        float startZ = -totalDepth / 2f + (cellSize / 2f);
        return new Vector3(startX, 0f, startZ);
    }

    private void SpawnCellVisual(int x, int y, Vector3 startPosition)
    {
        Vector3 spawnPosition = startPosition + new Vector3(
            x * (cellSize + cellSpacing),
            0f,
            y * (cellSize + cellSpacing)
        );
        Instantiate(cellPrefab, spawnPosition, Quaternion.identity, gridParent);
    }

    public CellNode GetCellNode(Vector2Int position)
    {
        return IsPositionValid(position) ? _gridMap[position.x, position.y] : null;
    }

    private bool IsPositionValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < _currentGridSize.x &&
               position.y >= 0 && position.y < _currentGridSize.y;
    }

    public void ClearGrid()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
        _gridMap = null;
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 startOffset = CalculateGridStartOffset(_currentGridSize);
        int x = Mathf.RoundToInt((worldPosition.x - startOffset.x) / (cellSize + cellSpacing));
        int y = Mathf.RoundToInt((worldPosition.z - startOffset.z) / (cellSize + cellSpacing));
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        if (!IsPositionValid(gridPosition)) return Vector3.zero;
        Vector3 startOffset = CalculateGridStartOffset(_currentGridSize);
        float worldX = startOffset.x + (gridPosition.x * (cellSize + cellSpacing));
        float worldZ = startOffset.z + (gridPosition.y * (cellSize + cellSpacing));
        return new Vector3(worldX, 0f, worldZ);
    }
}