// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CStartVehicleDepoUpgradeRequest : CCommTokenBasedRequest
	{
		public CStartVehicleDepoUpgradeRequest() : base(EHit.StartVehicleDepoUpgradeRequest)
		{
		}
	}
	
	public class CStartVehicleDepoUpgradeResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CStartVehicleDepoUpgradeResponse() : base(EHit.StartVehicleDepoUpgradeResponse)
		{
		}
		
		public CStartVehicleDepoUpgradeResponse(CModifiedUserDataDto modifiedData) : base(EHit.StartVehicleDepoUpgradeResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}