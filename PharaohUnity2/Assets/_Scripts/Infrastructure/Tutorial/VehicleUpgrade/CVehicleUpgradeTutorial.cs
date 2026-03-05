// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.12.2025
// =========================================

using System.Collections.Generic;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;
using UnityEngine;

namespace TycoonBuilder
{
	public class CVehicleUpgradeTutorial : CBaseTutorial, IInitializable
	{
		private readonly CInputLock _inputLock = new("Tutorial", EInputLockLayer.Tutorial);
		private readonly CDesignStoryContractConfigs _storyContractConfigs;
		private readonly CUpgradeVehicleTutorialFunnel _analyticsFunnel;
		private readonly CDesignVehicleConfigs _designVehicleConfigs;

		public CVehicleUpgradeTutorial(
			CDesignStoryContractConfigs storyContractConfigs,
			CUpgradeVehicleTutorialFunnel analyticsFunnel, 
			CDesignVehicleConfigs designVehicleConfigs, 
			ITutorialCommentator tutorialCommentator, 
			CLazyActionQueue actionQueue, 
			CEventSystem eventSystem, 
			IEventBus eventBus, 
			CUser user
			) 
			: base(eventBus, user, eventSystem, tutorialCommentator, actionQueue)
		{
			_designVehicleConfigs = designVehicleConfigs;
			_storyContractConfigs = storyContractConfigs;
			_analyticsFunnel = analyticsFunnel;
		}

		public void Initialize()
		{
			bool isTutorialCompleted = IsTutorialCompleted();
			if(isTutorialCompleted)
				return;
			
			EventBus.Subscribe<CStoryContractRewardsClaimedSignal>(OnContractClaimed);
			TryStartInLoad();
		}

		private void TryStartInLoad()
		{
			bool contractCompleted = User.Contracts.IsContractCompleted(EStaticContractId._1932_UpgradeWorkshop);
			if (!contractCompleted)
				return;
			
			bool canRun = CanRun();
			if (!canRun)
				return;
			
			RunTutorialAction(Run);
		}

		private void OnContractClaimed(CStoryContractRewardsClaimedSignal signal)
		{
			CContract contract = (CContract)signal.Contract;
			if(contract.StaticData.ContractId != EStaticContractId._1932_UpgradeWorkshop)
				return;

			if(!contract.StaticData.IsLastTask)
				return;
			
			if (!CanRun()) 
				return;
			
			RunTutorialAction(Run);
		}
		
		private bool IsTutorialCompleted()
		{
			return User.Tutorials.IsTutorialCompleted(EVehicleUpgradeTutorialStep.Completed);
		}

		private bool CanRun()
		{
			bool isTutorialCompleted = IsTutorialCompleted();
			return !isTutorialCompleted;
		}

		private async UniTask Run(CancellationToken ct)
		{
			EventSystem.AddInputLocker(_inputLock);
			
			_analyticsFunnel.Send(EUpgradeVehicleFunnelStep.Started);
			await HighlightVehicleDepotButton(ct);
			await HighlightVehicleToUpgrade(ct);
			await HighlightCurrencies(ct);
			await UpgradeVehicle(ct);
			ShowHelp();
			EventSystem.RemoveInputLocker(_inputLock);
		}
		
		private async UniTask HighlightVehicleDepotButton(CancellationToken ct)
		{
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.VehicleDepotButton));
			
			await UniTask.WaitForSeconds(0.3f, cancellationToken: ct);
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(00f, 50f)
				;
			
			EventBus.ProcessTask(showArrowTask);
			
			await TutorialCommentator.ShowCommentator(ITutorialCommentator.ESide.Right, "Tutorial.VehicleUpgrade1", false, ct);
			
			CInputLock clickLock = new("ClickVehicleDepot", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickLock);
			
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(
				signal => signal.MenuId == (int) EScreenId.Depot, EventBus, ct);
			
			_analyticsFunnel.Send(EUpgradeVehicleFunnelStep.VehicleDepoOpened);
			
			TryAddResources();
			DisableAllTutorialGraphics();
			TutorialCommentator.Hide(ct).Forget();
			EventSystem.RemoveInputLocker(clickLock);
		}
		
		private async UniTask HighlightVehicleToUpgrade(CancellationToken ct)
		{
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(
				signal => signal.MenuId == (int) EScreenId.Depot, EventBus, ct);
			
			CGetUiScrollRectResponse scrollRect = EventBus.ProcessTask<CGetUiScrollRectRequest, CGetUiScrollRectResponse>(new CGetUiScrollRectRequest(EUiScrollRect.VehicleDepotRegion1));
			scrollRect.ScrollRect.enabled = false;
			
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.VehicleToUpgradeRect));
			
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(0f, -80f)
					.SetClockwiseArrowRotation(180f)
				;
			
			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(rect.RectTransform, "Tutorial.VehicleUpgrade2")
					.SetAnchoredOffset(410f, -145f)
					.SetSide(ETutorialTooltipSide.Left)
				;
			
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(rect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(40f, 40f)
				;

			EventBus.ProcessTask(highlightTask);
			EventBus.ProcessTask(showArrowTask);
			EventBus.ProcessTask(tooltip);
			
			CInputLock clickLock = new("ClickVehicleDepot", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickLock);
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(
				signal => signal.MenuId == (int) EScreenId.VehicleDetail, EventBus, ct);
			
			_analyticsFunnel.Send(EUpgradeVehicleFunnelStep.VehicleDetailOpened);
			
			EventSystem.RemoveInputLocker(clickLock);
			scrollRect.ScrollRect.enabled = true;
			
			DisableAllTutorialGraphics();
		}
		
		private async UniTask HighlightCurrencies(CancellationToken ct)
		{
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.CurrenciesRect));
			
			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(rect.RectTransform, "Tutorial.VehicleUpgrade3")
					.SetAnchoredOffset(-1120f, -150f)
					.SetSide(ETutorialTooltipSide.Top)
					.SetShowContinueButton(true)
				;
			
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(rect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(-300f, 30f)
					.SetAnchoredOffset(-310f, 0f)
				;
			
			EventBus.ProcessTask(tooltip);
			EventBus.ProcessTask(highlightTask);

			await CWaitForSignal.WaitForSignalAsync<CTutorialContinueClickedSignal>(EventBus, ct);
			_analyticsFunnel.Send(EUpgradeVehicleFunnelStep.CurrencyContinueClicked);
			
			DisableAllTutorialGraphics();
		}
		
		private async UniTask UpgradeVehicle(CancellationToken ct)
		{
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.VehicleUpgradeBlock));
			
			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(rect.RectTransform, "Tutorial.VehicleUpgrade4")
					.SetAnchoredOffset(-840f, -40f)
					.SetSide(ETutorialTooltipSide.Right)
				;
			
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(rect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(80f, 70f)
					.SetAnchoredOffset(0f, 0f)
				;
			
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(100f, -40f)
					.SetClockwiseArrowRotation(180f)
				;

			EventBus.ProcessTask(tooltip);
			EventBus.ProcessTask(showArrowTask);
			EventBus.ProcessTask(highlightTask);
			
			CInputLock clickLock = new("ClickVehicleDepot", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickLock);

			await CWaitForSignal.WaitForSignalAsync<CVehicleUpgradedSignal>(EventBus, ct);
			_analyticsFunnel.Send(EUpgradeVehicleFunnelStep.Completed);
			
			EventSystem.RemoveInputLocker(clickLock);
			DisableAllTutorialGraphics();
			
			User.Tutorials.SetUpgradeVehicleTutorialStep(EVehicleUpgradeTutorialStep.Completed);
		}

		private void ShowHelp()
		{
			EventBus.ProcessTask(new CShowInfoScreenTask(EScreenInfoId.UpgradeVehicles));
		}

		private void TryAddResources()
		{
			IEnumerable<IValuable> upgradePrice = _designVehicleConfigs.GetUpgradePrice(0, EVehicleStat.Capacity, ERegion.Region1);

			foreach (IValuable valuable in upgradePrice)
			{
				User.OwnedValuables.ForceToHaveAtLeastSomeValuables(valuable);
			}
		}
	}
}