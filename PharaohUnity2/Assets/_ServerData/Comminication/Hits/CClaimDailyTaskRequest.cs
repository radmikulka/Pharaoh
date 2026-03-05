// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.02.2026
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimDailyTaskRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string Uid { get; set; }
		
		public CClaimDailyTaskRequest() : base(EHit.ClaimDailyTaskRequest)
		{
		}
		
		public CClaimDailyTaskRequest(string uid) : base(EHit.ClaimDailyTaskRequest)
		{
			Uid = uid;
		}
	}
	
	public class CClaimDailyTaskResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CClaimDailyTaskResponse() : base(EHit.ClaimDailyTaskResponse)
		{
		}

		public CClaimDailyTaskResponse(CModifiedUserDataDto modifiedData) : base(EHit.ClaimDailyTaskResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}