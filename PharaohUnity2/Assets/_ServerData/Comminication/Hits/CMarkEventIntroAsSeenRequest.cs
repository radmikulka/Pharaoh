// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.12.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CMarkEventIntroAsSeenRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public ELiveEvent LiveEvent { get; set; }
		
		public CMarkEventIntroAsSeenRequest() : base(EHit.MarkEventIntroAsSeenRequest)
		{
		}
		
		public CMarkEventIntroAsSeenRequest(ELiveEvent liveEvent) : base(EHit.MarkEventIntroAsSeenRequest)
		{
			LiveEvent = liveEvent;
		}
	}
	
	public class CMarkEventIntroAsSeenResponse : CResponseHit
	{
		public CMarkEventIntroAsSeenResponse() : base(EHit.MarkEventIntroAsSeenResponse)
		{
		}
	}
}