// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.10.2023
// =========================================

using AldaEngine;
using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CValidatePurchaseRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public CRealMoneyPurchaseDataDto RealMoneyPurchase { get; set; }
		[JsonProperty] public string OfferId { get; set; }

		public CValidatePurchaseRequest() : base(EHit.ValidatePurchaseRequest)
		{
		}
		
		public CValidatePurchaseRequest(CRealMoneyPurchaseDataDto realMoneyPurchase, string offerId) 
			: base(EHit.ValidatePurchaseRequest)
		{
			RealMoneyPurchase = realMoneyPurchase;
			OfferId = offerId;
		}
	}
	
	public class CValidatePurchaseResponse : CResponseHit
	{
		[JsonProperty] public bool IsTestUser { get; set; }

		public CValidatePurchaseResponse() : base(EHit.ValidatePurchaseResponse)
		{
		}
		
		public CValidatePurchaseResponse(bool isTestUser) : base(EHit.ValidatePurchaseResponse)
		{
			IsTestUser = isTestUser;
		}
	}
}