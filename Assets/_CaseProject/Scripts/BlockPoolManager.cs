using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A Singleton object pooling system that pre-allocates BlockActors at runtime.
/// Prevents expensive Instantiate/Destroy calls during gameplay to eliminate Garbage Collection spikes.
/// Reuses objects via a Queue mechanism, dynamically expanding only if the initial capacity is exceeded.
/// </summary>
public class BlockPoolManager : MonoBehaviour
{
    public static BlockPoolManager Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private BlockActor blockPrefab;
    [Tooltip("Number of ready blocks to store in memory at the start of the game.")]
    [SerializeField] private int initialPoolSize = 30;

    private Queue<BlockActor> _pool = new Queue<BlockActor>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            BlockActor newBlock = Instantiate(blockPrefab, transform);
            newBlock.gameObject.SetActive(false);
            _pool.Enqueue(newBlock);
        }
    }

    public BlockActor GetBlock(Vector3 position, Transform parent)
    {
        BlockActor block;

        if (_pool.Count > 0)
        {
            block = _pool.Dequeue();
        }
        else
        {
            block = Instantiate(blockPrefab);
        }

        block.transform.SetParent(parent);
        block.transform.position = position;

        block.transform.localScale = Vector3.one;
        block.transform.rotation = Quaternion.identity;

        block.gameObject.SetActive(true);
        return block;
    }

    public void ReturnBlock(BlockActor block)
    {
        if (block == null) return;

        block.gameObject.SetActive(false);
        block.transform.SetParent(transform);
        _pool.Enqueue(block);
    }
}
