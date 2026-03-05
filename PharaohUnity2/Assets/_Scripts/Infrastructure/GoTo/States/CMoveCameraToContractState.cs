// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;
using TycoonBuilder;

namespace TycoonBuilder.GoToStates
{
	public class CMoveCameraToContractState : CAwaitableState
	{
		private readonly CWorldMap _worldMap;
		private readonly CUser _user;
		private readonly CDesignStoryContractConfigs _designContractConfigs;
		
		public CMoveCameraToContractState(
			IEventBus eventBus, 
			CWorldMap worldMap, 
			CUser user,
			CDesignStoryContractConfigs designContractConfigs
			)
			: base(eventBus)
		{
			_worldMap = worldMap;
			_user = user;
			_designContractConfigs = designContractConfigs;
		}
		
		protected override async UniTask Run(CancellationToken ct)
		{
			EStaticContractId contractId = Context.GetEntry<EStaticContractId>(EGoToContextKey.ContractId);
			CSerializedContractPoint contractPoint = _worldMap.GetContractPoint(contractId);
			ERegion targetRegion = contractPoint.RegionPoint.Region;
			
			await EventBus.ProcessTaskAsync(new CFocusCameraOnRegionPointTask(contractPoint.RegionPoint.PointId, targetRegion), CancellationToken);
		}
	}
}