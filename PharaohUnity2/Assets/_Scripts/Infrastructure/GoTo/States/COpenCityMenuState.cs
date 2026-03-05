// =========================================
// AUTHOR: Juraj Joscak
// DATE:   16.02.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class COpenCityMenuState : CGoToFsmState
	{
		public COpenCityMenuState(IEventBus eventBus) : base(eventBus)
		{
			
		}

		public override void Start()
		{
			ECityMenuTab tab = Context.GetEntry<ECityMenuTab>(EGoToContextKey.CityMenuTab);
			EventBus.ProcessTaskAsync(new COpenCityMenuTask(tab), CancellationToken);
			IsCompleted = true;
		}
	}
}