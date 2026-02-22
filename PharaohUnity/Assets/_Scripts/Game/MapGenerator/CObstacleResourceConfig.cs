using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    [CreateAssetMenu(menuName = "____Pharaoh/Configs/Resource/ObstacleResourceConfig")]
    public class CObstacleResourceConfig : ScriptableObject, IResourceConfigBase<EObstacleType>, IIHaveBundleLinks
    {
        [SerializeField] private EObstacleType _id;
        [SerializeField] [BundleLink(false, typeof(GameObject))] private CBundleLink _prefab;

        public EObstacleType Id => _id;
        public CBundleLink Prefab => _prefab;

        public IEnumerable<IBundleLink> GetBundleLinks()
        {
            _prefab.SetBundleId((int)EBundleId.BaseGame);
            yield return _prefab;
        }
    }
}
