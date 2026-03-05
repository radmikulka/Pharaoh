// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.08.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CRemoveResourceRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public SResource Resource { get; set; }
		
		public CRemoveResourceRequest() : base(EHit.RemoveResourceRequest)
		{
		}
		
		public CRemoveResourceRequest(SResource resource) : base(EHit.RemoveResourceRequest)
		{
			Resource = resource;
		}
	}

	public class CRemoveResourceResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedUserData { get; set; }
		
		public CRemoveResourceResponse() : base(EHit.RemoveResourceResponse)
		{
		}
		
		public CRemoveResourceResponse(CModifiedUserDataDto modifiedUserData) : base(EHit.RemoveResourceResponse)
		{
			ModifiedUserData = modifiedUserData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedUserData;
		}
	}
}