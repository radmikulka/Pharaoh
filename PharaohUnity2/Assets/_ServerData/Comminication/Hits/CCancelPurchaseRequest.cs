// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CCancelPurchaseRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string OfferId { get; set; }
		[JsonProperty] public string ProductId { get; set; }
		
		public CCancelPurchaseRequest() : base(EHit.CancelPurchaseRequest)
		{
		}
	
		public CCancelPurchaseRequest(string offerId, string productId) : base(EHit.CancelPurchaseRequest)
		{
			ProductId = productId;
			OfferId = offerId;
		}
	}

	public class CCancelPurchaseResponse : CResponseHit
	{
		public CCancelPurchaseResponse() : base(EHit.CancelPurchaseResponse)
		{
		}
	}
}