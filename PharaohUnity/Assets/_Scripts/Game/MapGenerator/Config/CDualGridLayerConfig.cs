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

        [Tooltip("Maps each EDualGridMask combination to the prefab variant to spawn at that corner.\n" +
                 "Leave entries out (or set Prefabs empty) to skip spawning for that mask.")]
        public CSerializableDictionary<EDualGridMask, CDualGridTileVariant> Tiles;
    }
}
