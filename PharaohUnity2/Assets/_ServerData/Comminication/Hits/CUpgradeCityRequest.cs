// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CUpgradeCityRequest : CCommTokenBasedRequest
	{
		public CUpgradeCityRequest() : base(EHit.UpgradeCityRequest)
		{
		}
	}

	public class CUpgradeCityResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CUpgradeCityResponse() : base(EHit.UpgradeCityResponse)
		{
		}
		
		public CUpgradeCityResponse(CModifiedUserDataDto modifiedData) : base(EHit.UpgradeCityResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}