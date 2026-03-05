// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimFuelStationUpgradeRequest : CCommTokenBasedRequest
	{
		public CClaimFuelStationUpgradeRequest() : base(EHit.ClaimFuelStationUpgradeRequest)
		{
		}
	}

	public class CClaimFuelStationUpgradeResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CClaimFuelStationUpgradeResponse() : base(EHit.ClaimFuelStationUpgradeResponse)
		{
		}
		
		public CClaimFuelStationUpgradeResponse(CModifiedUserDataDto modifiedData) : base(EHit.ClaimFuelStationUpgradeResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}