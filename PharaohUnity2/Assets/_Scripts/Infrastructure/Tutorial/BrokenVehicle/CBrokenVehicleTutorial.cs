// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.11.2025
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
	public class CBrokenVehicleTutorial : CBaseTutorial, IInitializable
	{
		private readonly CInputLock _inputLock = new("Tutorial", EInputLockLayer.Tutorial);
		private readonly CBrokenVehicleTutorialFunnel _analyticsFunnel;
		private readonly CDesignVehicleConfigs _designVehicleConfigs;
		private readonly ICtsProvider _ctsProvider;
		private readonly IServerTime _serverTime;
		private bool _tutorialRunning;

		public CBrokenVehicleTutorial(
			CBrokenVehicleTutorialFunnel analyticsFunnel,
			CDesignVehicleConfigs designVehicleConfigs,
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
			_designVehicleConfigs = designVehicleConfigs;
			_analyticsFunnel = analyticsFunnel;
			_ctsProvider = ctsProvider;
			_serverTime = serverTime;
		}

		public void Initialize()
		{
			bool isTutorialCompleted = IsTutorialCompleted();
			if(isTutorialCompleted)
				return;
			
			EventBus.Subscribe<CDamagedVehicleShownSignal>(OnDamagedVehicleShown);
		}

		private void OnDamagedVehicleShown(CDamagedVehicleShownSignal signal)
		{
			bool canRun = CanRun();
			if (!canRun) 
				return;
			
			Run(1f, _ctsProvider.Token).Forget();
		}
		
		private bool IsTutorialCompleted()
		{
			return User.Tutorials.IsTutorialCompleted(EBrokenVehicleTutorialStep.Completed);
		}

		private bool CanRun()
		{
			if (_tutorialRunning)
				return false;
			
			bool isTutorialCompleted = IsTutorialCompleted();
			return !isTutorialCompleted;
		}

		private async UniTaskVoid Run(float delay, CancellationToken ct)
		{
			_tutorialRunning = true;
			EventSystem.AddInputLocker(_inputLock);
			await UniTask.WaitForSeconds(delay, cancellationToken: ct);
			await HighlightBrokenVehicleButton(ct);
			await PlayRepairTutorial(ct);
			await ShowDurabilityInfo(ct);
			EventSystem.RemoveInputLocker(_inputLock);
			_tutorialRunning = false;
		}
		
		private async UniTask HighlightBrokenVehicleButton(CancellationToken ct)
		{
			_analyticsFunnel.Send(EBrokenVehicleFunnelStep.Started);
			
			CGetUiRectResponse vehicleRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.BrokenVehicleRect));
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(vehicleRect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(50f, 50f)
				;

			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(vehicleRect.RectTransform, "Tutorial.BrokenVehicle1")
					.SetAnchoredOffset(-80f, 320f)
					.SetSide(ETutorialTooltipSide.Bottom)
				;

			EventBus.ProcessTask(highlightTask);
			EventBus.ProcessTask(tooltip);
			
			CInputLock inputLock = new("BrokenVehicleTutorial_Lock", EInputLockLayer.Tutorial, vehicleRect.RectTransform);
			EventSystem.AddInputLocker(inputLock);
			
			CBeforeVehicleDetailOpenStartSignal signal = await CWaitForSignal.WaitForSignalAsync<CBeforeVehicleDetailOpenStartSignal>(EventBus, ct);
			
			_analyticsFunnel.Send(EBrokenVehicleFunnelStep.VehicleDetailOpened);
			TryAddResources(signal.VehicleToShow);
			
			EventSystem.RemoveInputLocker(inputLock);
			DisableAllTutorialGraphics();
		}

		private async UniTask PlayRepairTutorial(CancellationToken ct)
		{
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(EventBus, ct);
			await HighlightVehicleDurabilityInfo(ct);
			await RepairVehicle(ct);
		}

		private async UniTask HighlightVehicleDurabilityInfo(CancellationToken ct)
		{
			CGetUiRectResponse vehicleRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.VehicleDurabilityInfo));
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(vehicleRect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(110f, 150f)
					.SetAnchoredOffset(-30f, 30f)
				;

			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(vehicleRect.RectTransform, "Tutorial.BrokenVehicle2")
					.SetAnchoredOffset(30f, 405f)
					.SetSide(ETutorialTooltipSide.Bottom)
					.SetShowContinueButton(true)
				;
			
			EventBus.ProcessTask(highlightTask);
			EventBus.ProcessTask(tooltip);
			
			await CWaitForSignal.WaitForSignalAsync<CTutorialContinueClickedSignal>(EventBus, ct);
			_analyticsFunnel.Send(EBrokenVehicleFunnelStep.ContinueClicked);
		}
		
		private async UniTask RepairVehicle(CancellationToken ct)
		{
			CGetUiRectResponse vehicleRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.VehicleRepairButton));
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(vehicleRect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(400f, 120f)
					.SetAnchoredOffset(-130f, 30f)
				;

			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(vehicleRect.RectTransform, "Tutorial.BrokenVehicle3")
					.SetAnchoredOffset(-335f, 315f)
					.SetSide(ETutorialTooltipSide.Bottom)
				;
			
			EventBus.ProcessTask(highlightTask);
			EventBus.ProcessTask(tooltip);
			
			CInputLock inputLock = new("BrokenVehicleTutorial_Lock", EInputLockLayer.Tutorial, vehicleRect.RectTransform);
			EventSystem.AddInputLocker(inputLock);
			
			await CWaitForSignal.WaitForSignalAsync<CVehicleRepairedSignal>(EventBus, ct);
			DisableAllTutorialGraphics();
			_analyticsFunnel.Send(EBrokenVehicleFunnelStep.VehicleRepaired);

			await CWaitForSignal.WaitForSignalAsync<CScreenCloseStartSignal>(EventBus, ct);

			User.Tutorials.SetBrokenVehicleTutorialStep(EBrokenVehicleTutorialStep.Completed);
			EventSystem.RemoveInputLocker(inputLock);
			DisableAllTutorialGraphics();
			
			await CWaitForSignal.WaitForSignalAsync<CScreenCloseEndSignal>(EventBus, ct);
		}

		private async UniTask ShowDurabilityInfo(CancellationToken ct)
		{
			CGetUiRectResponse vehicleRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(EUiRect.DurabilityContractInfoRect));
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(vehicleRect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(50f, 50f)
				;

			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(vehicleRect.RectTransform, "Tutorial.BrokenVehicle4")
					.SetAnchoredOffset(175f, 83f)
					.SetSide(ETutorialTooltipSide.Left)
					.SetShowContinueButton(true)
				;
			
			EventBus.ProcessTask(highlightTask);
			EventBus.ProcessTask(tooltip);
			
			await CWaitForSignal.WaitForSignalAsync<CTutorialContinueClickedSignal>(EventBus, ct);
			_analyticsFunnel.Send(EBrokenVehicleFunnelStep.Completed);
			DisableAllTutorialGraphics();
		}
		
		private void TryAddResources(EVehicle vehicle)
		{
			COwnedVehicle vehicleData = User.Vehicles.GetVehicle(vehicle);
			int missingDurability = vehicleData.GetMissingDurability(_serverTime.GetTimestampInMs());
			IValuable[] price = _designVehicleConfigs.GetRepairPrice(missingDurability, User.Progress.Region);

			foreach (IValuable valuable in price)
			{
				User.OwnedValuables.ForceToHaveAtLeastSomeValuables(valuable);
			}
		}
	}
}