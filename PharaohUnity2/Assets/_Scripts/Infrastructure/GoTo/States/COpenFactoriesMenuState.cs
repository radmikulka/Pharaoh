// =========================================
// AUTHOR: Juraj Joscak
// DATE:   11.02.2026
// =========================================

using AldaEngine;

namespace TycoonBuilder.GoToStates
{
	public class COpenFactoriesMenuState : CGoToFsmState
	{
		public COpenFactoriesMenuState(IEventBus eventBus) : base(eventBus)
		{
			
		}

		public override void Start()
		{
			EventBus.ProcessTask(new CShowScreenTask(EScreenId.Factories));
			IsCompleted = true;
		}
	}
}