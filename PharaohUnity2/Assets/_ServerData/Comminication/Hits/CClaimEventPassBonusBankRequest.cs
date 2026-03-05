// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.02.2026
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimEventPassBonusBankRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public ELiveEvent EventId { get; set; }
		
		public CClaimEventPassBonusBankRequest() : base(EHit.ClaimEventPassBonusBankRequest)
		{
		}
		
		public CClaimEventPassBonusBankRequest(ELiveEvent eventId) : base(EHit.ClaimEventPassBonusBankRequest)
		{
			EventId = eventId;
		}
	}

	public class CClaimEventPassBonusBankResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CClaimEventPassBonusBankResponse() : base(EHit.ClaimEventPassBonusBankResponse)
		{
		}
		
		public CClaimEventPassBonusBankResponse(CModifiedUserDataDto modifiedData) : base(EHit.ClaimEventPassBonusBankResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}