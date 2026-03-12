using AldaEngine;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
    [CreateAssetMenu(menuName = "____Pharaoh/Configs/Monument")]
    public class CMonumentResourceConfig : ScriptableObject, IResourceConfigBase<EMonumentId>
    {
        [SerializeField] [SearchableEnum] private EMonumentId _id;
        [SerializeField, BundleLink(true, typeof(GameObject))] private CBundleLink _prefab;

        public EMonumentId Id => _id;
        public CBundleLink Prefab => _prefab;
    }
}
