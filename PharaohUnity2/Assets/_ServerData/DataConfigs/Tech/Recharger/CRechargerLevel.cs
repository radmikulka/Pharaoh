// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

namespace ServerData
{
	public class CRechargerLevel
	{
		public readonly int MaxCapacity;
		public readonly int ProductionPerTick;
		public readonly int TickTimeInSeconds;

		public CRechargerLevel(int maxCapacity, int productionPerTick, int tickTimeInSeconds)
		{
			MaxCapacity = maxCapacity;
			ProductionPerTick = productionPerTick;
			TickTimeInSeconds = tickTimeInSeconds;
		}
	}
}