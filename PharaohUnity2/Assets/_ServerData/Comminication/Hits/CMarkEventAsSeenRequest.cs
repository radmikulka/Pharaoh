// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.02.2026
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CMarkEventAsSeenRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public ELiveEvent LiveEvent { get; set; }
		
		public CMarkEventAsSeenRequest() : base(EHit.MarkEventAsSeenRequest)
		{
		}
		
		public CMarkEventAsSeenRequest(ELiveEvent liveEvent) : base(EHit.MarkEventAsSeenRequest)
		{
			LiveEvent = liveEvent;
		}
	}

	public class CMarkEventAsSeenResponse : CResponseHit
	{
		public CMarkEventAsSeenResponse() : base(EHit.MarkEventAsSeenResponse)
		{
		}
	}
}