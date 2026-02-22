using System;
using System.Collections.Generic;
using Pharaoh.Map;
using Pharaoh;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Final pipeline step — instantiates physical Unity GameObjects from CMapData
    /// and stores them in a CMapInstance component.
    ///
    /// Hierarchy produced:
    ///   _mapRoot (or transform.parent)
    ///   └── MapInstance  (CMapInstance)
    ///       ├── Tile_0_0
    ///       ├── Obstacle_Rock_3_7
    ///       ├── ...
    ///       └── WaterPlane
    /// </summary>
    public class CSpawnMapStep : MonoBehaviour, IMapGenerationStep
    {
        [Header("Tile")]
        [SerializeField] private GameObject _landTilePrefab;
        [SerializeField] private GameObject _waterPlanePrefab;

        [Header("Obstacles")]
        [SerializeField] private CObstacleEntry[] _obstacles;

        [Header("Container")]
        [Tooltip("Parent for the spawned MapInstance. If null, uses this step's parent (MapGenerator).")]
        [SerializeField] private Transform _mapRoot;

        public string StepName => "Spawn Physical Map";

        public void Execute(CMapData mapData, int seed)
        {
            Transform parent = _mapRoot != null ? _mapRoot : transform.parent;

            // ── Destroy any existing MapInstance ────────────────────────────
            var existing = parent.GetComponentInChildren<CMapInstance>();
            if (existing != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(existing.gameObject);
#else
                Destroy(existing.gameObject);
#endif
            }

            // ── Create MapInstance ───────────────────────────────────────────
            var mapInstanceGO = new GameObject("MapInstance");
            mapInstanceGO.transform.SetParent(parent, false);

            var mapInstance = mapInstanceGO.AddComponent<CMapInstance>();
            mapInstance.Initialize(mapData.Width, mapData.Height);

            // ── Build obstacle prefab lookup ─────────────────────────────────
            var obstacleMap = new Dictionary<EObstacleType, GameObject>(_obstacles.Length);
            foreach (var entry in _obstacles)
            {
                if (entry.Prefab != null)
                    obstacleMap[entry.Type] = entry.Prefab;
            }

            // ── Optional water plane ─────────────────────────────────────────
            if (_waterPlanePrefab != null)
            {
                var water = Instantiate(_waterPlanePrefab, mapInstanceGO.transform);
                water.name = "WaterPlane";
                water.transform.localPosition = new Vector3(mapData.Width * 0.5f - 0.5f, 0f, mapData.Height * 0.5f - 0.5f);
            }

            // ── Spawn land tiles and obstacles ───────────────────────────────
            int tileCount = 0;
            int obstacleCount = 0;

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);
                    var cell = new CMapCell(x, y, tile.Type, tile.BiomeType, tile.ObstacleType);

                    if (tile.Type == ETileType.Land && _landTilePrefab != null)
                    {
                        var pos = new Vector3(x, 0f, y);

                        var tileGO = Instantiate(_landTilePrefab, pos, Quaternion.identity, mapInstanceGO.transform);
                        tileGO.name = $"Tile_{x}_{y}";
                        cell.TileObject = tileGO;
                        tileCount++;

                        if (tile.ObstacleType != EObstacleType.None &&
                            obstacleMap.TryGetValue(tile.ObstacleType, out var obstaclePrefab))
                        {
                            var obstacleGO = Instantiate(obstaclePrefab, pos, Quaternion.identity, mapInstanceGO.transform);
                            obstacleGO.name = $"Obstacle_{tile.ObstacleType}_{x}_{y}";
                            cell.ObstacleObject = obstacleGO;
                            obstacleCount++;
                        }
                    }

                    mapInstance.SetCell(x, y, cell);
                }
            }

            // ── Compute cell tags ───────────────────────────────────────────
            mapInstance.ComputeCellTags();

            // ── Add click detector ─────────────────────────────────────────
            var detector = mapInstanceGO.AddComponent<CMapClickDetector>();
            detector.Initialize(mapInstance);

            var collider = mapInstanceGO.AddComponent<BoxCollider>();
            collider.center = new Vector3(mapData.Width * 0.5f - 0.5f, 0f, mapData.Height * 0.5f - 0.5f);
            collider.size = new Vector3(mapData.Width, 0.2f, mapData.Height);
            mapInstanceGO.layer = CObjectLayer.RaycastTarget;

            Debug.Log($"[CSpawnMapStep] Spawned MapInstance — {tileCount} tiles, {obstacleCount} obstacles.");
        }
    }

    [Serializable]
    public struct CObstacleEntry
    {
        public EObstacleType Type;
        public GameObject Prefab;
    }
}
