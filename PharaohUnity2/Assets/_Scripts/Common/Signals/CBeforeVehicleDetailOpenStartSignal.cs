// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.11.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CBeforeVehicleDetailOpenStartSignal : IEventBusSignal
	{
		public readonly EVehicle VehicleToShow;

		public CBeforeVehicleDetailOpenStartSignal(EVehicle vehicleToShow)
		{
			VehicleToShow = vehicleToShow;
		}
	}
}