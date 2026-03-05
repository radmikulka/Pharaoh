// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System;
using ServerData;
using TycoonBuilder.GoToStates;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CFsmStateFactory
	{
		private readonly DiContainer _container;
		
		public CFsmStateFactory(DiContainer container)
		{
			_container = container;
		}

		public CCloseAllMenusState CloseAllMenus => Create<CCloseAllMenusState>();
		public CMoveCameraToContractState MoveCameraToContract => Create<CMoveCameraToContractState>();
		public CMoveCameraToResourceMineState MoveCameraToResourceMine => Create<CMoveCameraToResourceMineState>();
		public CMoveToRegionState MoveToRegion => Create<CMoveToRegionState>();
		public CMoveCameraToSideCityState MoveCameraToCity => Create<CMoveCameraToSideCityState>();
		public CFocusOnContractDetailState FocusOnContractDetail => Create<CFocusOnContractDetailState>();
		public CFocusOnIndustryDetailState FocusOnIndustryDetail => Create<CFocusOnIndustryDetailState>();
		public CFocusOnCityDetailState FocusOnCityDetail => Create<CFocusOnCityDetailState>();
		public COpenContractDispatchMenuState OpenContractDispatchMenu => Create<COpenContractDispatchMenuState>();
		public CClaimContractState ClaimContractState => Create<CClaimContractState>();
		public COpenResourceDispatchMenuState OpenResourceDispatchMenu => Create<COpenResourceDispatchMenuState>();
		public COpenCityDispatchMenuState OpenCityDispatchMenu => Create<COpenCityDispatchMenuState>();
		public CMoveCameraToRegionPointState MoveCameraToRegionPoint => Create<CMoveCameraToRegionPointState>();
		public CMoveCameraToRegionPointInstantState MoveCameraToRegionPointInstant => Create<CMoveCameraToRegionPointInstantState>();
		public COpenFactoryMenuState OpenFactoryMenu => Create<COpenFactoryMenuState>();
		public COpenFactoriesMenuState OpenFactoriesMenu => Create<COpenFactoriesMenuState>();
		public COpenShopMenuState OpenShopMenu => Create<COpenShopMenuState>();
		public COpenLiveEventMenuState OpenLiveEventMenu => Create<COpenLiveEventMenuState>();
		public COpenCityMenuState OpenCityMenu => Create<COpenCityMenuState>();
		public COpenContractsMenuState OpenContractsMenu => Create<COpenContractsMenuState>();
		public CPulseRectState PulseRect => Create<CPulseRectState>();
		public COpenGetMoreResourcesMenuState OpenGetMoreResourcesMenu => Create<COpenGetMoreResourcesMenuState>();
		public CUpgradeCityState UpgradeCity => Create<CUpgradeCityState>();
		public CShowNameTagsState ShowNameTagsState => Create<CShowNameTagsState>();
		public CHideNameTagsState HideNameTagsState => Create<CHideNameTagsState>();
		public COpenMenuState OpenMenuState => Create<COpenMenuState>();
		public CKillMenuState KillMenuState => Create<CKillMenuState>();
		public COpenCityUpgradeMenuState OpenCityUpgradeMenuState => Create<COpenCityUpgradeMenuState>();
		public CLoadLiveEventState LoadLiveEvent => Create<CLoadLiveEventState>();
		public CLoadCoreGameState LoadCoreGame => Create<CLoadCoreGameState>();
		public CFocusOnRegionalOfficeState FocusOnRegionalOffice => Create<CFocusOnRegionalOfficeState>();
		public COpenVehicleDetailState OpenVehicleUpgrade => Create<COpenVehicleDetailState>();
		public CScrollToPassRewardState ScrollToPassReward => Create<CScrollToPassRewardState>();
		public COpenSpecialBuildingMenuState OpenSpecialBuildingMenu => Create<COpenSpecialBuildingMenuState>();
		public COpenBuildingParcelMenuState OpenBuildingParcelMenu => Create<COpenBuildingParcelMenuState>();

		private T Create<T>() where T : CGoToFsmState
		{
			try
			{
				return _container.Resolve<T>();
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				throw;
			}
		}
	}
}