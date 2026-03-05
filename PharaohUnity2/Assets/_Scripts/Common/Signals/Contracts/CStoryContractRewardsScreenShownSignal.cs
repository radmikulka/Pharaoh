// =========================================
// AUTHOR: Marek Karaba
// DATE:   30.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CStoryContractRewardsScreenShownSignal : IEventBusSignal
	{
		public readonly EStaticContractId ContractId;
		public readonly bool LastStageCompleted;
		
		public CStoryContractRewardsScreenShownSignal(EStaticContractId contractId, bool lastStageCompleted)
		{
			ContractId = contractId;
			LastStageCompleted = lastStageCompleted;
		}
	}
}