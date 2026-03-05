// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimCityUpgradeRequest : CCommTokenBasedRequest
	{
		public CClaimCityUpgradeRequest() : base(EHit.ClaimCityUpgradeRequest)
		{
		}
	}
	
	public class CClaimCityUpgradeResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CClaimCityUpgradeResponse() : base(EHit.ClaimCityUpgradeResponse)
		{
		}
		
		public CClaimCityUpgradeResponse(CModifiedUserDataDto modifiedData) : base(EHit.ClaimCityUpgradeResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}