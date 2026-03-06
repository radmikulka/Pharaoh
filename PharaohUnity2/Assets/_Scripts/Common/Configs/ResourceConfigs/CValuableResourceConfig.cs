// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
    [CreateAssetMenu(menuName = "____Pharaoh/Configs/Valuable")]
    public class CValuableResourceConfig : ScriptableObject, IResourceConfigBase<EValuable>, IIHaveBundleLinks
    {
        [SerializeField] private EValuable _id;
        [SerializeField] [BundleLink(false, typeof(Sprite))]  private CBundleLink _sprite;

        public EValuable Id => _id;
        public CBundleLink Sprite => _sprite;

        public IEnumerable<IBundleLink> GetBundleLinks()
        {
            _sprite.SetBundleId((int) EBundleId.BaseGame);
            yield return _sprite;
        }
    }
}