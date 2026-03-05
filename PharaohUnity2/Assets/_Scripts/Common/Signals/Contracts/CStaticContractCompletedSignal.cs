// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CStaticContractCompletedSignal : IEventBusSignal
	{
		public readonly IContract Contract;

		public CStaticContractCompletedSignal(IContract contract)
		{
			Contract = contract;
		}
	}
}