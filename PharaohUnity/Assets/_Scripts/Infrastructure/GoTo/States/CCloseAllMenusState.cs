// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using AldaEngine;
using Pharaoh;
using UnityEngine;

namespace Pharaoh.GoToStates
{
	public class CCloseAllMenusState : CGoToFsmState
	{
		public CCloseAllMenusState(IEventBus eventBus) : base(eventBus)
		{
		}

		public override void Tick()
		{
			CIsAnyScreenActiveResponse response = EventBus.ProcessTask<CIsAnyScreenActiveRequest, CIsAnyScreenActiveResponse>();
			if (!response.IsActive)
			{
				IsCompleted = true;
				return;
			}
			EventBus.ProcessTask(new CTryCloseActiveScreenTask());
		}

		public override void End()
		{
			base.End();
			IsCompleted = false;
		}
	}
}