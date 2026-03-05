// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CDispatchForContractOpenedSignal : IEventBusSignal
	{
		public readonly IContract Contract;

		public CDispatchForContractOpenedSignal(IContract contract)
		{
			Contract = contract;
		}
	}
}