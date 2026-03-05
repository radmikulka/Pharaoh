// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.10.2024
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using TycoonBuilder.Shop;
using TycoonBuilder.Ui;
using TycoonBuilder.Ui.CityMenu;
using TycoonBuilder.Ui.DecadePassMenu;
using TycoonBuilder.Ui.DispatchMenu;
using TycoonBuilder.Ui.SpecialBuildings;
using TycoonBuilder.Ui.VehicleDetailMenu;
using TycoonBuilder.Ui.VehicleFollow;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CScreenManager : AldaEngine.CScreenManager, IInitializable
	{
		[SerializeField] private CanvasGroup[] _rootCanvasGroups;
		
		private IEventBus _eventBus;
		private CUser _user;
		private IAuthService _authService;
		private CInfoScreenContentsConfig _infoScreenContentsConfig;
		private CGameModeManager _gameModeManager;

		[Inject]
		private void Inject(IEventBus eventBus, CUser user, IAuthService authService, CInfoScreenContentsConfig infoScreenContentsConfig, CGameModeManager gameModeManager)
		{
			_eventBus = eventBus;
			_user = user;
			_authService = authService;
			_infoScreenContentsConfig = infoScreenContentsConfig;
			_gameModeManager = gameModeManager;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CIsAnyScreenActiveRequest, CIsAnyScreenActiveResponse>(HandleIsAnyScreenActiveRequest);
			_eventBus.AddAsyncTaskHandler<ShowDialogueWindowRequest, ShowDialogueWindowResponse>(OnShowDialogueWindowRequest);
			_eventBus.AddTaskHandler<CIsScreenOpenedRequest, CIsScreenOpenedResponse>(HandleIsScreenOpenedRequest);
			_eventBus.AddTaskHandler<COpenContractDispatchMenuTask>(ProcessShowContractDispatchMenuCommand);
			_eventBus.AddTaskHandler<COpenResourceDispatchMenuTask>(ProcessShowResourceDispatchMenuCommand);
			_eventBus.AddTaskHandler<COpenCityDispatchMenuTask>(ProcessOpenCityDispatchMenuTask);
			_eventBus.AddTaskHandler<CMenuManagerStateRequest, CMenuManagerStateResponse>(OnStateRequest);
			_eventBus.AddTaskHandler<CTryCloseActiveScreenTask>(HandleTryCloseActiveScreenRequest);
			_eventBus.AddTaskHandler<CTryKillScreenTask>(HandleTryKillScreenRequest);
			_eventBus.Subscribe<CStoryContractInstanceUpgradeCompleteSignal>(ShowContractRewards);
			_eventBus.Subscribe<CPassengerContractCompletedSignal>(ShowPassengerContractRewards);
			_eventBus.AddTaskHandler<CCloseTopmostScreenTask>(ProcessCloseTopmostScreenCommand);
			_eventBus.AddAsyncTaskHandler<CShowPopUpOfferTask>(ProcessShowPopUpOfferCommand);
			_eventBus.AddTaskHandler<COpenVehicleDetailTask>(ProcessShowVehicleDetailCommand);
			_eventBus.AddTaskHandler<CShowParcelMenuTask>(ProcessShowParcelMenuCommand);
			_eventBus.AddTaskHandler<CShowNewMaterialUnlockedMenuTask>(ProcessShowNewMaterialUnlockedMenuTask);
			_eventBus.AddTaskHandler<CShowSpecialBuildingMenuTask>(ProcessShowSpecialBuildingMenuCommand);
			_eventBus.AddTaskHandler<COpenInspectVehicleMenuTask>(ProcessOpenInspectVehicleMenuCommand);
			_eventBus.AddAsyncTaskHandler<COpenFactoryTask>(ProcessOpenFactoryCommand);
			_eventBus.AddAsyncTaskHandler<COpenFactoryManagementTask>(ProcessOpenFactoryManagementCommand);
			_eventBus.AddAsyncTaskHandler<COpenShopTask>(ProcessOpenShopCommand);
			_eventBus.AddAsyncTaskHandler<COpenCityMenuTask>(ProcessOpenCityMenuCommand);
			_eventBus.AddTaskHandler<COpenNewLocationScreenTask>(ProcessOpenNewLocationScreen);
			_eventBus.AddTaskHandler<COpenProfileConflictTask>(OpenProfileConflictMenu);
			_eventBus.AddTaskHandler<CShowScreenTask>(ProcessShowScreenCommand);
			_eventBus.AddAsyncTaskHandler<CShowScreenTask>(ProcessShowScreenAsyncCommand);
			_eventBus.AddTaskHandler<CShowDialogMenuTask>(ShowDialog);
			_eventBus.AddTaskHandler<CShowGetMoreMaterialTask>(ShowGetMoreMaterial);
			_eventBus.AddTaskHandler<CHideAllMenusTask>(HideAllMenus);
			_eventBus.AddTaskHandler<CShowInfoScreenTask>(ProcessShowInfoScreenCommand);
			_eventBus.AddTaskHandler<CShowVehicleFollowScreenTask>(ProcessShowVehicleFollowScreenCommand);
			_eventBus.AddTaskHandler<COpenTermsOfUseScreenTask>(ProcessOpenTermsOfUseScreenCommand);
			_eventBus.AddTaskHandler<COpenBadGameVersionScreenTask>(ProcessOpenBadGameVersionScreenCommand);
			_eventBus.AddTaskHandler<COpenCityUpgradeMenuTask>(ProcessOpenCityUpgradeMenu);
			_eventBus.AddTaskHandler<CShowSaveProgressPopUpTask>(ProcessShowSaveProgressPopUpCommand);
			_eventBus.AddAsyncTaskHandler<CShowDispatcherHiredMenuTask>(ProcessShowDispatcherHiredMenuCommand);
			_eventBus.AddAsyncTaskHandler<COpenEventOverviewTask>(ProcessOpenEventOverviewCommand);
			_eventBus.AddTaskHandler<COpenEventPremiumPassTask>(ProcessOpenEventPremiumPassCommand);
			_eventBus.AddTaskHandler<COpenLeaderboardRewardsTask>(ProcessOpenLeaderboardRewardsCommand);
			_eventBus.AddTaskHandler<CShowLeaderboardFinishedTask>(ProcessShowLeaderboardFinishedRewardsCommand);
			_eventBus.AddAsyncTaskHandler<CCloseAllScreensTask>(ProcessCloseAllScreensCommand);
			_eventBus.AddTaskHandler<CShowCompanyProfileTask>(ProcessShowCompanyProfileCommand);
		}

		private void ProcessShowCompanyProfileCommand(CShowCompanyProfileTask task)
		{
			OpenMenu<CCompanyProfileMenu>((int) EScreenId.CompanyProfile, screen =>
			{
				screen.SetData(task.UserUid);
			});
		}

		private async Task ProcessCloseAllScreensCommand(CCloseAllScreensTask task, CancellationToken ct)
		{
			while (IsActive)
			{
				TryCloseTopmostMenu();
				await UniTask.Yield(ct);
			}
		}

		private void ProcessShowLeaderboardFinishedRewardsCommand(CShowLeaderboardFinishedTask task)
		{
			OpenMenu<CLeaderboardFinishedMenu>((int) EScreenId.LeaderboardFinished, screen =>
			{
				screen.Set(task.LiveEventId, task.Rank, task.PointsOnRank, task.Rewards);
			});
		}

		private void ProcessOpenLeaderboardRewardsCommand(COpenLeaderboardRewardsTask task)
		{
			OpenMenu<CLeaderboardRewardsMenu>((int) EScreenId.LeaderboardRewards, screen =>
			{
				screen.Init(task.LiveEventId);
			});
		}

		private void ProcessOpenEventPremiumPassCommand(COpenEventPremiumPassTask task)
		{
			OpenMenu<CEventPassPremiumMenu>((int) EScreenId.EventPassPremium, screen =>
			{
				screen.Init(task.LiveEventId);
			});
		}

		private async Task ProcessOpenEventOverviewCommand(COpenEventOverviewTask task, CancellationToken ct)
		{
			ILiveEvent liveEvent = _user.LiveEvents.GetActiveEventOrDefault(task.LiveEventId);
			bool isNotInLiveEventMode = _gameModeManager.ActiveGameMode.Id != EGameModeId.RegionLiveEvent;

			if (liveEvent != null
			    && isNotInLiveEventMode
			    && _user.LiveEvents.IsEventWithRegion(liveEvent)
			    && !_user.LiveEvents.IsIntroSeen(task.LiveEventId)
			    && !CDebugConfig.Instance.ShouldSkip(EEditorSkips.IntroCutscene))
			{
				await _eventBus.ProcessTaskAsync(new CLoadGameModeTask(new CRegionLiveEventGameGameModeData(task.LiveEventId)), ct);
				return;
			}

			OpenMenu<CEventOverviewMenu>((int) EScreenId.EventOverview, screen =>
			{
				screen.Init(task.LiveEventId);
			});

			if (task.Tab != EEventOverviewTab.None)
			{
				CEventOverviewMenu menu = GetMenu<CEventOverviewMenu>((int)EScreenId.EventOverview);
				await UniTask.Yield(ct);
				menu.SwitchTab((int)task.Tab - 1);
			}
		}

		private async Task ProcessShowDispatcherHiredMenuCommand(CShowDispatcherHiredMenuTask showTask, CancellationToken ct)
		{
			CDispatcherHiredScreen menu = GetMenu<CDispatcherHiredScreen>((int)EScreenId.DispatcherHired);
			OpenMenu<CDispatcherHiredScreen>((int) EScreenId.DispatcherHired, screen =>
			{
				screen.Init(showTask.DispatcherId, showTask.ExpirationTime);
			});
			
			await UniTask.WaitUntil(() => menu.State == EScreenState.Closed, cancellationToken: ct);
		}

		private void ProcessShowSaveProgressPopUpCommand(CShowSaveProgressPopUpTask task)
		{
			if(_authService.IsSignedIn(EAuthType.Google) || _authService.IsSignedIn(EAuthType.Apple) || _authService.IsSignedIn(EAuthType.Meta))
				return;
			
			OpenMenu((int) EScreenId.SaveProgressPopUp);
		}

		private void ProcessOpenCityUpgradeMenu(COpenCityUpgradeMenuTask signal)
		{
			OpenMenu<CCityUpgradeMenu>((int) EScreenId.CityUpgrade, screen =>
			{
				screen.Init(signal.OpenCityMenuOnClose);
			});
		}

		private void ProcessOpenNewLocationScreen(COpenNewLocationScreenTask task)
		{
			OpenMenu<CNewLocationUnlockedMenu>((int) EScreenId.NewLocationUnlocked, screen =>
			{
				screen.Init(task.Location);
			});
		}

		private void HideAllMenus(CHideAllMenusTask task)
		{
			foreach (CanvasGroup canvasGroup in _rootCanvasGroups)
			{
				canvasGroup.alpha = task.State ? 1f : 0f;;
			}
		}

		private void ProcessOpenBadGameVersionScreenCommand(COpenBadGameVersionScreenTask task)
		{
			OpenMenu<CBadGameVersionScreen>((int) EScreenId.BadGameVersion, screen =>
			{
				screen.Init(task.TimeToRelease);
			});
		}

		private void ProcessOpenInspectVehicleMenuCommand(COpenInspectVehicleMenuTask task)
		{
			OpenMenu<CFullscreenVehicleDetailMenu>((int) EScreenId.FullscreenVehicleDetail, screen =>
			{
				screen.SetVehicle(task.Vehicle, task.ShowStats);
			});
		}

		private CIsScreenOpenedResponse HandleIsScreenOpenedRequest(CIsScreenOpenedRequest task)
		{
			bool isActive = GetMenu<IScreen>((int)task.ScreenId).State is EScreenState.Opened;
			return new CIsScreenOpenedResponse(isActive);
		}

		private void OpenProfileConflictMenu(COpenProfileConflictTask task)
		{
			OpenMenu<CProfileConflictMenu>((int) EScreenId.ProfileConflict, screen =>
			{
				screen.SetData(task.DeviceData, task.RemoteData, task.OnUseRemote, task.OnUseDevice, task.OnClose);
			});
		}

		private void ProcessOpenTermsOfUseScreenCommand(COpenTermsOfUseScreenTask task)
		{
			OpenMenu<CTermsOfUseScreen>((int) EScreenId.TermsOfUse, screen =>
			{
				screen.Init(task.PrivacyPolicyLink, task.TermsOfUseLink, task.OnConfirmed);
			});
		}

		private void ProcessShowNewMaterialUnlockedMenuTask(CShowNewMaterialUnlockedMenuTask task)
		{
			OpenMenu<CNewMaterialUnlockedMenu>((int) EScreenId.NewMaterialUnlocked, screen =>
			{
				screen.SetData(task.ResourceId);
			});
		}
		
		private void ShowGetMoreMaterial(CShowGetMoreMaterialTask task)
		{
			OpenMenu<CGetMoreMaterialMenu>((int) EScreenId.GetMoreMaterial, screen =>
			{
				screen.Set(task.RequiredResource, task.Source);
			});
		}

		private void ProcessShowSpecialBuildingMenuCommand(CShowSpecialBuildingMenuTask task)
		{
			OpenMenu<CSpecialBuildingsMenu>((int) EScreenId.SpecialBuildings, screen =>
			{
				screen.SetData(task.Index, task.TabIndex);
			});
		}

		private void ProcessShowParcelMenuCommand(CShowParcelMenuTask task)
		{
			OpenMenu<CParcelMenu>((int) EScreenId.Parcel, screen =>
			{
				screen.SetData(task.Index, task.IsUnlocked);
			});
		}

		private void HandleTryCloseActiveScreenRequest(CTryCloseActiveScreenTask task)
		{
			TryCloseTopmostMenu();
		}
		
		private void HandleTryKillScreenRequest(CTryKillScreenTask task)
		{
			KillScreen((int) task.ScreenId);
		}

		private CIsAnyScreenActiveResponse HandleIsAnyScreenActiveRequest(CIsAnyScreenActiveRequest task)
		{
			return new CIsAnyScreenActiveResponse(IsActive);
		}

		private void ProcessShowResourceDispatchMenuCommand(COpenResourceDispatchMenuTask task)
		{
			TryCloseTopmostMenu();
			CDispatchMenu menu = GetMenu<CDispatchMenu>((int)EScreenId.Dispatch);
			OpenMenu(menu.Id);
			menu.SetData(task.Industry);
		}
		
		private void ProcessOpenCityDispatchMenuTask(COpenCityDispatchMenuTask task)
		{
			CDispatchMenu menu = GetMenu<CDispatchMenu>((int)EScreenId.Dispatch);
			OpenMenu(menu.Id);
			menu.SetData(task.City);
		}

		private void ProcessShowContractDispatchMenuCommand(COpenContractDispatchMenuTask task)
		{
			TryCloseTopmostMenu();
			CDispatchMenu menu = GetMenu<CDispatchMenu>((int)EScreenId.Dispatch);
			OpenMenu(menu.Id);
			menu.SetData(task.ContractId);
		}
		
		private void ShowContractRewards(CStoryContractInstanceUpgradeCompleteSignal task)
		{
			CContractRewardsScreen menu = GetMenu<CContractRewardsScreen>((int)EScreenId.ContractRewards);
			OpenMenu(menu.Id);
			menu.Set(task.StoryContract);
		}
		
		private void ShowPassengerContractRewards(CPassengerContractCompletedSignal signal)
		{
			CContractRewardsScreen menu = GetMenu<CContractRewardsScreen>((int)EScreenId.ContractRewards);
			TryCloseTopmostMenu();
			OpenMenu(menu.Id);
			menu.Set(signal.PassengerContract);
		}
		
		private void ProcessShowVehicleFollowScreenCommand(CShowVehicleFollowScreenTask task)
		{
			CVehicleFollowScreen menu = GetMenu<CVehicleFollowScreen>((int)EScreenId.VehicleFollow);
			OpenMenu(menu.Id);
			menu.Set(task.Dispatch);
		}

		private void ProcessShowVehicleDetailCommand(COpenVehicleDetailTask task)
		{
			EVehicleDepotTutorialStep vehicleDepoTutorial = _user.Tutorials.GetVehicleDepotTutorialStep();
			if (vehicleDepoTutorial < EVehicleDepotTutorialStep.Started)
			{
				_eventBus.ProcessTask(new CShowTooltipTask("Tooltip.TutorialRunning", true));
				return;
			}

			CBeforeVehicleDetailOpenStartSignal beforeOpenSignal = new(task.VehicleId);
			_eventBus.Send(beforeOpenSignal);
			CVehicleDetailMenu menu = GetMenu<CVehicleDetailMenu>((int)EScreenId.VehicleDetail);
			OpenMenu(menu.Id);
			menu.SetData(task.VehicleId, task.OpeningSource, task.RequiredDurability);
		}

		private void ProcessShowScreenCommand(CShowScreenTask task)
		{
			if (task.ClosePreviousScreen)
			{
				TryCloseTopmostMenu();
			}
			OpenMenu((int) task.ScreenId);
		}
		
		private async Task ProcessShowScreenAsyncCommand(CShowScreenTask task, CancellationToken ct)
		{
			if (task.ClosePreviousScreen)
			{
				TryCloseTopmostMenu();
			}

			CTycoonBuilderScreen menu = GetMenu<CTycoonBuilderScreen>((int)task.ScreenId);
			OpenMenu(menu.Id);
			
			await UniTask.WaitUntil(() => menu.State == EScreenState.Opened, cancellationToken: ct);
		}
		
		private void ProcessCloseTopmostScreenCommand(CCloseTopmostScreenTask task)
		{
			TryCloseTopmostMenu();
		}

		private async Task ProcessOpenShopCommand(COpenShopTask task, CancellationToken ct)
		{
			CShopMenu menu = GetMenu<CShopMenu>((int)EScreenId.Shop);
			if (menu.State != EScreenState.Opened && menu.State != EScreenState.Opening)
			{
				OpenMenu(menu.Id);
				await UniTask.DelayFrame(2, cancellationToken: ct);
			}

			if (task.Tab != EShopTab.None)
			{
				await UniTask.Yield(ct);
				menu.SwitchTab((int)task.Tab - 1);
			}
			
			if (task.TargetReward != EValuable.None)
			{
				await UniTask.Yield(ct);
				menu.ScrollToReward(task.TargetReward);
			}
		}
		
		private async Task ProcessOpenCityMenuCommand(COpenCityMenuTask task, CancellationToken ct)
		{
			CCityMenu menu = GetMenu<CCityMenu>((int)EScreenId.City);
			if (menu.State != EScreenState.Opened && menu.State != EScreenState.Opening)
			{
				OpenMenu(menu.Id);
				await UniTask.DelayFrame(2, cancellationToken: ct);
			}

			menu.SwitchTab(task.Tab);
		}
		
		private async Task ProcessOpenFactoryCommand(COpenFactoryTask task, CancellationToken ct)
		{
			CFactoryMenu menu = GetMenu<CFactoryMenu>((int)EScreenId.Factory);
			if (menu.State != EScreenState.Opened && menu.State != EScreenState.Opening)
			{
				OpenMenu(menu.Id);
			}

			await UniTask.Yield(ct);
			menu.Set(task.Factory);

			if (task.Product != EResource.None)
			{
				menu.SetProduct(task.Product);
			}
		}
		
		private async Task ProcessOpenFactoryManagementCommand(COpenFactoryManagementTask task, CancellationToken ct)
		{
			CFactoryManagementMenu menu = GetMenu<CFactoryManagementMenu>((int)EScreenId.FactoryManagement);
			if (menu.State != EScreenState.Opened && menu.State != EScreenState.Opening)
			{
				OpenMenu(menu.Id);
			}

			await UniTask.Yield(ct);
			menu.Set(task.Factory);
		}
		
		private async Task ProcessShowPopUpOfferCommand(CShowPopUpOfferTask task, CancellationToken ct)
		{
			CPopUpOfferMenu menu = GetMenu<CPopUpOfferMenu>((int)EScreenId.PopUpOfferMenu);
			OpenMenu(menu.Id);

			if (task.IsGroup)
			{
				menu.SetGroup(task.OfferId, false);
			}
			else
			{
				menu.SetOffer(task.OfferId);
			}

			await UniTask.WaitUntil(() => menu.State == EScreenState.Closed, cancellationToken: ct);
		}

		private void ProcessShowInfoScreenCommand(CShowInfoScreenTask task)
		{
			CInfoScreenContentsConfig.SInfoScreenContent data = _infoScreenContentsConfig.GetContent(task.ScreenInfoId);
			if (data._useAlternateScreen)
			{
				CInfoScreenAlternate menu = GetMenu<CInfoScreenAlternate>((int)EScreenId.InfoScreenAlternate);
				menu.SetId(task.ScreenInfoId);
				OpenMenu(menu.Id);
			}
			else
			{
				CInfoScreen menu = GetMenu<CInfoScreen>((int)EScreenId.Info);
				menu.SetId(task.ScreenInfoId);
				OpenMenu(menu.Id);
			}
		}

		private void ShowDialog(CShowDialogMenuTask task)
		{
			CDialog menu = task.DialogTask.IsOverlay
				? GetMenu<CDialogOverlay>((int)EScreenId.DialogOverlay)
				: GetMenu<CDialog>((int)EScreenId.Dialog)
				;
			
			OpenMenu(menu.Id);
			menu.Set(task.DialogTask);
		}

		private async Task<ShowDialogueWindowResponse> OnShowDialogueWindowRequest(ShowDialogueWindowRequest request, CancellationToken cancellationToken)
		{
			IDialogueWindow dialogueWindow = null;
			
			OpenMenu<IDialogueWindow>((int) EScreenId.DialogueWindow, screen =>
			{
				dialogueWindow = screen;
			});

			await UniTask.WaitUntil(() => dialogueWindow != null, cancellationToken: cancellationToken);
			ShowDialogueWindowResponse response = new (dialogueWindow);
			return response;
		}

		private CMenuManagerStateResponse OnStateRequest(CMenuManagerStateRequest request)
		{
			CMenuManagerStateResponse response = new(IsActive, ActiveMenus.Count);
			return response;
		}
	}
}