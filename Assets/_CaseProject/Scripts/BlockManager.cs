using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Core manager responsible for the lifecycle, physical movement, and collision logic of all blocks.
/// Handles spawning blocks from the object pool, validates grid occupation during drag operations, 
/// calculates clamping bounds for smooth movement, and evaluates whether a block can successfully exit through a gate.
/// </summary>
public class BlockManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private SortingManager sortingManager;

    private List<BlockActor> _activeBlocks = new List<BlockActor>();

    public int GetActiveBlockCount() => _activeBlocks.Count;
    public int ActiveBlockCount => _activeBlocks.Count;

    public void SpawnBlocksForLevel(LevelData levelData)
    {
        ClearBlocks();
        foreach (var blockData in levelData.InitialBlocks)
        {
            SpawnSingleBlock(blockData);
        }
    }

    private void SpawnSingleBlock(BlockSpawnData data)
    {
        Vector3 worldPos = gridManager.GridToWorldPosition(data.RootPosition);

        BlockActor newBlock = BlockPoolManager.Instance.GetBlock(worldPos, transform);
        newBlock.Initialize(data, worldPos, gridManager.CellSize + gridManager.CellSpacing);

        List<Vector2Int> occupiedCells = newBlock.GetOccupiedGridPositions();
        foreach (var pos in occupiedCells)
        {
            CellNode cell = gridManager.GetCellNode(pos);
            if (cell != null && !cell.IsBlocked)
            {
                cell.SetOccupant(newBlock);
            }
            else
            {
                Debug.LogError($"[BlockManager] Conflict! Block {data.ShapeType} cannot occupy coordinate {pos}.");
            }
        }

        _activeBlocks.Add(newBlock);
    }

    public void ClearBlocks()
    {
        foreach (var block in _activeBlocks)
        {
            if (block != null) BlockPoolManager.Instance.ReturnBlock(block);
        }
        _activeBlocks.Clear();
    }

    public bool DragBlock(BlockActor block, Vector3 desiredWorldPos)
    {
        Vector2Int desiredGridPos = gridManager.WorldToGridPosition(desiredWorldPos);

        if (desiredGridPos != block.RootGridPosition)
        {
            Vector3 logicalCenter = gridManager.GridToWorldPosition(block.RootGridPosition);
            Vector3 delta = desiredWorldPos - logicalCenter;

            Vector2Int step = Vector2Int.zero;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.z)) step.x = (int)Mathf.Sign(delta.x);
            else step.y = (int)Mathf.Sign(delta.z);

            Vector2Int nextPos = block.RootGridPosition + step;

            FreeOccupiedCells(block);
            if (CanBlockOccupy(block.ShapeType, nextPos)) block.UpdateLogicalPosition(nextPos);
            OccupyCells(block);
        }

        if (CheckForGateVacuum(block, desiredWorldPos)) return false;

        Vector3 currentLogicalCenter = gridManager.GridToWorldPosition(block.RootGridPosition);
        float flex = (gridManager.CellSize + gridManager.CellSpacing) * 0.49f;

        float minX = currentLogicalCenter.x, maxX = currentLogicalCenter.x;
        float minZ = currentLogicalCenter.z, maxZ = currentLogicalCenter.z;

        FreeOccupiedCells(block);
        if (CanBlockOccupy(block.ShapeType, block.RootGridPosition + Vector2Int.left)) minX -= flex;
        if (CanBlockOccupy(block.ShapeType, block.RootGridPosition + Vector2Int.right)) maxX += flex;
        if (CanBlockOccupy(block.ShapeType, block.RootGridPosition + Vector2Int.down)) minZ -= flex;
        if (CanBlockOccupy(block.ShapeType, block.RootGridPosition + Vector2Int.up)) maxZ += flex;
        OccupyCells(block);

        Vector3 clampedPos = new Vector3(
            Mathf.Clamp(desiredWorldPos.x, minX, maxX),
            desiredWorldPos.y,
            Mathf.Clamp(desiredWorldPos.z, minZ, maxZ)
        );

        block.transform.position = Vector3.Lerp(block.transform.position, clampedPos, Time.deltaTime * 25f);
        return true;
    }

    public void EndDragBlock(BlockActor block)
    {
        Vector3 snapPos = gridManager.GridToWorldPosition(block.RootGridPosition);
        block.transform.position = snapPos;
    }

    private bool CheckForGateVacuum(BlockActor block, Vector3 desiredWorldPos)
    {
        Vector3 logicalCenter = gridManager.GridToWorldPosition(block.RootGridPosition);
        Vector3 pullDelta = desiredWorldPos - logicalCenter;

        float vacuumThreshold = gridManager.CellSize * 0.4f;
        if (pullDelta.magnitude < vacuumThreshold) return false;

        Vector2Int exitStep = Vector2Int.zero;
        Vector3 exitDir = Vector3.forward;

        if (Mathf.Abs(pullDelta.x) > Mathf.Abs(pullDelta.z))
        {
            exitStep.x = (int)Mathf.Sign(pullDelta.x);
            exitDir = new Vector3(Mathf.Sign(pullDelta.x), 0, 0);
        }
        else
        {
            exitStep.y = (int)Mathf.Sign(pullDelta.z);
            exitDir = new Vector3(0, 0, Mathf.Sign(pullDelta.z));
        }

        bool isValidGateDrag = false;
        foreach (var pos in block.GetOccupiedGridPositions())
        {
            CellNode cell = gridManager.GetCellNode(pos);
            if (cell != null && cell.GateInfo != null && cell.GateInfo.Value.TargetColor == block.ColorType)
            {
                Direction reqDir = cell.GateInfo.Value.ExitDirection;
                if ((reqDir == Direction.Up && exitStep.y > 0) ||
                    (reqDir == Direction.Down && exitStep.y < 0) ||
                    (reqDir == Direction.Right && exitStep.x > 0) ||
                    (reqDir == Direction.Left && exitStep.x < 0))
                {
                    isValidGateDrag = true;
                    break;
                }
            }
        }

        if (!isValidGateDrag) return false;

        if (!CanBlockExit(block, exitStep))
        {
            return false;
        }

        if (sortingManager.TryDockBlock(block, exitDir))
        {
            FreeOccupiedCells(block);
            _activeBlocks.Remove(block);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CheckWinCondition();
            }

            return true;
        }

        return false;
    }

    private bool CanBlockExit(BlockActor block, Vector2Int exitStep)
    {
        List<Vector2Int> currentPositions = block.GetOccupiedGridPositions();
        int maxDimension = Mathf.Max(gridManager.GridSize.x, gridManager.GridSize.y);

        for (int step = 1; step <= maxDimension; step++)
        {
            bool isFullyOutside = true;

            foreach (Vector2Int pos in currentPositions)
            {
                Vector2Int projectedPos = pos + (exitStep * step);

                if (projectedPos.x >= 0 && projectedPos.x < gridManager.GridSize.x &&
                    projectedPos.y >= 0 && projectedPos.y < gridManager.GridSize.y)
                {
                    isFullyOutside = false;

                    CellNode cell = gridManager.GetCellNode(projectedPos);

                    if (cell != null && cell.IsOccupied && cell.OccupyingBlock != block)
                    {
                        return false;
                    }
                }
            }

            if (isFullyOutside)
            {
                break;
            }
        }

        return true;
    }

    private void FreeOccupiedCells(BlockActor block)
    {
        foreach (var pos in block.GetOccupiedGridPositions())
        {
            CellNode cell = gridManager.GetCellNode(pos);
            if (cell != null) cell.ClearOccupant();
        }
    }

    private void OccupyCells(BlockActor block)
    {
        foreach (var pos in block.GetOccupiedGridPositions())
        {
            CellNode cell = gridManager.GetCellNode(pos);
            if (cell != null) cell.SetOccupant(block);
        }
    }

    private bool CanBlockOccupy(BlockShapeType shape, Vector2Int rootPos)
    {
        List<Vector2Int> offsets = BlockShapeDatabase.GetShapeOffsets(shape);
        foreach (Vector2Int offset in offsets)
        {
            Vector2Int checkPos = rootPos + offset;
            CellNode cell = gridManager.GetCellNode(checkPos);
            if (cell == null || cell.IsOccupied) return false;
        }
        return true;
    }
}