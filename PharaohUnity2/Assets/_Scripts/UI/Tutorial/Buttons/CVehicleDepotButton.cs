// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.10.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TycoonBuilder.Ui;
using Zenject;

namespace TycoonBuilder
{
	public class CVehicleDepotButton : CTutorialButton, IInitializable, IUpgradeMarkerValueSource
	{
		private IServerTime _serverTime;
		private IEventBus _eventBus;
		private	CUser _user;

		[Inject]
		private void Inject(IServerTime serverTime, IEventBus eventBus, CUser user)
		{
			_serverTime = serverTime;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			LoadInitialState();
			_eventBus.AddTaskHandler<CActivateVehicleDepotButtonTask>(ActivateVehicleDepotButton);
		}

		private void ActivateVehicleDepotButton(CActivateVehicleDepotButtonTask task)
		{
			ActivateAnimated();
		}

		private void LoadInitialState()
		{
			SetActive(_user.Tutorials.IsTutorialCompleted(EVehicleDepotTutorialStep.Completed));
		}

		public EUpgradeMarkerState GetUpgradeMarkerState()
		{
			if (_user.VehicleDepo.IsFullyUpgraded())
				return EUpgradeMarkerState.Locked;
			
			if (_user.VehicleDepo.IsCompleted(_serverTime.GetTimestampInMs()))
				return EUpgradeMarkerState.Completed;
			
			if(_user.VehicleDepo.IsUpgradeRunning())
				return EUpgradeMarkerState.Running;
			
			if(_user.VehicleDepo.CanAffordUpgrade())
				return EUpgradeMarkerState.Available;

			IReadOnlyList<IUpgradeRequirement> requirements = _user.VehicleDepo.GetNextLevelUpgradeRequirements();
			CYearMilestoneRequirement yearRequirement = requirements.FirstOrDefault(r => r is CYearMilestoneRequirement) as CYearMilestoneRequirement;
			if (yearRequirement == null || CLevelData.CanAffordRequirements(new[] { yearRequirement }, _user))
				return EUpgradeMarkerState.Unlocked;

			return EUpgradeMarkerState.Locked;
		}
	}
}