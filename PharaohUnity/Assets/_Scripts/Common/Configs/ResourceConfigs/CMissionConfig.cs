using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
    [CreateAssetMenu(menuName = "____Pharaoh/Configs/Scene")]
    public class CMissionConfig : ScriptableObject, IResourceConfigBase<EMissionId>
    {
        [SerializeField] [SearchableEnum] private EMissionId _id;
        [SerializeField] [SearchableEnum] private ESceneId _sceneId;
        [SerializeField] private EBuildingId[] _availableBuildings;
        [SerializeField] private EResource[] _availableResources;

        public EMissionId Id => _id;
        public ESceneId Scene => _sceneId;
        public EBuildingId[] AvailableBuildings => _availableBuildings;
        public EResource[] AvailableResources => _availableResources;
    }
}