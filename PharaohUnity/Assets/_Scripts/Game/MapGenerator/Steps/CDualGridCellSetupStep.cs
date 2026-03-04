using Pharaoh.Map;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// First visual pipeline step — creates the CMapInstance with all CMapCell objects,
    /// adds the BoxCollider and CMapClickDetector for input.
    ///
    /// Must run before CDualGridTerrainStep and other visual steps that need CMapInstance.
    ///
    /// Hierarchy produced:
    ///   _mapRoot (or transform.parent)
    ///   └── MapInstance  (CMapInstance, BoxCollider, CMapClickDetector)
    /// </summary>
    public class CDualGridCellSetupStep : CMapGenerationStepBase
    {
        [Tooltip("Parent for the spawned MapInstance. If null, uses this step's parent (MapGenerator).")]
        [SerializeField] private Transform _mapRoot;

        public override string StepName        => "Dual Grid Cell Setup";
        public override string StepDescription => "Vytvoří CMapInstance a CMapCell objekty pro každou buňku a přidá BoxCollider + CMapClickDetector.";

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

            // ── Create CMapCell for every logical grid position ──────────────
            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    STile tile = mapData.Get(x, y);
                    var cell   = new CMapCell(x, y, tile.Type, tile.DecorationType, tile.ContentTag);
                    mapInstance.SetCell(x, y, cell);
                }
            }

            // ── Compute proximity / coastal tags ─────────────────────────────
            mapInstance.ComputeCellTags();

            // ── Raycast collider + click detector ────────────────────────────
            var col    = mapInstanceGO.AddComponent<BoxCollider>();
            col.center = new Vector3(mapData.Width * 0.5f - 0.5f, 0f, mapData.Height * 0.5f - 0.5f);
            col.size   = new Vector3(mapData.Width, 0.2f, mapData.Height);
            mapInstanceGO.layer = CObjectLayer.RaycastTarget;

            var detector = mapInstanceGO.AddComponent<CMapClickDetector>();
            detector.Initialize(mapInstance);

            Debug.Log($"[{StepName}] CMapInstance created — {mapData.Width}×{mapData.Height} cells.");
        }
    }
}
