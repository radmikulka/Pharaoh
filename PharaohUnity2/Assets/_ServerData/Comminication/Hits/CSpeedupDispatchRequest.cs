// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.08.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CSpeedupDispatchRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string DispatchUid { get; set; }
		
		public CSpeedupDispatchRequest() : base(EHit.SpeedupDispatchRequest)
		{
		}
		
		public CSpeedupDispatchRequest(string dispatchUid) : base(EHit.SpeedupDispatchRequest)
		{
			DispatchUid = dispatchUid;
		}
	}

	public class CSpeedupDispatchResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CSpeedupDispatchResponse() : base(EHit.SpeedupDispatchResponse)
		{
		}
		
		public CSpeedupDispatchResponse(CModifiedUserDataDto modifiedData) : base(EHit.SpeedupDispatchResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}