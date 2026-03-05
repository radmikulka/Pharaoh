// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.07.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimDecadePassTierRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public int Index { get; set; }
		[JsonProperty] public bool IsPremium { get; set; }
		[JsonProperty] public bool IsDouble { get; set; }
		
		public CClaimDecadePassTierRequest() : base(EHit.ClaimDecadePassTierRequest)
		{
		}
		
		public CClaimDecadePassTierRequest(int index, bool isPremium, bool isDouble) : base(EHit.ClaimDecadePassTierRequest)
		{
			IsPremium = isPremium;
			IsDouble = isDouble;
			Index = index;
		}
	}

	public class CClaimDecadePassTierResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
	
		public CClaimDecadePassTierResponse() : base(EHit.ClaimDecadePassTierResponse)
		{
		}
	
		public CClaimDecadePassTierResponse(CModifiedUserDataDto modifiedData) : base(EHit.ClaimDecadePassTierResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}