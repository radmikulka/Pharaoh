// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.08.2024
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CCancelAccountDeletionRequest : CRequestHit
	{
		[JsonProperty] public string UserAuthUid { get; set; }
		
		public CCancelAccountDeletionRequest() : base(EHit.CancelAccountDeletionRequest)
		{
		}
		
		public CCancelAccountDeletionRequest(string userAuthUid) : base(EHit.CancelAccountDeletionRequest)
		{
			UserAuthUid = userAuthUid;
		}
	}
	
	public class CCancelAccountDeletionResponse : CResponseHit
	{
		public CCancelAccountDeletionResponse() : base(EHit.CancelAccountDeletionResponse)
		{
		}
	}
}