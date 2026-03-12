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
using Pharaoh.Loading;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using Pharaoh;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Pharaoh
{
    public class CConnectionScreen : MonoBehaviour, IInitializable
    {
        private CServerConnectionInitializer _serverConnectionInitializer;
        private CInternetReachabilityChecker _internetReachabilityChecker;
        private CServiceConnectionThread _serviceConnectionThread;
        private CServerConnectionThread _serverConnectionThread;
        private CLoadingFunnelTracker _loadingFunnelTracker;
        private IRemoteDatabase _remoteDatabase;
        private IBundleManager _bundleManager;
        private ILoadingScreen _loadingScreen;
        private ISceneManager _sceneManager;
        private CErrorHandler _errorHandler;
        private ICtsProvider _ctsProvider;
        private IInAppUpdate _inAppUpdate;
        private ISettings _settingsData;
        private IEventBus _eventBus;

        [Inject]
        private void Inject(
            CServerConnectionInitializer serverConnectionInitializer,
            CInternetReachabilityChecker internetReachabilityChecker,
            CServiceConnectionThread serviceConnectionThread,
            CServerConnectionThread serverConnectionThread,
            IRestartGameHandler restartGameHandler,
            CLoadingFunnelTracker loadingFunnelTracker,
            IExitAppHandler exitAppHandler,
            IRemoteDatabase remoteDatabase,
            ILoadingScreen loadingScreen, 
            IBundleManager bundleManager,
            CErrorHandler errorHandler, 
            ISceneManager sceneManager, 
            IAuthService authService, 
            IInAppUpdate inAppUpdate, 
            ICtsProvider ctsProvider, 
            ITranslation translation, 
            CRequestSender hitBuilder,
            ISettings settingsData,
            IEventBus eventBus
            )
        {
            _serverConnectionInitializer = serverConnectionInitializer;
            _internetReachabilityChecker = internetReachabilityChecker;
            _serviceConnectionThread = serviceConnectionThread;
            _serverConnectionThread = serverConnectionThread;
            _loadingFunnelTracker = loadingFunnelTracker;
            _remoteDatabase = remoteDatabase;
            _loadingScreen = loadingScreen;
            _bundleManager = bundleManager;
            _errorHandler = errorHandler;
            _sceneManager = sceneManager;
            _settingsData = settingsData;
            _inAppUpdate = inAppUpdate;
            _ctsProvider = ctsProvider;
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
            _loadingFunnelTracker.Send(ELoadingFunnelStep.Start);
            
            CSrDebugger srDebugger = new();
            await srDebugger.TryActivate(ct);
            
            await LoadBundles(ct, EBundleId.BaseGameScene);
            _loadingFunnelTracker.Send(ELoadingFunnelStep.BaseGameBundlesLoaded);

            UniTask baseSceneLoadTask = _sceneManager.StartBaseSceneLoadingAsync(ct);

            UniTask serviceThread = _serviceConnectionThread.InitOnlineServicesAsync();
            UniTask<CResponseHit> serverThread = _serverConnectionThread.ConnectAsync(ct);
            
            _loadingFunnelTracker.Send(ELoadingFunnelStep.WaitingForServer);

            await serverThread;
            
            _loadingFunnelTracker.Send(ELoadingFunnelStep.ServerResponseReceived);

            CResponseHit serverResponse = await serverThread;
            CConnectResponse connectResponse = serverResponse as CConnectResponse;

            //if (connectResponse is not null)
            {
                await _serverConnectionInitializer.InitializeServerConnection(connectResponse, ct);
            }
            
            _sceneManager.AllowBaseSceneActivation();
            await baseSceneLoadTask;
            
            _loadingFunnelTracker.Send(ELoadingFunnelStep.BaseGameSceneActivated);
            
            await LoadBundles(ct, EBundleId.CoreGameScenes);
            await _sceneManager.LoadSceneAsync(ESceneId.Ui, LoadSceneMode.Additive, ct);
            
            _loadingFunnelTracker.Send(ELoadingFunnelStep.UiSceneActivated);
            
            _loadingScreen.Show(ct, 0f).Forget();
            _loadingScreen.SetInfoText("Loading.LoadingGame", true);
            
            _serviceConnectionThread.InitOfflineServices();
            _loadingFunnelTracker.Send(ELoadingFunnelStep.OfflineServicesInited);
            bool isOnline = await _internetReachabilityChecker.CheckForInternetConnection(ct);
            if (!isOnline)
            {
                ShowNoInternetError();
                return;
            }
            _loadingFunnelTracker.Send(ELoadingFunnelStep.NetworkStatusChecked);

            /*if (connectResponse is null)
            {
                HandleBadServerResponse(serverResponse, ct).Forget();
                return;
            }*/

            _inAppUpdate.TryFlexibleUpdateAsync().Forget();
            
            await serviceThread;
            _loadingFunnelTracker.Send(ELoadingFunnelStep.ServicesInited);
            //CCoreGameModeData coreGameModeData = new(connectResponse.User.Progress.MissionId);
            CCoreGameModeData coreGameModeData = new(EMissionId.Mission1_1, EMonumentId.Pyramid);
            _eventBus.ProcessTaskAsync(new CLoadGameModeTask(coreGameModeData), ct).Forget();
            
            await SceneManager.UnloadSceneAsync(gameObject.scene).ToUniTask(cancellationToken: ct);
            _loadingFunnelTracker.Send(ELoadingFunnelStep.Completed);
        }

        private async UniTaskVoid HandleBadServerResponse(CResponseHit response, CancellationToken ct)
        {
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

        private async UniTask LoadBundles(CancellationToken ct, params EBundleId[] bundles)
        {
            int[] bundleIds = bundles.Select(b => (int)b).ToArray();
            CBundleLoadResult load = _bundleManager.LoadBundles(bundleIds, ct);
            await UniTask.WaitUntil(() => load.IsDone(), cancellationToken: ct);
        }
    }
}