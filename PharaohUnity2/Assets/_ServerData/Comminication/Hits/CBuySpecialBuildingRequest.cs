// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CBuySpecialBuildingRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public ESpecialBuilding Building { get; set; }
		[JsonProperty] public bool Free { get; set; }
		
		public CBuySpecialBuildingRequest() : base(EHit.BuySpecialBuildingRequest)
		{
		}
	
		public CBuySpecialBuildingRequest(ESpecialBuilding building, bool free) : base(EHit.BuySpecialBuildingRequest)
		{
			Building = building;
			Free = free;
		}
	}

	public class CBuySpecialBuildingResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CBuySpecialBuildingResponse() : base(EHit.BuySpecialBuildingResponse)
		{
		}
		
		public CBuySpecialBuildingResponse(CModifiedUserDataDto modifiedData) : base(EHit.BuySpecialBuildingResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}