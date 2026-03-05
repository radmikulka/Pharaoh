// =========================================
// AUTHOR: Juraj Joscak
// DATE:   10.09.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using NaughtyAttributes;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/Factory")]
	public class CFactoryResourceConfig : ScriptableObject, IResourceConfigBase<EFactory>, IIHaveBundleLinks
	{
		[SerializeField] private EFactory _id;
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _smallVisual;
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _bigVisual;
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _upgradeVisual;
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _getMoreVisual;

		public EFactory Id => _id;
		public CBundleLink SmallVisual => _smallVisual;
		public CBundleLink BigVisual => _bigVisual;
		public CBundleLink UpgradeVisual => _upgradeVisual;
		public CBundleLink GetMoreVisual => _getMoreVisual;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			RefreshBundleLinks();
			
			yield return _smallVisual;
			yield return _bigVisual;
			yield return _upgradeVisual;
			yield return _getMoreVisual;
		}

		private int GetBundleId()
		{
			#if UNITY_EDITOR
			CFactoryConfig factoryConfig = CStaticConfigs.Factories.GetFactoryConfig(Id);
			if (factoryConfig.LiveEvent != ELiveEvent.None)
			{
				CLiveEventBundle eventConfig = CStaticConfigs.LiveEventBundles.GetConfig(factoryConfig.LiveEvent);
				return (int)eventConfig.UiBundle;
			}
			EYearMilestone unlockYear = factoryConfig.GetUnlockYear();
			ERegion region = CStaticConfigs.Regions.GetRegionFromYear(unlockYear);
			CRegionConfig regionConfig = CStaticConfigs.Regions.GetRegionConfig(region);
			return (int)regionConfig.ContentBundleId;
			#else
			return 0;
			#endif
		}

		[Button]
		private void RefreshBundleLinks()
		{
			int bundleId = GetBundleId();
			_smallVisual.SetBundleId(bundleId);
			_bigVisual.SetBundleId(bundleId);
			_upgradeVisual.SetBundleId(bundleId);
			_getMoreVisual.SetBundleId(bundleId);
		}
	}
}