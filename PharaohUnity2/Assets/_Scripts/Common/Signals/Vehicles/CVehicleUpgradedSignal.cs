// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.12.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CVehicleUpgradedSignal : IEventBusSignal
	{
		public readonly EVehicle VehicleId;

		public CVehicleUpgradedSignal(EVehicle vehicleId)
		{
			VehicleId = vehicleId;
		}
	}
}