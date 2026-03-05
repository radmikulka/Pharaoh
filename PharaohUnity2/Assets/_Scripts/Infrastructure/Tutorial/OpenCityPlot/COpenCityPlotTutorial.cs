// =========================================
// AUTHOR: Radek Mikulka
// DATE:   31.10.2025
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
	public class COpenCityPlotTutorial : CBaseTutorial, IInitializable
	{
		private readonly CInputLock _inputLock = new("Tutorial", EInputLockLayer.Tutorial);
		private readonly CDesignMainCityConfigs _designMainCityConfigs;
		private readonly COpenCityPlotTutorialFunnel _analyticsFunnel;
		private readonly IGoToHandler _goToHandler;
		private readonly ICtsProvider _ctsProvider;

		public COpenCityPlotTutorial(
			CDesignMainCityConfigs designMainCityConfigs,
			COpenCityPlotTutorialFunnel analyticsFunnel,
			ITutorialCommentator tutorialCommentator, 
			CLazyActionQueue actionQueue, 
			CEventSystem eventSystem, 
			IGoToHandler goToHandler, 
			ICtsProvider ctsProvider, 
			IEventBus eventBus, 
			CUser user
			) 
			: base(eventBus, user, eventSystem, tutorialCommentator, actionQueue)
		{
			_designMainCityConfigs = designMainCityConfigs;
			_analyticsFunnel = analyticsFunnel;
			_goToHandler = goToHandler;
			_ctsProvider = ctsProvider;
		}
		
		public void Initialize()
		{
			TryRunOnStart();
		}
		
		private void TryRunOnStart()
		{
			int cityLevel = User.City.GetLevel();
			bool canRun = CanRun(cityLevel);
			if (!canRun)
				return;
			RunTutorialAction(Run);
		}

		public bool TryRunUpgrade(int cityLevel)
		{
			bool canRun = CanRun(cityLevel);
			if (!canRun)
				return false;
			RunWithUpgrade(_ctsProvider.Token).Forget();
			return true;
		}

		private bool CanRun(int cityLevel)
		{
			bool isCompleted = User.Tutorials.IsTutorialCompleted(EOpenCityPlotStep.Completed);
			if (isCompleted)
				return false;

			CMainCityLevelConfig cityConfig = _designMainCityConfigs.GetLevelConfig(cityLevel);
			bool upgradeHavePlot = cityConfig.HaveNewBuildingPlot();
			return upgradeHavePlot;
		}
		
		private async UniTask RunWithUpgrade(CancellationToken ct)
		{
			_analyticsFunnel.Send(EOpenCityPlotFunnelStep.Started);
			EventSystem.AddInputLocker(_inputLock);
			await _goToHandler.GoToCityAndUpgradeIt(ct);
			await Run(ct);
		}

		private async UniTask Run(CancellationToken ct)
		{
			EventSystem.AddInputLocker(_inputLock);
			_analyticsFunnel.Send(EOpenCityPlotFunnelStep.CityHighlgihted);
			
			await ShowArrowOverPlot(ct);
			await BuyPlot(ct);
			await BuyBuilding(ct);
			
			await UniTask.WaitForSeconds(2f, cancellationToken: ct);
			EventSystem.RemoveInputLocker(_inputLock);

			EventBus.ProcessTask(new CShowInfoScreenTask(EScreenInfoId.Parcels));
		}
		
		private async UniTask ShowArrowOverPlot(CancellationToken ct)
		{
			RectTransform nameTagRect = EventBus.ProcessTask<CGetCityPlotNameTagRectRequest, RectTransform>(new CGetCityPlotNameTagRectRequest(0));
			CInputLock waitForClickLock = new("Tutorial_WaitForClick", EInputLockLayer.Tutorial, nameTagRect);
			
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(nameTagRect)
				.SetAnchoredOffset(40f, 40f)
				.SetClockwiseArrowRotation(45f);
			EventBus.ProcessTask(showArrowTask);
			
			EventSystem.AddInputLocker(waitForClickLock);
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(EventBus, ct);
			_analyticsFunnel.Send(EOpenCityPlotFunnelStep.PlotSelected);
			DisableAllTutorialGraphics();
			EventSystem.RemoveInputLocker(waitForClickLock);
		}
		
		private async UniTask BuyPlot(CancellationToken ct)
		{
			bool anyPlotUnlocked = User.City.IsAnyBuildingPlotUnlocked();
			if(anyPlotUnlocked)
				return;
			
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.CityPlotBuyButton));
			
			CInputLock waitForClickLock = new("Tutorial_WaitForClick", EInputLockLayer.Tutorial, rect.RectTransform);
			
			EventSystem.AddInputLocker(waitForClickLock);
			await WaitForMenuOpenEnd(EScreenId.Parcel, ct);
			
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
				.SetAnchoredOffset(80f, 40f)
				.SetClockwiseArrowRotation(20f);
			
			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(rect.RectTransform, "Tutorial.CityPlot1")
					.SetAnchoredOffset(-750f, 130f)
					.SetSide(ETutorialTooltipSide.Right)
				;
			
			EventBus.ProcessTask(showArrowTask);
			EventBus.ProcessTask(tooltip);
			
			await WaitForMenuCloseStart(EScreenId.Parcel, ct);
			_analyticsFunnel.Send(EOpenCityPlotFunnelStep.PlotBought);
			DisableAllTutorialGraphics();
			EventSystem.RemoveInputLocker(waitForClickLock);
		}
		
		private async UniTask BuyBuilding(CancellationToken ct)
		{
			await WaitForMenuOpenEnd(EScreenId.SpecialBuildings, ct);
			
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.FirstSpecialBuildingRect));
			
			CInputLock waitForClickLock = new("Tutorial_WaitForClick", EInputLockLayer.Tutorial, rect.RectTransform);
			
			CShowTutorialTooltipTask tooltip = new CShowTutorialTooltipTask(rect.RectTransform, "Tutorial.CityPlot2")
					.SetAnchoredOffset(-130f, -420f)
					.SetSizeOffset(30f, 0f)
					.SetSide(ETutorialTooltipSide.Top)
				;
			
			CShowTutorialHighlightTask highlightTask = new CShowTutorialHighlightTask(rect.RectTransform, ETutorialHighlightRectType.Rectangle)
					.SetSizeOffset(80f, 80f)
				;
			
			EventSystem.AddInputLocker(waitForClickLock);
			EventBus.ProcessTask(highlightTask);
			EventBus.ProcessTask(tooltip);

			CPreviewSpecialBuildingSelectedSignal signal = await CWaitForSignal.WaitForSignalAsync<CPreviewSpecialBuildingSelectedSignal>(EventBus, ct);
			User.OwnedValuables.ForceToHaveAtLeastSomeValuables(signal.Price);
			DisableAllTutorialGraphics();
			EventSystem.RemoveInputLocker(waitForClickLock);

			CGetUiRectResponse buyRect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.BuySpecialBuildingButton));
			CInputLock buyClickLock = new("Tutorial_WaitForClick", EInputLockLayer.Tutorial, buyRect.RectTransform);
			EventSystem.AddInputLocker(buyClickLock);
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(buyRect.RectTransform)
				.SetAnchoredOffset(0f, 30f)
				.SetClockwiseArrowRotation(20f);
			EventBus.ProcessTask(showArrowTask);
			
			await CWaitForSignal.WaitForSignalAsync<CSpecialBuildingBoughtSignal>(EventBus, ct);
			
			User.Tutorials.SetCityPlotStep(EOpenCityPlotStep.Completed);
			_analyticsFunnel.Send(EOpenCityPlotFunnelStep.Completed);
			EventSystem.RemoveInputLocker(buyClickLock);
			
			DisableAllTutorialGraphics();
		}
	}
}