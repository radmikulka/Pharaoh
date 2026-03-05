// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.01.2026
// =========================================

using System.Collections.Generic;
using AldaEngine;
using KBCore.Refs;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CLiveEventBuildingsConfig : ValidatedMonoBehaviour, IIHaveBundleLinks
	{
		[SerializeField, Self] private CLiveEventResourceConfig _config;
		[Header("BuildingBg")] 
		[SerializeField, BundleLink(true, typeof(Sprite))] private CBundleLink _bgSprite;
		
		public CBundleLink BgSprite => _bgSprite;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			#if UNITY_EDITOR
			CLiveEventBundle bundleConfig = CStaticConfigs.LiveEventBundles.GetConfig(_config.Id);
			EBundleId uiBundle = bundleConfig.UiBundle;
			
			_bgSprite.SetBundleId((int) uiBundle);
			#endif
			
			yield return _bgSprite;
		}
	}
}