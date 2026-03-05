// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.10.2025
// =========================================

namespace ServerData
{
	public class CContractUnlockRequirement : IUnlockRequirement
	{
		public readonly EStaticContractId ContractId;

		public CContractUnlockRequirement(EStaticContractId contractId)
		{
			ContractId = contractId;
		}
	}
}