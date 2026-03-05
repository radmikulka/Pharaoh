// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.10.2025
// =========================================

using System;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public abstract class CBaseTutorial
	{
		protected readonly ITutorialCommentator TutorialCommentator;
		protected readonly CLazyActionQueue LazyActionsQueue;
		protected readonly CEventSystem EventSystem;
		protected readonly IEventBus EventBus;
		protected readonly CUser User;

		protected CBaseTutorial(
			IEventBus eventBus, 
			CUser user, 
			CEventSystem eventSystem, 
			ITutorialCommentator tutorialCommentator, 
			CLazyActionQueue actionQueue
			)
		{
			TutorialCommentator = tutorialCommentator;
			LazyActionsQueue = actionQueue;
			EventSystem = eventSystem;
			EventBus = eventBus;
			User = user;
		}

		protected void DisableAllTutorialGraphics()
		{
			EventBus.ProcessTask(new CHideTutorialHighlightTask());
			EventBus.ProcessTask(new CHideTutorialArrowTask());
			EventBus.ProcessTask(new CHideTutorialTooltipTask());
		}

		protected async UniTask<Transform> GetDispatchedVehicle(EVehicle id, CancellationToken ct)
		{
			while (true)
			{
				CGetVehicleInstanceOrDefaultResponse vehicleResponse = EventBus.ProcessTask<CGetVehicleInstanceOrDefaultRequest, CGetVehicleInstanceOrDefaultResponse>(
					new CGetVehicleInstanceOrDefaultRequest(id));
				if (vehicleResponse.Transform)
				{
					return vehicleResponse.Transform;
				}

				await UniTask.Yield(ct);
			}
		}
		
		protected async UniTask WaitForMenuCloseStart(EScreenId menuId, CancellationToken ct)
		{
			await CWaitForSignal.WaitForSignalAsync<CScreenCloseStartSignal>(screen => screen.MenuId == (int) menuId, EventBus, ct);
		}
		
		protected async UniTask WaitForMenuOpenEnd(EScreenId menuId, CancellationToken ct)
		{
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(screen => screen.MenuId == (int) menuId, EventBus, ct);
		}
		
		protected void RunTutorialAction(Func<CancellationToken, UniTask> action)
		{
			CTutorialAction lazyAction = new(action);
			LazyActionsQueue.AddAction(lazyAction);
		}
		
		private class CTutorialAction : ILazyAction
		{
			private readonly Func<CancellationToken, UniTask> _action;
			
			public int Priority => int.MaxValue;

			public CTutorialAction(Func<CancellationToken, UniTask> action)
			{
				_action = action;
			}

			public async UniTask Execute(CancellationToken ct)
			{
				await _action(ct);
			}
		}
	}
}