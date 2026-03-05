// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.10.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder
{
	public class CContractsMenuTutorial : CBaseTutorial, IInitializable
	{
		private readonly CInputLock _inputLock = new("Tutorial", EInputLockLayer.Tutorial);

		public CContractsMenuTutorial(
			ITutorialCommentator tutorialCommentator,
			CLazyActionQueue actionQueue, 
			CEventSystem eventSystem, 
			IEventBus eventBus, 
			CUser user
		) 
			: base(eventBus, user, eventSystem, tutorialCommentator, actionQueue)
		{
		}

		public void Initialize()
		{
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;
			EventBus.Subscribe<CNewResourceMenuClosedSignal>(OnNewResourceMenuClosed);
		}
		
		public void TryStartOnGameLoad()
		{
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;

			bool isMineCompleted = User.Contracts.IsContractCompleted(EStaticContractId._1930_IronOreMine);
			if(!isMineCompleted)
				return;

			RunTutorialAction(Run);
		}

		private void OnNewResourceMenuClosed(CNewResourceMenuClosedSignal signal)
		{
			if(signal.ResourceId != EResource.IronOre)
				return;
			TryRun();
		}

		private void TryRun()
		{
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;
			
			RunTutorialAction(Run);
		}

		private bool IsCompleted()
		{
			return User.Tutorials.IsTutorialCompleted(EContractsTutorialStep.Completed);
		}

		private async UniTask Run(CancellationToken ct)
		{
			CLockObject tutorialLock = new("ContractsTutorial");
			LazyActionsQueue.AddLockObject(tutorialLock);
			EventSystem.AddInputLocker(_inputLock);
			
			await HighlightWarehouseButton(ct);
			await HighlightContract(ct);
			
			User.Tutorials.SetContractsTutorialStep(EContractsTutorialStep.Completed);
			EventSystem.RemoveInputLocker(_inputLock);
			
			// delay to avoid possible popup right after tutorial
			await UniTask.WaitForSeconds(2f, cancellationToken: ct);
			LazyActionsQueue.RemoveLockObject(tutorialLock);
		}
		
		private async UniTask HighlightWarehouseButton(CancellationToken ct)
		{
			await TutorialCommentator.ShowCommentator(ITutorialCommentator.ESide.Right, "Tutorial.Commentator.Contracts1", true, ct);
			await TutorialCommentator.ShowText("Tutorial.Commentator.Contracts2", true, ct);
			await TutorialCommentator.Hide(ct);
			
			EventBus.ProcessTask(new CActivateContractsButtonTask());
			
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.ContractsMenuButton));
			
			CInputLock clickOnVehicleLock = new("Click", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickOnVehicleLock);

			await UniTask.WaitForSeconds(0.3f, cancellationToken: ct);
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(50f, 50f)
					.SetClockwiseArrowRotation(45f)
				;
			
			EventBus.ProcessTask(showArrowTask);
			
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(
				signal => signal.MenuId == (int) EScreenId.Contracts, EventBus, ct);
			
			DisableAllTutorialGraphics();
			
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(
				signal => signal.MenuId == (int) EScreenId.Contracts, EventBus, ct);
			
			EventSystem.RemoveInputLocker(clickOnVehicleLock);
		}
		
		private async UniTask HighlightContract(CancellationToken ct)
		{
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.RoadSlopeContractButton));
			
			CInputLock clickOnVehicleLock = new("Click", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickOnVehicleLock);

			await UniTask.WaitForSeconds(0.3f, cancellationToken: ct);
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(0f, 50f)
					.SetClockwiseArrowRotation(0f)
				;
			
			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(rect.RectTransform, "Tutorial.Contracts")
					.SetAnchoredOffset(250f, 200f)
					.SetSide(ETutorialTooltipSide.None)
				;
			
			CShowTutorialHighlightTask highlight = new CShowTutorialHighlightTask(rect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetAnchoredOffset(0f, 275f)
					.SetSizeOffset(320f, 650f)
				;
			
			EventBus.ProcessTask(showArrowTask);
			EventBus.ProcessTask(highlight);
			EventBus.ProcessTask(tooltip);

			CGetUiScrollRectResponse scrollRect = EventBus.ProcessTask<CGetUiScrollRectRequest, CGetUiScrollRectResponse>(new CGetUiScrollRectRequest(EUiScrollRect.ContractsMenuStoryContractsSection));
			scrollRect.ScrollRect.enabled = false;
			
			await CWaitForSignal.WaitForSignalAsync<CScreenCloseStartSignal>(
				signal => signal.MenuId == (int) EScreenId.Contracts, EventBus, ct);
			
			DisableAllTutorialGraphics();
			EventSystem.RemoveInputLocker(clickOnVehicleLock);
		}
	}
}