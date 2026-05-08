using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the docking station (tray) where blocks land after successfully exiting the grid.
/// Handles the visual fly-out animations, manages the queue slots, checks for Match-3 conditions, 
/// and triggers game over if the dock capacity is exceeded.
/// </summary>
public class SortingManager : MonoBehaviour
{
    [Header("Dock Settings")]
    [SerializeField] private int maxDockSize = 20;
    [SerializeField] private Transform dockParent;
    [SerializeField] private float slotSpacing = 1.2f;

    private List<BlockActor> _dockedBlocks = new List<BlockActor>();
    private Dictionary<BlockActor, Coroutine> _moveCoroutines = new Dictionary<BlockActor, Coroutine>();

    public int GetDockedBlockCount() => _dockedBlocks.Count;

    public bool TryDockBlock(BlockActor block, Vector3 exitDir)
    {
        if (_dockedBlocks.Count >= maxDockSize)
        {
            Debug.LogWarning("[SortingManager] Dock is FULL! Game Over.");
            GameManager.Instance.ChangeState(GameState.GameOver);
            return false;
        }

        _dockedBlocks.Add(block);

        block.transform.SetParent(null);
        _moveCoroutines[block] = StartCoroutine(FlyOutAndDockRoutine(block, exitDir));

        return true;
    }

    private IEnumerator FlyOutAndDockRoutine(BlockActor block, Vector3 exitDir)
    {
        Vector3 startWorldPos = block.transform.position;
        Vector3 targetWorldPos = startWorldPos + (exitDir * 3.5f);
        Vector3 startScale = block.transform.localScale;

        float flyDuration = 0.18f;
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            if (block == null) yield break;
            block.transform.position = Vector3.Lerp(startWorldPos, targetWorldPos, elapsed / flyDuration);
            block.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / flyDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (block != null)
        {
            block.transform.SetParent(dockParent);
            int index = _dockedBlocks.IndexOf(block);
            Vector3 targetLocalPos = new Vector3(index * slotSpacing, 0f, 0f);
            block.transform.localPosition = targetLocalPos;

            elapsed = 0f;
            float popDuration = 0.15f;
            while (elapsed < popDuration)
            {
                if (block == null) yield break;
                block.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 0.7f, elapsed / popDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (block != null)
            {
                block.transform.localScale = Vector3.one * 0.7f;
                _moveCoroutines.Remove(block);

                CheckForMatch(block.ColorType);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CheckWinCondition();
                }
            }
        }
    }

    private void CheckForMatch(BlockColorType color)
    {
        var matchedBlocks = _dockedBlocks.Where(b => b.ColorType == color).ToList();

        if (matchedBlocks.Count >= 3)
        {
            foreach (var b in matchedBlocks)
            {
                _dockedBlocks.Remove(b);
                if (_moveCoroutines.ContainsKey(b))
                {
                    StopCoroutine(_moveCoroutines[b]);
                    _moveCoroutines.Remove(b);
                }
                BlockPoolManager.Instance.ReturnBlock(b);
            }

            UpdateDockVisuals();
        }
    }

    private void UpdateDockVisuals()
    {
        for (int i = 0; i < _dockedBlocks.Count; i++)
        {
            BlockActor block = _dockedBlocks[i];

            if (block.transform.parent == dockParent)
            {
                Vector3 targetLocalPos = new Vector3(i * slotSpacing, 0f, 0f);
                if (_moveCoroutines.ContainsKey(block)) StopCoroutine(_moveCoroutines[block]);
                _moveCoroutines[block] = StartCoroutine(ShiftSlotRoutine(block, targetLocalPos));
            }
        }
    }

    private IEnumerator ShiftSlotRoutine(BlockActor block, Vector3 targetLocalPos)
    {
        Vector3 startLocalPos = block.transform.localPosition;
        float elapsed = 0f;
        float duration = 0.15f;
        while (elapsed < duration)
        {
            if (block == null) yield break;
            block.transform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (block != null) block.transform.localPosition = targetLocalPos;
    }

    public void ClearDock()
    {
        foreach (var block in _dockedBlocks)
        {
            if (block != null) BlockPoolManager.Instance.ReturnBlock(block);
        }
        _dockedBlocks.Clear();
        foreach (var coroutine in _moveCoroutines.Values)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        _moveCoroutines.Clear();
    }
}