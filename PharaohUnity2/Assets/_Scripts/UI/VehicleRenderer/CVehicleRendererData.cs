// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.10.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CVehicleRendererData
	{
		public CVehicleResourceConfig VehicleConfig;
		public EVehicleRendererMode Mode;
		
		public EVehicle VehicleId => VehicleConfig.Id;

		public CVehicleRendererData(CVehicleResourceConfig vehicleConfig, EVehicleRendererMode mode)
		{
			VehicleConfig = vehicleConfig;
			Mode = mode;
		}
	}
}