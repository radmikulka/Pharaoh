// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CSetActiveCityDetailCameraSignal : IEventBusSignal
	{
		public readonly ECity CityId;
		public readonly bool State;

		public CSetActiveCityDetailCameraSignal(ECity cityId, bool state)
		{
			CityId = cityId;
			State = state;
		}
	}
}