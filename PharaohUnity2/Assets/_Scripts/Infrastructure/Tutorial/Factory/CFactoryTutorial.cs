// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.10.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;
using ServerData.Hits;

namespace TycoonBuilder
{
	public class CFactoryTutorial : CBaseTutorial, IInitializable
	{
		private readonly CInputLock _inputLock = new("FactoryTutorial", EInputLockLayer.Tutorial);
		private readonly CFactoryTutorialFunnel _analyticsFunnel;
		private readonly CDesignFactoryConfigs _factoryConfigs;
		private readonly ICtsProvider _ctsProvider;

		public CFactoryTutorial(
			ITutorialCommentator tutorialCommentator,
			CFactoryTutorialFunnel analyticsFunnel, 
			CDesignFactoryConfigs factoryConfigs, 
			CLazyActionQueue actionQueue, 
			CEventSystem eventSystem, 
			ICtsProvider ctsProvider, 
			IEventBus eventBus, 
			CUser user
			) 
			: base(eventBus, user, eventSystem, tutorialCommentator, actionQueue)
		{
			_analyticsFunnel = analyticsFunnel;
			_factoryConfigs = factoryConfigs;
			_ctsProvider = ctsProvider;
		}

		public void Initialize()
		{
			TryStartOrBindToYearChange();
		}

		private void TryStartOrBindToYearChange()
		{
			EFactoryTutorialStep currentStep = User.Tutorials.GetFactoryTutorialStep();
			if (currentStep == EFactoryTutorialStep.None)
			{
				if (User.Progress.Year >= EYearMilestone._1931)
				{
					RunTutorialAction(Run);
					return;
				}
				EventBus.Subscribe<CYearSeenSignal>(OnYearSeen);
			}
		}

		private void OnYearSeen(CYearSeenSignal signal)
		{
			if(signal.YearMilestone != EYearMilestone._1931)
				return;
			
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;

			RunTutorialAction(Run);
		}

		private bool IsCompleted()
		{
			return User.Tutorials.IsTutorialCompleted(EFactoryTutorialStep.Completed);
		}

		private async UniTask Run(CancellationToken ct)
		{
			EventSystem.AddInputLocker(_inputLock);
			await HighlightFactoryButton(ct);
			EventSystem.RemoveInputLocker(_inputLock);
		}
		
		private async UniTask HighlightFactoryButton(CancellationToken ct)
		{
			bool isCompleted = User.Tutorials.IsTutorialCompleted(EFactoryTutorialStep.Completed);
			if (isCompleted)
				return;
			
			_analyticsFunnel.Send(EFactoryTutorialFunnelStep.Started);
			
			await TutorialCommentator.ShowCommentator(ITutorialCommentator.ESide.Right, "Tutorial.Factory1", true, ct);
			await TutorialCommentator.ShowText("Tutorial.Factory2", true, ct);
			await TutorialCommentator.Hide(ct);
			
			_analyticsFunnel.Send(EFactoryTutorialFunnelStep.DialogueCompleted);
			
			EventBus.ProcessTask(new CActivateFactoriesButtonTask());
			
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.FactoriesMenuButton));

			await UniTask.WaitForSeconds(0.3f, cancellationToken: ct);
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(0f, 50f)
					.SetClockwiseArrowRotation(0f)
				;
			
			CInputLock clickOnFactoryLock = new("ClickFactory", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickOnFactoryLock);

			EventBus.ProcessTask(showArrowTask);

			AddResourceToCreateSteel();

			await CWaitForSignal.WaitForSignalAsync<CScreenOpenStartSignal>(
				signal => signal.MenuId == (int) EScreenId.Factories, EventBus, ct);
			
			_analyticsFunnel.Send(EFactoryTutorialFunnelStep.FactoryMenuOpened);
			DisableAllTutorialGraphics();
			
			await CWaitForSignal.WaitForSignalAsync<CScreenOpenEndSignal>(
				signal => signal.MenuId == (int) EScreenId.Factories, EventBus, ct);
			
			EventSystem.RemoveInputLocker(clickOnFactoryLock);

			await ClickOnFoundryButton(ct);
			await StartFactoryProduction(ct);
			
			await UniTask.WaitForSeconds(2f, cancellationToken: ct);
			EventBus.ProcessTask(new CShowInfoScreenTask(EScreenInfoId.Factories));
		}

		private async UniTask ClickOnFoundryButton(CancellationToken ct)
		{
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.FoundryFactoryButton));
			
			await UniTask.WaitForSeconds(1.15f, cancellationToken: ct);
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(50f, 50f)
					.SetClockwiseArrowRotation(45f)
				;
			
			CInputLock clickLock = new("ClickFactory", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickLock);

			EventBus.ProcessTask(showArrowTask);
			
			await CWaitForSignal.WaitForSignalAsync<CFactoryProductSelectedSignal>(EventBus, ct);
			_analyticsFunnel.Send(EFactoryTutorialFunnelStep.FactoryClicked);
			
			DisableAllTutorialGraphics();
			EventSystem.RemoveInputLocker(clickLock);
		}
		
		private async UniTask StartFactoryProduction(CancellationToken ct)
		{
			CGetUiRectResponse rect = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(
				new CGetUiRectRequest(EUiRect.CraftInFactoryButton));
			
			await UniTask.WaitForSeconds(0.2f, cancellationToken: ct);
			CShowTutorialArrowTask showArrowTask = new CShowTutorialArrowTask(rect.RectTransform)
					.SetAnchoredOffset(180f, 0f)
					.SetClockwiseArrowRotation(90f)
				;
			
			EventBus.ProcessTask(showArrowTask);
			
			CInputLock clickLock = new("ClickCraft", EInputLockLayer.Tutorial, rect.RectTransform);
			EventSystem.AddInputLocker(clickLock);
			
			await CWaitForSignal.WaitForSignalAsync<CFactoryProductionStartedSignal>(EventBus, ct);
			_analyticsFunnel.Send(EFactoryTutorialFunnelStep.Completed);
			User.Tutorials.SetFactoryStep(EFactoryTutorialStep.Completed);
			
			EventSystem.RemoveInputLocker(clickLock);
			DisableAllTutorialGraphics();
		}

		private void AddResourceToCreateSteel()
		{
			CFactoryConfig factory = _factoryConfigs.GetFactoryConfig(EFactory.Foundry);
			CFactoryProduct product = factory.GetProduct(EResource.Steel, 1);
			foreach (SResource resource in product.Requirements)
			{
				int ownedResource = User.Warehouse.GetResourceAmount(resource.Id);
				int neededAmount = resource.Amount - ownedResource;
				if (neededAmount > 0)
				{
					User.Warehouse.AddResource(new SResource(resource.Id, neededAmount));
				}
			}
		}
	}
}