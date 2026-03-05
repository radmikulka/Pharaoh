// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimVehicleDepoUpgradeRequest : CCommTokenBasedRequest
	{
		public CClaimVehicleDepoUpgradeRequest() : base(EHit.ClaimVehicleDepoUpgradeRequest)
		{
		}
	}
	
	public class CClaimVehicleDepoUpgradeResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CClaimVehicleDepoUpgradeResponse() : base(EHit.ClaimVehicleDepoUpgradeResponse)
		{
		}
		
		public CClaimVehicleDepoUpgradeResponse(CModifiedUserDataDto modifiedData) : base(EHit.ClaimVehicleDepoUpgradeResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}