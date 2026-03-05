// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;

namespace ServerData
{
	public class CFactoryProductConfig
	{
		private readonly SResource[] _requirements;

		public readonly int ProductionTimeSeconds;
		public readonly SResource Resource;
		
		public IReadOnlyList<SResource> Requirements => _requirements;
		public long ProductionTimeInMs => ProductionTimeSeconds * CTimeConst.Second.InMilliseconds;

		public CFactoryProductConfig(
			SResource[] requirements, 
			int productionTimeSeconds, 
			SResource resource
			)
		{
			_requirements = requirements;
			ProductionTimeSeconds = productionTimeSeconds;
			Resource = resource;
		}
	}
}