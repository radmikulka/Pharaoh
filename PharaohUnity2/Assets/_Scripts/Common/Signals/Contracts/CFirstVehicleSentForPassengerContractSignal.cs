// =========================================
// AUTHOR: Marek Karaba
// DATE:   10.11.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CFirstVehicleSentForPassengerContractSignal : IEventBusSignal
	{
		public ECity CityId { get; }

		public CFirstVehicleSentForPassengerContractSignal(ECity cityId)
		{
			CityId = cityId;
		}
	}
}