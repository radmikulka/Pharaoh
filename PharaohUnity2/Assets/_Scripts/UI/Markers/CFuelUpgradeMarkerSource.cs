// =========================================
// AUTHOR: Juraj Joscak
// DATE:   18.11.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CFuelUpgradeMarkerSource : ValidatedMonoBehaviour, IUpgradeMarkerValueSource, IAldaFrameworkComponent
	{
		private IServerTime _serverTime;
		private CUser _user;
		
		[Inject]
		private void Inject(IServerTime serverTime, CUser user)
		{
			_serverTime = serverTime;
			_user = user;
		}
		
		public bool ShowUpgradeMarker()
		{
			return _user.FuelStation.CanAffordUpgrade();
		}

		public EUpgradeMarkerState GetUpgradeMarkerState()
		{
			if (_user.FuelStation.IsCompleted(_serverTime.GetTimestampInMs()))
				return EUpgradeMarkerState.Completed;
			
			if(_user.FuelStation.IsUpgradeRunning())
				return EUpgradeMarkerState.Running;
		
			if (_user.FuelStation.IsFullyUpgraded())
				return EUpgradeMarkerState.Locked;
			
			if(_user.FuelStation.CanAffordUpgrade())
				return EUpgradeMarkerState.Available;

			IReadOnlyList<IUpgradeRequirement> requirements = _user.FuelStation.GetUpgradeRequirements();
			CYearMilestoneRequirement yearRequirement = requirements.FirstOrDefault(r => r is CYearMilestoneRequirement) as CYearMilestoneRequirement;
			if (yearRequirement == null || CLevelData.CanAffordRequirements(new[] { yearRequirement }, _user))
				return EUpgradeMarkerState.Unlocked;

			return EUpgradeMarkerState.Locked;
		}
	}
}