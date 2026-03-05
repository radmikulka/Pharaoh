// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CSpeedupWarehouseUpgradeRequest : CCommTokenBasedRequest
	{
		public CSpeedupWarehouseUpgradeRequest() : base(EHit.SpeedupWarehouseUpgradeRequest)
		{
		}
	}
	
	public class CSpeedupWarehouseUpgradeResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CSpeedupWarehouseUpgradeResponse() : base(EHit.SpeedupWarehouseUpgradeResponse)
		{
		}
		
		public CSpeedupWarehouseUpgradeResponse(CModifiedUserDataDto modifiedData) : base(EHit.SpeedupWarehouseUpgradeResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}