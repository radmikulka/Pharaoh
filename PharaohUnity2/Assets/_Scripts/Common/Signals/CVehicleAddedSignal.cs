// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.08.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CVehicleAddedSignal : IEventBusSignal
	{
		public readonly EVehicleObtainSource ObtainSource;
		public readonly EVehicle VehicleId;

		public CVehicleAddedSignal(EVehicle vehicleId, EVehicleObtainSource obtainSource)
		{
			ObtainSource = obtainSource;
			VehicleId = vehicleId;
		}
	}
}