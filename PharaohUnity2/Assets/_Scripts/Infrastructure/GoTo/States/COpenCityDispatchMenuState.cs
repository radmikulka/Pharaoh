// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.09.2025
// =========================================

using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class COpenCityDispatchMenuState : CGoToFsmState
	{
		private readonly COpenDispatchMenuHandler _handler;
		
		public COpenCityDispatchMenuState(IEventBus eventBus, COpenDispatchMenuHandler handler) : base(eventBus)
		{
			_handler = handler;
		}

		public override void Start()
		{
			ECity cityId = Context.GetEntry<ECity>(EGoToContextKey.SideCityId);
			_handler.Execute(cityId);
			IsCompleted = true;
		}
	}
}