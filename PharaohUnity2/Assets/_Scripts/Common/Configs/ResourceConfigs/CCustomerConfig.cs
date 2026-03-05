// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.08.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/CustomerConfig", fileName = "cfg_customer_name")]
	public class CCustomerConfig : CScriptableResourceConfig<ECustomer>, IIHaveBundleLinks
	{
		[Header("Contract Sprite")]
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _contractSprite;
		[Header("Big Sprite")]
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _bigSprite;
		[Header("NameTag Sprite")]
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _nametagSprite;
		[Header("Dispatch Center Sprite")]
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _dispatchCenterSprite;
		
		public CBundleLink ContractSprite => _contractSprite;
		public CBundleLink BigSprite => _bigSprite;
		public CBundleLink NametagSprite => _nametagSprite;
		public CBundleLink DispatchCenterSprite => _dispatchCenterSprite;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_contractSprite.SetBundleId((int)EBundleId.BaseGame);
			_bigSprite.SetBundleId((int)EBundleId.BaseGame);
			_nametagSprite.SetBundleId((int)EBundleId.BaseGame);
			_dispatchCenterSprite.SetBundleId((int)EBundleId.BaseGame);
			return new[] { _contractSprite, _bigSprite, _nametagSprite, _dispatchCenterSprite };
		}
	}
}