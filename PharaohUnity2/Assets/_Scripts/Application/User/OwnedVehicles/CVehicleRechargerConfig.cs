// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CVehicleRechargerConfig : IRechargerConfig
	{
		private readonly CRepairAmountPerTickProvider _repairAmountPerTickProvider;
	
		public int MaxCapacity { get; }
		public int ProductionPerTick => _repairAmountPerTickProvider.GetValue();
		public long ProductionTickDurationInSeconds => CDesignVehicleConfigs.DurabilityRepairDurationInSecs;

		public CVehicleRechargerConfig(CRepairAmountPerTickProvider repairAmountPerTickProvider, int maxCapacity)
		{
			_repairAmountPerTickProvider = repairAmountPerTickProvider;
			MaxCapacity = maxCapacity;
		}
	}
}