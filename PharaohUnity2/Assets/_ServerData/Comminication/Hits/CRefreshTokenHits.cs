// =========================================
// AUTHOR: Radek Mikulka
// DATE:   27.11.2023
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CRefreshTokenRequest : CRequestHit
	{
		[JsonProperty] public string UserAuthUid { get; set; }
		[JsonProperty] public string CommunicationToken { get; set; }
		
		public CRefreshTokenRequest() : base(EHit.RefreshTokenRequest)
		{
		}
	
		public CRefreshTokenRequest(string userAuthUid, string communicationToken) : base(EHit.RefreshTokenRequest)
		{
			CommunicationToken = communicationToken;
			UserAuthUid = userAuthUid;
		}
	}

	public class CRefreshTokenResponse : CResponseHit
	{
		public CRefreshTokenResponse() : base(EHit.RefreshTokenResponse)
		{
		}
	}
}