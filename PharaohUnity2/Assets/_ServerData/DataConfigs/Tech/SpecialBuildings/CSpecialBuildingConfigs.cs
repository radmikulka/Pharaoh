// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.09.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CSpecialBuildingConfigs
	{
		private readonly Dictionary<ESpecialBuilding, CSpecialBuildingConfig> _configs = new();

		public CSpecialBuildingConfig GetConfig(ESpecialBuilding building)
		{
			if (_configs.TryGetValue(building, out var config))
				return config;
			throw new KeyNotFoundException($"No config found for special building {building}");
		}
		
		public void AddConfig(
			ESpecialBuilding id, 
			int passengersBonus, 
			EYearMilestone unlockMilestone, 
			IValuable price,
			bool isPremium = false
		)
		{
			_configs[id] = new CSpecialBuildingConfig(
				id, 
				passengersBonus, 
				IUnlockRequirement.Year(unlockMilestone), 
				price, 
				ELiveEvent.None,
				EBundleId.None,
				isPremium
			);
		}

		public void AddPremiumConfig(
			ESpecialBuilding id, 
			int passengersBonus, 
			EBundleId bundleId
			)
		{
			_configs[id] = new CSpecialBuildingConfig(
				id, 
				passengersBonus, 
				IUnlockRequirement.Null(),
				null, 
				ELiveEvent.None,
				bundleId,
				true
				);
		}
		
		public void AddEventConfig(
			ESpecialBuilding id, 
			int passengersBonus, 
			ELiveEvent liveEvent
		)
		{
			_configs[id] = new CSpecialBuildingConfig(
				id, 
				passengersBonus, 
				IUnlockRequirement.Null(), 
				null, 
				liveEvent,
				EBundleId.None,
				false
			);
		}

		public CSpecialBuildingConfig[] GetAllConfigs()
		{
			CSpecialBuildingConfig[] array = new CSpecialBuildingConfig[_configs.Count];
			_configs.Values.CopyTo(array, 0);
			return array;
		}
	}
}