// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CGetNameTagContractRectRequest
	{
		public readonly EStaticContractId ContractId;

		public CGetNameTagContractRectRequest(EStaticContractId contractId)
		{
			ContractId = contractId;
		}
	}
}