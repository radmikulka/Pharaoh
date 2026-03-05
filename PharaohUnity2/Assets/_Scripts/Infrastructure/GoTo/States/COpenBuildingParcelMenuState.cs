// =========================================
// AUTHOR: Juraj Joscak
// DATE:   02.03.2026
// =========================================

using AldaEngine;
using UnityEngine;

namespace TycoonBuilder.GoToStates
{
	public class COpenBuildingParcelMenuState : CGoToFsmState
	{
		private readonly CUser _user;
		
		public COpenBuildingParcelMenuState(IEventBus eventBus, CUser user) : base(eventBus)
		{
			_user = user;
		}

		public override void Start()
		{
			EventBus.ProcessTask(new CShowParcelMenuTask(_user.City.GetFirstLockedPlotIndex(), false));
			IsCompleted = true;
		}
	}
}