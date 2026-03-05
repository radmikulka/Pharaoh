// =========================================
// AUTHOR:
// DATE:   19.09.2025
// =========================================

namespace ServerData.Logging
{
	public interface IVehicleDataLogging
	{
		void LogFullUpgradeCost(EVehicle vehicleId);
		void LogAllVehiclesUpgradeCost(ERegion regionId);
		void ExportAllToCsv();
	}
}

