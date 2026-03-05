// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.10.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;
using TycoonBuilder.Infrastructure;
using UnityEngine;

namespace TycoonBuilder
{
	public class CPlayerCityTutorial : CBaseTutorial, IInitializable
	{
		private readonly CInputLock _inputLock = new("CityTutorial", EInputLockLayer.Tutorial);
		
		private readonly CPlayerCityTutorialFunnel _analyticsFunnel;
		private readonly IGoToHandler _goToHandler;
		private readonly IServerTime _serverTime;

		public CPlayerCityTutorial(
			CPlayerCityTutorialFunnel analyticsFunnel, 
			ITutorialCommentator tutorialCommentator, 
			CLazyActionQueue actionQueue, 
			CEventSystem eventSystem, 
			IGoToHandler goToHandler, 
			IServerTime serverTime, 
			IEventBus eventBus, 
			CUser user
			) 
			: base(eventBus, user, eventSystem, tutorialCommentator, actionQueue)
		{
			_analyticsFunnel = analyticsFunnel;
			_goToHandler = goToHandler;
			_serverTime = serverTime;
		}
		
		public void Initialize()
		{
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;
			EventBus.Subscribe<CYearSeenSignal>(OnYearSeen);

			TryRun();
		}

		private void OnYearSeen(CYearSeenSignal signal)
		{
			TryRun();
		}

		private void TryRun()
		{
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;
			
			bool isCityUnlocked = User.IsUnlockRequirementMet(CDesignMainCityConfigs.UnlockRequirement);
			if (!isCityUnlocked)
				return;
			
			RunTutorialAction(Run);
		}

		private bool IsCompleted()
		{
			return User.Tutorials.IsTutorialCompleted(EPlayerCityTutorialStep.Completed);
		}
		
		private async UniTask Run(CancellationToken ct)
		{
			EventSystem.AddInputLocker(_inputLock);
			await _goToHandler.GoToRegionPoint(ERegionPoint.MainCity, ERegion.Region1, ct);
			await ShowSmartArrow(ct);
			_analyticsFunnel.Send(EPlayerCityFunnelStep.Started);

			await HighlightPassengers(ct);
			await HighlightUpgrade(ct);
			TryAddFuel();
			await GoToContracts(ct);
			await HighlightContract(ct);
			await HighlightFirstContractToDispatch(ct);
			EventSystem.RemoveInputLocker(_inputLock);
		}

		private async UniTask ShowSmartArrow(CancellationToken ct)
		{
			RectTransform rect = EventBus.ProcessTask<CGetMainCityNameTagRequest, RectTransform>();
			CInputLock waitForClickLock = new("Tutorial_WaitForClick", EInputLockLayer.Tutorial, rect);
			
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect)
				.SetAnchoredOffset(40f, 40f)
				.SetClockwiseArrowRotation(45f);
			EventBus.ProcessTask(showArrowTask);
			EventSystem.AddInputLocker(waitForClickLock);
			
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(signal => signal.MenuId == (int)EScreenId.City,EventBus, ct);
			DisableAllTutorialGraphics();
			EventSystem.RemoveInputLocker(waitForClickLock);
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(signal => signal.MenuId == (int)EScreenId.City,EventBus, ct);
		}
		
		private async UniTask HighlightPassengers(CancellationToken ct)
		{
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.CityMenuPassengers));
			
			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(rect.RectTransform, "Tutorial.City1")
					.SetAnchoredOffset(320f, 400f)
					.SetSide(ETutorialTooltipSide.Left)
					.SetShowContinueButton(true)
				;
			
			CGetUiRectResponse highlightRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.CityMenuManageHighlightRect));
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(highlightRect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(50f, 50f)
				;
			
			EventBus.ProcessTask(tooltip);
			EventBus.ProcessTask(highlightTask);

			await CWaitForSignal.WaitForSignalAsync<CTutorialContinueClickedSignal>(EventBus, ct);
			_analyticsFunnel.Send(EPlayerCityFunnelStep.PassengersContinueClicked);
			DisableAllTutorialGraphics();
		}
		
		private async UniTask HighlightUpgrade(CancellationToken ct)
		{
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.CityMenuManageButton));
			
			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(rect.RectTransform, "Tutorial.City2")
					.SetAnchoredOffset(-370f, -180f)
					.SetSide(ETutorialTooltipSide.Top)
					.SetShowContinueButton(true)
				;
			
			CGetUiRectResponse highlightRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.CityMenuPassengersHighlightRect));
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(highlightRect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(70f, 20f)
				;
			
			EventBus.ProcessTask(tooltip);
			EventBus.ProcessTask(highlightTask);

			await CWaitForSignal.WaitForSignalAsync<CTutorialContinueClickedSignal>(EventBus, ct);
			_analyticsFunnel.Send(EPlayerCityFunnelStep.ManageContinueClicked);
			DisableAllTutorialGraphics();
		}

		private async UniTask GoToContracts(CancellationToken ct)
		{
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.CityMenuContractsTabButton));

			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(60f, 0f)
					.SetClockwiseArrowRotation(90f)
				;
			
			EventBus.ProcessTask(showArrowTask);
			
			CInputLock clickLock = new("ClickRect", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickLock);
			
			await CWaitForSignal.WaitForSignalAsync<CCityContractsTabOpenedSignal>(EventBus, ct);
			_analyticsFunnel.Send(EPlayerCityFunnelStep.ContractsTabOpened);
			EventSystem.RemoveInputLocker(clickLock);
		}
		
		private async UniTask HighlightContract(CancellationToken ct)
		{
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.FirstPassengerContract));

			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(rect.RectTransform, "Tutorial.City3")
					.SetAnchoredOffset(580f, -430f)
					.SetSide(ETutorialTooltipSide.Left)
				;

			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(-80f, -280f)
					.SetClockwiseArrowRotation(-45f)
				;
			
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(rect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(50f, 50f)
				;

			EventBus.ProcessTask(tooltip);
			EventBus.ProcessTask(showArrowTask);
			EventBus.ProcessTask(highlightTask);
			
			CInputLock clickLock = new("ClickRect", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickLock);
			
			await CWaitForSignal.WaitForSignalAsync<CScreenCloseStartSignal>(EventBus, ct);
			_analyticsFunnel.Send(EPlayerCityFunnelStep.ContractClicked);
			EventSystem.RemoveInputLocker(clickLock);
			
			DisableAllTutorialGraphics();
		}
		
		private async UniTask HighlightFirstContractToDispatch(CancellationToken ct)
		{
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(signal => signal.MenuId == (int)EScreenId.Dispatch, EventBus, ct);
			
			CGetUiRectResponse vehicleRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.VehicleInDispatchMenu));

			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(vehicleRect.RectTransform, "Tutorial.City4")
					.SetAnchoredOffset(400f, -40f)
					.SetSide(ETutorialTooltipSide.Left)
				;

			EventBus.ProcessTask(tooltip);
			
			CInputLock clickLock = new("ClickRect", EInputLockLayer.Tutorial, vehicleRect.RectTransform);
			EventSystem.AddInputLocker(clickLock);
			
			await CWaitForSignal.WaitForSignalAsync<CVehicleDispatchedSignal>(EventBus, ct);
			_analyticsFunnel.Send(EPlayerCityFunnelStep.Completed);
			EventSystem.RemoveInputLocker(clickLock);
			User.Tutorials.SetPlayerCityTutorialStep(EPlayerCityTutorialStep.Completed);
			DisableAllTutorialGraphics();
		}
		
		private void TryAddFuel()
		{
			CContract passengerContract = User.Contracts.GetAllPassengerContracts()[0];
			CTripPrice fuelPrice = passengerContract.TripPrice;
			int neededFuel = User.Vehicles.GetFuelEfficiency(EVehicle.DouglasDC3, fuelPrice.FuelPriceValue);
			int actualFuelAmount = User.FuelStation.Recharger.CurrentAmount;
			if (actualFuelAmount >= neededFuel)
				return;
			
			int neededAmount = neededFuel - actualFuelAmount;
			long time = _serverTime.GetDayRefreshTimeInMs();
			User.FuelStation.Recharger.ModifyOverCapacity(neededAmount, time, null);
		}
	}
}