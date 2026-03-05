// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.08.2024
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CCancelAccountDeletionOfServiceAuthRequest : CRequestHit
	{
		[JsonProperty] public EAuthType AuthType { get; set; }
		[JsonProperty] public string ServiceToken { get; set; }
		[JsonProperty] public string ServiceId { get; set; }
		
		public CCancelAccountDeletionOfServiceAuthRequest() : base(EHit.CancelAccountDeletionOfServiceAuthRequest)
		{
		}
		
		public CCancelAccountDeletionOfServiceAuthRequest(EAuthType authType, string serviceToken, string serviceId) : base(EHit.CancelAccountDeletionOfServiceAuthRequest)
		{
			ServiceToken = serviceToken;
			ServiceId = serviceId;
			AuthType = authType;
		}
	}
	
	public class CCancelAccountDeletionOfServiceAuthResponse : CResponseHit
	{
		public CCancelAccountDeletionOfServiceAuthResponse() : base(EHit.CancelAccountDeletionOfServiceAuthResponse)
		{
		}
	}
}