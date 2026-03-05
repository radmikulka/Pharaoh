// =========================================
// AUTHOR: Marek Karaba
// DATE:   26.11.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CDamagedVehicleShownSignal : IEventBusSignal
	{
		public readonly EVehicle VehicleId;

		public CDamagedVehicleShownSignal(EVehicle vehicleId)
		{
			VehicleId = vehicleId;
		}
	}
}