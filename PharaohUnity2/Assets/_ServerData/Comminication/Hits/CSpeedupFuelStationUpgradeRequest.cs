// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CSpeedupFuelStationUpgradeRequest : CCommTokenBasedRequest
	{
		public CSpeedupFuelStationUpgradeRequest() : base(EHit.SpeedupFuelStationUpgradeRequest)
		{
		}
	}
	
	public class CSpeedupFuelStationUpgradeResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CSpeedupFuelStationUpgradeResponse() : base(EHit.SpeedupFuelStationUpgradeResponse)
		{
		}
		
		public CSpeedupFuelStationUpgradeResponse(CModifiedUserDataDto modifiedData) : base(EHit.SpeedupFuelStationUpgradeResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}