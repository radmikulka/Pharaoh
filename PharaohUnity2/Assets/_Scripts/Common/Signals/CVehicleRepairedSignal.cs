// =========================================
// AUTHOR: Marek Karaba
// DATE:   24.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CVehicleRepairedSignal : IEventBusSignal
	{
		public EVehicle VehicleId { get; }

		public CVehicleRepairedSignal(EVehicle vehicleId)
		{
			VehicleId = vehicleId;
		}
	}
}