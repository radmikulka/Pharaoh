// =========================================
// AUTHOR: Juraj Joscak
// DATE:   10.11.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using NaughtyAttributes;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/Industry")]
	public class CIndustryResourceConfig : ScriptableObject, IResourceConfigBase<EIndustry>, IIHaveBundleLinks
	{
		[SerializeField] private EIndustry _id;
		[SerializeField] private bool _bundleIsSetManually;
		[SerializeField, BundleLink(true, typeof(Sprite))]  private CBundleLink _getMoreVisual;
		
		public EIndustry Id => _id;
		public CBundleLink GetMoreVisual => _getMoreVisual;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			AssignBundleIds();
			yield return _getMoreVisual;
		}
		
		[Button]
		private void AssignBundleIds()
		{
			int bundleId = GetBundleId();
			
			_getMoreVisual.SetBundleId(bundleId);
		}
		
		private int GetBundleId()
		{
			if (_bundleIsSetManually)
				return _getMoreVisual.BundleId;
			
			#if UNITY_EDITOR
			CResourceIndustryConfig industryConfig = CStaticConfigs.Industry.GetConfig(_id);
			if (industryConfig.LiveEvent != ELiveEvent.None)
			{
				return (int) CStaticConfigs.LiveEventBundles.GetConfig(industryConfig.LiveEvent).UiBundle;
			}
			CRegionConfig regionConfig = CStaticConfigs.Regions.GetRegionConfig(industryConfig.Region);
			return (int) regionConfig.ContentBundleId;
			#else
			return 0;
			#endif
		}
	}
}