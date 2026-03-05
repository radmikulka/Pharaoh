// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CSpeedupVehicleDepoUpgradeRequest : CCommTokenBasedRequest
	{
		public CSpeedupVehicleDepoUpgradeRequest() : base(EHit.SpeedupVehicleDepoUpgradeRequest)
		{
		}
	}
	
	public class CSpeedupVehicleDepoUpgradeResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CSpeedupVehicleDepoUpgradeResponse() : base(EHit.SpeedupVehicleDepoUpgradeResponse)
		{
		}
		
		public CSpeedupVehicleDepoUpgradeResponse(CModifiedUserDataDto modifiedData) : base(EHit.SpeedupVehicleDepoUpgradeResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}