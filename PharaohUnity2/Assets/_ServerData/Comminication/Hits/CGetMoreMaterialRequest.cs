// =========================================
// AUTHOR: Radek Mikulka
// DATE:   17.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CGetMoreMaterialRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public SResource Resource { get; set; }
		
		public CGetMoreMaterialRequest() : base(EHit.GetMoreMaterialRequest)
		{
		}
		
		public CGetMoreMaterialRequest(SResource resource) : base(EHit.GetMoreMaterialRequest)
		{
			Resource = resource;
		}
	}
	
	public class CGetMoreMaterialResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CGetMoreMaterialResponse() : base(EHit.GetMoreMaterialResponse)
		{
		}
		
		public CGetMoreMaterialResponse(CModifiedUserDataDto modifiedData) : base(EHit.GetMoreMaterialResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}