// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder.GoToStates
{
	public class CMoveCameraToResourceMineState : CAwaitableState
	{
		private readonly CDesignIndustryConfigs _industryConfigs;
		private readonly CWorldMap _worldMap;
		
		public CMoveCameraToResourceMineState(
			CDesignIndustryConfigs industryConfigs, 
			IEventBus eventBus, 
			CWorldMap worldMap
		) 
			: base(eventBus)
		{
			_industryConfigs = industryConfigs;
			_worldMap = worldMap;
		}
		
		protected override async UniTask Run(CancellationToken ct)
		{
			EIndustry industryId = Context.GetEntry<EIndustry>(EGoToContextKey.IndustryId);
			CResourceIndustryConfig config = _industryConfigs.GetConfig(industryId);
			CSerializedResourcePoint contractPoint = _worldMap.GetResourcePoint(config.Resource);
			
			await EventBus.ProcessTaskAsync(new CFocusCameraOnRegionPointTask(contractPoint.RegionPoint.PointId, config.Region), CancellationToken);
		}
	}
}