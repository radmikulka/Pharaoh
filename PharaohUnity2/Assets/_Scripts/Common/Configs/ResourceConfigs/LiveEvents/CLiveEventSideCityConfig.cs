// =========================================
// AUTHOR: Marek Karaba
// DATE:   12.01.2026
// =========================================

using System.Collections.Generic;
using AldaEngine;
using KBCore.Refs;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CLiveEventSideCityConfig : ValidatedMonoBehaviour, IIHaveBundleLinks
	{
		[SerializeField, Self] private CLiveEventResourceConfig _config;
		[SerializeField] private ECity _cityId;
		
		[Header("NameTag Sprite")]
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _nametagSprite;
		[Header("Contract Sprite")]
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _contractImage;
		
		public CBundleLink NametagSprite => _nametagSprite;
		public CBundleLink ContractImage => _contractImage;
		public ECity CityId => _cityId;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			#if UNITY_EDITOR
			CLiveEventBundle bundleConfig = CStaticConfigs.LiveEventBundles.GetConfig(_config.Id);
			EBundleId uiBundle = bundleConfig.UiBundle;
			
			_nametagSprite.SetBundleId((int) uiBundle);
			_contractImage.SetBundleId((int) uiBundle);
			#endif
			
			yield return _nametagSprite;
			yield return _contractImage;
		}
	}
}