// =========================================
// AUTHOR: Juraj Joscak
// DATE:   25.02.2026
// =========================================

using AldaEngine;

namespace TycoonBuilder.GoToStates
{
	public class COpenSpecialBuildingMenuState : CGoToFsmState
	{
		private readonly CUser _user;
		
		public COpenSpecialBuildingMenuState(IEventBus eventBus, CUser user) : base(eventBus)
		{
			_user = user;
		}

		public override void Start()
		{
			EventBus.ProcessTask(new CShowSpecialBuildingMenuTask(_user.City.GetEmptyPlotIndex(), _user.City.GetBuildingsInInventoryCount() == 0 ? 0 : 1));
			IsCompleted = true;
		}
	}
}