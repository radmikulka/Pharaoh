// =========================================
// AUTHOR: Marek Karaba
// DATE:   05.12.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CScreenInfoVehicleDetailsOpenButton : ValidatedMonoBehaviour, IInitializable, IScreenOpenStart, IScreenCloseEnd
	{
		[SerializeField, Self] private CUiButton _button;
		[SerializeField, Self] private CTweener _tweener;

		private CGlobalVariablesInfoScreenHandler _globalVariablesHandler;
		private IEventBus _eventBus;
		private CUser _user;

		[Inject]
		private void Inject(
			CGlobalVariablesInfoScreenHandler globalVariablesHandler,
			IEventBus eventBus,
			CUser user)
		{
			_globalVariablesHandler = globalVariablesHandler;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			_button.AddClickListener(OnClick);

			_eventBus.Subscribe<CScreenInfoSeenSignal>(OnScreenInfoSeen);
		}

		private void OnClick()
		{
			_eventBus.ProcessTask(new CShowInfoScreenTask(EScreenInfoId.UpgradeVehicles));
		}

		private void OnScreenInfoSeen(CScreenInfoSeenSignal signal)
		{
			if (signal.InfoScreen != EScreenInfoId.UpgradeVehicles)
				return;

			DisableTweener();
		}

		public void OnScreenOpenStart()
		{
			SetCorrectState();
		}

		private void SetCorrectState()
		{
			bool upgradesLocked = _user.Vehicles.VehicleUpgradesLocked();
			gameObject.SetActive(!upgradesLocked);

			if (upgradesLocked)
				return;

			bool seen = _globalVariablesHandler.GetGlobalVariable(EScreenInfoId.UpgradeVehicles);
			if (seen)
				return;

			_tweener.Enable();
		}

		public void OnScreenCloseEnd()
		{
			DisableTweener();
		}

		private void DisableTweener()
		{
			_tweener.Disable();
			transform.localScale = Vector3.one;
		}
	}
}