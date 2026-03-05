// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.09.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder.GoToStates
{
	public class CMoveCameraToSideCityState : CAwaitableState
	{
		private readonly CDesignCityConfigs _cityConfigs;
		private readonly CWorldMap _worldMap;
		
		public CMoveCameraToSideCityState(
			CDesignCityConfigs cityConfigs, 
			IEventBus eventBus, 
			CWorldMap worldMap
		) 
			: base(eventBus)
		{
			_cityConfigs = cityConfigs;
			_worldMap = worldMap;
		}

		protected override async UniTask Run(CancellationToken ct)
		{
			ECity cityId = Context.GetEntry<ECity>(EGoToContextKey.SideCityId);
			CCityConfig config = _cityConfigs.GetCityConfig(cityId);
			CSerializedCity cityWorldData = _worldMap.GetCity(cityId);
			
			await EventBus.ProcessTaskAsync(new CFocusCameraOnRegionPointTask(cityWorldData.RegionPoint.PointId, config.Region), CancellationToken);
		}
	}
}