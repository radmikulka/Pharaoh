// =========================================
// AUTHOR: Juraj Joscak
// DATE:   16.12.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class CLoadLiveEventState : CAwaitableState
	{
		private readonly CGameModeManager _gameModeManager;
		
		public CLoadLiveEventState(IEventBus eventBus, CGameModeManager gameModeManager)
			: base(eventBus)
		{
			_gameModeManager = gameModeManager;
		}
		
		protected override async UniTask Run(CancellationToken ct)
		{
			ELiveEvent eventId = Context.GetEntry<ELiveEvent>(EGoToContextKey.LiveEventId);
			CRegionLiveEventGameGameModeData eventGameMode = new(eventId);
			if(_gameModeManager.ActiveGameMode.Id == eventGameMode.GameModeId)
				return;
			
			await EventBus.ProcessTaskAsync(new CLoadGameModeTask(eventGameMode), ct);
		}
	}
}