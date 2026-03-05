// =========================================
// AUTHOR: Marek Karaba
// DATE:   17.10.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/SideCityConfig", fileName = "cfg_sideCity_name")]
	public class CSideCityConfig : CScriptableResourceConfig<ECity>, IIHaveBundleLinks
	{
		[Header("NameTag Sprite")]
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _nametagSprite;
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _contractImage;
		
		public CBundleLink NametagSprite => _nametagSprite;
		public CBundleLink ContractImage => _contractImage;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_nametagSprite.SetBundleId((int)EBundleId.BaseGame);
			_contractImage.SetBundleId((int)EBundleId.BaseGame);
			
			yield return _nametagSprite;
			yield return _contractImage;
		}
	}
}