// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.01.2026
// =========================================

using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CLiveEventResourceConfig : MonoBehaviour, IResourceConfigBase<ELiveEvent>, IIHaveBundleLinks
	{
		[SerializeField] private ELiveEvent _id;
		[SerializeField] private EResource _newProductsResourceType;
		[SerializeField] private EResource _newMaterialsResourceType;
		
		[Header("Event Color")]
		[SerializeField] private Color _color;
		[Header("Main")]
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _mainIconMainIconSprite;
		[Header("Overview Bg")]
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _overviewBgSprite;
		[Header("Event Coins")]
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _eventCoinsIconSprite;
		[Header("Event Points")]
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _eventPointsIconSprite;
		[Header("Finished Hud Banner")]
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _finishedHudBannerIconSprite;
		[Header("Active Hud Banner")]
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _activeHudBannerIconSprite;
		[Header("Hud Bg")] 
		[SerializeField, BundleLink(false, typeof(Sprite))] private CBundleLink _hudBgIconSprite;
		[Header("Contract Banner")] 
		[SerializeField, BundleLink(false, typeof(Sprite))] private CBundleLink _contractBanner;
		[Header("Dispatch Center Bgr")] 
		[SerializeField, BundleLink(false, typeof(Sprite))] private CBundleLink _dispatchCenterBg;

		public ELiveEvent Id => _id;
		public Color Color => _color;
		public CBundleLink MainIconSprite => _mainIconMainIconSprite;
		public CBundleLink OverviewBgSprite => _overviewBgSprite;
		public CBundleLink EventCoinsIconSprite => _eventCoinsIconSprite;
		public CBundleLink EventPointsIconSprite => _eventPointsIconSprite;
		public CBundleLink FinishedHudBannerIconSprite => _finishedHudBannerIconSprite;
		public CBundleLink ActiveHudBannerIconSprite => _activeHudBannerIconSprite;
		public CBundleLink HudBgIconSprite => _hudBgIconSprite;
		public CBundleLink ContractBanner => _contractBanner;
		public CBundleLink DispatchCenterBg => _dispatchCenterBg;
		
		public EResource NewProductsResourceType => _newProductsResourceType;
		public EResource NewMaterialsResourceType => _newMaterialsResourceType;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			#if UNITY_EDITOR
			CLiveEventBundle bundleConfig = CStaticConfigs.LiveEventBundles.GetConfig(_id);
			EBundleId uiBundle = bundleConfig.UiBundle;
			
			_mainIconMainIconSprite.SetBundleId((int) uiBundle);
			_overviewBgSprite.SetBundleId((int) uiBundle);
			_eventCoinsIconSprite.SetBundleId((int) uiBundle);
			_eventPointsIconSprite.SetBundleId((int) uiBundle);
			_finishedHudBannerIconSprite.SetBundleId((int) uiBundle);
			_activeHudBannerIconSprite.SetBundleId((int) uiBundle);
			_hudBgIconSprite.SetBundleId((int) uiBundle);
			_contractBanner.SetBundleId((int) uiBundle);
			_dispatchCenterBg.SetBundleId((int) uiBundle);
			#endif
            
			yield return _mainIconMainIconSprite;
			yield return _overviewBgSprite;
			yield return _eventCoinsIconSprite;
			yield return _eventPointsIconSprite;
			yield return _finishedHudBannerIconSprite;
			yield return _activeHudBannerIconSprite;

			if (_hudBgIconSprite.IsObjectAssigned)
			{
				yield return _hudBgIconSprite;
			}
			
			yield return _contractBanner;
			yield return _dispatchCenterBg;
		}
		
		public CLiveEventBuildingsConfig GetBuildingsConfig()
		{
			TryGetComponent(out CLiveEventBuildingsConfig buildingsConfig);
			if (!buildingsConfig)
			{
				Debug.LogError("CLiveEventResourceConfig does not have CLiveEventBuildingsConfig component attached! for LiveEvent: " + _id);
			}
			return buildingsConfig;
		}

		public bool HasSideCity(ECity cityId)
		{
			TryGetComponent(out CLiveEventSideCityConfig sideCityConfig);
			if (sideCityConfig == null)
				return false;
			
			bool isEventCity = sideCityConfig.CityId == cityId;
			return isEventCity;
		}
		
		public CLiveEventSideCityConfig GetSideCityConfig()
		{
			TryGetComponent(out CLiveEventSideCityConfig sideCityConfig);
			if (!sideCityConfig)
			{
				Debug.LogError("CLiveEventResourceConfig does not have CLiveEventSideCityConfig component attached! for LiveEvent: " + _id);
			}
			return sideCityConfig;
		}
	}
}