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

namespace Pharaoh
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

		public async UniTask LoadRegion(EMissionId region, CancellationToken ct)
		{
			await _loadingScreen.Show(ct);
			await OpenRegion(region, ct);
			await _loadingScreen.Hide(ct);
		}

		private async UniTask OpenRegion(EMissionId region, CancellationToken ct)
		{
			ActiveMission = region;

			await _requiredBundlesDownloader.DownloadBundlesAsync(region, ct);
			await _sceneManager.LoadSceneAsync(ESceneId.CoreGame, LoadSceneMode.Additive, ct);
			_activeMission = await LoadRegionScene(region, ct);

			_eventBus.Send(new CMissionActivatedSignal(region));
		}

		private async UniTask<CMissionController> LoadRegionScene(EMissionId region, CancellationToken ct)
		{
			CMissionResourceConfig regionConfig = _resourceConfigs.Missions.GetConfig(region);
			Scene scene = await _sceneManager.LoadSceneAsync(regionConfig.SceneId, LoadSceneMode.Additive, ct);
			return scene.GetRootGameObjects()[0].GetComponentInChildren<CMissionController>();
		}

		private async UniTask UnloadRegionScene(EMissionId mission, CancellationToken ct)
		{
			if(mission == EMissionId.None)
				return;
			
			_eventBus.Send(new CMissionUnloadedStartedSignal(mission));
			CMissionResourceConfig regionConfig = _resourceConfigs.Missions.GetConfig(mission);
			await _sceneManager.UnloadSceneAsync(regionConfig.SceneId, ct);
		}
	}
}