// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.12.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder.GoToStates
{
	public class CMoveToRegionState : CAwaitableState
	{
		private readonly IRegionsController _regionsController;
		
		public CMoveToRegionState(IEventBus eventBus, IRegionsController regionsController) 
			: base(eventBus)
		{
			_regionsController = regionsController;
		}

		protected override async UniTask Run(CancellationToken ct)
		{
			ERegion region = Context.GetEntry<ERegion>(EGoToContextKey.Region);
			if(_regionsController.ActiveRegion == region)
				return;
			await _regionsController.LoadRegion(region, ct);
		}
	}
}