// =========================================
// AUTHOR: Marek Karaba
// DATE:   25.08.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class COpenVehicleDetailTask
	{
		public readonly EVehicle VehicleId;
		public readonly EScreenId OpeningSource;
		public readonly int RequiredDurability;

		public COpenVehicleDetailTask(EVehicle vehicleId, EScreenId openingSource, int requiredDurability = 0)
		{
			VehicleId = vehicleId;
			OpeningSource = openingSource;
			RequiredDurability = requiredDurability;
		}
	}
}