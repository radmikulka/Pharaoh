// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.10.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimEventPassTierRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public ELiveEvent EventId { get; set; }
		[JsonProperty] public int Index { get; set; }
		[JsonProperty] public bool IsPremium { get; set; }
		[JsonProperty] public bool IsDouble { get; set; }
		
		public CClaimEventPassTierRequest() : base(EHit.ClaimEventPassTierRequest)
		{
		}
		
		public CClaimEventPassTierRequest(ELiveEvent eventId, int index, bool isPremium, bool isDouble) : base(EHit.ClaimEventPassTierRequest)
		{
			IsPremium = isPremium;
			IsDouble = isDouble;
			EventId = eventId;
			Index = index;
		}
	}

	public class CClaimEventPassTierResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
	
		public CClaimEventPassTierResponse() : base(EHit.ClaimEventPassTierResponse)
		{
		}
	
		public CClaimEventPassTierResponse(CModifiedUserDataDto modifiedData) : base(EHit.ClaimEventPassTierResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}