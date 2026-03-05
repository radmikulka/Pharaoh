// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class CFocusOnIndustryDetailState : CGoToFsmState
	{
		public CFocusOnIndustryDetailState(IEventBus eventBus) : base(eventBus)
		{
		}

		public override void Start()
		{
			EIndustry industryId = Context.GetEntry<EIndustry>(EGoToContextKey.IndustryId);
			EventBus.Send(new CSetActiveIndustryDetailCameraSignal(industryId, true));
			IsCompleted = true;
		}
	}
}