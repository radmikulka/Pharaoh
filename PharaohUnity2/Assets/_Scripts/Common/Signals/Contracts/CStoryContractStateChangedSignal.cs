// =========================================
// AUTHOR: Marek Karaba
// DATE:   10.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CStoryContractStateChangedSignal : IEventBusSignal
	{
		public readonly EStaticContractId ContractId;

		public CStoryContractStateChangedSignal(EStaticContractId contractId)
		{
			ContractId = contractId;
		}
	}
}