// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.10.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CDispatchCenterTutorial : CBaseTutorial, IInitializable
	{
		private readonly CDispatchCenterTutorialFunnel _analyticsFunnel;
		private readonly ISmartArrowLocker _smartArrowLocker;
		private readonly ICtsProvider _ctsProvider;
		private readonly IServerTime _serverTime;
		private readonly ICameraZoom _cameraZoom;
		private readonly CInputLock _inputLock = new("IntroTutorial", EInputLockLayer.Tutorial);

		public CDispatchCenterTutorial(
			CDispatchCenterTutorialFunnel analyticsFunnel,
			ITutorialCommentator tutorialCommentator,
			ISmartArrowLocker smartArrowLocker, 
			CLazyActionQueue actionQueue, 
			ICtsProvider ctsProvider, 
			CEventSystem eventSystem, 
			IServerTime serverTime, 
			ICameraZoom cameraZoom, 
			IEventBus eventBus, 
			CUser user
			) 
			: base(eventBus, user, eventSystem, tutorialCommentator, actionQueue)
		{
			_smartArrowLocker = smartArrowLocker;
			_analyticsFunnel = analyticsFunnel;
			_ctsProvider = ctsProvider;
			_serverTime = serverTime;
			_cameraZoom = cameraZoom;
		}

		public void Initialize()
		{
			EventBus.Subscribe<CDispatchToIndustryOpenedSignal>(OnDispatchToIndustryOpened);
		}

		public void TryStartOnGameLoad()
		{
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;

			bool coalMineCompleted = User.Contracts.IsContractCompleted(EStaticContractId._1930_CoalMine);
			if(!coalMineCompleted)
				return;
			
			bool resourceDispatchExists = ResourceDispatchExists();
			if(!resourceDispatchExists)
				return;
			
			RunTutorialAction(Run);
		}

		private void OnDispatchToIndustryOpened(CDispatchToIndustryOpenedSignal signal)
		{
			bool isCompleted = IsCompleted();
			if (isCompleted || signal.Industry != EIndustry.CoalMine)
				return;

			Run(_ctsProvider.Token).Forget();
		}

		private bool IsCompleted()
		{
			return User.Tutorials.IsTutorialCompleted(EDispatchCenterTutorialStep.Completed);
		}

		private async UniTask Run(CancellationToken ct)
		{
			EventSystem.AddInputLocker(_inputLock);
			CLockObject smartArrowLock = new("SmartArrowLock");
			_smartArrowLocker.AddLock(smartArrowLock);
			_analyticsFunnel.Send(EDispatchCenterFunnelStep.Started);

			bool resourceDispatchExists = ResourceDispatchExists();
			if (!resourceDispatchExists)
			{
				await HighlightVehicle(ct);
				await FollowVehicle(ct);
			}
			
			await HighlightDispatchCenterButton(ct);
			await HighlightFirstDispatch(ct);
			
			EventSystem.RemoveInputLocker(_inputLock);
			
			await UniTask.WaitForSeconds(0.5f, cancellationToken: ct);
			_smartArrowLocker.RemoveLock(smartArrowLock);
		}

		private bool ResourceDispatchExists()
		{
			return User.Dispatches.AnyResourceDispatchExists();
		}
		
		private async UniTask HighlightVehicle(CancellationToken ct)
		{
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(EventBus, ct);
			
			CGetUiRectResponse vehicleRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.VehicleInDispatchMenu));

			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(vehicleRect.RectTransform)
					.SetAnchoredOffset(0f, 70f)
					.SetClockwiseArrowRotation(0f)
				;

			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(vehicleRect.RectTransform, "Tutorial.SendVehicleForResource1")
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
			
			_analyticsFunnel.Send(EDispatchCenterFunnelStep.VehicleDispatched);
			EventSystem.RemoveInputLocker(clickOnVehicleLock);
			
			EventBus.ProcessTask(new CCloseTopmostScreenTask());
			
			DisableAllTutorialGraphics();
		}
		
		private async UniTask FollowVehicle(CancellationToken ct)
		{
			const float unlockDelayBeforeDispatchArrival = 4f;
			
			EventBus.ProcessTask(new CSetActiveLetterboxTask(true, true));
			
			COverrideZoomValue zoomOverride = new(1, 100f);
			_cameraZoom.AddZoomOverride(zoomOverride);
			
			CLockObject hudLock = new("HudLock");
			EventBus.ProcessTask(new CHudHideTask(hudLock));
			
			CInputLock followVehicleLock = new("FollowVehicleLock", EInputLockLayer.Tutorial);
			EventSystem.AddInputLocker(followVehicleLock);
			
			CDispatch dispatch = User.Dispatches.GetDispatchesForResource(EResource.Coal)[0];
			Transform vehicleTransform = await GetDispatchedVehicle(dispatch.VehicleId, ct);
			CSetCameraFollowTargetTask cameraFollowTask = new(vehicleTransform);
			EventBus.ProcessTask(cameraFollowTask);
			EventBus.ProcessTask(new CPlayVehicleFollowSoundTask(dispatch.VehicleId));

			long timeToTargetArrival = dispatch.CompletionTime - _serverTime.GetTimestampInMs();
			float followEndDuration = CMath.Floor(timeToTargetArrival / (float) CTimeConst.Second.InMilliseconds) - unlockDelayBeforeDispatchArrival;
			await UniTask.WaitForSeconds(followEndDuration, cancellationToken: ct);
			
			EventBus.ProcessTask(new CHudShowTask(hudLock));
			EventBus.ProcessTask(new CStopActiveVehicleFollowSoundTask());
			EventBus.ProcessTask(new CSetCameraFollowTargetTask(null));
			
			EventBus.ProcessTask(new CSetActiveLetterboxTask(false, true));
			await UnZoomCamera(zoomOverride, ct);
			_analyticsFunnel.Send(EDispatchCenterFunnelStep.FollowCompleted);
			_cameraZoom.RemoveZoomOverride(zoomOverride);
			
			EventSystem.RemoveInputLocker(followVehicleLock);
		}
		
		private async UniTask UnZoomCamera(COverrideZoomValue zoomValue, CancellationToken ct)
		{
			float velocity = 0f;
			float duration = 0.2f;
			float time = 0f;
			while (time < duration)
			{
				time += Time.deltaTime;
				zoomValue.ZoomValue = Mathf.SmoothDamp(zoomValue.ZoomValue, _cameraZoom.CurrentAbsoluteZoomValue, ref velocity, duration);
				await UniTask.Yield(ct);
			}
		}

		private async UniTask HighlightDispatchCenterButton(CancellationToken ct)
		{
			EventBus.ProcessTask(new CActivateDispatchCenterButtonTask());
			
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.DispatchCentralMenuButton));

			CInputLock clickOnVehicleLock = new("ClickDispatchCenter", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickOnVehicleLock);

			await UniTask.WaitForSeconds(0.3f, cancellationToken: ct);
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(10f, 50f)
					.SetClockwiseArrowRotation(0)
				;
			
			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(rect.RectTransform, "Tutorial.SendVehicleForResource2")
					.SetAnchoredOffset(470f, 80f)
					.SetSizeOffset(300f, 0f)
					.SetSide(ETutorialTooltipSide.Left)
				;
			
			EventBus.ProcessTask(showArrowTask);
			EventBus.ProcessTask(tooltip);

			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(
				signal => signal.MenuId == (int) EScreenId.DispatchCenter, EventBus, ct);
			_analyticsFunnel.Send(EDispatchCenterFunnelStep.DispatchCenterOpened);
			
			EventSystem.RemoveInputLocker(clickOnVehicleLock);
			
			DisableAllTutorialGraphics();
			
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(
				signal => signal.MenuId == (int) EScreenId.DispatchCenter, EventBus, ct);
		}

		private async UniTask HighlightFirstDispatch(CancellationToken ct)
		{
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.DispatchInDispatchCentralMenu));

			CInputLock waitForArrivalLock = new("WaitForArrivalLock", EInputLockLayer.Tutorial);
			EventSystem.AddInputLocker(waitForArrivalLock);

			CDispatch runningResourceDispatch = User.Dispatches.GetDispatchesForResource(EResource.Coal)[0];
			long timeToTargetArrival = runningResourceDispatch.CompletionTime - _serverTime.GetTimestampInMs();
			float timeToArrivalInSeconds = CMath.Ceil(timeToTargetArrival / (float) CTimeConst.Second.InMilliseconds);

			if (timeToArrivalInSeconds > 0f)
			{
				await UniTask.WaitForSeconds(timeToArrivalInSeconds, cancellationToken: ct);
			}
			
			EventSystem.RemoveInputLocker(waitForArrivalLock);
			
			CInputLock clickOnVehicleLock = new("ClickResourceDispatchCollect", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickOnVehicleLock);
			
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(0f, 20f)
					.SetClockwiseArrowRotation(0f)
				;
			
			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(rect.RectTransform, "Tutorial.SendVehicleForResource3")
					.SetAnchoredOffset(-400f, -50f)
					.SetSide(ETutorialTooltipSide.Top)
				;
			
			EventBus.ProcessTask(showArrowTask);
			EventBus.ProcessTask(tooltip);
			
			CGetUiScrollRectResponse scrollRect = EventBus.ProcessTask<CGetUiScrollRectRequest, CGetUiScrollRectResponse>(new CGetUiScrollRectRequest(EUiScrollRect.DispatchCenterMenuSection));
			scrollRect.ScrollRect.enabled = false;
			
			await CWaitForSignal.WaitForSignalAsync<CResourceDispatchCollectedSignal>(EventBus, ct);
			
			EventBus.ProcessTask(new CActivateWarehouseButtonTask());
			
			scrollRect.ScrollRect.enabled = true;
			User.Tutorials.SetDispatchCenterStep(EDispatchCenterTutorialStep.Completed);
			_analyticsFunnel.Send(EDispatchCenterFunnelStep.Completed);
			EventBus.ProcessTask(new CCloseTopmostScreenTask());
			DisableAllTutorialGraphics();
			await CWaitForSignal.WaitForSignalAsync<CScreenCloseEndSignal>(EventBus, ct);
			
			EventSystem.RemoveInputLocker(clickOnVehicleLock);
		}
	}
}