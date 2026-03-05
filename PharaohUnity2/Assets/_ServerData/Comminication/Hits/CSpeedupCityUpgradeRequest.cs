// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CSpeedupCityUpgradeRequest : CCommTokenBasedRequest
	{
		public CSpeedupCityUpgradeRequest() : base(EHit.SpeedupCityUpgradeRequest)
		{
		}
	}
	
	public class CSpeedupCityUpgradeResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CSpeedupCityUpgradeResponse() : base(EHit.SpeedupCityUpgradeResponse)
		{
		}
		
		public CSpeedupCityUpgradeResponse(CModifiedUserDataDto modifiedData) : base(EHit.SpeedupCityUpgradeResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}