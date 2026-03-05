// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CNewResourceMenuClosedSignal : IEventBusSignal
	{
		public readonly EResource ResourceId;

		public CNewResourceMenuClosedSignal(EResource resourceId)
		{
			ResourceId = resourceId;
		}
	}
}