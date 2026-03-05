using System;
using AldaEngine;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    [Serializable]
    public class CDualGridTileVariant
    {
        [Tooltip("Prefab variants — one is selected deterministically by world-position hash.")]
        public GameObject[] Prefabs;
    }

    [CreateAssetMenu(fileName = "cfg_dualGridLayer", menuName = "Pharaoh/MapGen/Dual Grid Layer")]
    public class CDualGridLayerConfig : ScriptableObject
    {
        public string LayerName;

        [Tooltip("Maps each tile shape to the prefab variant to spawn.\n" +
                 "Rotation is applied automatically by CDualGridShapeResolver.\n" +
                 "Leave an entry out (or set Prefabs empty) to skip spawning for that shape.")]
        public CSerializableDictionary<EDualGridShape, CDualGridTileVariant> Tiles;
    }
}
