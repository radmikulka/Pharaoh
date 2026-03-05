// =========================================
// AUTHOR: Marek Karaba
// DATE:   12.01.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CVehicleRepairAnimationFinishedSignal : IEventBusSignal
	{
		public CVehicleRepairAnimationFinishedSignal(EVehicle vehicleId)
		{
			VehicleId = vehicleId;
		}

		public EVehicle VehicleId { get; private set; }
	}
}