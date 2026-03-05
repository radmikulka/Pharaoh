// =========================================
// NAME: Marek Karaba
// DATE: 03.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CPassengerContractStateChangedSignal : IEventBusSignal
	{
		public readonly ECity CityId;

		public CPassengerContractStateChangedSignal(ECity cityId)
		{
			CityId = cityId;
		}
	}
}