// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CNewStoryContractCreatedSignal : IEventBusSignal
	{
		public readonly EStaticContractId ContractId;

		public CNewStoryContractCreatedSignal(EStaticContractId contractId)
		{
			ContractId = contractId;
		}
	}
}