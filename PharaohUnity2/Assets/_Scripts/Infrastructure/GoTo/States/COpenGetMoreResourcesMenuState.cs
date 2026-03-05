// =========================================
// AUTHOR: Juraj Joscak
// DATE:   18.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class COpenGetMoreResourcesMenuState : CGoToFsmState
	{
		public COpenGetMoreResourcesMenuState(IEventBus eventBus) : base(eventBus)
		{
			
		}

		public override void Start()
		{
			SResource resource = Context.GetEntry<SResource>(EGoToContextKey.Resource);
			string source = Context.GetEntry<string>(EGoToContextKey.EventSource);
			EventBus.ProcessTask(new CShowGetMoreMaterialTask(resource, source));
			IsCompleted = true;
		}
	}
}