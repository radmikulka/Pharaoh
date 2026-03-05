// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.10.2025
// =========================================

using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder
{
	public class CWarehouseTutorial : CBaseTutorial, IInitializable
	{
		private readonly CInputLock _inputLock = new("Tutorial", EInputLockLayer.Tutorial);
		private readonly CWarehouseTutorialFunnel _analyticsFunnel;
		private readonly ICtsProvider _ctsProvider;
		private readonly IServerTime _serverTime;

		public CWarehouseTutorial(
			CWarehouseTutorialFunnel analyticsFunnel,
			ITutorialCommentator tutorialCommentator,
			CLazyActionQueue actionQueue, 
			CEventSystem eventSystem, 
			ICtsProvider ctsProvider, 
			IServerTime serverTime, 
			IEventBus eventBus, 
			CUser user
			) 
			: base(eventBus, user, eventSystem, tutorialCommentator, actionQueue)
		{
			_analyticsFunnel = analyticsFunnel;
			_ctsProvider = ctsProvider;
			_serverTime = serverTime;
		}

		public void Initialize()
		{
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;
			EventBus.Subscribe<CResourceDispatchCompletedSignal>(OnResourceDispatchCompleted);
		}

		// tohle je asi zbytečné
		/*public void TryStartOnGameLoad()
		{
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;

			CDispatch existingDispatch = User.Dispatches.GetDispatchesForResource(EResource.IronOre).FirstOrDefault();
			long serverTime = _serverTime.GetTimestampInSecs();
			if (existingDispatch == null || existingDispatch.CompletionTime > serverTime)
				return;
			
			Run(_ctsProvider.Token).Forget();
		}*/

		private void OnResourceDispatchCompleted(CResourceDispatchCompletedSignal signal)
		{
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;

			bool ironMineCompleted = User.Contracts.IsContractCompleted(EStaticContractId._1930_IronOreMine);
			if(!ironMineCompleted)
				return;
			
			RunTutorialAction(Run);
		}

		private bool IsCompleted()
		{
			return User.Tutorials.IsTutorialCompleted(EWarehouseTutorialStep.Completed);
		}

		private async UniTask Run(CancellationToken ct)
		{
			EventSystem.AddInputLocker(_inputLock);
			_analyticsFunnel.Send(EWarehouseFunnelStep.Start);
			await CloseAllMenus(ct);
			await HighlightWarehouseButton(ct);
			EventSystem.RemoveInputLocker(_inputLock);
		}

		private async UniTask CloseAllMenus(CancellationToken ct)
		{
			while (IsAnyScreenActive())
			{
				EventBus.ProcessTask(new CCloseTopmostScreenTask());
				await CWaitForSignal.WaitForSignalAsync<CScreenCloseEndSignal>(EventBus, ct);
			}
		}

		private bool IsAnyScreenActive()
		{
			CIsAnyScreenActiveResponse isAnyScreenActive = EventBus.ProcessTask<CIsAnyScreenActiveRequest, CIsAnyScreenActiveResponse>();
			return isAnyScreenActive.IsActive;
		}
		
		private async UniTask HighlightWarehouseButton(CancellationToken ct)
		{
			bool isCompleted = User.Tutorials.IsTutorialCompleted(EWarehouseTutorialStep.Completed);
			if (isCompleted)
				return;
			
			await TutorialCommentator.ShowCommentator(ITutorialCommentator.ESide.Right, "Tutorial.Commentator.Warehouse", true, ct);
			await TutorialCommentator.Hide(ct);
			
			EventBus.ProcessTask(new CActivateWarehouseButtonTask());
			
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.WarehouseMenuButton));
			
			CInputLock clickOnVehicleLock = new("ClickWarehouse", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickOnVehicleLock);

			await UniTask.WaitForSeconds(0.3f, cancellationToken: ct);
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(50f, 0f)
					.SetClockwiseArrowRotation(90f)
				;
			
			EventBus.ProcessTask(showArrowTask);
			_analyticsFunnel.Send(EWarehouseFunnelStep.ButtonHighlighted);
			
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(
				signal => signal.MenuId == (int) EScreenId.Warehouse, EventBus, ct);
			
			_analyticsFunnel.Send(EWarehouseFunnelStep.Completed);
			EventSystem.RemoveInputLocker(clickOnVehicleLock);
			User.Tutorials.SetWarehouseTutorialStep(EWarehouseTutorialStep.Completed);
			
			DisableAllTutorialGraphics();
			
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(
				signal => signal.MenuId == (int) EScreenId.Warehouse, EventBus, ct);
		}
	}
}