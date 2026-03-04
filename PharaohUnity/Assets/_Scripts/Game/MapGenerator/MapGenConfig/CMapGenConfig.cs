using AldaEngine;
using Pharaoh.MapGenerator;
using UnityEngine;

namespace Pharaoh
{
    [CreateAssetMenu(fileName = "cfg_mapGen", menuName = "____Pharaoh/Configs/MapGen")]
    public class CMapGenConfig : ScriptableObject
    {
        [SerializeField] private CDualGridLayerConfig _landLayer;
        [SerializeField] private CSerializableDictionary<EContentTag, CDualGridLayerConfig> _biomeLayers;
        [SerializeField] private CSerializableDictionary<EObstacleType, CDualGridLayerConfig> _obstacleLayers;
        [SerializeField] private CSeabedConfig _seabed;

        public CDualGridLayerConfig LandLayer => _landLayer;
        public CSerializableDictionary<EContentTag, CDualGridLayerConfig> BiomeLayers    => _biomeLayers;
        public CSerializableDictionary<EObstacleType, CDualGridLayerConfig> ObstacleLayers => _obstacleLayers;
        public CSeabedConfig Seabed => _seabed;
    }
}
