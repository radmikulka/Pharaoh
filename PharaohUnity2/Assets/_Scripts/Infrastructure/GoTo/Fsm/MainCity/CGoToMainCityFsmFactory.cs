// =========================================
// AUTHOR: Marek Karaba
// DATE:   09.01.2026
// =========================================

using AldaEngine;
using ServerData;
using TycoonBuilder.GoToStates;

namespace TycoonBuilder
{
	public class CGoToMainCityFsmFactory
	{
		private readonly CFsmStateFactory _fsmFactory;
		private readonly ICtsProvider _ctsProvider;
		private readonly CUser _user;

		public CGoToMainCityFsmFactory(CFsmStateFactory fsmFactory, ICtsProvider ctsProvider, CUser user)
		{
			_ctsProvider = ctsProvider;
			_fsmFactory = fsmFactory;
			_user = user;
		}

		public CGoToFsm CreateGoToCityAndUpgradeIt()
		{
			CLockObject lockObject = new("CGoToHandler.GoToCityAndUpgrade");
			
			CGoToCityFsmBuilder builder = new (_fsmFactory, _ctsProvider);
			CloseMenusAndUpgradeCity(builder, lockObject);
			builder.ShowNameTagsState(lockObject);
			builder.SetBlockInput(true);
			
			CGoToFsm fsm = builder.Build();
			return fsm;
		}

		public CGoToFsm CreateGoToCityUpgradeItAndGoBackToMenu()
		{
			CLockObject lockObject = new("CGoToHandler.GoToCityAndUpgrade");
		
			CGoToCityFsmBuilder builder = new (_fsmFactory, _ctsProvider);
			CloseMenusAndUpgradeCity(builder, lockObject);
			builder.OpenCityUpgradeMenuState();
			builder.ShowNameTagsState(lockObject);
			builder.SetBlockInput(true);
			
			CGoToFsm fsm = builder.Build();
			return fsm;
		}

		public CGoToFsm CreateGoToMainCity(ERegionPoint regionPoint, ERegion region)
		{
			COpenMenuState openCityState = _fsmFactory.OpenMenuState;
			openCityState.SetData(EScreenId.City);
			
			CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.RegionPoint, regionPoint)
					.SetContextEntry(EGoToContextKey.Region, region)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.MoveToRegion)
					.AddState(_fsmFactory.MoveCameraToRegionPoint)
					.AddState(openCityState)
				;
			return fsm;
		}

		private void CloseMenusAndUpgradeCity(CGoToCityFsmBuilder builder, CLockObject lockObject)
		{
			builder.SetContextEntry(EGoToContextKey.RegionPoint, ERegionPoint.MainCity);
			builder.SetContextEntry(EGoToContextKey.Region, _user.Progress.Region);
			builder.HideNameTagsState(lockObject);
			builder.KillCityMenuState();
			builder.CloseAllMenusState();
			builder.MoveToRegionState();
			builder.MoveCameraToRegionPointState();
			builder.UpgradeCityState();
		}

		private CGoToFsm GetNewFsm()
		{
			return new CGoToFsm(_ctsProvider.Token);
		}
	}
}