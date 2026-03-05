// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using AldaEngine.Tcp;
using Cysharp.Threading.Tasks;
using TycoonBuilder.Loading;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using TycoonBuilder;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace TycoonBuilder
{
    public class CConnectionScreen : MonoBehaviour, IInitializable
    {
        [SerializeField] private CUiComponentText _text;
        
        private CValidServerConnectionPostprocess _validServerConnectionPostprocess;
        private CInternetReachabilityChecker _internetReachabilityChecker;
        private CServiceConnectionThread _serviceConnectionThread;
        private CServerConnectionThread _serverConnectionThread;
        private IRestartGameHandler _restartGameHandler;
        private CLoadingTechFlow _loadingTechFlow;
        private IRemoteDatabase _remoteDatabase;
        private IExitAppHandler _exitAppHandler;
        private IBundleManager _bundleManager;
        private ILoadingScreen _loadingScreen;
        private ISceneManager _sceneManager;
        private CSettingsData _settingsData;
        private CErrorHandler _errorHandler;
        private ICtsProvider _ctsProvider;
        private IInAppUpdate _inAppUpdate;
        private ITranslation _translation;
        private IAuthService _authService;
        private ICFtueFunnel _ftueFunnel;
        private CHitBuilder _hitBuilder;
        private IEventBus _eventBus;

        [Inject]
        private void Inject(
            CValidServerConnectionPostprocess validServerConnectionPostprocess,
            CInternetReachabilityChecker internetReachabilityChecker,
            CServiceConnectionThread serviceConnectionThread,
            CServerConnectionThread serverConnectionThread,
            IRestartGameHandler restartGameHandler,
            CLoadingTechFlow loadingTechFlow,
            IExitAppHandler exitAppHandler,
            IRemoteDatabase remoteDatabase,
            ILoadingScreen loadingScreen, 
            IBundleManager bundleManager,
            CErrorHandler errorHandler, 
            ISceneManager sceneManager, 
            CSettingsData settingsData, 
            IAuthService authService, 
            IInAppUpdate inAppUpdate, 
            ICtsProvider ctsProvider, 
            ITranslation translation,
            ICFtueFunnel ftueFunnel,
            CHitBuilder hitBuilder,
            IEventBus eventBus
            )
        {
            _validServerConnectionPostprocess = validServerConnectionPostprocess;
            _internetReachabilityChecker = internetReachabilityChecker;
            _serviceConnectionThread = serviceConnectionThread;
            _serverConnectionThread = serverConnectionThread;
            _restartGameHandler = restartGameHandler;
            _loadingTechFlow = loadingTechFlow;
            _exitAppHandler = exitAppHandler;
            _remoteDatabase = remoteDatabase;
            _loadingScreen = loadingScreen;
            _bundleManager = bundleManager;
            _errorHandler = errorHandler;
            _sceneManager = sceneManager;
            _settingsData = settingsData;
            _authService = authService;
            _inAppUpdate = inAppUpdate;
            _ctsProvider = ctsProvider;
            _translation = translation;
            _hitBuilder = hitBuilder;
            _ftueFunnel = ftueFunnel;
            _eventBus = eventBus;
        }
        
        private void Start()
        {
            CAldaFrameworkInvoker.EnsureExistence();
        }

        public void Initialize()
        {
            LoadGame(_ctsProvider.Token).Forget();
        }

        private async UniTask LoadGame(CancellationToken ct)
        {
            _ftueFunnel.Send(EFtueFunnelStep.GameStart);
            _loadingTechFlow.Send(ELoadingTechFlow.Start);
            
            CSrDebugger srDebugger = new();
            await srDebugger.TryActivate(ct);
            
            await LoadBundles(ct, EBundleId.BaseGameScene);
            _loadingTechFlow.Send(ELoadingTechFlow.BaseGameBundlesLoaded);

            _translation.SetLanguage((ELanguageCode)_settingsData.Language.Value);
            SetLoadingText("Loading.ConnectingToServer");
            float loadingStartTime = Time.realtimeSinceStartup;

            UniTask baseSceneLoadTask = _sceneManager.StartBaseSceneLoadingAsync(ct);

            UniTask serviceThread = _serviceConnectionThread.InitOnlineServicesAsync();
            UniTask<CResponseHit> serverThread = _serverConnectionThread.ConnectAsync(ct);
            
            _loadingTechFlow.Send(ELoadingTechFlow.WaitingForServer);

            while (serverThread.Status == UniTaskStatus.Pending)
            {
                float loadingTime = Time.realtimeSinceStartup - loadingStartTime;
                bool isLoadingTooLong = loadingTime > 10f;
                if(isLoadingTooLong)
                {
                    SetLoadingText("Loading.RetryingConnection");
                }
                await UniTask.Yield(ct);
            }
            
            _loadingTechFlow.Send(ELoadingTechFlow.ServerResponseReceived);

            CResponseHit serverResponse = await serverThread;
            CConnectResponse connectResponse = serverResponse as CConnectResponse;

            if (connectResponse is not null)
            {
                await _validServerConnectionPostprocess.PostprocessServerConnection(connectResponse, ct);
            }
            
            _sceneManager.AllowBaseSceneActivation();
            await baseSceneLoadTask;
            
            _loadingTechFlow.Send(ELoadingTechFlow.BaseGameSceneActivated);
            
            await LoadBundles(ct, EBundleId.CoreGameScenes);
            await _sceneManager.LoadSceneAsync(ESceneId.Ui, LoadSceneMode.Additive, ct);
            
            _loadingTechFlow.Send(ELoadingTechFlow.UiSceneActivated);
            
            _loadingScreen.Show(ct, 0f).Forget();
            _loadingScreen.SetInfoText("Loading.LoadingGame", true);
            
            _serviceConnectionThread.InitOfflineServices();
            _loadingTechFlow.Send(ELoadingTechFlow.OfflineServicesInited);
            bool isOnline = await _internetReachabilityChecker.CheckForInternetConnection(ct);
            if (!isOnline)
            {
                ShowNoInternetError();
                return;
            }
            _loadingTechFlow.Send(ELoadingTechFlow.NetworkStatusChecked);

            if (connectResponse is null)
            {
                HandleBadServerResponse(serverResponse, ct).Forget();
                return;
            }
            
            _loadingTechFlow.Send(ELoadingTechFlow.ServerResponseParsed);
            
            await TryAcceptPrivacyPolicy(connectResponse, ct);
            _loadingTechFlow.Send(ELoadingTechFlow.PrivacyAccepted);
            
            _ftueFunnel.Send(EFtueFunnelStep.IntroStart);
            await TryShowIntro(connectResponse.User, ct);
            _ftueFunnel.Send(EFtueFunnelStep.IntroFinish);
            
            _loadingTechFlow.Send(ELoadingTechFlow.IntroSeen);

            _inAppUpdate.TryFlexibleUpdateAsync().Forget();
            
            await serviceThread;
            _loadingTechFlow.Send(ELoadingTechFlow.ServicesInited);
            CCoreGameGameModeData coreGameModeData = new(connectResponse.User.Progress.Region);
            _eventBus.ProcessTaskAsync(new CLoadGameModeTask(coreGameModeData), ct).Forget();
            
            await SceneManager.UnloadSceneAsync(gameObject.scene).ToUniTask(cancellationToken: ct);
            _loadingTechFlow.Send(ELoadingTechFlow.Completed);
        }
        
        private void SetLoadingText(string langKey)
        {
            string content = _translation.GetText(langKey);
            _text.SetValue(content);
        }

        private async UniTaskVoid HandleBadServerResponse(CResponseHit response, CancellationToken ct)
        {
            if (response is CAccountDeletionPendingResponse deletionPending)
            {
                await ProcessAccountDeletion(deletionPending.TimeToDeleteAccountInMs, ct);
                return;
            }
            
            if (response != null && response is not CErrorResponse)
            {
                _errorHandler.HandleErrorResponse(response);
                return;
            }
            
            bool serverStatus = await GetServerStatusAsync(ct);
            if (serverStatus)
            {
                ShowNoInternetError();
                return;
            }
            _errorHandler.ShowPlanedMaintenanceDialog();
        }
            
        private async UniTask ProcessAccountDeletion(long deletionTimeInMs, CancellationToken ct)
        {
            bool reactivateAccount = await _errorHandler.ShowAccountDeletionPending(deletionTimeInMs);
            if (reactivateAccount)
            {
                string authUid = _authService.GetAuthUidOrDefault();
                await _hitBuilder.GetBuilder(new CCancelAccountDeletionRequest(authUid))
                    .SetExecuteImmediately()
                    .BuildAndSendAsync<CAccountDeletionPendingResponse>(ct);
                _restartGameHandler.RestartGame(null);
                return;
            }
			
            _exitAppHandler.ExitApp();
        }

        private async UniTask<bool> GetServerStatusAsync(CancellationToken ct)
        {
            string response = await _remoteDatabase.TryGetValueAsync(ct, "ServerStatus");

            if (int.TryParse(response, out int status))
                return status == 1;
            return false;
        }

        private void ShowNoInternetError()
        {
            _errorHandler.ShowNoInternetConnectionDialog();
        }

        private async UniTask TryAcceptPrivacyPolicy(CConnectResponse response, CancellationToken ct)
        {
            if(CDebugConfig.Instance.ShouldSkip(EEditorSkips.Popups))
                return;
			
            if(response.User.Account.PrivacyConsent != EPrivacyConsentStatus.NotDetermined)
                return;
            
            const string privacyPolicyLink = "https://aldagames.com/application-privacy-statement/";
            const string termsOfUseLink = "https://aldagames.com/terms-of-service/";

            COpenTermsOfUseScreenTask task = new(privacyPolicyLink, termsOfUseLink, OnConfirm);
            _eventBus.ProcessTask(task);
            await CWaitForSignal.WaitForSignalAsync<CScreenCloseStartSignal>(signal => signal.MenuId == (int) EScreenId.TermsOfUse, _eventBus, ct);
            return;

            void OnConfirm()
            {
                CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CAcceptPrivacyStatusRequest());
                hit.BuildAndSend();
            }
        }

        private async UniTask TryShowIntro(CUserDto user, CancellationToken ct)
        {
            if(CDebugConfig.Instance.ShouldSkip(EEditorSkips.IntroCutscene))
                return;
            
            CGlobalVariableDto seen = user.GlobalVariables.GlobalVariables.FirstOrDefault(entry => entry.Id == EGlobalVariable.IntroSeen);
            if(seen is { StringValue: "1" })
                return;
            
            CanvasGroup canvasGroup = gameObject.GetComponentInChildren<CanvasGroup>(true);
            canvasGroup.alpha = 0;

            await LoadBundles(ct, EBundleId.Intro);
            
            await _sceneManager.LoadSceneAsync(ESceneId.Intro, LoadSceneMode.Additive, ct);
            await CWaitForSignal.WaitForSignalAsync<CIntroFinishedSignal>(_eventBus, ct);
            await _sceneManager.UnloadSceneAsync(ESceneId.Intro, ct);
            
            _bundleManager.UnloadBundle((int) EBundleId.Intro, true);
            
            canvasGroup.alpha = 1;
        }

        private async UniTask LoadBundles(CancellationToken ct, params EBundleId[] bundles)
        {
            int[] bundleIds = bundles.Select(b => (int)b).ToArray();
            CBundleLoadResult load = _bundleManager.LoadBundles(bundleIds, ct);
            await UniTask.WaitUntil(() => load.IsDone(), cancellationToken: ct);
        }
    }
}