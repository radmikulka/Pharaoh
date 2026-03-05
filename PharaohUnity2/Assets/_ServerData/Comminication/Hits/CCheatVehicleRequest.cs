// =========================================
// AUTHOR: Radek Mikulka
// DATE:   25.11.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CCheatVehicleRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EVehicle VehicleId { get; set; }
				
		public CCheatVehicleRequest() : base(EHit.CheatVehicleRequest)
		{
		}
				
		public CCheatVehicleRequest(EVehicle vehicleId) : base(EHit.CheatVehicleRequest)
		{
			VehicleId = vehicleId;
		}
	}
	
	public class CCheatVehicleResponse : CResponseHit
	{
		public CCheatVehicleResponse() : base(EHit.CheatVehicleResponse)
		{
		}
	}
}