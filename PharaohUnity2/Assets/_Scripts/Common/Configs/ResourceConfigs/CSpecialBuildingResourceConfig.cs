// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.09.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/SpecialBuildings")]
	public class CSpecialBuildingResourceConfig : ScriptableObject, IResourceConfigBase<ESpecialBuilding>, IIHaveBundleLinks
	{
		[SerializeField] private ESpecialBuilding _id;
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _sprite;
		[SerializeField, BundleLink(false, typeof(GameObject))] private CBundleLink _instancePrefab;
		[SerializeField] private Vector3 _shopCameraLocalPosition;

		public ESpecialBuilding Id => _id;
		public CBundleLink Sprite => _sprite;
		public CBundleLink InstancePrefab => _instancePrefab;
		public Vector3 ShopCameraLocalPosition => _shopCameraLocalPosition;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			#if UNITY_EDITOR
			int bundleId = GetBundleId();
			int uiBundleId = GetUiBundleId();
			
			_sprite.SetBundleId(uiBundleId);
			_instancePrefab.SetBundleId(bundleId);
			#endif
            
			yield return _sprite;
			yield return _instancePrefab;
		}

		#if UNITY_EDITOR
		private int GetBundleId()
		{
			CSpecialBuildingConfig buildingConfig = CStaticConfigs.Buildings.GetConfig(_id);
			if (buildingConfig.BundleId != EBundleId.None)
				return (int)buildingConfig.BundleId;
			if (buildingConfig.LiveEvent != ELiveEvent.None)
			{
				EBundleId eventContentBundle = CStaticConfigs.LiveEventBundles.GetConfig(buildingConfig.LiveEvent).ContentBundle;
				return (int)eventContentBundle;
			}

			EYearMilestone unlockYear = buildingConfig.GetUnlockYear();
			ERegion region = CStaticConfigs.Regions.GetRegionFromYear(unlockYear);
			CRegionConfig regionConfig = CStaticConfigs.Regions.GetRegionConfig(region);
			return (int)regionConfig.ContentBundleId;
		}
		
		private int GetUiBundleId()
		{
			CSpecialBuildingConfig buildingConfig = CStaticConfigs.Buildings.GetConfig(_id);
			if (buildingConfig.LiveEvent != ELiveEvent.None)
			{
				EBundleId eventContentBundle = CStaticConfigs.LiveEventBundles.GetConfig(buildingConfig.LiveEvent).UiBundle;
				return (int)eventContentBundle;
			}
			
			return GetBundleId();
		}
		#endif
	}
}