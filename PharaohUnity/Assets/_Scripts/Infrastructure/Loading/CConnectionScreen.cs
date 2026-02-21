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
        [SerializeField] private CUiComponentText _text;
        
        private CConnectionEventsHandler _connectionEventsHandler;
        private CInternetReachabilityChecker _internetReachabilityChecker;
        private CServiceConnectionThread _serviceConnectionThread;
        private CServerConnectionThread _serverConnectionThread;
        private IBundleManager _bundleManager;
        private ILoadingScreen _loadingScreen;
        private ISceneManager _sceneManager;
        private CErrorHandler _errorHandler;
        private ICtsProvider _ctsProvider;
        private IInAppUpdate _inAppUpdate;
        private ITranslation _translation;
        private IEventBus _eventBus;

        [Inject]
        private void Inject(
            CConnectionEventsHandler connectionEventsHandler,
            CInternetReachabilityChecker internetReachabilityChecker,
            CServiceConnectionThread serviceConnectionThread,
            CServerConnectionThread serverConnectionThread,
            IRestartGameHandler restartGameHandler,
            IExitAppHandler exitAppHandler,
            ILoadingScreen loadingScreen, 
            IBundleManager bundleManager,
            CErrorHandler errorHandler, 
            ISceneManager sceneManager, 
            IInAppUpdate inAppUpdate, 
            ICtsProvider ctsProvider, 
            ITranslation translation,
            CHitBuilder hitBuilder,
            IEventBus eventBus
            )
        {
            _connectionEventsHandler = connectionEventsHandler;
            _internetReachabilityChecker = internetReachabilityChecker;
            _serviceConnectionThread = serviceConnectionThread;
            _serverConnectionThread = serverConnectionThread;
            _loadingScreen = loadingScreen;
            _bundleManager = bundleManager;
            _errorHandler = errorHandler;
            _sceneManager = sceneManager;
            _inAppUpdate = inAppUpdate;
            _ctsProvider = ctsProvider;
            _translation = translation;
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
            CSrDebugger srDebugger = new();
            await srDebugger.TryActivate(ct);
            
            _connectionEventsHandler.PreprocessServerConnection();
            
            await LoadBundles(ct, EBundleId.BaseGameScene);

            _translation.SetLanguage(ELanguageCode.En);
            SetLoadingText("Loading.ConnectingToServer");
            float loadingStartTime = Time.realtimeSinceStartup;

            UniTask baseSceneLoadTask = _sceneManager.StartBaseSceneLoadingAsync(ct);

            UniTask serviceThread = _serviceConnectionThread.InitOnlineServicesAsync();
            UniTask<CResponseHit> serverThread = _serverConnectionThread.ConnectAsync(ct);

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

            CResponseHit serverResponse = await serverThread;
            CConnectResponse connectResponse = serverResponse as CConnectResponse;

            if (connectResponse is not null)
            {
                _connectionEventsHandler.PostprocessServerConnection(connectResponse);
            }
            
            _sceneManager.AllowBaseSceneActivation();
            await baseSceneLoadTask;
            
            await LoadBundles(ct, EBundleId.CoreGameScenes);
            await _sceneManager.LoadSceneAsync(ESceneId.Ui, LoadSceneMode.Additive, ct);
            
            _loadingScreen.Show(ct, 0f).Forget();
            _loadingScreen.SetInfoText("Loading.LoadingGame", true);
            
            _serviceConnectionThread.InitOfflineServices();

            if (connectResponse is null)
            {
                HandleBadServerResponse(serverResponse);
                return;
            }

            _inAppUpdate.TryFlexibleUpdateAsync().Forget();
            
            await serviceThread;
            CCoreGameGameModeData coreGameModeData = new(connectResponse.User.ActiveMission.MissionId);
            _eventBus.ProcessTaskAsync(new CLoadGameModeTask(coreGameModeData), ct).Forget();
            
            await SceneManager.UnloadSceneAsync(gameObject.scene).ToUniTask(cancellationToken: ct);
        }
        
        private void SetLoadingText(string langKey)
        {
            string content = _translation.GetText(langKey);
            _text.SetValue(content);
        }

        private void HandleBadServerResponse(CResponseHit response)
        {
            if (response != null && response is not CErrorResponse)
            {
                _errorHandler.HandleErrorResponse(response);
            }
        }

        private async UniTask LoadBundles(CancellationToken ct, params EBundleId[] bundles)
        {
            int[] bundleIds = bundles.Select(b => (int)b).ToArray();
            CBundleLoadResult load = _bundleManager.LoadBundles(bundleIds, ct);
            await UniTask.WaitUntil(() => load.IsDone(), cancellationToken: ct);
        }
    }
}