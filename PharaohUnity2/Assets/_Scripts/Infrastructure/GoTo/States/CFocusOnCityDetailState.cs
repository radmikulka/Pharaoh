// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class CFocusOnCityDetailState : CGoToFsmState
	{
		public CFocusOnCityDetailState(IEventBus eventBus) : base(eventBus)
		{
		}

		public override void Start()
		{
			ECity cityId = Context.GetEntry<ECity>(EGoToContextKey.SideCityId);
			EventBus.Send(new CSetActiveCityDetailCameraSignal(cityId, true));
			IsCompleted = true;
		}
	}
}