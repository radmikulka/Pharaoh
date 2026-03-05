// =========================================
// AUTHOR: Marek Karaba
// DATE:   24.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CNotEnoughResourcesSignal : IEventBusSignal
	{
		public EResource ResourceId { get; }

		public CNotEnoughResourcesSignal(EResource resourceId)
		{
			ResourceId = resourceId;
		}
	}
}