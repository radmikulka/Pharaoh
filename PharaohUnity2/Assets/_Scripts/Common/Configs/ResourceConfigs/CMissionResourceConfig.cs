using System.IO;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
    [CreateAssetMenu(menuName = "____Pharaoh/Configs/Mission")]
    public class CMissionResourceConfig : ScriptableObject, IResourceConfigBase<EMissionId>
    {
        [SerializeField] [SearchableEnum] private EMissionId _id;
        [SerializeField] [SearchableEnum] private ESceneId _sceneId;

        public EMissionId Id => _id;
        public ESceneId SceneId => _sceneId;
    }
}