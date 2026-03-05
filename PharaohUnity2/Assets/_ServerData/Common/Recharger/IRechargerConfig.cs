// =========================================
// AUTHOR: Radek Mikulka
// DATE:   17.09.2025
// =========================================

using AldaEngine;

namespace ServerData
{
	public interface IRechargerConfig
	{
		int MaxCapacity { get; }
		int ProductionPerTick { get; }
		long ProductionTickDurationInSeconds { get; }
		long ProductionTickDurationInMs => ProductionTickDurationInSeconds * CTimeConst.Second.InMilliseconds;
	}
}