// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.10.2024
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using Pharaoh.Ui;
using ServerData;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CScreenManager : AldaEngine.CScreenManager, IInitializable
	{
		[SerializeField] private CanvasGroup[] _rootCanvasGroups;
		
		private IEventBus _eventBus;
		private CUser _user;
		private IAuthService _authService;
		private CGameModeManager _gameModeManager;

		[Inject]
		private void Inject(IEventBus eventBus, CUser user, IAuthService authService , CGameModeManager gameModeManager)
		{
			_eventBus = eventBus;
			_user = user;
			_authService = authService;
			_gameModeManager = gameModeManager;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CIsAnyScreenActiveRequest, CIsAnyScreenActiveResponse>(HandleIsAnyScreenActiveRequest);
			_eventBus.AddTaskHandler<CIsScreenOpenedRequest, CIsScreenOpenedResponse>(HandleIsScreenOpenedRequest);
			_eventBus.AddTaskHandler<CMenuManagerStateRequest, CMenuManagerStateResponse>(OnStateRequest);
			_eventBus.AddTaskHandler<CTryCloseActiveScreenTask>(HandleTryCloseActiveScreenRequest);
			_eventBus.AddTaskHandler<CCloseTopmostScreenTask>(ProcessCloseTopmostScreenCommand);
			_eventBus.AddAsyncTaskHandler<CCloseAllScreensTask>(ProcessCloseAllScreensCommand);
			_eventBus.AddAsyncTaskHandler<CShowScreenTask>(ProcessShowScreenAsyncCommand);
			_eventBus.AddTaskHandler<CTryKillScreenTask>(HandleTryKillScreenRequest);
			_eventBus.AddTaskHandler<CShowScreenTask>(ProcessShowScreenCommand);
			_eventBus.AddTaskHandler<CHideAllMenusTask>(HideAllMenus);
		}

		private async Task ProcessCloseAllScreensCommand(CCloseAllScreensTask task, CancellationToken ct)
		{
			while (IsActive)
			{
				TryCloseTopmostMenu();
				await UniTask.Yield(ct);
			}
		}

		private void HideAllMenus(CHideAllMenusTask task)
		{
			foreach (CanvasGroup canvasGroup in _rootCanvasGroups)
			{
				canvasGroup.alpha = task.State ? 1f : 0f;;
			}
		}

		private CIsScreenOpenedResponse HandleIsScreenOpenedRequest(CIsScreenOpenedRequest task)
		{
			bool isActive = GetMenu<IScreen>((int)task.ScreenId).State is EScreenState.Opened;
			return new CIsScreenOpenedResponse(isActive);
		}
		
		private void HandleTryCloseActiveScreenRequest(CTryCloseActiveScreenTask task)
		{
			TryCloseTopmostMenu();
		}
		
		private void HandleTryKillScreenRequest(CTryKillScreenTask task)
		{
			KillScreen((int) task.ScreenId);
		}

		private CIsAnyScreenActiveResponse HandleIsAnyScreenActiveRequest(CIsAnyScreenActiveRequest task)
		{
			return new CIsAnyScreenActiveResponse(IsActive);
		}

		private void ProcessShowScreenCommand(CShowScreenTask task)
		{
			if (task.ClosePreviousScreen)
			{
				TryCloseTopmostMenu();
			}
			OpenMenu((int) task.ScreenId);
		}
		
		private async Task ProcessShowScreenAsyncCommand(CShowScreenTask task, CancellationToken ct)
		{
			if (task.ClosePreviousScreen)
			{
				TryCloseTopmostMenu();
			}

			CPharaohScreen menu = GetMenu<CPharaohScreen>((int)task.ScreenId);
			OpenMenu(menu.Id);
			
			await UniTask.WaitUntil(() => menu.State == EScreenState.Opened, cancellationToken: ct);
		}
		
		private void ProcessCloseTopmostScreenCommand(CCloseTopmostScreenTask task)
		{
			TryCloseTopmostMenu();
		}

		private CMenuManagerStateResponse OnStateRequest(CMenuManagerStateRequest request)
		{
			CMenuManagerStateResponse response = new(IsActive, ActiveMenus.Count);
			return response;
		}
	}
}