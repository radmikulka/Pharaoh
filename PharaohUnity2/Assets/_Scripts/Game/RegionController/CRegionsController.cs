// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.09.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;
using ServerData.Hits;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TycoonBuilder
{
	public class CRegionsController : ICameraPlaneProvider, IInitializable, IRegionsController
	{
		private readonly Dictionary<ERegion, CRegionController> _loadedRegions = new();

		private readonly CRequiredBundlesDownloader _requiredBundlesDownloader;
		private readonly CSceneLightingHandler _sceneLightingHandler;
		private readonly CResourceConfigs _resourceConfigs;
		private readonly ILoadingScreen _loadingScreen;
		private readonly ISceneManager _sceneManager;
		private readonly ICtsProvider _ctsProvider;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;

		private CSceneLightingLock _activeSceneLightingLock;
		private CRegionController _activeRegion;
		private ERegion _activeCoreGameRegion;
		
		public ERegion ActiveRegion { get; private set; }

		public CRegionsController(
			CRequiredBundlesDownloader requiredBundlesDownloader, 
			CSceneLightingHandler sceneLightingHandler, 
			CResourceConfigs resourceConfigs,
			ILoadingScreen loadingScreen,
			ISceneManager sceneManager, 
			ICtsProvider ctsProvider, 
			CHitBuilder hitBuilder, 
			IEventBus eventBus, 
			CUser user
			)
		
		{
			_requiredBundlesDownloader = requiredBundlesDownloader;
			_sceneLightingHandler = sceneLightingHandler;
			_resourceConfigs = resourceConfigs;
			_loadingScreen = loadingScreen;
			_sceneManager = sceneManager;
			_ctsProvider = ctsProvider;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			_eventBus.AddAsyncTaskHandler<CMoveToNextRegionTask>(HandleMoveToNextRegion);
		}

		public Plane GetCameraPlane()
		{
			if (!_activeRegion)
				return new Plane(Vector3.zero, Vector3.up);
			return _activeRegion.GetCameraPlane();
		}

		public async UniTask LoadRegion(ERegion region, CancellationToken ct)
		{
			await _loadingScreen.Show(ct);
			await OpenRegion(region, ct);
			await _loadingScreen.Hide(ct);
		}

		public async UniTask LoadCurrentRegion(ERegion region, CancellationToken ct)
		{
			if (_activeCoreGameRegion != region)
				await UnloadRegionScene(_activeCoreGameRegion, ct);

			_activeCoreGameRegion = region;
			await OpenRegion(region, ct);
		}

		public async UniTask LoadLiveEvent(ELiveEvent liveEventId, CancellationToken ct)
		{
			ILiveEventContent liveEvent = _user.LiveEvents.GetEventContent<ILiveEventContent>(liveEventId);
			ERegion region = liveEvent switch
			{
				CNormalEventContent normalEventContent => normalEventContent.Region,
				_ => ERegion.None
			};
			await OpenRegion(region, ct);
		}

		public bool IsRegionActive(ERegion regionId)
		{
			return ActiveRegion == regionId;
		}

		private async Task HandleMoveToNextRegion(CMoveToNextRegionTask task, CancellationToken ct)
		{
			_user.Progress.IncreaseRegion();
			_hitBuilder.GetBuilder(new CGoNextRegionRequest()).BuildAndSend();
			CCoreGameGameModeData data = new(_user.Progress.Region);

			Task entranceSequenceTask = null;
			await _eventBus.ProcessTaskAsync(new CLoadGameModeTask(data, loadToken =>
			{
				entranceSequenceTask = PlayEntranceSequenceAsync(loadToken);
				return Task.CompletedTask;
			}), _ctsProvider.Token);

			await entranceSequenceTask;
			return;

			async Task PlayEntranceSequenceAsync(CancellationToken entranceToken)
			{
				_eventBus.ProcessTask(new CTryKillScreenTask(EScreenId.DecadePass));
				_activeRegion.ActivateCoveringFog();
				
				await _eventBus.ProcessTaskAsync(new CPingPongFullScreenOverlayTask(async blendToken =>
				{
					await Task.Delay(2000, blendToken);
				}, 0f), entranceToken);
				
				_eventBus.Send(new CYearIncreasedSignal(_user.Progress.Year));
				await CWaitForSignal.WaitForSignalAsync<CYearSeenSignal>(_eventBus, entranceToken);
				_activeRegion.PlayUncoverAnimation(entranceToken).Forget();
			}
		}

		private async UniTask OpenRegion(ERegion region, CancellationToken ct)
		{
			ActiveRegion = region;
			
			_sceneLightingHandler.RemoveSceneLightingLock(_activeSceneLightingLock);
			if (_activeRegion)
			{
				_activeRegion.SetActive(false);
			}
			
			await _requiredBundlesDownloader.DownloadBundlesAsync(region, ct);
			await _sceneManager.LoadSceneAsync(ESceneId.CoreGame, LoadSceneMode.Additive, ct);

			if (!_loadedRegions.TryGetValue(region, out CRegionController regionObj))
			{
				regionObj = await LoadRegionScene(region, ct);
				_loadedRegions.Add(region, regionObj);
			}

			_activeRegion = regionObj;
			_activeRegion.SetActive(true);
			
			ActivateActiveRegionLighting(region);

			_eventBus.Send(new CRegionActivatedSignal(region));
		}

		private void ActivateActiveRegionLighting(ERegion region)
		{
			CRegionResourceConfig regionConfig = _resourceConfigs.Regions.GetConfig(region);
			_activeSceneLightingLock = CSceneLightingLock.Region(regionConfig.MainScene);
			_sceneLightingHandler.AddSceneLightingLock(_activeSceneLightingLock);
		}

		private async UniTask<CRegionController> LoadRegionScene(ERegion region, CancellationToken ct)
		{
			CRegionResourceConfig regionConfig = _resourceConfigs.Regions.GetConfig(region);
			Scene scene = await _sceneManager.LoadSceneAsync(regionConfig.MainScene, LoadSceneMode.Additive, ct);
			return scene.GetRootGameObjects()[0].GetComponentInChildren<CRegionController>();
		}

		private async UniTask UnloadRegionScene(ERegion region, CancellationToken ct)
		{
			if(region == ERegion.None)
				return;
			
			_eventBus.Send(new CRegionUnloadedStartedSignal(region));
			CRegionResourceConfig regionConfig = _resourceConfigs.Regions.GetConfig(region);
			await _sceneManager.UnloadSceneAsync(regionConfig.MainScene, ct);
		}
	}
}