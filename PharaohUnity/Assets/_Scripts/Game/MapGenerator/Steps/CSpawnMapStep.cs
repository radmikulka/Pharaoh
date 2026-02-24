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
    /// Two-pass approach:
    ///   Pass 1 — creates all CMapCell objects and spawns tile / decoration GOs.
    ///   Pass 2 — spawns obstacle prefabs and assigns ObstacleObject on the tile.
    ///
    /// Hierarchy produced:
    ///   _mapRoot (or transform.parent)
    ///   └── MapInstance  (CMapInstance)
    ///       ├── Tile_0_0
    ///       ├── Obstacle_3_7
    ///       └── WaterPlane
    /// </summary>
    public class CSpawnMapStep : CMapGenerationStepBase
    {
        [Header("Tile")]
        [SerializeField] private GameObject _landTilePrefab;
        [SerializeField] private GameObject _sandTilePrefab;
        [SerializeField] private GameObject _waterPlanePrefab;

        [Header("Decorations")]
        [SerializeField] private CDecorationEntry[] _decorations;

        [Header("Container")]
        [Tooltip("Parent for the spawned MapInstance. If null, uses this step's parent (MapGenerator).")]
        [SerializeField] private Transform _mapRoot;

        [Header("Sand Tint")]
        [SerializeField] private Color _sandTintA = new(0.90f, 0.82f, 0.55f);
        [SerializeField] private Color _sandTintB = new(0.95f, 0.88f, 0.60f);

        private static readonly int TintColorId = Shader.PropertyToID("_TintColor");
        private MaterialPropertyBlock _tileMpb;

        public override string StepName => "Spawn Physical Map";
        public override string StepDescription => "Spawní fyzické herní objekty z CMapData a sestavuje CMapInstance se všemi dlaždicemi, překážkami a dekoracemi.";

        // Deterministic hash [0,1] from tile position — same seed → same result every generate.
        private static float TileHash(int x, int y)
        {
            int h = x * 374761393 + y * 1099087573;
            h = (h ^ (h >> 13)) * 1597334677;
            h ^= h >> 16;
            return (h & 0x7FFFFFFF) / (float)0x7FFFFFFF;
        }

        public override void Execute(CMapData mapData, int seed)
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

            // ── Create MapInstance root ──────────────────────────────────────
            var mapInstanceGO = new GameObject("MapInstance");
            mapInstanceGO.transform.SetParent(parent, false);

            var mapInstance = mapInstanceGO.AddComponent<CMapInstance>();
            mapInstance.Initialize(mapData.Width, mapData.Height);

            // ── Build decoration prefab lookup ──────────────────────────────
            var decorationMap = new Dictionary<EDecorationType, GameObject>(_decorations.Length);
            foreach (var entry in _decorations)
            {
                if (entry.Prefab != null)
                    decorationMap[entry.Type] = entry.Prefab;
            }

            // ── Optional water plane ─────────────────────────────────────────
            if (_waterPlanePrefab != null)
            {
                var water = Instantiate(_waterPlanePrefab, mapInstanceGO.transform);
                water.name = "WaterPlane";
                water.transform.localPosition = new Vector3(mapData.Width * 0.5f - 0.5f, 0f, mapData.Height * 0.5f - 0.5f);
            }

            _tileMpb = new MaterialPropertyBlock();
            int tileCount = 0;
            int decorationCount = 0;

            // ── Pass 1: create cells, spawn tiles and decorations ────────────
            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);
                    var cell = new CMapCell(x, y, tile.Type, tile.DecorationType);

                    if (tile.Type.IsBuildable())
                    {
                        var pos = new Vector3(x, 0f, y);

                        bool isSand = tile.Type == ETileType.Sand;
                        var prefab = isSand && _sandTilePrefab != null ? _sandTilePrefab : _landTilePrefab;

                        if (prefab != null)
                        {
                            var tileGO = Instantiate(prefab, pos, Quaternion.identity, mapInstanceGO.transform);
                            tileGO.name = $"Tile_{x}_{y}";
                            cell.TileObject = tileGO;
                            tileCount++;

                            if (isSand)
                            {
                                float t = TileHash(x, y);
                                _tileMpb.SetColor(TintColorId, Color.Lerp(_sandTintA, _sandTintB, t));
                                var renderer = tileGO.GetComponentInChildren<Renderer>();
                                if (renderer != null)
                                    renderer.SetPropertyBlock(_tileMpb);
                            }
                        }

                        if (tile.DecorationType != EDecorationType.None &&
                            decorationMap.TryGetValue(tile.DecorationType, out var decorationPrefab))
                        {
                            var decoGO = Instantiate(decorationPrefab, pos, Quaternion.identity, mapInstanceGO.transform);
                            decoGO.name = $"Decoration_{tile.DecorationType}_{x}_{y}";
                            cell.DecorationObject = decoGO;
                            decorationCount++;
                        }
                    }

                    mapInstance.SetCell(x, y, cell);
                }
            }

            // ── Pass 2: spawn obstacles, mark formation footprints ───────────
            int obstacleCount = 0;

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);
                    if (!tile.Type.IsBuildable()) continue;
                    if (tile.ObstaclePrefab == null) continue;

                    var pos = new Vector3(x, 0f, y);
                    var obstacleGO = Instantiate(tile.ObstaclePrefab, pos, Quaternion.identity, mapInstanceGO.transform);
                    obstacleGO.name = $"Obstacle_{x}_{y}";
                    obstacleCount++;

                    mapInstance.GetCell(x, y).ObstacleObject = obstacleGO;
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

            Debug.Log($"[CSpawnMapStep] Spawned MapInstance — {tileCount} tiles, {obstacleCount} obstacles, {decorationCount} decorations.");
        }
    }

    [Serializable]
    public struct CDecorationEntry
    {
        public EDecorationType Type;
        public GameObject Prefab;
    }
}
