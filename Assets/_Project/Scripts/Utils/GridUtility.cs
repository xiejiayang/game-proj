using UnityEngine;
using Dujiangyan.Data;

namespace Dujiangyan.Utils
{
    /// <summary>
    /// 地形网格工具：世界坐标与网格坐标互转
    /// </summary>
    public static class GridUtility
    {
        public static Vector2Int WorldToGrid(Vector3 worldPos, in TerrainConfig terrain)
        {
            Vector3 local = worldPos - terrain.origin;
            int x = Mathf.FloorToInt(local.x / terrain.cellSize);
            int z = Mathf.FloorToInt(local.z / terrain.cellSize);
            return new Vector2Int(x, z);
        }

        public static Vector3 GridToWorld(Vector2Int gridPos, in TerrainConfig terrain, float y = 0f)
        {
            return terrain.origin + new Vector3(
                gridPos.x * terrain.cellSize,
                y,
                gridPos.y * terrain.cellSize);
        }

        public static bool IsInsideGrid(Vector2Int gridPos, in TerrainConfig terrain)
        {
            return gridPos.x >= 0 && gridPos.x < terrain.width
                && gridPos.y >= 0 && gridPos.y < terrain.depth;
        }

        public static Vector3 SnapToGrid(Vector3 worldPos, in TerrainConfig terrain, float y = 0f)
        {
            Vector2Int grid = WorldToGrid(worldPos, terrain);
            return GridToWorld(grid, terrain, y);
        }
    }
}
