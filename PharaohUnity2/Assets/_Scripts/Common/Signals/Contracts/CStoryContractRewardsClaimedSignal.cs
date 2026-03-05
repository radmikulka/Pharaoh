// =========================================
// AUTHOR: Marek Karaba
// DATE:   17.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CStoryContractRewardsClaimedSignal : IEventBusSignal
	{
		public readonly IContract Contract;

		public CStoryContractRewardsClaimedSignal(IContract contract)
		{
			Contract = contract;
		}
	}
}