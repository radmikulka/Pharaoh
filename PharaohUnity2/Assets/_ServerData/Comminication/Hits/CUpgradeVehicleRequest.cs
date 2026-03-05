// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.08.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CUpgradeVehicleRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EVehicle VehicleId { get; set; }
		[JsonProperty] public EVehicleStat Stat { get; set; }
		
		public CUpgradeVehicleRequest() : base(EHit.UpgradeVehicleRequest)
		{
		}
		
		public CUpgradeVehicleRequest(EVehicle vehicleId, EVehicleStat stat) : base(EHit.UpgradeVehicleRequest)
		{
			VehicleId = vehicleId;
			Stat = stat;
		}
	}

	public class CUpgradeVehicleResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CUpgradeVehicleResponse() : base(EHit.UpgradeVehicleResponse)
		{
		}
		
		public CUpgradeVehicleResponse(CModifiedUserDataDto modifiedData) : base(EHit.UpgradeVehicleResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}