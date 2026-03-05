// =========================================
// AUTHOR: Radek Mikulka
// DATE:   27.02.2026
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimPromoCodeRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string Code { get; set; }

		public CClaimPromoCodeRequest() : base(EHit.ClaimPromoCodeRequest)
		{
		}

		public CClaimPromoCodeRequest(string code) : base(EHit.ClaimPromoCodeRequest)
		{
			Code = code;
		}
	}

	public class CClaimPromoCodeResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public EPromoCodeState State { get; set; }
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }

		public CClaimPromoCodeResponse() : base(EHit.ClaimPromoCodeResponse)
		{
		}

		public CClaimPromoCodeResponse(EPromoCodeState state, CModifiedUserDataDto modifiedData) : base(EHit.ClaimPromoCodeResponse)
		{
			State = state;
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}
