// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CSpeedupFactoryUpgradeRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EFactory Factory { get; set; }
		
		public CSpeedupFactoryUpgradeRequest() : base(EHit.SpeedupFactoryUpgradeRequest)
		{
		}
		
		public CSpeedupFactoryUpgradeRequest(EFactory factory) : base(EHit.SpeedupFactoryUpgradeRequest)
		{
			Factory = factory;
		}
	}
	
	public class CSpeedupFactoryUpgradeResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CSpeedupFactoryUpgradeResponse() : base(EHit.SpeedupFactoryUpgradeResponse)
		{
		}
		
		public CSpeedupFactoryUpgradeResponse(CModifiedUserDataDto modifiedData) : base(EHit.SpeedupFactoryUpgradeResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}