// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

namespace TycoonBuilder
{
	public class CIntroTutorial : CBaseTutorial, IInitializable
	{
		private readonly ISmartArrowLocker _smartArrowLocker;
		private readonly CIntroTutorialAnalytics _analytics;
		private readonly IDialogueHandler _dialogueHandler;
		private readonly IGoToHandler _goToHandler;
		private readonly ICameraZoom _cameraZoom;
		private readonly IServerTime _serverTime;

		private readonly CInputLock _inputLock = new("IntroTutorial", EInputLockLayer.Tutorial);
		private readonly CLockObject _hudLockObject = new("IntroTutorial_HudLock");
		private readonly COverrideZoomValue _overrideZoom = new(10, CDesignCameraConfig.DefaultZoomInTutorial);

		public CIntroTutorial(
			ITutorialCommentator tutorialCommentator,
			CIntroTutorialAnalytics analytics, 
			ISmartArrowLocker smartArrowLocker, 
			IDialogueHandler dialogueHandler, 
			CLazyActionQueue actionQueue, 
			IGoToHandler goToHandler, 
			CEventSystem eventSystem, 
			ICameraZoom cameraZoom, 
			IServerTime serverTime, 
			IEventBus eventBus, 
			CUser user
			) : base(eventBus, user, eventSystem, tutorialCommentator, actionQueue)
		{
			_dialogueHandler = dialogueHandler;
			_smartArrowLocker = smartArrowLocker;
			_goToHandler = goToHandler;
			_cameraZoom = cameraZoom;
			_serverTime = serverTime;
			_analytics = analytics;
		}

		public void Initialize()
		{
			TryDisableInitialUI();
			TryRegisterTutorialSignals();
		}

		private void TryRegisterTutorialSignals()
		{
			bool shouldRun = IsCompleted();
			if (shouldRun)
				return;

			EventBus.Subscribe<CStaticContractCompletedSignal>(OnStoryContractCompleted);
		}

		private void OnStoryContractCompleted(CStaticContractCompletedSignal signal)
		{
			if(((CContract)signal.Contract).StaticData.ContractId != EStaticContractId._1930_StonesOnTheRoad)
				return;
			
			User.Tutorials.SetIntroStep(EIntroTutorialStep.FirstContractCompleted);
		}

		private void TryDisableInitialUI()
		{
			bool shouldRun = User.Tutorials.IsTutorialCompleted(EIntroTutorialStep.FirstContractCompleted);
			if (shouldRun)
				return;
			
			EventBus.ProcessTask(new CHudHideTask(_hudLockObject, false, true));
			EventBus.ProcessTask(new CAddFloatingWindowsBlockerTask(_hudLockObject));
		}

		public async UniTask TryRunAsync(CancellationToken ct)
		{
			bool shouldRun = IsCompleted();
			if (shouldRun)
				return;

			await RunAsync(ct);
			RunSmartArrowsTutorial(ct).Forget();
		}
		
		private async UniTask RunAsync(CancellationToken ct)
		{
			CIntroTutorialSceneReferences sceneReferences = EventBus.ProcessTask<CGetIntroTutorialSceneReferencesRequest, CGetIntroTutorialSceneReferencesResponse>(
				new CGetIntroTutorialSceneReferencesRequest()).References;
			
			await HandleFirstContract(sceneReferences, ct);
		}
		
		private async UniTask RunSmartArrowsTutorial(CancellationToken ct)
		{
			await HandleSmartArrows(ct);
			User.Tutorials.SetIntroStep(EIntroTutorialStep.Completed);
		}

		private async UniTask HandleFirstContract(CIntroTutorialSceneReferences sceneReferences, CancellationToken ct)
		{
			EIntroTutorialStep currentState = User.Tutorials.GetIntroStep();
			if (currentState >= EIntroTutorialStep.FirstContractCompleted)
				return;
			
			_analytics.Send(EFtueFunnelStep.TutorialStarted);
			
			EventSystem.AddInputLocker(_inputLock);
			
			_overrideZoom.ZoomValue = CDesignCameraConfig.DefaultZoomInTutorial;
			_cameraZoom.AddZoomOverride(_overrideZoom);
			
			if (currentState == EIntroTutorialStep.None)
			{
				bool dispatchIsRunning = User.Dispatches.IsAnyVehicleDispatched();
				bool contractReadyToClaim = User.Contracts.IsContractReadyToClaim(EStaticContractId._1930_StonesOnTheRoad);
				if (!dispatchIsRunning && !contractReadyToClaim)
				{
					sceneReferences.StonesOnRoadAnimation.PrepareStonesOnRoadAnimation();

					await _goToHandler.GoToRegionPoint(ERegionPoint.StonesOnTheRoad, ERegion.Region1, ct);
					await sceneReferences.StonesOnRoadAnimation.PlayStonesOnRoadAnimation(ct);
					_analytics.Send(EFtueFunnelStep.StonesOnRoadAnimationSeen);
			
					EventBus.ProcessTask(new CRemoveFloatingWindowsBlockerTask(_hudLockObject));
			
					await WaitForClickOnNameTag(EStaticContractId._1930_StonesOnTheRoad, ct);
					sceneReferences.StonesOnRoadAnimation.ReleaseFirstTaskTutorialCamera();
			
					await HighlightVehicle(ct);
					await FollowVehicle(sceneReferences, ct);
				}
				
				EventBus.ProcessTask(new CRemoveFloatingWindowsBlockerTask(_hudLockObject));
				await _goToHandler.GoToRegionPoint(ERegionPoint.StonesOnTheRoad, ERegion.Region1, ct);
				await HighlightContractNameTag(EStaticContractId._1930_StonesOnTheRoad, ct);
			}

			EventBus.ProcessTask(new CHudShowTask(_hudLockObject, false, true));
			EventSystem.RemoveInputLocker(_inputLock);

			_cameraZoom.SetAbsoluteCameraZoom(CDesignCameraConfig.DefaultZoomInTutorial);
			_cameraZoom.RemoveZoomOverride(_overrideZoom);

			await CWaitForSignal.WaitForSignalAsync<CStoryContractTierCompletedSignal>(EventBus, ct);
			_analytics.Send(EFtueFunnelStep.FirstContractCollectContract);
			
			await CWaitForSignal.WaitForSignalAsync<CStoryContractRewardsClaimedSignal>(
				signal => ((CContract)signal.Contract).StaticData.ContractId == EStaticContractId._1930_StonesOnTheRoad, EventBus, ct);
			_analytics.Send(EFtueFunnelStep.FirstContractClaimReward);
		}

		private async UniTask HandleSmartArrows(CancellationToken ct)
		{
			await TryPlayBrickworks1(ct);
			await TryPlayBrickworks2(ct);
			await ShowSmartArrow(new SStaticContractPointer(EStaticContractId._1930_CoalMine, 1), ct);
			await ShowSmartArrow(new SStaticContractPointer(EStaticContractId._1930_SmallPowerPlant, 1), ct);
		}

		private async UniTask TryPlayBrickworks1(CancellationToken ct)
		{
			SStaticContractPointer contractPointer = new(EStaticContractId._1930_Brickworks, 1);
			bool isCompleted = User.Contracts.IsContractCompleted(contractPointer);
			if (isCompleted)
				return;
			
			await ShowSmartArrow(contractPointer, ct);
			await TutorialCommentator.ShowCommentator(ITutorialCommentator.ESide.Right, "Tutorial.Commentator.Brickworks1_1", true, ct);
			await TutorialCommentator.Hide(ct);
		}

		private async UniTask TryPlayBrickworks2(CancellationToken ct)
		{
			SStaticContractPointer contractPointer = new(EStaticContractId._1930_Brickworks, 2);
			bool isCompleted = User.Contracts.IsContractCompleted(contractPointer);
			if (isCompleted)
				return;
			
			await ShowSmartArrow(contractPointer, ct);
			await TutorialCommentator.ShowCommentator(ITutorialCommentator.ESide.Right, "Tutorial.Commentator.Brickworks2_1", true, ct);
			await TutorialCommentator.ShowText("Tutorial.Commentator.Brickworks2_2", true, ct);
			await TutorialCommentator.Hide(ct);
			
			_cameraZoom.SetTargetZoom(1f);
		}

		private async UniTask ShowSmartArrow(SStaticContractPointer contractPointer, CancellationToken ct)
		{
			bool isCompleted = User.Contracts.IsContractCompleted(contractPointer);
			if (isCompleted)
				return;
			
			await UniTask.WaitForSeconds(1f, cancellationToken: ct);
			CSmartArrowOverContract smartArrow = new(TutorialCommentator, _smartArrowLocker, _dialogueHandler, EventBus, User);
			smartArrow.Show(contractPointer, ct).Forget();
			await CWaitForSignal.WaitForSignalAsync<CStoryContractRewardsClaimedSignal>(
				signal => ((CContract)signal.Contract).StaticData.ContractId == contractPointer.Id, EventBus, ct);
			
			await CWaitForSignal.WaitForSignalAsync<CScreenCloseEndSignal>(EventBus, ct);
		}

		private async UniTask HighlightContractInfo(CancellationToken ct)
		{
			CGetUiRectResponse contractInfoRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.ContractInfoRect));
			
			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(contractInfoRect.RectTransform, "Tutorial.ContractInfo")
					.SetAnchoredOffset(530f, -220f)
					.SetSide(ETutorialTooltipSide.Left)
					.SetShowContinueButton(true)
				;
			
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(contractInfoRect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(50f, 50f)
				;
			
			EventBus.ProcessTask(tooltip);
			EventBus.ProcessTask(highlightTask);
			
			await CWaitForSignal.WaitForSignalAsync<CTutorialContinueClickedSignal>(EventBus, ct);
			_analytics.Send(EFtueFunnelStep.FirstContractClickContinue);
			DisableAllTutorialGraphics();
		}

		/// Volba poslání prvního vozidla
		private async UniTask HighlightVehicle(CancellationToken ct)
		{
			await HighlightContractInfo(ct);
			
			CGetUiRectResponse vehicleRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.VehicleInDispatchMenu));

			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(vehicleRect.RectTransform)
				.SetAnchoredOffset(40f, 50f)
				.SetClockwiseArrowRotation(20f)
				;

			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(vehicleRect.RectTransform, "Tutorial.SendTruck")
				.SetAnchoredOffset(400f, -40f)
				.SetSide(ETutorialTooltipSide.Left)
				;

			EventBus.ProcessTask(showArrowTask);
			EventBus.ProcessTask(tooltip);

			CInputLock clickOnVehicleLock = new("ClickOnVehicleLock", EInputLockLayer.Tutorial, vehicleRect.RectTransform);
			EventSystem.AddInputLocker(clickOnVehicleLock);
			
			CGetUiScrollRectResponse scrollRect = EventBus.ProcessTask<CGetUiScrollRectRequest, CGetUiScrollRectResponse>(new CGetUiScrollRectRequest(EUiScrollRect.DispatchMenuVehicleSection));
			scrollRect.ScrollRect.enabled = false;
			
			await CWaitForSignal.WaitForSignalAsync<CVehicleDispatchedSignal>(EventBus, ct);
			scrollRect.ScrollRect.enabled = true;
			
			_analytics.Send(EFtueFunnelStep.FirstContractStartContract);
			EventSystem.RemoveInputLocker(clickOnVehicleLock);
			EventBus.ProcessTask(new CCloseTopmostScreenTask());
			
			DisableAllTutorialGraphics();
		}
		
		private async UniTask FollowVehicle(CIntroTutorialSceneReferences sceneReferences, CancellationToken ct)
		{
			_overrideZoom.ZoomValue = CDesignCameraConfig.FirstContractCarCameraZoom;
			
			EventBus.ProcessTask(new CSetActiveLetterboxTask(true, true));
			
			CDispatch dispatch = User.Dispatches.GetFirstDispatchForContract(EStaticContractId._1930_StonesOnTheRoad);
			Transform vehicleTransform = await GetDispatchedVehicle(dispatch.VehicleId, ct);
			CSetCameraFollowTargetTask cameraFollowTask = new(vehicleTransform);
			EventBus.ProcessTask(cameraFollowTask);
			EventBus.ProcessTask(new CPlayVehicleFollowSoundTask(dispatch.VehicleId));

			CInputLock followVehicleLock = new("FollowVehicleLock", EInputLockLayer.Tutorial);
			EventSystem.AddInputLocker(followVehicleLock);

			long remainingTime = dispatch.TripCompletionTime - _serverTime.GetTimestampInMs() - CTimeConst.Second.InMilliseconds;
			if (remainingTime > 0)
			{
				await UniTask.Delay((int)remainingTime, cancellationToken: ct);
			}
			
			_analytics.Send(EFtueFunnelStep.FirstContractAnimationFinish);
			
			EventSystem.RemoveInputLocker(followVehicleLock);

			EventBus.ProcessTask(new CStopActiveVehicleFollowSoundTask());
			EventBus.ProcessTask(new CSetCameraFollowTargetTask(null));
			EventBus.ProcessTask(new CSetActiveLetterboxTask(false, true));

			sceneReferences.StonesOnRoadCenterViewCamera.enabled = true;
			await UniTask.WaitForSeconds(0.6f, cancellationToken: ct);
			
			_overrideZoom.ZoomValue = CDesignCameraConfig.DefaultZoomInTutorial;
			_goToHandler.GoToRegionPointInstant(ERegionPoint.StonesOnTheRoad, ERegion.Region1);
			sceneReferences.StonesOnRoadCenterViewCamera.enabled = false;
		}
		
		/// <summary>
		/// První kontrakt tutorialu - Sipka na nametag "kameny na ceste"
		/// </summary>
		private async UniTask WaitForClickOnNameTag(EStaticContractId contractId, CancellationToken ct)
		{
			RectTransform nameTagRect = EventBus.ProcessTask<CGetNameTagContractRectRequest, RectTransform>(new CGetNameTagContractRectRequest(contractId));
			CInputLock waitForClickLock = new("IntroTutorial_WaitForClick", EInputLockLayer.Tutorial, nameTagRect);
			
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(nameTagRect)
					.SetAnchoredOffset(40f, 40f)
					.SetClockwiseArrowRotation(45f);
			EventBus.ProcessTask(showArrowTask);
			
			EventSystem.AddInputLocker(waitForClickLock);
			
			await CWaitForSignal.WaitForSignalAsync<COpenDispatchMenuTriggeredSignal>(EventBus, ct);
			_analytics.Send(EFtueFunnelStep.StonesOnRoadContractClicked);
			DisableAllTutorialGraphics();
			await WaitForMenuOpenEnd(EScreenId.Dispatch, ct);
			EventSystem.RemoveInputLocker(waitForClickLock);
		}

		private async UniTask HighlightContractNameTag(EStaticContractId contractId, CancellationToken ct)
		{
			RectTransform nameTagRect = EventBus.ProcessTask<CGetNameTagContractRectRequest, RectTransform>(new CGetNameTagContractRectRequest(contractId));
			
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(nameTagRect)
				.SetAnchoredOffset(40f, 40f)
				.SetClockwiseArrowRotation(45f);
			EventBus.ProcessTask(showArrowTask);
			
			CInputLock waitForClickLock = new("IntroTutorial_WaitForClick", EInputLockLayer.Tutorial, nameTagRect);
			EventSystem.AddInputLocker(waitForClickLock);
			await WaitForContractMenuOpenStart(ct);
			DisableAllTutorialGraphics();
			await WaitForMenuOpenEnd(EScreenId.Dispatch, ct);
			EventSystem.RemoveInputLocker(waitForClickLock);
		}

		private async UniTask WaitForContractMenuOpenStart(CancellationToken ct)
		{
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(screen => screen.MenuId == (int) EScreenId.Dispatch, EventBus, ct);
		}
		
		private bool IsCompleted()
		{
			return User.Tutorials.IsTutorialCompleted(EIntroTutorialStep.Completed);
		}
	}
}