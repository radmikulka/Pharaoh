// =========================================
// AUTHOR: Juraj Joščák
// DATE:   24.2.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CScrollToPassRewardState : CGoToFsmState
	{
		public CScrollToPassRewardState(IEventBus eventBus) : base(eventBus)
		{
			
		}
		
		public override void Start()
		{
			int rewardIndex = Context.GetEntry<int>(EGoToContextKey.PassRewardIndex);
			EventBus.Send(new CScrollToPassRewardSignal(rewardIndex));
			IsCompleted = true;
		}
	}
}