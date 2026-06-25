using System;
using System.Collections.Generic;
using UnityEngine;
using Dujiangyan.Data;
using Dujiangyan.Utils;

namespace Dujiangyan.Systems
{
    /// <summary>
    /// 规则粒子水流模拟（带可选视觉对象池）
    /// </summary>
    public class WaterSimulation : MonoBehaviour
    {
        public static WaterSimulation Instance { get; private set; }

        public event Action<BlockInstance> OnBlockDestroyed;

        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private int maxParticles = 200;

        private LevelConfigSO config;
        private PuzzleRuntime runtime;
        private readonly List<WaterParticle> particles = new List<WaterParticle>();
        private readonly List<GameObject> visualPool = new List<GameObject>();
        private float emissionAccumulator;

        private const float BounceDamage = 5f;
        private const float SplitAngle = 45f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(LevelConfigSO levelConfig, PuzzleRuntime levelRuntime)
        {
            config = levelConfig;
            runtime = levelRuntime;
            particles.Clear();
            emissionAccumulator = 0f;
            SetVisualActiveCount(0);
        }

        public void Tick(float deltaTime)
        {
            if (config == null || runtime == null) return;

            EmitParticles(deltaTime);
            UpdateParticles(deltaTime);
            SyncVisuals();
        }

        public void Clear()
        {
            particles.Clear();
            emissionAccumulator = 0f;
            SetVisualActiveCount(0);
        }

        public bool HasActiveParticles => particles.Count > 0;

        private void EmitParticles(float deltaTime)
        {
            emissionAccumulator += config.waterSource.emissionRate * deltaTime;
            int count = Mathf.FloorToInt(emissionAccumulator);
            emissionAccumulator -= count;

            Vector3 dir = config.waterSource.emitDirection.normalized;
            for (int i = 0; i < count; i++)
            {
                if (particles.Count >= maxParticles) break;

                particles.Add(new WaterParticle
                {
                    position = config.waterSource.position,
                    velocity = dir * config.waterSource.particleSpeed,
                    lifetime = config.waterSource.particleLifetime,
                    isAlive = true
                });
            }
        }

        private void UpdateParticles(float deltaTime)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.position += p.velocity * deltaTime;
                p.lifetime -= deltaTime;

                if (p.lifetime <= 0f || !IsInsideTerrain(p.position))
                {
                    p.isAlive = false;
                    particles[i] = p;
                    continue;
                }

                Vector2Int grid = GridUtility.WorldToGrid(p.position, config.terrain);
                BlockInstance block = FindBlockAt(grid);

                if (block != null)
                {
                    HandleBlockCollision(ref p, block);
                }
                else if (IsInsideVillage(p.position))
                {
                    runtime.villageHitCount++;
                    p.isAlive = false;
                }

                particles[i] = p;
                if (!p.isAlive)
                    particles.RemoveAt(i);
            }
        }

        private void HandleBlockCollision(ref WaterParticle p, BlockInstance block)
        {
            if (block.interaction == WaterInteraction.Bounce)
            {
                Vector3 normal = GetBounceNormal(block, p.velocity);
                p.velocity = Vector3.Reflect(p.velocity, normal);
                p.position += normal * 0.05f;

                if (!block.isIndestructible)
                {
                    float prevHealth = block.health;
                    block.health -= BounceDamage;
                    if (prevHealth > 0f && block.health <= 0f)
                        OnBlockDestroyed?.Invoke(block);
                }
            }
            else if (block.interaction == WaterInteraction.Split)
            {
                // 移除当前粒子，产生两股分流
                p.isAlive = false;
                if (particles.Count + 1 < maxParticles)
                {
                    Vector3 vLeft = Quaternion.AngleAxis(-SplitAngle, Vector3.up) * p.velocity;
                    Vector3 vRight = Quaternion.AngleAxis(SplitAngle, Vector3.up) * p.velocity;
                    particles.Add(new WaterParticle { position = p.position, velocity = vLeft, lifetime = p.lifetime, isAlive = true });
                    particles.Add(new WaterParticle { position = p.position, velocity = vRight, lifetime = p.lifetime, isAlive = true });
                }
            }
        }

        private void SyncVisuals()
        {
            SetVisualActiveCount(particles.Count);
            for (int i = 0; i < particles.Count; i++)
            {
                if (visualPool[i] != null)
                    visualPool[i].transform.position = particles[i].position;
            }
        }

        private void SetVisualActiveCount(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (i >= visualPool.Count)
                    visualPool.Add(CreateVisual());
                visualPool[i].SetActive(true);
            }
            for (int i = count; i < visualPool.Count; i++)
            {
                if (visualPool[i] != null)
                    visualPool[i].SetActive(false);
            }
        }

        private GameObject CreateVisual()
        {
            GameObject go;
            if (particlePrefab != null)
            {
                go = Instantiate(particlePrefab);
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(go.GetComponent<Collider>());
                go.transform.localScale = Vector3.one * 0.15f;
            }
            go.SetActive(false);
            return go;
        }

        private BlockInstance FindBlockAt(Vector2Int grid)
        {
            if (runtime.placedBlocks == null) return null;
            foreach (var block in runtime.placedBlocks)
            {
                Vector2Int blockGrid = GridUtility.WorldToGrid(block.position, config.terrain);
                if (blockGrid == grid && block.health > 0f)
                    return block;
            }
            return null;
        }

        private bool IsInsideTerrain(Vector3 pos)
        {
            Vector2Int grid = GridUtility.WorldToGrid(pos, config.terrain);
            return GridUtility.IsInsideGrid(grid, config.terrain);
        }

        private bool IsInsideVillage(Vector3 pos)
        {
            Vector3 local = pos - config.village.center;
            return Mathf.Abs(local.x) <= config.village.size.x * 0.5f
                && Mathf.Abs(local.z) <= config.village.size.y * 0.5f;
        }

        private Vector3 GetBounceNormal(BlockInstance block, Vector3 incoming)
        {
            // 偶数 rotStep：墙面朝 +/-Z；奇数 rotStep：墙面朝 +/-X
            bool faceZ = block.rotStep % 2 == 0;
            Vector3[] candidates = faceZ
                ? new[] { Vector3.forward, Vector3.back }
                : new[] { Vector3.right, Vector3.left };

            Vector3 best = candidates[0];
            float bestDot = Vector3.Dot(best, -incoming);
            for (int i = 1; i < candidates.Length; i++)
            {
                float d = Vector3.Dot(candidates[i], -incoming);
                if (d > bestDot)
                {
                    bestDot = d;
                    best = candidates[i];
                }
            }
            return best;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private struct WaterParticle
        {
            public Vector3 position;
            public Vector3 velocity;
            public float lifetime;
            public bool isAlive;
        }
    }
}
