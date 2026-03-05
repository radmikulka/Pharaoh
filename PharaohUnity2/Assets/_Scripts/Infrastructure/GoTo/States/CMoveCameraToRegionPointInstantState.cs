// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.09.2025
// =========================================

using System.Threading.Tasks;
using AldaEngine;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class CMoveCameraToRegionPointInstantState : CGoToFsmState
	{
		public CMoveCameraToRegionPointInstantState(IEventBus eventBus) : base(eventBus)
		{
		}

		public override void Start()
		{
			ERegionPoint regionPoint = Context.GetEntry<ERegionPoint>(EGoToContextKey.RegionPoint);
			ERegion region = Context.GetEntry<ERegion>(EGoToContextKey.Region);
			
			EventBus.ProcessTask(new CFocusCameraOnRegionPointInstantTask(regionPoint, region));
			IsCompleted = true;
		}
	}
}