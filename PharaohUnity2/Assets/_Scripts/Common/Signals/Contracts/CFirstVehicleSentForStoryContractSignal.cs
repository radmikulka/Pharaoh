// =========================================
// AUTHOR: Marek Karaba
// DATE:   10.11.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CFirstVehicleSentForStoryContractSignal : IEventBusSignal
	{
		public IContract Contract { get; }

		public CFirstVehicleSentForStoryContractSignal(IContract contract)
		{
			Contract = contract;
		}
	}
}