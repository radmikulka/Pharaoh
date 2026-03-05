// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CUseConsumableRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EResource ConsumableId { get; set; }

		public CUseConsumableRequest() : base(EHit.UseConsumableRequest)
		{
		}

		public CUseConsumableRequest(EResource id) : base(EHit.UseConsumableRequest)
		{
			ConsumableId = id;
		}
	}

	public class CUseConsumableResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CUseConsumableResponse() : base(EHit.UseConsumableResponse)
		{
		}
		
		public CUseConsumableResponse(CModifiedUserDataDto modifiedData) : base(EHit.UseConsumableResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}