// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class COpenResourceDispatchMenuState : CGoToFsmState
	{
		private readonly COpenDispatchMenuHandler _handler;
		
		public COpenResourceDispatchMenuState(IEventBus eventBus, COpenDispatchMenuHandler handler) : base(eventBus)
		{
			_handler = handler;
		}

		public override void Start()
		{
			EIndustry industryId = Context.GetEntry<EIndustry>(EGoToContextKey.IndustryId);
			_handler.Execute(industryId);
			IsCompleted = true;
		}
	}
}