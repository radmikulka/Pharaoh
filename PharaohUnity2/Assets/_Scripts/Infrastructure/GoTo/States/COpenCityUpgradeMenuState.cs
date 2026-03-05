// =========================================
// AUTHOR: Marek Karaba
// DATE:   21.11.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder.GoToStates
{
	public class COpenCityUpgradeMenuState : CGoToFsmState
	{
		public COpenCityUpgradeMenuState(IEventBus eventBus) : base(eventBus)
		{
		}

		public override void Start()
		{
			EventBus.ProcessTask(new COpenCityUpgradeMenuTask(true));
			IsCompleted = true;
		}
	}
}