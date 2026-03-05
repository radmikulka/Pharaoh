// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.09.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class CRealMoneyPurchaseDataDto
	{
		[JsonProperty] public string Token { get; private set; }
		[JsonProperty] public string ProductId { get; private set; }
		[JsonProperty] public long PurchaseTime { get; private set; }
		[JsonProperty] public EStoreId StoreId { get; set; }
		[JsonProperty] public bool InitiatedByUser { get; set; }

		public CRealMoneyPurchaseDataDto()
		{
		}

		public CRealMoneyPurchaseDataDto(string token, string productId, EStoreId storeId, long purchaseTime, bool initiatedByUser)
		{
			InitiatedByUser = initiatedByUser;
			PurchaseTime = purchaseTime;
			ProductId = productId;
			StoreId = storeId;
			Token = token;
		}
	}
}