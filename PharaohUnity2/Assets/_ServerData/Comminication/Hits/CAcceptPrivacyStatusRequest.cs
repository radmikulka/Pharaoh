// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.08.2024
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CAcceptPrivacyStatusRequest : CCommTokenBasedRequest
	{
		public CAcceptPrivacyStatusRequest() : base(EHit.AcceptPrivacyStatusRequest)
		{
		}
	}

	public class CAcceptPrivacyStatusResponse : CResponseHit
	{
		public CAcceptPrivacyStatusResponse() : base(EHit.AcceptPrivacyStatusResponse)
		{
		}
	}
}