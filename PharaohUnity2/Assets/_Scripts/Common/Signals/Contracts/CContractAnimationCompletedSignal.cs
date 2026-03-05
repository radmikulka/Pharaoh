// =========================================
// AUTHOR: Marek Karaba
// DATE:   11.11.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CContractAnimationCompletedSignal : IEventBusSignal
	{
		public readonly EStaticContractId ContractId;

		public CContractAnimationCompletedSignal(EStaticContractId contractId)
		{
			ContractId = contractId;
		}
	}
}