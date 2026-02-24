using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// MonoBehaviour placed on each editor tile GameObject by CMapGenerator.RefreshEditorTiles().
    /// Lets designers click a tile in the Scene view, edit its type/decoration in the Inspector,
    /// and mark it as overridden so Generate() won't overwrite it.
    /// </summary>
    public class CMapTileData : MonoBehaviour
    {
        [HideInInspector] [SerializeField] private int _x;
        [HideInInspector] [SerializeField] private int _y;

        [SerializeField] private ETileType       _tileType;
        [SerializeField] private EContentTag     _contentTag;
        [SerializeField] private EDecorationType _decorationType;
        [SerializeField] private bool            _isObstacleBlocked;
        [SerializeField] private GameObject      _obstaclePrefab;

        /// <summary>When true, Generate() will not overwrite this tile with pipeline output.</summary>
        [SerializeField] private bool _isOverridden;

        public bool IsOverridden => _isOverridden;

        // ─── Public API ──────────────────────────────────────────────────────

        public void Initialize(STile tile)
        {
            _x                 = tile.X;
            _y                 = tile.Y;
            _tileType          = tile.Type;
            _contentTag        = tile.ContentTag;
            _decorationType    = tile.DecorationType;
            _isObstacleBlocked = tile.IsObstacleBlocked;
            _obstaclePrefab    = tile.ObstaclePrefab;
            _isOverridden      = false;
            RefreshColor();
        }

        public STile ToSTile()
        {
            return new STile
            {
                X                 = _x,
                Y                 = _y,
                Type              = _tileType,
                DecorationType    = _decorationType,
                IsObstacleBlocked = _isObstacleBlocked,
                ObstaclePrefab    = _obstaclePrefab,
            };
        }

        // ─── Unity callbacks ─────────────────────────────────────────────────

        private void OnValidate()
        {
            RefreshColor();
        }

        // ─── Internals ───────────────────────────────────────────────────────

        private void RefreshColor()
        {
            var meshRenderer = GetComponent<Renderer>();
            if (meshRenderer == null) return;

            Color color = _tileType == ETileType.Water
                ? new Color(0.20f, 0.45f, 0.85f)
                : _contentTag == EContentTag.Coast
                    ? new Color(0.92f, 0.84f, 0.50f)
                    : new Color(0.30f, 0.65f, 0.25f);

            var mpb = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", color);
            meshRenderer.SetPropertyBlock(mpb);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CMapTileData))]
    public class CMapTileDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var so = serializedObject;
            so.Update();

            // Coordinates header
            int x = so.FindProperty("_x").intValue;
            int y = so.FindProperty("_y").intValue;
            EditorGUILayout.LabelField($"Tile ({x}, {y})", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            // Draw all serialized fields except _x, _y, and m_Script
            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (prop.name == "m_Script" || prop.name == "_x" || prop.name == "_y")
                    continue;

                EditorGUILayout.PropertyField(prop, true);
            }

            // ApplyModifiedProperties triggers OnValidate automatically
            so.ApplyModifiedProperties();
        }
    }
#endif
}
