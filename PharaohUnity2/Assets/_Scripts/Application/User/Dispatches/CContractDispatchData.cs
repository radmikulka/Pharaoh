// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.02.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CContractDispatchData
	{
		public readonly EStaticContractId Contract;
		public readonly int ResourceAmount;

		public CContractDispatchData(EStaticContractId contract, int resourceAmount)
		{
			Contract = contract;
			ResourceAmount = resourceAmount;
		}
	}
}
