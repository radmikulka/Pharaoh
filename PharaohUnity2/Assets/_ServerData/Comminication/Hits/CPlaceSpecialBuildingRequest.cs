// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CPlaceSpecialBuildingRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public int Index { get; set; }
		[JsonProperty] public ESpecialBuilding Building { get; set; }
		
		public CPlaceSpecialBuildingRequest() : base(EHit.PlaceSpecialBuildingRequest)
		{
		}
	
		public CPlaceSpecialBuildingRequest(int index, ESpecialBuilding building) : base(EHit.PlaceSpecialBuildingRequest)
		{
			Building = building;
			Index = index;
		}
	}
	
	public class CPlaceSpecialBuildingResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CPlaceSpecialBuildingResponse() : base(EHit.PlaceSpecialBuildingResponse)
		{
		}
		
		public CPlaceSpecialBuildingResponse(CModifiedUserDataDto modifiedData) : base(EHit.PlaceSpecialBuildingResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}