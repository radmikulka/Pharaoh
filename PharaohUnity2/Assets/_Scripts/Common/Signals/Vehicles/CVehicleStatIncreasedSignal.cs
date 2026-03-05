// =========================================
// AUTHOR: Marek Karaba
// DATE:   26.08.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CVehicleStatIncreasedSignal : IEventBusSignal
	{
		public EVehicle VehicleId { get; }
		public EVehicleStat Stat { get; }

		public CVehicleStatIncreasedSignal(EVehicle vehicleId, EVehicleStat stat)
		{
			VehicleId = vehicleId;
			Stat = stat;
		}
	}
}