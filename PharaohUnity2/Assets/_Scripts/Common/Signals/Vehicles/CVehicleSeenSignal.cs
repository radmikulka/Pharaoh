// =========================================
// AUTHOR: Marek Karaba
// DATE:   17.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CVehicleSeenSignal : IEventBusSignal
	{
		public EVehicle VehicleId { get; }

		public CVehicleSeenSignal(EVehicle vehicleId)
		{
			VehicleId = vehicleId;
		}
	}
}