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
	public class CMissionController : ICameraPlaneProvider, IMissionController
	{
		private readonly CRequiredBundlesDownloader _requiredBundlesDownloader;
		private readonly CResourceConfigs _resourceConfigs;
		private readonly ILoadingScreen _loadingScreen;
		private readonly ISceneManager _sceneManager;
		private readonly IEventBus _eventBus;

		private CMission _activeMission;

		public EMissionId ActiveMissionId => _activeMission.MissionId;

		public CMissionController(
			CRequiredBundlesDownloader requiredBundlesDownloader, 
			CResourceConfigs resourceConfigs,
			ILoadingScreen loadingScreen,
			ISceneManager sceneManager, 
			IEventBus eventBus
			)
		
		{
			_requiredBundlesDownloader = requiredBundlesDownloader;
			_resourceConfigs = resourceConfigs;
			_loadingScreen = loadingScreen;
			_sceneManager = sceneManager;
			_eventBus = eventBus;
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
			await OpenRegion(mission, ct);
			await _loadingScreen.Hide(ct);
		}

		private async UniTask OpenRegion(EMissionId mission, CancellationToken ct)
		{
			await _requiredBundlesDownloader.DownloadBundlesAsync(mission, ct);
			await _sceneManager.LoadSceneAsync(ESceneId.CoreGame, LoadSceneMode.Additive, ct);
			
			_activeMission = await LoadMissionScene(mission, ct);
			_eventBus.Send(new CMissionActivatedSignal(mission));
		}

		private async UniTask<CMission> LoadMissionScene(EMissionId mission, CancellationToken ct)
		{
			CMissionConfig missionConfig = _resourceConfigs.Missions.GetConfig(mission);
			Scene scene = await _sceneManager.LoadSceneAsync(missionConfig.Scene, LoadSceneMode.Additive, ct);
			return scene.GetRootGameObjects()[0].GetComponentInChildren<CMission>();
		}
	}
}