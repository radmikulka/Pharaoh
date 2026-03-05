// =========================================
// AUTHOR:
// DATE:   07.11.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CMarkVehicleAsSeenRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EVehicle VehicleId { get; set; }
				
		public CMarkVehicleAsSeenRequest() : base(EHit.MarkVehicleAsSeenRequest)
		{
		}
				
		public CMarkVehicleAsSeenRequest(EVehicle vehicleId) : base(EHit.MarkVehicleAsSeenRequest)
		{
			VehicleId = vehicleId;
		}
	}
	
	public class CMarkVehicleAsSeenResponse : CResponseHit
	{
		public CMarkVehicleAsSeenResponse() : base(EHit.MarkVehicleAsSeenResponse)
		{
		}
	}
}

