// =========================================
// AUTHOR: Marek Karaba
// DATE:   10.11.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CDispatchForPassengersOpenedSignal : IEventBusSignal
	{
		public readonly ECity CityId;

		public CDispatchForPassengersOpenedSignal(ECity cityId)
		{
			CityId = cityId;
		}
	}
}