// =========================================
// AUTHOR:
// DATE:   01.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CPassengerContractRewardsClaimedSignal : IEventBusSignal
	{
		public readonly CContract Contract;
		

		public CPassengerContractRewardsClaimedSignal(CContract contract)
		{
			Contract = contract;
		}
	}
}