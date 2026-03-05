// =========================================
// AUTHOR: Juraj Joscak
// DATE:   18.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class COpenFactoryMenuState : CGoToFsmState
	{
		public COpenFactoryMenuState(IEventBus eventBus) : base(eventBus)
		{
			
		}

		public override void Start()
		{
			EResource resource = Context.GetEntry<EResource>(EGoToContextKey.Resource);
			EFactory factoryId = Context.GetEntry<EFactory>(EGoToContextKey.FactoryId);
			EventBus.ProcessTaskAsync(new COpenFactoryTask(factoryId, resource), CancellationToken);
			IsCompleted = true;
		}
	}
}