// =========================================
// AUTHOR: Juraj Joščák
// DATE:   24.2.2026
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CScrollToPassRewardSignal : IEventBusSignal
	{
		public readonly int RewardIndex;

		public CScrollToPassRewardSignal(int rewardIndex)
		{
			RewardIndex = rewardIndex;
		}
	}
}