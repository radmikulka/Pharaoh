// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

namespace ServerData.Hits
{
	public class CClaimWarehouseUpgradeRequest : CCommTokenBasedRequest
	{
		public CClaimWarehouseUpgradeRequest() : base(EHit.ClaimWarehouseUpgradeRequest)
		{
		}
	}

	public class CClaimWarehouseUpgradeResponse : CResponseHit
	{
		public CClaimWarehouseUpgradeResponse() : base(EHit.ClaimWarehouseUpgradeResponse)
		{
		}
	}
}