using UnityEngine;

/// <summary>
/// Handles player input and translates 2D screen-space interactions into 3D world-space logic.
/// Uses efficient mathematical plane raycasting to detect block selection, calculates drag offsets, 
/// and forwards the movement intents to the BlockManager.
/// </summary>
public class BlockInteractionManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BlockManager blockManager;
    [SerializeField] private GridManager gridManager;

    private Camera _mainCamera;
    private Plane _gridPlane;

    private BlockActor _selectedBlock;
    private Vector3 _dragOffset;

    private void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("[BlockInteractionManager] No camera tagged 'MainCamera' found in the scene!");
        }

        _gridPlane = new Plane(Vector3.up, Vector3.zero);

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPointerDownEvent += HandlePointerDown;
            InputManager.Instance.OnPointerDragEvent += HandlePointerDrag;
            InputManager.Instance.OnPointerUpEvent += HandlePointerUp;
        }
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPointerDownEvent -= HandlePointerDown;
            InputManager.Instance.OnPointerDragEvent -= HandlePointerDrag;
            InputManager.Instance.OnPointerUpEvent -= HandlePointerUp;
        }
    }

    private void HandlePointerDown(Vector2 screenPos)
    {
        if (_mainCamera == null || gridManager == null) return;

        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        if (_gridPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector2Int gridPos = gridManager.WorldToGridPosition(hitPoint);
            CellNode cell = gridManager.GetCellNode(gridPos);

            if (cell != null && cell.IsOccupied && cell.OccupyingBlock != null && cell.OccupyingBlock.gameObject.activeInHierarchy)
            {
                GameManager.Instance.StartTimer();
                _selectedBlock = cell.OccupyingBlock;
                _dragOffset = _selectedBlock.transform.position - hitPoint;

                _selectedBlock.EnableOutline();
            }
        }
    }

    private void HandlePointerDrag(Vector2 screenPos)
    {
        if (_selectedBlock == null || _mainCamera == null) return;

        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        if (_gridPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 desiredWorldPos = hitPoint + _dragOffset;

            bool isStillActive = blockManager.DragBlock(_selectedBlock, desiredWorldPos);

            if (!isStillActive)
            {
                _selectedBlock = null;
            }
        }
    }

    private void HandlePointerUp(Vector2 screenPos)
    {
        if (_selectedBlock != null)
        {
            _selectedBlock.DisableOutline();

            blockManager.EndDragBlock(_selectedBlock);
            _selectedBlock = null;
        }
    }
}