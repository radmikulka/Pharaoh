// =========================================
// NAME: Marek Karaba
// DATE: 07.10.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public interface IVehicleNameProvider
	{
		string GetVehicleName(EVehicle vehicleId);
	}
}