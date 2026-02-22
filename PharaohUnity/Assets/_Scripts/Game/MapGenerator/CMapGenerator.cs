using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using AldaEngine;
using UnityEditor;
#endif

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Editor tool — orchestrates IMapGenerationStep components found on direct child GameObjects.
    /// Steps execute in the order their GameObjects appear in the hierarchy.
    ///
    /// Hierarchy example:
    ///   MapGenerator              ← this component
    ///   ├── BasicLayout           ← CBasicLayoutStep
    ///   ├── VoronoiRegions        ← CVoronoiRegionStep (+ BiomePoint children)
    ///   ├── Obstacles_Rocks       ← CObstaclePlacementStep
    ///   └── Obstacles_Trees       ← CObstaclePlacementStep
    /// </summary>
    public class CMapGenerator : MonoBehaviour
    {
        [Header("Map Size")]
        [SerializeField] private int _width = 100;
        [SerializeField] private int _height = 100;

        [Header("Seed")]
        [SerializeField] private int _seed = 42;

        [Header("Gizmos")]
        [SerializeField] private bool _showObstacles = true;

        // Public accessors used by child steps
        public int Width  => _width;
        public int Height => _height;

        // Runtime data — not serialized, lives only while the Editor session is open.
        // Public so the custom editor (separate assembly) can modify tiles directly.
        public CMapData MapData => _mapData;
        private CMapData _mapData;

        // ─── Buttons ────────────────────────────────────────────────────────

        [Button]
        public void Generate()
        {
            var steps = CollectSteps();

            if (steps.Count == 0)
            {
                Debug.LogWarning("[CMapGenerator] No IMapGenerationStep components found on child GameObjects.");
                return;
            }

            _mapData = new CMapData(_width, _height);

            foreach (var step in steps)
            {
                Debug.Log($"[CMapGenerator] Running: {step.StepName}");
                step.Execute(_mapData, _seed);
            }

            Debug.Log($"[CMapGenerator] Done — {_width}×{_height} tiles generated.");

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        [Button]
        public void Clear()
        {
            _mapData = null;

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        public void GenerateUpTo(IMapGenerationStep stopAfterStep)
        {
            if (stopAfterStep == null)
            {
                Debug.LogWarning("[CMapGenerator] GenerateUpTo called with a null step — aborting.");
                return;
            }

            var steps = CollectSteps();

            if (steps.Count == 0)
            {
                Debug.LogWarning("[CMapGenerator] No IMapGenerationStep components found on child GameObjects.");
                return;
            }

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

            if (!found)
                Debug.LogWarning($"[CMapGenerator] GenerateUpTo: step '{stopAfterStep.StepName}' was not found among active children — all steps were executed.");
            else
                Debug.Log($"[CMapGenerator] Done (up to '{stopAfterStep.StepName}') — {_width}×{_height} tiles.");

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        // ─── Helpers ────────────────────────────────────────────────────────

        /// <summary>
        /// Collects IMapGenerationStep components from direct children in sibling order.
        /// Inactive child GameObjects are skipped.
        /// </summary>
        private List<IMapGenerationStep> CollectSteps()
        {
            var result = new List<IMapGenerationStep>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (!child.gameObject.activeSelf) continue;

                var step = child.GetComponent<IMapGenerationStep>();
                if (step != null)
                    result.Add(step);
            }
            return result;
        }

        // ─── Biome colors (shared with CBiomePoint and editor painter) ───────

        private static readonly Color[] _biomeColors = new Color[(int)EBiomeType.COUNT]
        {
            new(0.50f, 0.50f, 0.50f, 0.85f), // None     — gray
            new(0.90f, 0.75f, 0.35f, 0.85f), // Desert   — sandy
            new(0.40f, 0.78f, 0.30f, 0.85f), // Grassland— green
            new(0.10f, 0.48f, 0.10f, 0.85f), // Forest   — dark green
            new(0.30f, 0.42f, 0.20f, 0.85f), // Swamp    — murky
            new(0.75f, 0.90f, 0.95f, 0.85f), // Tundra   — ice blue
            new(0.45f, 0.10f, 0.05f, 0.85f), // Volcano  — dark red
            new(0.78f, 0.82f, 0.20f, 0.85f), // Savanna  — yellow-green
        };

        internal static Color GetBiomeColor(EBiomeType biome)
        {
            int idx = (int)biome;
            return idx >= 0 && idx < _biomeColors.Length ? _biomeColors[idx] : _biomeColors[0];
        }

        // ─── Gizmos ─────────────────────────────────────────────────────────

        private static readonly Color _waterColor    = new(0.20f, 0.45f, 0.85f, 0.80f);
        private static readonly Color _landColor     = new(0.30f, 0.65f, 0.25f, 0.80f);
        private static readonly Color _obstacleColor = new(0.10f, 0.10f, 0.10f, 0.90f);

        private void OnDrawGizmos()
        {
            if (_mapData == null) return;

            Vector3 origin   = transform.position;
            const float ts   = 1f;
            var tileSize     = Vector3.one * (ts * 0.95f);
            var obstacleSize = Vector3.one * (ts * 0.40f);

            for (int x = 0; x < _mapData.Width; x++)
            {
                for (int y = 0; y < _mapData.Height; y++)
                {
                    STile tile = _mapData.Get(x, y);
                    Vector3 pos = origin + new Vector3(x * ts, 0f, y * ts);

                    // Base tile color
                    if (tile.Type == ETileType.Water)
                        Gizmos.color = _waterColor;
                    else if (tile.BiomeType != EBiomeType.None)
                        Gizmos.color = GetBiomeColor(tile.BiomeType);
                    else
                        Gizmos.color = _landColor;

                    Gizmos.DrawCube(pos, tileSize);

                    // Obstacle marker — small dark cube raised above the tile
                    if (_showObstacles && tile.ObstacleType != EObstacleType.None)
                    {
                        Gizmos.color = _obstacleColor;
                        Gizmos.DrawCube(pos + Vector3.up * 0.5f, obstacleSize);
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CMapGenerator))]
    public class CMapGeneratorEditor : CBaseEditor<CMapGenerator>
    {
        // ─── Paint state ─────────────────────────────────────────────────────

        private bool _paintMode;

        private enum EPaintTarget { Obstacle, Biome, TileType }
        private EPaintTarget _paintTarget = EPaintTarget.Obstacle;

        private EObstacleType _selectedObstacle = EObstacleType.Rock;
        private EBiomeType    _selectedBiome    = EBiomeType.Desert;
        private ETileType     _selectedTileType = ETileType.Land;

        // ─── Coordinate input state ──────────────────────────────────────────

        private int _coordX;
        private int _coordY;

        // ─── Hover state ─────────────────────────────────────────────────────

        private Vector2Int _hoveredTile = new(-1, -1);

        private CMapGenerator Generator => (CMapGenerator)target;

        // ─── Inspector GUI ───────────────────────────────────────────────────

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var gen = Generator;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Tile Painter", EditorStyles.boldLabel);

            if (gen.MapData == null)
            {
                EditorGUILayout.HelpBox("No map data — run Generate first.", MessageType.Warning);
                return;
            }

            // Paint mode toggle
            bool newPaintMode = EditorGUILayout.Toggle("Paint Mode", _paintMode);
            if (newPaintMode != _paintMode)
            {
                _paintMode = newPaintMode;
                SceneView.RepaintAll();
            }

            if (_paintMode)
            {
                EditorGUI.indentLevel++;

                _paintTarget = (EPaintTarget)EditorGUILayout.EnumPopup("Target", _paintTarget);

                switch (_paintTarget)
                {
                    case EPaintTarget.Obstacle:
                        _selectedObstacle = (EObstacleType)EditorGUILayout.EnumPopup("Obstacle", _selectedObstacle);
                        break;
                    case EPaintTarget.Biome:
                        _selectedBiome = (EBiomeType)EditorGUILayout.EnumPopup("Biome", _selectedBiome);
                        break;
                    case EPaintTarget.TileType:
                        _selectedTileType = (ETileType)EditorGUILayout.EnumPopup("Tile Type", _selectedTileType);
                        break;
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.HelpBox("LMB: paint     RMB: erase / clear", MessageType.None);
            }

            // ── Coordinate input ─────────────────────────────────────────────

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Set by Coordinates", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _coordX = EditorGUILayout.IntField("X", _coordX);
            _coordY = EditorGUILayout.IntField("Y", _coordY);
            EditorGUILayout.EndHorizontal();

            if (gen.MapData.IsValid(_coordX, _coordY))
            {
                STile preview = gen.MapData.Get(_coordX, _coordY);
                EditorGUILayout.HelpBox(
                    $"Tile ({_coordX}, {_coordY})  |  {preview.Type}  |  Biome: {preview.BiomeType}  |  Obstacle: {preview.ObstacleType}",
                    MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("Coordinates out of bounds.", MessageType.Warning);
            }

            if (GUILayout.Button("Apply to Tile at Coordinates"))
            {
                if (gen.MapData.IsValid(_coordX, _coordY))
                {
                    ApplyPaint(gen, new Vector2Int(_coordX, _coordY), erase: false);
                    SceneView.RepaintAll();
                }
            }
        }

        // ─── Scene GUI ───────────────────────────────────────────────────────

        private void OnSceneGUI()
        {
            var gen = Generator;
            if (gen.MapData == null) return;

            var evt = Event.current;

            // Track hovered tile on all mouse events
            if (evt.type is EventType.MouseMove or EventType.MouseDrag or EventType.MouseDown)
            {
                var newHover = WorldToTile(gen);
                if (newHover != _hoveredTile)
                {
                    _hoveredTile = newHover;
                    SceneView.RepaintAll();
                }
            }

            // Draw hover highlight
            DrawHoverHighlight(gen);

            if (!_paintMode) return;

            // Prevent the scene view from deselecting the GameObject while painting
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlId);

            // Paint / erase on left or right click and drag
            if ((evt.type is EventType.MouseDown or EventType.MouseDrag) &&
                (evt.button == 0 || evt.button == 1))
            {
                var tile = WorldToTile(gen);
                if (tile.x >= 0)
                {
                    ApplyPaint(gen, tile, erase: evt.button == 1);
                    SceneView.RepaintAll();
                    evt.Use(); // prevent camera navigation when hitting a tile
                }
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        /// <summary>
        /// Casts a ray from the current mouse position and returns the tile coordinates
        /// it hits on the map plane, or (-1,-1) if the ray misses the map.
        /// </summary>
        private Vector2Int WorldToTile(CMapGenerator gen)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            // Intersect with the horizontal plane at the generator's Y position
            float planeY = gen.transform.position.y;
            if (Mathf.Abs(ray.direction.y) < 0.0001f) return new Vector2Int(-1, -1);

            float t = (planeY - ray.origin.y) / ray.direction.y;
            if (t < 0) return new Vector2Int(-1, -1);

            Vector3 hit   = ray.GetPoint(t);
            Vector3 local = hit - gen.transform.position;

            // Tiles are centered at integer positions (tileSize = 1)
            int tx = Mathf.RoundToInt(local.x);
            int ty = Mathf.RoundToInt(local.z);

            return gen.MapData.IsValid(tx, ty) ? new Vector2Int(tx, ty) : new Vector2Int(-1, -1);
        }

        private void DrawHoverHighlight(CMapGenerator gen)
        {
            if (_hoveredTile.x < 0) return;

            // Show tile info in the scene label
            STile tile = gen.MapData.Get(_hoveredTile.x, _hoveredTile.y);
            Vector3 center = gen.transform.position + new Vector3(_hoveredTile.x, 0.1f, _hoveredTile.y);

            // Wireframe highlight
            Handles.color = _paintMode ? Color.yellow : new Color(1f, 1f, 1f, 0.5f);
            Handles.DrawWireCube(center, new Vector3(1.05f, 0.05f, 1.05f));

            // Tooltip label
            string label = $"({_hoveredTile.x}, {_hoveredTile.y})  {tile.Type}";
            if (tile.BiomeType != EBiomeType.None)    label += $"  {tile.BiomeType}";
            if (tile.ObstacleType != EObstacleType.None) label += $"  [{tile.ObstacleType}]";
            Handles.Label(center + Vector3.up * 1.5f, label);
        }

        private void ApplyPaint(CMapGenerator gen, Vector2Int pos, bool erase)
        {
            STile tile = gen.MapData.Get(pos.x, pos.y);

            switch (_paintTarget)
            {
                case EPaintTarget.Obstacle:
                    tile.ObstacleType = erase ? EObstacleType.None : _selectedObstacle;
                    break;
                case EPaintTarget.Biome:
                    tile.BiomeType = erase ? EBiomeType.None : _selectedBiome;
                    break;
                case EPaintTarget.TileType:
                    tile.Type = erase ? ETileType.Water : _selectedTileType;
                    break;
            }

            gen.MapData.Set(pos.x, pos.y, tile);
        }
    }
#endif
}
