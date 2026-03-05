// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimFactoryUpgradeRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EFactory Factory { get; set; }
		
		public CClaimFactoryUpgradeRequest() : base(EHit.ClaimFactoryUpgradeRequest)
		{
		}
	
		public CClaimFactoryUpgradeRequest(EFactory factory) : base(EHit.ClaimFactoryUpgradeRequest)
		{
			Factory = factory;
		}
	}

	public class CClaimFactoryUpgradeResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CClaimFactoryUpgradeResponse() : base(EHit.ClaimFactoryUpgradeResponse)
		{
		}
		
		public CClaimFactoryUpgradeResponse(CModifiedUserDataDto modifiedData) : base(EHit.ClaimFactoryUpgradeResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}