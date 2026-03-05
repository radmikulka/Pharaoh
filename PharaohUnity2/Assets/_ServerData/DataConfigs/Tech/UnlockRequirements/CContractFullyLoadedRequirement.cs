// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.10.2025
// =========================================

namespace ServerData
{
	public class CContractFullyLoadedRequirement : IUnlockRequirement
	{
		public readonly EStaticContractId ContractId;

		public CContractFullyLoadedRequirement(EStaticContractId contractId)
		{
			ContractId = contractId;
		}
	}
}