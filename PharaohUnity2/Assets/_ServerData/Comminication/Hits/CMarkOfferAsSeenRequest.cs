// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.10.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CMarkOfferAsSeenRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string Guid { get; set; }
		
		public CMarkOfferAsSeenRequest() : base(EHit.MarkOfferAsSeenRequest)
		{
		}
		
		public CMarkOfferAsSeenRequest(string guid) : base(EHit.MarkOfferAsSeenRequest)
		{
			Guid = guid;
		}
	}

	public class CMarkOfferAsSeenResponse : CResponseHit
	{
		public CMarkOfferAsSeenResponse() : base(EHit.MarkOfferAsSeenResponse)
		{
		}
	}
}