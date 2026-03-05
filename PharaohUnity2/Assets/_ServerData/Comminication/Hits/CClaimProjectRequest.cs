// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.02.2026
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimProjectRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EProjectId ProjectId { get; set; }
		
		public CClaimProjectRequest() : base(EHit.ClaimProjectRequest)
		{
		}
		
		public CClaimProjectRequest(EProjectId projectId) : base(EHit.ClaimProjectRequest)
		{
			ProjectId = projectId;
		}
	}
	
	public class CClaimProjectResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CClaimProjectResponse() : base(EHit.ClaimProjectResponse)
		{
		}
		
		public CClaimProjectResponse(CModifiedUserDataDto modifiedData) : base(EHit.ClaimProjectResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}