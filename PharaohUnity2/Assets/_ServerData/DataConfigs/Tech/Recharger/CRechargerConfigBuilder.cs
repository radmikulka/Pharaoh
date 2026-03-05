// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CRechargerConfigBuilder
	{
		private readonly List<CRechargerLevel> _levels = new(10);
		
		public CRechargerConfigBuilder AddLevel(int maxCapacity, int productionPerTick, int tickTimeInSeconds)
		{
			_levels.Add(new CRechargerLevel(maxCapacity, productionPerTick, tickTimeInSeconds));
			return this;
		}
		
		public CRechargerConfig Build()
		{
			return new CRechargerConfig(_levels.ToArray());
		}
	}
}