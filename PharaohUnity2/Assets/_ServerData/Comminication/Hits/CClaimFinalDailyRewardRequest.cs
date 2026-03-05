// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.02.2026
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimFinalDailyRewardRequest : CCommTokenBasedRequest
	{
		public CClaimFinalDailyRewardRequest() : base(EHit.ClaimFinalDailyRewardRequest)
		{
		}
	}
	
	public class CClaimFinalDailyRewardResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CClaimFinalDailyRewardResponse() : base(EHit.ClaimFinalDailyRewardResponse)
		{
		}

		public CClaimFinalDailyRewardResponse(CModifiedUserDataDto modifiedData) : base(EHit.ClaimFinalDailyRewardResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}