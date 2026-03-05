// =========================================
// AUTHOR: Juraj Joščák
// DATE:   24.2.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class COpenLiveEventMenuState : CGoToFsmState 
	{
		public COpenLiveEventMenuState(IEventBus eventBus) : base(eventBus)
		{
			
		}

		public override void Start()
		{
			EEventOverviewTab tab = Context.GetEntry<EEventOverviewTab>(EGoToContextKey.LiveEventTab);
			ELiveEvent eventId = Context.GetEntry<ELiveEvent>(EGoToContextKey.LiveEventId);
			//EValuable valuable = Context.GetEntryOrDefault<EValuable>(EGoToContextKey.ValuableType);
			EventBus.ProcessTaskAsync(new COpenEventOverviewTask(eventId, tab), CancellationToken);
			IsCompleted = true;
		}
	}
}