// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.08.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CBuyVehicleRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EVehicle VehicleId { get; set; }
		
		public CBuyVehicleRequest() : base(EHit.BuyVehicleRequest)
		{
		}
		
		public CBuyVehicleRequest(EVehicle vehicleId) : base(EHit.BuyVehicleRequest)
		{
			VehicleId = vehicleId;
		}
	}
	
	public class CBuyVehicleResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CBuyVehicleResponse() : base(EHit.BuyVehicleResponse)
		{
		}
		
		public CBuyVehicleResponse(CModifiedUserDataDto modifiedData) : base(EHit.BuyVehicleResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}