// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.07.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CCollectDispatchedVehicleRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string DispatchUid { get; set; }
		
		public CCollectDispatchedVehicleRequest() : base(EHit.CollectDispatchedVehicleRequest)
		{
		}
		
		public CCollectDispatchedVehicleRequest(string dispatchUid) : base(EHit.CollectDispatchedVehicleRequest)
		{
			DispatchUid = dispatchUid;
		}
	}

	public class CCollectDispatchedVehicleResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CCollectDispatchedVehicleResponse() : base(EHit.CollectDispatchedVehicleResponse)
		{
		}
		
		public CCollectDispatchedVehicleResponse(CModifiedUserDataDto modifiedData) : base(EHit.CollectDispatchedVehicleResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}