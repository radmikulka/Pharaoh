// =========================================
// AUTHOR: Marek Karaba
// DATE:   23.02.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CVehicleShownSignal : IEventBusSignal
	{
		public readonly EVehicleShownType VehicleShownType;
		public readonly EVehicle VehicleId;

		public CVehicleShownSignal(EVehicleShownType vehicleShownType, EVehicle vehicleId)
		{
			VehicleShownType = vehicleShownType;
			VehicleId = vehicleId;
		}
	}
}