// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CBuyBuildingPlotRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public int Index { get; set; }
		
		public CBuyBuildingPlotRequest() : base(EHit.BuyBuildingPlotRequest)
		{
		}
	
		public CBuyBuildingPlotRequest(int index) : base(EHit.BuyBuildingPlotRequest)
		{
			Index = index;
		}
	}
	
	public class CBuyBuildingPlotResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CBuyBuildingPlotResponse() : base(EHit.BuyBuildingPlotResponse)
		{
		}
		
		public CBuyBuildingPlotResponse(CModifiedUserDataDto modifiedData) : base(EHit.BuyBuildingPlotResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}