// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.10.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;
using UnityEngine;

namespace TycoonBuilder
{
	public class CGetMoreMaterialTutorial : CBaseTutorial, IInitializable
	{
		private readonly CInputLock _inputLock = new("GetMoreMaterialTutorial", EInputLockLayer.Tutorial);
		private readonly CGetMoreMaterialTutorialFunnel _analyticsFunnel;
		private readonly ISmartArrowLocker _smartArrowLocker;
		private readonly IGoToHandler _goToHandler;

		public CGetMoreMaterialTutorial(
			CGetMoreMaterialTutorialFunnel analyticsFunnel,
			ITutorialCommentator tutorialCommentator,
			ISmartArrowLocker smartArrowLocker, 
			CLazyActionQueue actionQueue, 
			CEventSystem eventSystem, 
			IGoToHandler goToHandler, 
			IEventBus eventBus, 
			CUser user
			) 
			: base(eventBus, user, eventSystem, tutorialCommentator, actionQueue)
		{
			_smartArrowLocker = smartArrowLocker;
			_analyticsFunnel = analyticsFunnel;
			_goToHandler = goToHandler;
		}

		public void Initialize()
		{
			EventBus.Subscribe<CNewResourceMenuClosedSignal>(OnNewResourceMenuClosed);
			TryStart();
		}

		private void OnNewResourceMenuClosed(CNewResourceMenuClosedSignal signal)
		{
			if(signal.ResourceId != EResource.Coal)
				return;
			
			TryStart();
		}

		private void TryStart()
		{
			bool canRun = CanRun();
			if (!canRun)
				return;
			
			bool coalMineCompleted = User.Contracts.IsContractCompleted(EStaticContractId._1930_CoalMine);
			if(!coalMineCompleted)
				return;

			RunTutorialAction(Run);
		}

		private bool CanRun()
		{
			return !User.Tutorials.IsTutorialCompleted(EGetMoreMaterialTutorialStep.Completed);
		}

		private async UniTask Run(CancellationToken ct)
		{
			EventSystem.AddInputLocker(_inputLock);
			_analyticsFunnel.Send(EGetMoreMaterialFunnelStep.Started);
			await GoToContract(ct);
			await ShowVehicleHighlight(ct);
			await ActivateGetMoreButton(ct);
			await HighlightGoButton(ct);
			HideSmartArrowsForWhile(ct).Forget();
			EventSystem.RemoveInputLocker(_inputLock);
		}

		private async UniTask GoToContract(CancellationToken ct)
		{
			await _goToHandler.GoToRegionPoint(ERegionPoint.SmallPowerPlant, ERegion.Region1, ct);
			
			RectTransform nameTagRect = EventBus.ProcessTask<CGetNameTagContractRectRequest, RectTransform>(
				new CGetNameTagContractRectRequest(EStaticContractId._1930_SmallPowerPlant)
				);
			
			CInputLock waitForClickLock = new("Tutorial_WaitForClick", EInputLockLayer.Tutorial, nameTagRect);
			EventSystem.AddInputLocker(waitForClickLock);
			
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(screen => screen.MenuId == (int) EScreenId.Dispatch, EventBus, ct);
			DisableAllTutorialGraphics();
			
			await WaitForMenuOpenEnd(EScreenId.Dispatch, ct);
			EventSystem.RemoveInputLocker(waitForClickLock);
		}

		private async UniTaskVoid HideSmartArrowsForWhile(CancellationToken ct)
		{
			CLockObject lockObject = new("GetMoreMaterialTutorial");
			_smartArrowLocker.AddLock(lockObject);
			await UniTask.WaitForSeconds(2f, cancellationToken: ct);
			_smartArrowLocker.RemoveLock(lockObject);
		}

		private async UniTask ShowVehicleHighlight(CancellationToken ct)
		{
			await UniTask.WaitForSeconds(0.2f, cancellationToken: ct);
			
			CGetUiRectResponse vehicleRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.ContractWarehouseIcon));

			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(vehicleRect.RectTransform, "Tutorial.GetMoreResources1")
					.SetAnchoredOffset(80f, 69f)
					.SetSide(ETutorialTooltipSide.Left)
					.SetShowContinueButton(true)
				;

			EventBus.ProcessTask(tooltip);
			
			await CWaitForSignal.WaitForSignalAsync<CTutorialContinueClickedSignal>(EventBus, ct);
			_analyticsFunnel.Send(EGetMoreMaterialFunnelStep.ContinueClicked);
			DisableAllTutorialGraphics();
		}
		
		private async UniTask ActivateGetMoreButton(CancellationToken ct)
		{
			EventBus.ProcessTask(new CActivateGetMoreButtonTask());
			
			await UniTask.WaitForSeconds(0.2f, cancellationToken: ct);
			
			// Sipka na Get More button
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.GetMoreMaterialButton));
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(20f, 20f)
					.SetClockwiseArrowRotation(45f)
				;
			
			EventBus.ProcessTask(showArrowTask);
			
			CInputLock clickLock = new("ClickRect", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickLock);
			
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(EventBus, ct);
			_analyticsFunnel.Send(EGetMoreMaterialFunnelStep.GetMoreClicked);
			EventSystem.RemoveInputLocker(clickLock);
			DisableAllTutorialGraphics();
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(EventBus, ct);
		}
		
		private async UniTask HighlightGoButton(CancellationToken ct)
		{
			await UniTask.WaitForSeconds(0.2f, cancellationToken: ct);
			
			// Sipka na button Go Get More
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.GoInNeedResourcesScreen));
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(100f, 20f)
					.SetClockwiseArrowRotation(45f)
				;
			
			EventBus.ProcessTask(showArrowTask);
			
			CInputLock clickLock = new("ClickRect", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickLock);
			
			await CWaitForSignal.WaitForSignalAsync<CScreenCloseStartSignal>(EventBus, ct);
			_analyticsFunnel.Send(EGetMoreMaterialFunnelStep.Completed);
			EventSystem.RemoveInputLocker(clickLock);
			DisableAllTutorialGraphics();
			User.Tutorials.SetGetMoreMaterialStep(EGetMoreMaterialTutorialStep.Completed);
		}
	}
}