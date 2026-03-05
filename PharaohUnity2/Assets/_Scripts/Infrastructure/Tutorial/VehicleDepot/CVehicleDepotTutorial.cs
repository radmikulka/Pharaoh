// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.10.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder
{
	public class CVehicleDepotTutorial : CBaseTutorial, IInitializable
	{
		private readonly CInputLock _inputLock = new("IntroTutorial", EInputLockLayer.Tutorial);
		private readonly CLockObject _smartArrowLock = new("SmartArrowTutorial");
		private readonly CVehicleDepotTutorialFunnel _analyticsFunnel;
		private readonly ISmartArrowLocker _smartArrowLocker;
		private readonly ICtsProvider _ctsProvider;

		public CVehicleDepotTutorial(
			ITutorialCommentator tutorialCommentator,
			CVehicleDepotTutorialFunnel analyticsFunnel,
			ISmartArrowLocker smartArrowLocker,
			CLazyActionQueue actionQueue, 
			CEventSystem eventSystem, 
			ICtsProvider ctsProvider, 
			IEventBus eventBus, 
			CUser user
		) 
			: base(eventBus, user, eventSystem, tutorialCommentator, actionQueue)
		{
			_smartArrowLocker = smartArrowLocker;
			_analyticsFunnel = analyticsFunnel;
			_ctsProvider = ctsProvider;
		}

		public void Initialize()
		{
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;
			EventBus.Subscribe<CDispatchForContractOpenedSignal>(DispatchForContractOpened);
		}

		private void DispatchForContractOpened(CDispatchForContractOpenedSignal signal)
		{
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;

			if (signal.Contract is CContract { StaticData: { ContractId: EStaticContractId._1930_Brickworks, Task: 2 } })
			{
				Run(_ctsProvider.Token).Forget();
			}
		}

		private bool IsCompleted()
		{
			return User.Tutorials.IsTutorialCompleted(EVehicleDepotTutorialStep.Completed);
		}

		private async UniTask Run(CancellationToken ct)
		{
			EventSystem.AddInputLocker(_inputLock);
			User.Tutorials.SetVehicleDepotTutorialStep(EVehicleDepotTutorialStep.Started);
			_analyticsFunnel.Send(EVehicleDepotFunnelStep.Started);
			_smartArrowLocker.AddLock(_smartArrowLock);
			await ShowNoVehiclesInfo(ct);
			await HighlightVehicleDepotButton(ct);
			await HighlightVehicleToBuy(ct);
			await HighlightBuyVehicle(ct);
			EventSystem.RemoveInputLocker(_inputLock);
			_smartArrowLocker.RemoveLock(_smartArrowLock);
		}
		
		private async UniTask ShowNoVehiclesInfo(CancellationToken ct)
		{
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(EventBus, ct);
			
			CGetUiRectResponse vehicleRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.VehicleInDispatchMenu));
			float highlightWidth = vehicleRect.RectTransform.rect.size.x;
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(vehicleRect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(highlightWidth * 3f - 150f, 50f)
					.SetAnchoredOffset(highlightWidth / 2f + 180f, 0f)
				;

			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(vehicleRect.RectTransform, "Tutorial.NoVehicles")
					.SetAnchoredOffset(550f, 270f)
					.SetSizeOffset(180f, 0f)
					.SetShowContinueButton(true)
				;
			
			EventBus.ProcessTask(highlightTask);
			EventBus.ProcessTask(tooltip);

			await CWaitForSignal.WaitForSignalAsync<CTutorialContinueClickedSignal>(EventBus, ct);
			_analyticsFunnel.Send(EVehicleDepotFunnelStep.ClickedOnContinue);
			
			DisableAllTutorialGraphics();
			
			EventBus.ProcessTask(new CCloseTopmostScreenTask());
			await CWaitForSignal.WaitForSignalAsync<CScreenCloseEndSignal>(EventBus, ct);
		}
		
		private async UniTask HighlightVehicleDepotButton(CancellationToken ct)
		{
			EventBus.ProcessTask(new CActivateVehicleDepotButtonTask());
			
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.VehicleDepotButton));
			
			CInputLock clickLock = new("ClickVehicleDepot", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickLock);
			
			await UniTask.WaitForSeconds(0.3f, cancellationToken: ct);
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(00f, 50f)
					.SetClockwiseArrowRotation(0f)
				;
			
			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(rect.RectTransform, "Tutorial.BuyVehicle")
					.SetAnchoredOffset(180f, 85f)
					.SetSide(ETutorialTooltipSide.Left)
				;
			
			EventBus.ProcessTask(showArrowTask);
			EventBus.ProcessTask(tooltip);
			
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(
				signal => signal.MenuId == (int) EScreenId.Depot, EventBus, ct);
			
			_analyticsFunnel.Send(EVehicleDepotFunnelStep.DepotOpened);
			
			EventSystem.RemoveInputLocker(clickLock);
			DisableAllTutorialGraphics();
		}

		private async UniTask HighlightVehicleToBuy(CancellationToken ct)
		{
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(
				signal => signal.MenuId == (int) EScreenId.Depot, EventBus, ct);
			
			CGetUiScrollRectResponse scrollRect = EventBus.ProcessTask<CGetUiScrollRectRequest, CGetUiScrollRectResponse>(new CGetUiScrollRectRequest(EUiScrollRect.VehicleDepotRegion1));
			scrollRect.ScrollRect.enabled = false;
			
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.TutorialDepotVehicle));
			
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(0f, 0f)
					.SetClockwiseArrowRotation(0f)
				;
			
			EventBus.ProcessTask(showArrowTask);
			
			CInputLock clickLock = new("ClickVehicleDepot", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickLock);
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(
				signal => signal.MenuId == (int) EScreenId.VehicleDetail, EventBus, ct);
			_analyticsFunnel.Send(EVehicleDepotFunnelStep.HighlightedVehicleClicked);
			EventSystem.RemoveInputLocker(clickLock);
			scrollRect.ScrollRect.enabled = true;
			
			DisableAllTutorialGraphics();
		}

		private async UniTask HighlightBuyVehicle(CancellationToken ct)
		{
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(
				signal => signal.MenuId == (int) EScreenId.VehicleDetail, EventBus, ct);
			
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.BuyVehicleButton));
			
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(-120f, 0f)
					.SetClockwiseArrowRotation(270f)
				;
			
			EventBus.ProcessTask(showArrowTask);
			
			CInputLock clickLock = new("ClickVehicleDepot", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickLock);
			await CWaitForSignal.WaitForSignalAsync<CVehicleBoughtSignal>(EventBus, ct);
			_analyticsFunnel.Send(EVehicleDepotFunnelStep.Completed);
			EventSystem.RemoveInputLocker(clickLock);
			
			User.Tutorials.SetVehicleDepotTutorialStep(EVehicleDepotTutorialStep.Completed);
			
			DisableAllTutorialGraphics();
		}
	}
}