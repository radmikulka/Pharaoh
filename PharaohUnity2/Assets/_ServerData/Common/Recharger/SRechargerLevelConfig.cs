// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using AldaEngine;

namespace ServerData
{
	public struct SRechargerLevelConfig : IRechargerConfig
	{
		public int MaxCapacity { get; }
		public int ProductionPerTick { get; }
		public long ProductionTickDurationInSeconds { get; }
	
		public long ProductionTickDurationInMs => ProductionTickDurationInSeconds * CTimeConst.Second.InMilliseconds;
	
		public SRechargerLevelConfig(
			int maxCapacity, 
			int productionPerTick, 
			long productionTickDurationInSeconds
		)
		{
			MaxCapacity = maxCapacity;
			ProductionPerTick = productionPerTick;
			ProductionTickDurationInSeconds = productionTickDurationInSeconds;
		}
	}
}