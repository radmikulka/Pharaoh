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
		[JsonProperty] public EModificationSource OverrideSource { get; set; }
		[JsonProperty] public string OfferId { get; set; }

		public CValidatePurchaseRequest() : base(EHit.ValidatePurchaseRequest)
		{
		}
		
		public CValidatePurchaseRequest(CRealMoneyPurchaseDataDto realMoneyPurchase, string offerId, EModificationSource overrideSource) 
			: base(EHit.ValidatePurchaseRequest)
		{
			RealMoneyPurchase = realMoneyPurchase;
			OverrideSource = overrideSource;
			OfferId = offerId;
		}
	}
	
	public class CValidatePurchaseResponse : CResponseHit
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		[JsonProperty] public bool IsTestUser { get; set; }

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}

		public CValidatePurchaseResponse() : base(EHit.ValidatePurchaseResponse)
		{
		}
		
		public CValidatePurchaseResponse(CModifiedUserDataDto modifiedData, bool isTestUser) 
			: base(EHit.ValidatePurchaseResponse)
		{
			ModifiedData = modifiedData;
			IsTestUser = isTestUser;
		}
	}
}