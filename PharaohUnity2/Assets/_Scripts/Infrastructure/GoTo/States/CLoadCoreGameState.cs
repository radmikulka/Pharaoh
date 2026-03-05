// =========================================
// AUTHOR: Juraj Joscak
// DATE:   26.01.2026
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class CLoadCoreGameState : CAwaitableState
	{
		private readonly CGameModeManager _gameModeManager;
		
		public CLoadCoreGameState(IEventBus eventBus, CGameModeManager gameModeManager)
			: base(eventBus)
		{
			_gameModeManager = gameModeManager;
		}
		
		protected override async UniTask Run(CancellationToken ct)
		{
			ERegion region = Context.GetEntry<ERegion>(EGoToContextKey.Region);
			CCoreGameGameModeData coreGameMode = new(region);
			if(_gameModeManager.ActiveGameMode.Id == coreGameMode.GameModeId)
				return;
			
			await EventBus.ProcessTaskAsync(new CLoadGameModeTask(coreGameMode), ct);
		}
	}
}