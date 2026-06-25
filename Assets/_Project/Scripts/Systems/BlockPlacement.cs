using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Dujiangyan.Data;
using Dujiangyan.Utils;

namespace Dujiangyan.Systems
{
    /// <summary>
    /// 构件放置交互：拖拽 ghost、网格吸附、旋转、撤销/重置视觉同步
    /// </summary>
    public class BlockPlacement : MonoBehaviour
    {
        public static BlockPlacement Instance { get; private set; }

        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;

        private Camera mainCamera;
        private GameObject ghost;
        private MeshRenderer ghostRenderer;
        private string selectedBlockId;
        private int pendingRotation;
        private readonly Dictionary<string, GameObject> placedVisuals = new Dictionary<string, GameObject>();

        public string SelectedBlockId => selectedBlockId;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            if (InputSystem.Instance != null)
            {
                InputSystem.Instance.OnPointerDown += OnPointerDown;
                InputSystem.Instance.OnPointerMove += OnPointerMove;
                InputSystem.Instance.OnPointerUp += OnPointerUp;
            }

            if (PuzzleSystem.Instance != null)
            {
                PuzzleSystem.Instance.OnBlockPlaced += OnBlockPlaced;
                PuzzleSystem.Instance.OnBlockRemoved += OnBlockRemoved;
                PuzzleSystem.Instance.OnBlockRotated += OnBlockRotated;
                PuzzleSystem.Instance.OnEditingStarted += OnEditingStarted;
            }
        }

        private void OnDisable()
        {
            if (InputSystem.Instance != null)
            {
                InputSystem.Instance.OnPointerDown -= OnPointerDown;
                InputSystem.Instance.OnPointerMove -= OnPointerMove;
                InputSystem.Instance.OnPointerUp -= OnPointerUp;
            }

            if (PuzzleSystem.Instance != null)
            {
                PuzzleSystem.Instance.OnBlockPlaced -= OnBlockPlaced;
                PuzzleSystem.Instance.OnBlockRemoved -= OnBlockRemoved;
                PuzzleSystem.Instance.OnBlockRotated -= OnBlockRotated;
                PuzzleSystem.Instance.OnEditingStarted -= OnEditingStarted;
            }
        }

        public void SelectBlock(string blockId)
        {
            selectedBlockId = blockId;
            pendingRotation = 0;
        }

        public void RotatePending()
        {
            pendingRotation = (pendingRotation + 1) % 4;
            if (ghost != null)
                ghost.transform.rotation = Quaternion.Euler(0, pendingRotation * 90f, 0);
        }

        private void OnPointerDown(Vector2 screenPos)
        {
            if (PuzzleSystem.Instance?.GetState() != PuzzleState.Editing) return;
            if (string.IsNullOrEmpty(selectedBlockId)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (TryGetGroundPosition(screenPos, out Vector3 worldPos))
            {
                CreateGhost(worldPos);
            }
        }

        private void OnPointerMove(Vector2 screenPos)
        {
            if (ghost == null) return;
            if (TryGetGroundPosition(screenPos, out Vector3 worldPos))
            {
                Vector3 snapped = GridUtility.SnapToGrid(worldPos, PuzzleSystem.Instance.CurrentConfig.terrain, 0f);
                ghost.transform.position = snapped;
                ghost.transform.rotation = Quaternion.Euler(0, pendingRotation * 90f, 0);

                bool valid = IsValidPlacement(snapped);
                ghostRenderer.material = valid ? validMaterial : invalidMaterial;
            }
        }

        private void OnPointerUp(Vector2 screenPos)
        {
            if (ghost == null) return;

            Vector3 pos = ghost.transform.position;
            bool valid = IsValidPlacement(pos);
            Destroy(ghost);
            ghost = null;
            ghostRenderer = null;

            if (valid)
            {
                var result = PuzzleSystem.Instance.TryPlaceBlock(selectedBlockId, pos, pendingRotation);
                if (!result.success)
                    Debug.LogWarning($"[BlockPlacement] Place failed: {result.errorMessage}");
            }

            selectedBlockId = null;
            pendingRotation = 0;
        }

        private void CreateGhost(Vector3 worldPos)
        {
            ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(ghost.GetComponent<Collider>());
            ghost.name = "BlockGhost";
            ghost.transform.localScale = Vector3.one * 0.9f;
            ghostRenderer = ghost.GetComponent<MeshRenderer>();

            if (validMaterial == null)
                validMaterial = ghostRenderer.material;
            if (invalidMaterial == null)
                invalidMaterial = ghostRenderer.material;

            Vector3 snapped = GridUtility.SnapToGrid(worldPos, PuzzleSystem.Instance.CurrentConfig.terrain, 0f);
            ghost.transform.position = snapped;
            ghost.transform.rotation = Quaternion.Euler(0, pendingRotation * 90f, 0);
        }

        private bool TryGetGroundPosition(Vector2 screenPos, out Vector3 worldPos)
        {
            worldPos = Vector3.zero;
            if (mainCamera == null) return false;

            Ray ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
            {
                worldPos = hit.point;
                return true;
            }
            return false;
        }

        private bool IsValidPlacement(Vector3 worldPos)
        {
            var config = PuzzleSystem.Instance.CurrentConfig;
            Vector2Int grid = GridUtility.WorldToGrid(worldPos, config.terrain);
            return GridUtility.IsInsideGrid(grid, config.terrain) && !IsGridOccupied(grid);
        }

        private bool IsGridOccupied(Vector2Int grid)
        {
            var runtime = PuzzleSystem.Instance.Runtime;
            if (runtime?.placedBlocks == null) return false;
            foreach (var block in runtime.placedBlocks)
            {
                Vector2Int blockGrid = GridUtility.WorldToGrid(block.position, PuzzleSystem.Instance.CurrentConfig.terrain);
                if (blockGrid == grid && block.health > 0f)
                    return true;
            }
            return false;
        }

        private void OnBlockPlaced(BlockInstance instance)
        {
            if (placedVisuals.ContainsKey(instance.instanceId)) return;

            var blockConfig = Resources.Load<BlockDatabaseSO>("BlockDatabase")?.GetBlock(instance.blockId);
            GameObject visual = blockConfig != null && blockConfig.prefab != null
                ? Instantiate(blockConfig.prefab)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);

            visual.name = $"Block_{instance.blockId}_{instance.instanceId}";
            visual.transform.position = instance.position;
            visual.transform.rotation = Quaternion.Euler(0, instance.rotStep * 90f, 0);
            placedVisuals[instance.instanceId] = visual;
        }

        private void OnBlockRemoved(BlockInstance instance)
        {
            if (placedVisuals.TryGetValue(instance.instanceId, out GameObject visual))
            {
                Destroy(visual);
                placedVisuals.Remove(instance.instanceId);
            }
        }

        private void OnBlockRotated(BlockInstance instance)
        {
            if (placedVisuals.TryGetValue(instance.instanceId, out GameObject visual))
            {
                visual.transform.rotation = Quaternion.Euler(0, instance.rotStep * 90f, 0);
            }
        }

        private void OnEditingStarted()
        {
            // 清理玩家放置的临时视觉对象（预置构件由场景或初始化时另外管理）
            foreach (var kvp in placedVisuals)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value);
            }
            placedVisuals.Clear();
            selectedBlockId = null;
            pendingRotation = 0;
            if (ghost != null)
            {
                Destroy(ghost);
                ghost = null;
                ghostRenderer = null;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
