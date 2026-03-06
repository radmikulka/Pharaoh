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

		public async UniTask LoadMission(EMissionId mission, CancellationToken ct)
		{
			await _loadingScreen.Show(ct);
			await OpenMission(mission, ct);
			await _loadingScreen.Hide(ct);
		}

		private async UniTask OpenMission(EMissionId mission, CancellationToken ct)
		{
			ActiveMission = mission;

			await _requiredBundlesDownloader.DownloadBundlesAsync(mission, ct);
			await _sceneManager.LoadSceneAsync(ESceneId.CoreGame, LoadSceneMode.Additive, ct);
			_activeMission = await LoadMissionScene(mission, ct);

			_eventBus.Send(new CMissionActivatedSignal(mission));
		}

		private async UniTask<CMissionController> LoadMissionScene(EMissionId mission, CancellationToken ct)
		{
			CMissionResourceConfig missionConfig = _resourceConfigs.Missions.GetConfig(mission);
			Scene scene = await _sceneManager.LoadSceneAsync(missionConfig.SceneId, LoadSceneMode.Additive, ct);
			return scene.GetRootGameObjects()[0].GetComponentInChildren<CMissionController>();
		}

		private async UniTask UnloadMissionScene(EMissionId mission, CancellationToken ct)
		{
			if(mission == EMissionId.None)
				return;

			_eventBus.Send(new CMissionUnloadedStartedSignal(mission));
			CMissionResourceConfig missionConfig = _resourceConfigs.Missions.GetConfig(mission);
			await _sceneManager.UnloadSceneAsync(missionConfig.SceneId, ct);
		}
	}
}