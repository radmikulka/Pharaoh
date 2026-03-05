// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.09.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine.SceneManagement;

namespace TycoonBuilder
{
	public class CRegionLiveEventGameGameMode : CGameMode<CRegionLiveEventGameGameModeData>
	{
		private readonly IRegionsController _regionsController;
		private readonly IEventBus _eventBus;

		public CRegionLiveEventGameGameMode(
			IRegionsController regionsController, 
			IEventBus eventBus
			) : base(EGameModeId.RegionLiveEvent)
		{
			_regionsController = regionsController;
			_eventBus = eventBus;
		}

		public override async UniTask Load(IGameModeData taskData, CancellationToken ct)
		{
			await base.Load(taskData, ct);
			await _regionsController.LoadLiveEvent(Data.LiveEventId, ct);
			_eventBus.Send(new CRegionLiveEventGameModeLoadedSignal(Data.LiveEventId));
		}
	}
}