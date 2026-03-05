// =========================================
// AUTHOR: Radek Mikulka
// DATE:   17.09.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder.GoToStates
{
	public class CMoveCameraToRegionPointState : CAwaitableState
	{
		
		public CMoveCameraToRegionPointState(IEventBus eventBus) : base(eventBus)
		{
		}

		protected override async UniTask Run(CancellationToken ct)
		{
			ERegionPoint regionPoint = Context.GetEntry<ERegionPoint>(EGoToContextKey.RegionPoint);
			ERegion region = Context.GetEntry<ERegion>(EGoToContextKey.Region);
			
			await EventBus.ProcessTaskAsync(new CFocusCameraOnRegionPointTask(regionPoint, region), CancellationToken);
		}
	}
}