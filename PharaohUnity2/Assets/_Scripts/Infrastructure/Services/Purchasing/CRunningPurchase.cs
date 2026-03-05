// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.09.2025
// =========================================

namespace ServiceEngine.Purchasing
{
	public class CRunningPurchase
	{
		public readonly long PurchaseTime;
		public readonly string ProductId;
		public readonly string OfferId;

		public CRunningPurchase(long purchaseTime, string productId, string offerId)
		{
			PurchaseTime = purchaseTime;
			ProductId = productId;
			OfferId = offerId;
		}
	}
}