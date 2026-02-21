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

		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CIsAnyScreenActiveRequest, CIsAnyScreenActiveResponse>(HandleIsAnyScreenActiveRequest);
			_eventBus.AddTaskHandler<CIsScreenOpenedRequest, CIsScreenOpenedResponse>(HandleIsScreenOpenedRequest);
			_eventBus.AddTaskHandler<COpenBadGameVersionScreenTask>(ProcessOpenBadGameVersionScreenCommand);
			_eventBus.AddTaskHandler<CTryCloseActiveScreenTask>(HandleTryCloseActiveScreenRequest);
			_eventBus.AddTaskHandler<CTryKillScreenTask>(HandleTryKillScreenRequest);
			_eventBus.AddAsyncTaskHandler<CCloseAllScreensTask>(ProcessCloseAllScreensCommand);
			_eventBus.AddTaskHandler<CCloseTopmostScreenTask>(ProcessCloseTopmostScreenCommand);
			_eventBus.AddTaskHandler<CShowScreenTask>(ProcessShowScreenCommand);
			_eventBus.AddAsyncTaskHandler<CShowScreenTask>(ProcessShowScreenAsyncCommand);
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

		private void ProcessOpenBadGameVersionScreenCommand(COpenBadGameVersionScreenTask task)
		{
			/*OpenMenu<CBadGameVersionScreen>((int) EScreenId.BadGameVersion, screen =>
			{
				screen.Init(task.TimeToRelease);
			});*/
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

			CTycoonBuilderScreen menu = GetMenu<CTycoonBuilderScreen>((int)task.ScreenId);
			OpenMenu(menu.Id);
			
			await UniTask.WaitUntil(() => menu.State == EScreenState.Opened, cancellationToken: ct);
		}
		
		private void ProcessCloseTopmostScreenCommand(CCloseTopmostScreenTask task)
		{
			TryCloseTopmostMenu();
		}
	}
}