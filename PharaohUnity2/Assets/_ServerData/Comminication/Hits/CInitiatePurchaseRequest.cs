// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.09.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CInitiatePurchaseRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string OfferId { get; set; }
		[JsonProperty] public string ProductId { get; set; }
		[JsonProperty] public CPurchasePayloads Payloads { get; set; }

		public CInitiatePurchaseRequest() : base(EHit.InitiatePurchaseRequest)
		{
			
		}
	
		public CInitiatePurchaseRequest(string offerId, string productId, CPurchasePayloads payloads) : base(EHit.InitiatePurchaseRequest)
		{
			ProductId = productId;
			Payloads = payloads;
			OfferId = offerId;
		}
	}

	public class CInitiatePurchaseResponse : CResponseHit
	{
		public CInitiatePurchaseResponse() : base(EHit.InitiatePurchaseResponse)
		{
		}
	}
}