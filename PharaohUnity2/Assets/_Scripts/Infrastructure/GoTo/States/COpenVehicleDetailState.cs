// =========================================
// AUTHOR: Juraj Joscak
// DATE:   20.02.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class COpenVehicleDetailState : CGoToFsmState
	{
		public COpenVehicleDetailState(IEventBus eventBus) : base(eventBus)
		{
			
		}

		public override void Start()
		{
			EVehicle vehicleId = Context.GetEntry<EVehicle>(EGoToContextKey.Vehicle);
			EventBus.ProcessTask(new COpenVehicleDetailTask(vehicleId, EScreenId.Depot));
			IsCompleted = true;
		}
	}
}