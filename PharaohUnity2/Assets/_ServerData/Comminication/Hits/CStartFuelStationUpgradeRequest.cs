// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CStartFuelStationUpgradeRequest : CCommTokenBasedRequest
	{
		public CStartFuelStationUpgradeRequest() : base(EHit.StartFuelStationUpgradeRequest)
		{
		}
	}

	public class CStartFuelStationUpgradeResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CStartFuelStationUpgradeResponse() : base(EHit.StartFuelStationUpgradeResponse)
		{
		}
		
		public CStartFuelStationUpgradeResponse(CModifiedUserDataDto modifiedData) : base(EHit.StartFuelStationUpgradeResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}