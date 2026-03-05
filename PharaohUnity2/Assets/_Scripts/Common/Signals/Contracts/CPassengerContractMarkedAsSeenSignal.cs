// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CPassengerContractMarkedAsSeenSignal : IEventBusSignal
	{
		public readonly ECity CityId;

		public CPassengerContractMarkedAsSeenSignal(ECity cityId)
		{
			CityId = cityId;
		}
	}
}