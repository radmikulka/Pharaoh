// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.10.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CModifiedVehicleRechargerDto : CRechargerDto
	{
		[JsonProperty] public EVehicle VehicleId { get; set; }

		public CModifiedVehicleRechargerDto(
			EVehicle vehicleId,
			long lastTickTime,
			int currentAmount
		) : base(lastTickTime, currentAmount)
		{
			VehicleId = vehicleId;
		}

		public override string ToString()
		{
			return $"{base.ToString()}, {nameof(VehicleId)}: {VehicleId}";
		}
	}
}