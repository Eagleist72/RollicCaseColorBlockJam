using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents the dynamic physical and logical entity of a block in the level.
/// Procedurally constructs its own multi-cell visual body at runtime using base unit prefabs 
/// based on the mathematical offsets defined in the BlockShapeDatabase.
/// Tracks its own root position and calculates its total grid occupancy.
/// </summary>
public class BlockActor : MonoBehaviour
{
    public BlockShapeType ShapeType { get; private set; }
    public BlockColorType ColorType { get; private set; }
    public Vector2Int RootGridPosition { get; private set; }

    [Header("Visual Engine")]
    [Tooltip("A basic 1x1 cube prefab containing only a MeshRenderer, with NO Colliders.")]
    [SerializeField] private GameObject unitVisualPrefab;
    [SerializeField] private Transform visualParent;
    [Tooltip("Assign color materials in the exact order of the BlockColorType enum.")]
    [SerializeField] private Material[] colorMaterials;

    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = Color.white;
    [Range(1f, 10f)][SerializeField] private float outlineWidth = 4f;
    [SerializeField] private Outline.Mode outlineMode = Outline.Mode.OutlineAll;

    private List<GameObject> _visualUnits = new List<GameObject>();
    private List<Outline> _outlineComponents = new List<Outline>();

    private void OnDisable()
    {
        DisableOutline();
    }

    public void Initialize(BlockSpawnData data, Vector3 worldPos, float gridCellOffset)
    {
        ShapeType = data.ShapeType;
        ColorType = data.ColorType;
        RootGridPosition = data.RootPosition;
        transform.position = worldPos;

        BuildVisuals(gridCellOffset);
    }

    private void BuildVisuals(float cellOffset)
    {
        foreach (var unit in _visualUnits)
        {
            if (unit != null) Destroy(unit);
        }
        _visualUnits.Clear();
        _outlineComponents.Clear();

        Material targetMat = GetMaterialForColor(ColorType);
        List<Vector2Int> offsets = BlockShapeDatabase.GetShapeOffsets(ShapeType);

        foreach (Vector2Int offset in offsets)
        {
            GameObject visualUnit = Instantiate(unitVisualPrefab, visualParent);
            visualUnit.transform.localPosition = new Vector3(offset.x * cellOffset, 0f, offset.y * cellOffset);

            MeshRenderer rend = visualUnit.GetComponent<MeshRenderer>();
            if (rend != null && targetMat != null)
            {
                rend.sharedMaterial = targetMat;
            }

            Outline unitOutline = visualUnit.AddComponent<Outline>();
            unitOutline.OutlineMode = outlineMode;
            unitOutline.OutlineColor = outlineColor;
            unitOutline.OutlineWidth = outlineWidth;
            unitOutline.enabled = false;

            _outlineComponents.Add(unitOutline);
            _visualUnits.Add(visualUnit);
        }
    }

    private Material GetMaterialForColor(BlockColorType color)
    {
        int index = (int)color;
        if (index >= 0 && index < colorMaterials.Length)
        {
            return colorMaterials[index];
        }
        Debug.LogWarning($"[BlockActor] No material found for color {color}! Please check the material array in the Inspector.");
        return null;
    }

    public void UpdateLogicalPosition(Vector2Int newPos)
    {
        RootGridPosition = newPos;
    }

    public List<Vector2Int> GetOccupiedGridPositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        List<Vector2Int> offsets = BlockShapeDatabase.GetShapeOffsets(ShapeType);

        foreach (var offset in offsets)
        {
            positions.Add(RootGridPosition + offset);
        }
        return positions;
    }


    public void EnableOutline()
    {
        foreach (var outline in _outlineComponents)
        {
            if (outline != null) outline.enabled = true;
        }
    }

    public void DisableOutline()
    {
        foreach (var outline in _outlineComponents)
        {
            if (outline != null) outline.enabled = false;
        }
    }
}