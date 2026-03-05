// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.02.2026
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimWeeklyTaskRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string Uid { get; set; }
		
		public CClaimWeeklyTaskRequest() : base(EHit.ClaimWeeklyTaskRequest)
		{
		}
		
		public CClaimWeeklyTaskRequest(string uid) : base(EHit.ClaimWeeklyTaskRequest)
		{
			Uid = uid;
		}
	}

public class CClaimWeeklyTaskResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CClaimWeeklyTaskResponse() : base(EHit.ClaimWeeklyTaskResponse)
		{
		}

		public CClaimWeeklyTaskResponse(CModifiedUserDataDto modifiedData) : base(EHit.ClaimWeeklyTaskResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}