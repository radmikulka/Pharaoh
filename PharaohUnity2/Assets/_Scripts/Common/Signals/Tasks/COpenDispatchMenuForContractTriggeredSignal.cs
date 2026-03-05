// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class COpenDispatchMenuForContractTriggeredSignal : IEventBusSignal
	{
		public readonly SStaticContractPointer Contract;

		public COpenDispatchMenuForContractTriggeredSignal(SStaticContractPointer contract)
		{
			Contract = contract;
		}
	}
}