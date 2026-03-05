// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CVehicleDurabilityRecharger
	{
		private readonly CRepairAmountPerTickProvider _repairAmountPerTickProvider;
		private readonly CDesignVehicleConfigs _vehicleConfigs;
		private readonly EVehicle _vehicleId;
		private CRecharger _recharger;
		public int CurrentAmount => _recharger.CurrentAmount;

		public static CVehicleDurabilityRecharger New(
			EVehicle vehicleId, 
			CDesignVehicleConfigs vehicleConfigs, 
			CRepairAmountPerTickProvider repairAmountPerTickProvider
		)
		{
			CVehicleDurabilityRecharger instance = new(vehicleId, vehicleConfigs, repairAmountPerTickProvider);
			instance.CreateNewRecharger();
			return instance;
		}
	
		public static CVehicleDurabilityRecharger Existing(
			EVehicle vehicleId, 
			CDesignVehicleConfigs vehicleConfigs, 
			CRepairAmountPerTickProvider repairAmountPerTickProvider, 
			int currentAmount, 
			long lastTickTime,
			int durabilityLevel
		)
		{
			CVehicleDurabilityRecharger instance =  new(vehicleId, vehicleConfigs, repairAmountPerTickProvider);
			instance.CreateExistingRecharger(currentAmount, lastTickTime, durabilityLevel);
			return instance;
		}

		private CVehicleDurabilityRecharger(
			EVehicle vehicleId, 
			CDesignVehicleConfigs vehicleConfigs, 
			CRepairAmountPerTickProvider repairAmountPerTickProvider
		)
		{
			_repairAmountPerTickProvider = repairAmountPerTickProvider;
			_vehicleConfigs = vehicleConfigs;
			_vehicleId = vehicleId;
		}
	
		public void RemoveDurability(int amount, long time)
		{
			_recharger.Remove(amount, time);
		}

		public bool CompareWithDto(CRechargerDto dto)
		{
			return _recharger.CompareWithDto(dto);
		}
		
		public void AddDurability(int amount, long time)
		{
			_recharger.Add(amount, time);
		}
	
		public void UpgradeDurability(long time, int statLevel)
		{
			IRechargerConfig levelConfig = GetDurabilityConfig(statLevel);
			_recharger.Upgrade(levelConfig, time);
		}

		public void UpdateDurability(long time)
		{
			_recharger.Update(time);
		}

		private void CreateNewRecharger()
		{
			CVehicleRechargerConfig rechargerConfig = GetDurabilityConfig(1);
			_recharger = CRecharger.New(rechargerConfig);
		}

		private void CreateExistingRecharger(int currentAmount, long lastTickTime, int durabilityLevel)
		{
			CVehicleRechargerConfig rechargerConfig = GetDurabilityConfig(durabilityLevel);
			_recharger = CRecharger.Existing(lastTickTime, currentAmount, rechargerConfig);
		}

		private CVehicleRechargerConfig GetDurabilityConfig(int statLevel)
		{
			CVehicleConfig vehicleConfig = _vehicleConfigs.GetConfig(_vehicleId);
			int statValue = vehicleConfig.GetStat(EVehicleStat.Durability, statLevel - 1);
			return new CVehicleRechargerConfig(_repairAmountPerTickProvider, statValue);
		}

		public long GetNextRechargeRemainingTime(long timestampInMs)
		{
			return _recharger.GetNextRechargeRemainingTime(timestampInMs);
		}

		public long GetFullRechargeRemainingTime(long timestampInMs)
		{
			return _recharger.GetFullRechargeRemainingTime(timestampInMs);
		}

		public override string ToString()
		{
			return $"{nameof(_recharger)}: {_recharger}";
		}
	}
}