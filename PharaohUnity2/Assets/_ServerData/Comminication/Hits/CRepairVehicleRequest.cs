// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CRepairVehicleRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EVehicle Vehicle { get; set; }
		[JsonProperty] public int RepairAmount { get; set; }
		[JsonProperty] public ERegion RegionId { get; set; }

		public CRepairVehicleRequest() : base(EHit.RepairVehicleRequest)
		{
		}
	
		public CRepairVehicleRequest(EVehicle vehicle, int repairAmount, ERegion regionId) : base(EHit.RepairVehicleRequest)
		{
			Vehicle = vehicle;
			RepairAmount = repairAmount;
			RegionId = regionId;
		}
	}

	public class CRepairVehicleResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CRepairVehicleResponse() : base(EHit.RepairVehicleResponse)
		{
		}
		
		public CRepairVehicleResponse(CModifiedUserDataDto modifiedData) : base(EHit.RepairVehicleResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}