using System;
using System.Collections.Generic;
using UnityEngine;
using Dujiangyan.Data;
using Dujiangyan.Utils;

namespace Dujiangyan.Systems
{
    /// <summary>
    /// 核心解谜状态机：协调编辑、模拟、结算
    /// </summary>
    public class PuzzleSystem : MonoBehaviour
    {
        public static PuzzleSystem Instance { get; private set; }

        public event Action OnEditingStarted;
        public event Action OnSimulationStarted;
        public event Action OnSimulationPaused;
        public event Action OnSimulationResumed;
        public event Action<PuzzleResult> OnLevelSettled;
        public event Action<BlockInstance> OnBlockPlaced;
        public event Action<BlockInstance> OnBlockRemoved;
        public event Action<BlockInstance> OnBlockRotated;
        public event Action<BlockInventory> OnInventoryChanged;

        [SerializeField] private float simulationFixedStep = 0.02f;

        private LevelDatabaseSO levelDatabase;
        private BlockDatabaseSO blockDatabase;
        private LevelConfigSO currentConfig;
        private PuzzleRuntime runtime;
        private float simulationAccumulator;
        private int preplacedBlockCount;

        public PuzzleRuntime Runtime => runtime;
        public LevelConfigSO CurrentConfig => currentConfig;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            levelDatabase = Resources.Load<LevelDatabaseSO>("LevelDatabase");
            blockDatabase = Resources.Load<BlockDatabaseSO>("BlockDatabase");
        }

        private void Update()
        {
            if (runtime == null || runtime.state != PuzzleState.Simulating)
                return;

            simulationAccumulator += Time.deltaTime;
            while (simulationAccumulator >= simulationFixedStep)
            {
                simulationAccumulator -= simulationFixedStep;
                runtime.simulationTime += simulationFixedStep;

                WaterSimulation.Instance.Tick(simulationFixedStep);

                var check = LevelResult.Evaluate(currentConfig, runtime);
                if (check.isSuccess || check.failReason != FailReason.None)
                {
                    Settle(check);
                    return;
                }
            }
        }

        public PuzzleRuntime InitLevel(string levelId)
        {
            currentConfig = levelDatabase != null ? levelDatabase.GetLevel(levelId) : null;
            if (currentConfig == null)
            {
                Debug.LogError($"[PuzzleSystem] Level config not found: {levelId}");
                return null;
            }

            runtime = new PuzzleRuntime { levelId = levelId };
            runtime.inventory = currentConfig.inventory.Clone();
            runtime.consumedResource = ResourceCost.Zero;
            runtime.state = PuzzleState.Editing;

            preplacedBlockCount = 0;
            if (currentConfig.preplacedBlocks != null)
            {
                foreach (var pre in currentConfig.preplacedBlocks)
                {
                    var config = blockDatabase != null ? blockDatabase.GetBlock(pre.blockId) : null;
                    if (config == null)
                    {
                        Debug.LogWarning($"[PuzzleSystem] Preplaced block config missing: {pre.blockId}");
                        continue;
                    }

                    var instance = CreateBlockInstance(config, pre.position, pre.rotStep);
                    instance.isIndestructible = pre.isIndestructible;
                    runtime.placedBlocks.Add(instance);
                    preplacedBlockCount++;
                }
            }

            simulationAccumulator = 0f;
            OnInventoryChanged?.Invoke(runtime.inventory);
            OnEditingStarted?.Invoke();
            return runtime;
        }

        public OperationResult TryPlaceBlock(string blockId, Vector3 worldPos, int rotStep)
        {
            if (runtime == null)
                return new OperationResult { success = false, errorMessage = "Level not initialized." };
            if (runtime.state != PuzzleState.Editing)
                return new OperationResult { success = false, errorMessage = "Can only place blocks in editing state." };

            var config = blockDatabase != null ? blockDatabase.GetBlock(blockId) : null;
            if (config == null)
                return new OperationResult { success = false, errorMessage = $"Block config not found: {blockId}" };

            if (!HasInventory(config.type))
                return new OperationResult { success = false, errorMessage = $"No {config.type} left in inventory." };

            Vector3 snapped = GridUtility.SnapToGrid(worldPos, currentConfig.terrain, 0f);
            Vector2Int grid = GridUtility.WorldToGrid(snapped, currentConfig.terrain);
            if (!GridUtility.IsInsideGrid(grid, currentConfig.terrain))
                return new OperationResult { success = false, errorMessage = "Position outside grid." };

            if (IsGridOccupied(grid))
                return new OperationResult { success = false, errorMessage = "Grid cell already occupied." };

            ConsumeInventory(config.type);
            runtime.consumedResource += config.cost;

            var instance = CreateBlockInstance(config, snapped, rotStep);
            runtime.placedBlocks.Add(instance);

            runtime.undoStack.Add(new EditAction
            {
                actionType = EditActionType.Place,
                blockId = blockId,
                instanceId = instance.instanceId,
                position = snapped,
                rotStep = rotStep
            });

            OnBlockPlaced?.Invoke(instance);
            OnInventoryChanged?.Invoke(runtime.inventory);
            return new OperationResult { success = true };
        }

        public OperationResult TryMoveBlock(string instanceId, Vector3 worldPos)
        {
            if (runtime == null || runtime.state != PuzzleState.Editing)
                return new OperationResult { success = false, errorMessage = "Cannot move block now." };

            var instance = FindInstance(instanceId);
            if (instance == null)
                return new OperationResult { success = false, errorMessage = "Block instance not found." };

            Vector3 snapped = GridUtility.SnapToGrid(worldPos, currentConfig.terrain, 0f);
            Vector2Int grid = GridUtility.WorldToGrid(snapped, currentConfig.terrain);
            if (!GridUtility.IsInsideGrid(grid, currentConfig.terrain))
                return new OperationResult { success = false, errorMessage = "Position outside grid." };

            Vector2Int oldGrid = GridUtility.WorldToGrid(instance.position, currentConfig.terrain);
            if (grid != oldGrid && IsGridOccupied(grid))
                return new OperationResult { success = false, errorMessage = "Target grid cell occupied." };

            Vector3 oldPos = instance.position;
            instance.position = snapped;

            runtime.undoStack.Add(new EditAction
            {
                actionType = EditActionType.Place,
                blockId = instance.blockId,
                instanceId = instance.instanceId,
                position = oldPos,
                rotStep = instance.rotStep
            });

            OnBlockPlaced?.Invoke(instance);
            return new OperationResult { success = true };
        }

        public OperationResult RotateBlock(string instanceId)
        {
            if (runtime == null || runtime.state != PuzzleState.Editing)
                return new OperationResult { success = false, errorMessage = "Cannot rotate block now." };

            var instance = FindInstance(instanceId);
            if (instance == null)
                return new OperationResult { success = false, errorMessage = "Block instance not found." };

            int oldRot = instance.rotStep;
            instance.rotStep = (instance.rotStep + 1) % 4;

            runtime.undoStack.Add(new EditAction
            {
                actionType = EditActionType.Rotate,
                blockId = instance.blockId,
                instanceId = instance.instanceId,
                position = instance.position,
                rotStep = oldRot
            });

            OnBlockRotated?.Invoke(instance);
            return new OperationResult { success = true };
        }

        public OperationResult RemoveBlock(string instanceId)
        {
            if (runtime == null || runtime.state != PuzzleState.Editing)
                return new OperationResult { success = false, errorMessage = "Cannot remove block now." };

            var instance = FindInstance(instanceId);
            if (instance == null)
                return new OperationResult { success = false, errorMessage = "Block instance not found." };
            if (instance.isIndestructible)
                return new OperationResult { success = false, errorMessage = "Cannot remove preplaced block." };

            var config = blockDatabase != null ? blockDatabase.GetBlock(instance.blockId) : null;
            if (config != null)
            {
                ReturnInventory(config.type);
                runtime.consumedResource -= config.cost;
            }

            runtime.placedBlocks.Remove(instance);

            runtime.undoStack.Add(new EditAction
            {
                actionType = EditActionType.Remove,
                blockId = instance.blockId,
                instanceId = instance.instanceId,
                position = instance.position,
                rotStep = instance.rotStep
            });

            OnBlockRemoved?.Invoke(instance);
            OnInventoryChanged?.Invoke(runtime.inventory);
            return new OperationResult { success = true };
        }

        public SimulationResult StartSimulation()
        {
            if (runtime == null)
                return new SimulationResult { success = false, errorMessage = "Level not initialized." };
            if (runtime.state != PuzzleState.Editing)
                return new SimulationResult { success = false, errorMessage = "Can only start simulation from editing state." };

            runtime.state = PuzzleState.Simulating;
            runtime.simulationTime = 0f;
            runtime.villageHitCount = 0;
            runtime.result = new PuzzleResult();

            WaterSimulation.Instance.Initialize(currentConfig, runtime);
            simulationAccumulator = 0f;

            OnSimulationStarted?.Invoke();
            return new SimulationResult { success = true };
        }

        public void PauseSimulation()
        {
            if (runtime == null || runtime.state != PuzzleState.Simulating) return;
            runtime.state = PuzzleState.Paused;
            OnSimulationPaused?.Invoke();
        }

        public void ResumeSimulation()
        {
            if (runtime == null || runtime.state != PuzzleState.Paused) return;
            runtime.state = PuzzleState.Simulating;
            OnSimulationResumed?.Invoke();
        }

        public bool Undo()
        {
            if (runtime == null || runtime.state != PuzzleState.Editing)
                return false;
            if (runtime.undoStack.Count == 0)
                return false;

            EditAction action = runtime.undoStack[runtime.undoStack.Count - 1];
            runtime.undoStack.RemoveAt(runtime.undoStack.Count - 1);

            switch (action.actionType)
            {
                case EditActionType.Place:
                {
                    var instance = FindInstance(action.instanceId);
                    if (instance != null)
                    {
                        var config = blockDatabase != null ? blockDatabase.GetBlock(instance.blockId) : null;
                        if (config != null)
                        {
                            ReturnInventory(config.type);
                            runtime.consumedResource -= config.cost;
                        }
                        runtime.placedBlocks.Remove(instance);
                        OnBlockRemoved?.Invoke(instance);
                    }
                    break;
                }
                case EditActionType.Remove:
                {
                    var config = blockDatabase != null ? blockDatabase.GetBlock(action.blockId) : null;
                    if (config != null)
                    {
                        ConsumeInventory(config.type);
                        runtime.consumedResource += config.cost;
                        var instance = CreateBlockInstance(config, action.position, action.rotStep);
                        instance.instanceId = action.instanceId;
                        runtime.placedBlocks.Add(instance);
                        OnBlockPlaced?.Invoke(instance);
                    }
                    break;
                }
                case EditActionType.Rotate:
                {
                    var instance = FindInstance(action.instanceId);
                    if (instance != null)
                    {
                        instance.rotStep = action.rotStep;
                        OnBlockRotated?.Invoke(instance);
                    }
                    break;
                }
            }

            OnInventoryChanged?.Invoke(runtime.inventory);
            return true;
        }

        public void Reset()
        {
            if (runtime == null || currentConfig == null) return;

            runtime.placedBlocks.RemoveRange(preplacedBlockCount, runtime.placedBlocks.Count - preplacedBlockCount);
            runtime.inventory = currentConfig.inventory.Clone();
            runtime.consumedResource = ResourceCost.Zero;
            runtime.undoStack.Clear();
            runtime.simulationTime = 0f;
            runtime.villageHitCount = 0;
            runtime.result = new PuzzleResult();
            runtime.state = PuzzleState.Editing;
            simulationAccumulator = 0f;

            WaterSimulation.Instance.Clear();
            OnInventoryChanged?.Invoke(runtime.inventory);
            OnEditingStarted?.Invoke();
        }

        public PuzzleState GetState()
        {
            return runtime != null ? runtime.state : PuzzleState.Editing;
        }

        public PuzzleResult GetResult()
        {
            return runtime != null ? runtime.result : null;
        }

        private BlockInstance CreateBlockInstance(BlockConfigSO config, Vector3 position, int rotStep)
        {
            return new BlockInstance
            {
                instanceId = System.Guid.NewGuid().ToString(),
                blockId = config.id,
                position = position,
                rotStep = rotStep,
                health = config.maxHealth,
                maxHealth = config.maxHealth,
                interaction = config.interaction,
                isIndestructible = false
            };
        }

        private BlockInstance FindInstance(string instanceId)
        {
            if (runtime?.placedBlocks == null) return null;
            foreach (var block in runtime.placedBlocks)
            {
                if (block.instanceId == instanceId)
                    return block;
            }
            return null;
        }

        private bool IsGridOccupied(Vector2Int grid)
        {
            if (runtime?.placedBlocks == null) return false;
            foreach (var block in runtime.placedBlocks)
            {
                Vector2Int blockGrid = GridUtility.WorldToGrid(block.position, currentConfig.terrain);
                if (blockGrid == grid && block.health > 0f)
                    return true;
            }
            return false;
        }

        private bool HasInventory(BlockType type)
        {
            return runtime.inventory.GetCount(type) > 0;
        }

        private void ConsumeInventory(BlockType type)
        {
            int count = runtime.inventory.GetCount(type);
            if (count > 0)
                runtime.inventory.SetCount(type, count - 1);
        }

        private void ReturnInventory(BlockType type)
        {
            int count = runtime.inventory.GetCount(type);
            runtime.inventory.SetCount(type, count + 1);
        }

        private void Settle(PuzzleResult result)
        {
            if (runtime == null) return;
            runtime.state = PuzzleState.Settling;
            runtime.result = result;
            WaterSimulation.Instance.Clear();
            OnLevelSettled?.Invoke(result);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
