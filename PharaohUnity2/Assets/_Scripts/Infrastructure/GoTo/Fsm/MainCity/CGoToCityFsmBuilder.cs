// =========================================
// AUTHOR: Marek Karaba
// DATE:   09.01.2026
// =========================================

using AldaEngine;
using ServerData;
using TycoonBuilder.GoToStates;

namespace TycoonBuilder
{
	public class CGoToCityFsmBuilder
	{
		private readonly CFsmStateFactory _fsmFactory;
		private readonly CGoToFsm _fsm;

		public CGoToCityFsmBuilder(CFsmStateFactory fsmFactory, ICtsProvider ctsProvider)
		{
			_fsm = new CGoToFsm(ctsProvider.Token);
			_fsmFactory = fsmFactory;
		}

		public CGoToCityFsmBuilder SetContextEntry<T>(EGoToContextKey key, T value)
		{
			_fsm.SetContextEntry(key, value);
			return this;
		}
		
		public CGoToCityFsmBuilder HideNameTagsState(CLockObject lockObject)
		{
			CHideNameTagsState hideNameTagsState = _fsmFactory.HideNameTagsState;
			hideNameTagsState.SetData(lockObject);
			_fsm.AddState(hideNameTagsState);
			return this;
		}
		
		public CGoToCityFsmBuilder KillCityMenuState()
		{
			CKillMenuState killCityMenuState = _fsmFactory.KillMenuState;
			killCityMenuState.SetData(EScreenId.City);
			_fsm.AddState(killCityMenuState);
			return this;
		}

		public CGoToCityFsmBuilder ShowNameTagsState(CLockObject lockObject)
		{
			CShowNameTagsState showNameTagsState = _fsmFactory.ShowNameTagsState;
			showNameTagsState.SetData(lockObject);
			_fsm.AddState(showNameTagsState);
			return this;
		}

		public CGoToCityFsmBuilder CloseAllMenusState()
		{
			_fsm.AddState(_fsmFactory.CloseAllMenus);
			return this;
		}
		
		public CGoToCityFsmBuilder MoveToRegionState()
		{
			_fsm.AddState(_fsmFactory.MoveToRegion);
			return this;
		}
		
		public CGoToCityFsmBuilder MoveCameraToRegionPointState()
		{
			_fsm.AddState(_fsmFactory.MoveCameraToRegionPoint);
			return this;
		}
		
		public CGoToCityFsmBuilder UpgradeCityState()
		{
			_fsm.AddState(_fsmFactory.UpgradeCity);
			return this;
		}
		
		public CGoToCityFsmBuilder OpenCityUpgradeMenuState()
		{
			_fsm.AddState(_fsmFactory.OpenCityUpgradeMenuState);
			return this;
		}

		public CGoToCityFsmBuilder SetBlockInput(bool blockInput)
		{
			_fsm.SetBlockInput(blockInput);
			return this;
		}

		public CGoToFsm Build()
		{
			return _fsm;
		}
	}
}