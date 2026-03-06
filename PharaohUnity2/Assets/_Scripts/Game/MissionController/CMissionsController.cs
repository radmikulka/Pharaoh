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
using ServerData.Hits;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TycoonBuilder
{
	public class CMissionsController : ICameraPlaneProvider, IMissionController
	{
		private readonly CRequiredBundlesDownloader _requiredBundlesDownloader;
		private readonly CResourceConfigs _resourceConfigs;
		private readonly ILoadingScreen _loadingScreen;
		private readonly ISceneManager _sceneManager;
		private readonly ICtsProvider _ctsProvider;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;

		private CMissionController _activeMission;
		
		public EMissionId ActiveMission { get; private set; }

		public CMissionsController(
			CRequiredBundlesDownloader requiredBundlesDownloader, 
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
			_resourceConfigs = resourceConfigs;
			_loadingScreen = loadingScreen;
			_sceneManager = sceneManager;
			_ctsProvider = ctsProvider;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			_user = user;
		}

		public Plane GetCameraPlane()
		{
			if (!_activeMission)
				return new Plane(Vector3.zero, Vector3.up);
			return _activeMission.GetCameraPlane();
		}

		public UniTask LoadRegion(EMissionId region, CancellationToken ct)
		{
			throw new System.NotImplementedException();
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

		private async UniTask OpenRegion(ERegion region, CancellationToken ct)
		{
			ActiveMission = region;
			
			_sceneLightingHandler.RemoveSceneLightingLock(_activeSceneLightingLock);
			if (_activeMission)
			{
				_activeMission.SetActive(false);
			}
			
			await _requiredBundlesDownloader.DownloadBundlesAsync(region, ct);
			await _sceneManager.LoadSceneAsync(ESceneId.CoreGame, LoadSceneMode.Additive, ct);

			if (!_loadedRegions.TryGetValue(region, out CMissionController regionObj))
			{
				regionObj = await LoadRegionScene(region, ct);
				_loadedRegions.Add(region, regionObj);
			}

			_activeMission = regionObj;
			_activeMission.SetActive(true);
			
			ActivateActiveRegionLighting(region);

			_eventBus.Send(new CMissionActivatedSignal(region));
		}

		private async UniTask<CMissionController> LoadRegionScene(ERegion region, CancellationToken ct)
		{
			CRegionResourceConfig regionConfig = _resourceConfigs.Regions.GetConfig(region);
			Scene scene = await _sceneManager.LoadSceneAsync(regionConfig.MainScene, LoadSceneMode.Additive, ct);
			return scene.GetRootGameObjects()[0].GetComponentInChildren<CMissionController>();
		}

		private async UniTask UnloadRegionScene(ERegion region, CancellationToken ct)
		{
			if(region == ERegion.None)
				return;
			
			_eventBus.Send(new CMissionUnloadedStartedSignal(region));
			CRegionResourceConfig regionConfig = _resourceConfigs.Regions.GetConfig(region);
			await _sceneManager.UnloadSceneAsync(regionConfig.MainScene, ct);
		}
	}
}