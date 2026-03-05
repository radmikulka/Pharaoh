using System.Collections.Generic;
using NaughtyAttributes;
using Pharaoh.Map;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Editor tool — orchestrates IMapGenerationStep components found on direct child GameObjects.
    /// Steps execute in the order their GameObjects appear in the hierarchy.
    ///
    /// After generation, each map cell becomes a clickable CMapTileData GameObject in the scene.
    /// Edit tile types in the Inspector and mark IsOverridden to protect them across re-generates.
    /// Use BakeMapInstance to convert the editor tiles into a physical CMapInstance.
    ///
    /// Hierarchy example:
    ///   MapGenerator              ← this component
    ///   ├── BasicLayout           ← CBasicLayoutStep
    ///   ├── Obstacles_Rocks       ← CObstaclePlacementStep
    ///   ├── DualGridCellSetup     ← CDualGridCellSetupStep
    ///   ├── DualGridTerrain       ← CDualGridTerrainStep
    ///   └── EditorTiles           ← managed by this class (CMapTileData children)
    /// </summary>
    public class CMapGenerator : MonoBehaviour
    {
        [Header("Map Size")]
        [SerializeField] private int _width = 100;
        [SerializeField] private int _height = 100;

        [Header("Seed")]
        [SerializeField] private int _seed = 42;

        /// <summary>
        /// Root for all editor tile GameObjects. Persists across domain reload because it is
        /// serialized — tiles remain clickable after script recompile or Unity restart.
        /// </summary>
        [SerializeField] private Transform _editorTilesRoot;

        // Public accessors used by child steps
        public int Width  => _width;
        public int Height => _height;

        // Runtime data — not serialized, lives only while the Editor session is open.
        public CMapData MapData => _mapData;
        private CMapData _mapData;

        // ─── Buttons ────────────────────────────────────────────────────────

        [Button]
        public void Generate()
        {
            var steps = CollectSteps();

            if (steps.Length == 0)
            {
                Debug.LogWarning("[CMapGenerator] No IMapGenerationStep components found on child GameObjects.");
                return;
            }

            var overrides = CollectTileOverrides();

            _mapData = new CMapData(_width, _height);

            foreach (var step in steps)
            {
                Debug.Log($"[CMapGenerator] Running: {step.StepName}");
                step.Execute(_mapData, _seed);
            }

            foreach (var kvp in overrides)
                _mapData.Set(kvp.Key.x, kvp.Key.y, kvp.Value);

            Debug.Log($"[CMapGenerator] Done — {_width}×{_height} tiles generated.");

#if UNITY_EDITOR
            RefreshEditorTiles();
            SceneView.RepaintAll();
#endif
        }

        [Button]
        public void Clear()
        {
            _mapData = null;

            // Destroy any spawned MapInstance in the scene
            var existing = GetComponentInChildren<CMapInstance>();
            if (existing != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(existing.gameObject);
#else
                Destroy(existing.gameObject);
#endif
            }

#if UNITY_EDITOR
            if (_editorTilesRoot != null)
            {
                DestroyImmediate(_editorTilesRoot.gameObject);
                _editorTilesRoot = null;
            }

            SceneView.RepaintAll();
#endif
        }

        [Button]
        public void ToggleEditorTiles()
        {
            if (_editorTilesRoot == null)
            {
                Debug.LogWarning("[CMapGenerator] ToggleEditorTiles: no editor tiles — run Generate first.");
                return;
            }

            _editorTilesRoot.gameObject.SetActive(!_editorTilesRoot.gameObject.activeSelf);

#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }

        [Button]
        public void BakeMapInstance()
        {
            if (_editorTilesRoot == null)
            {
                Debug.LogWarning("[CMapGenerator] BakeMapInstance: no editor tiles — run Generate first.");
                return;
            }

            // Rebuild CMapData from the current state of all editor tile GOs
            // (works even after domain reload when _mapData is null)
            var mapData = new CMapData(_width, _height);
            foreach (var tileData in _editorTilesRoot.GetComponentsInChildren<CMapTileData>())
            {
                STile tile = tileData.ToSTile();
                mapData.Set(tile.X, tile.Y, tile);
            }

            // Execute all visual steps starting from CDualGridCellSetupStep
            var allSteps   = CollectSteps();
            bool foundVisual = false;
            foreach (var step in allSteps)
            {
                if (!foundVisual && step is CDualGridCellSetupStep)
                    foundVisual = true;

                if (foundVisual)
                    step.Execute(mapData, _seed);
            }

            if (!foundVisual)
            {
                Debug.LogWarning("[CMapGenerator] BakeMapInstance: CDualGridCellSetupStep not found — no visual steps executed.");
                return;
            }

            _editorTilesRoot.gameObject.SetActive(false);

#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }

        public void GenerateUpTo(IMapGenerationStep stopAfterStep)
        {
            if (_editorTilesRoot)
            {
                _editorTilesRoot.gameObject.SetActive(false);
            }
            
            if (stopAfterStep == null)
            {
                Debug.LogWarning("[CMapGenerator] GenerateUpTo called with a null step — aborting.");
                return;
            }

            var steps = CollectSteps();

            if (steps.Length == 0)
            {
                Debug.LogWarning("[CMapGenerator] No IMapGenerationStep components found on child GameObjects.");
                return;
            }

            var overrides = CollectTileOverrides();

            _mapData = new CMapData(_width, _height);

            bool found = false;
            foreach (var step in steps)
            {
                Debug.Log($"[CMapGenerator] Running: {step.StepName}");
                step.Execute(_mapData, _seed);

                if (ReferenceEquals(step, stopAfterStep))
                {
                    found = true;
                    break;
                }
            }

            foreach (var kvp in overrides)
                _mapData.Set(kvp.Key.x, kvp.Key.y, kvp.Value);

            if (!found)
                Debug.LogWarning($"[CMapGenerator] GenerateUpTo: step '{stopAfterStep.StepName}' was not found among active children — all steps were executed.");
            else
                Debug.Log($"[CMapGenerator] Done (up to '{stopAfterStep.StepName}') — {_width}×{_height} tiles.");

#if UNITY_EDITOR
            RefreshEditorTiles();
            SceneView.RepaintAll();
#endif
        }

        // ─── Helpers ────────────────────────────────────────────────────────

        /// <summary>
        /// Collects IMapGenerationStep components from direct children in sibling order.
        /// Inactive child GameObjects are skipped.
        /// </summary>
        private IMapGenerationStep[] CollectSteps()
        {
            return GetComponentsInChildren<IMapGenerationStep>();
        }

        /// <summary>
        /// Returns all tiles that have IsOverridden = true, keyed by (x, y).
        /// Call before wiping _mapData so overrides survive regeneration.
        /// </summary>
        private Dictionary<(int x, int y), STile> CollectTileOverrides()
        {
            var result = new Dictionary<(int x, int y), STile>();
            if (_editorTilesRoot == null) return result;

            foreach (var tileData in _editorTilesRoot.GetComponentsInChildren<CMapTileData>())
            {
                if (!tileData.IsOverridden) continue;
                STile tile = tileData.ToSTile();
                result[(tile.X, tile.Y)] = tile;
            }

            return result;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Destroys any existing editor tile GOs and creates one flat cube per cell in _mapData.
        /// Each cube gets a CMapTileData component initialized from the corresponding STile.
        /// </summary>
        private void RefreshEditorTiles()
        {
            if (_editorTilesRoot != null)
                DestroyImmediate(_editorTilesRoot.gameObject);

            var root = new GameObject("EditorTiles");
            root.transform.SetParent(transform, false);
            _editorTilesRoot = root.transform;
            _editorTilesRoot.gameObject.SetActive(false);

            for (int x = 0; x < _mapData.Width; x++)
            {
                for (int y = 0; y < _mapData.Height; y++)
                {
                    STile tile = _mapData.Get(x, y);

                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = $"Tile_{x}_{y}";
                    go.transform.SetParent(_editorTilesRoot, false);
                    go.transform.localPosition = new Vector3(x, 0f, y);
                    go.transform.localScale    = new Vector3(0.95f, 0.1f, 0.95f);

                    var data = go.AddComponent<CMapTileData>();
                    data.Initialize(tile);
                }
            }
        }
#endif
    }
}
