// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.02.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class CFocusOnRegionalOfficeState : CGoToFsmState
	{
		public CFocusOnRegionalOfficeState(IEventBus eventBus) : base(eventBus)
		{
		}

		public override void Start()
		{
			EventBus.ProcessTask(new CActivateRegionalOfficeDetailCameraSignal());
			IsCompleted = true;
		}
	}
}