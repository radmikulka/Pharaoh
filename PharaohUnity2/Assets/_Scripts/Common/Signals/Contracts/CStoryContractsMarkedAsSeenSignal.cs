// =========================================
// AUTHOR: Juraj Joscak
// DATE:   06.11.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CStoryContractsMarkedAsSeenSignal : IEventBusSignal
	{
		public EStaticContractId ContractId { get; }
		
		public CStoryContractsMarkedAsSeenSignal(EStaticContractId contractId)
		{
			ContractId = contractId;
		}
	}
}