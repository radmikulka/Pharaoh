// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.08.2025
// =========================================

using System;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using ServerData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using Zenject;

namespace TycoonBuilder
{
	public class CVehicleRenderer : MonoBehaviour, IAldaFrameworkComponent
	{
		private CSceneLightingHandler _sceneLightingHandler;
		private CVehicleRendererScene _rendererScene;
		private CancellationTokenSource _activeCts;
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		private bool _rendererSceneIsLoading;
		private RenderTexture _renderTexture;
		private ISceneManager _sceneManager;

		private readonly CSceneLightingLock _sceneLightingLock = CSceneLightingLock.VehicleDetail(); 

		[Inject]
		private void Inject(
			CSceneLightingHandler sceneLightingHandler, 
			CRegionsController regionsController, 
			CResourceConfigs resourceConfigs, 
			IBundleManager bundleManager, 
			ISceneManager sceneManager
			)
		{
			_sceneLightingHandler = sceneLightingHandler;
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
			_sceneManager = sceneManager;
		}
		
		public RenderTexture GetOrCreateRenderTexture(int width, int height)
		{
			if (_renderTexture && _renderTexture.width == width)
				return _renderTexture;

			if (_renderTexture)
			{
				Destroy(_renderTexture);
			}
			
			GraphicsFormat rtFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR);
			_renderTexture = new RenderTexture(width, height, 16, rtFormat)
			{
				antiAliasing = 4
			};
			_rendererScene?.InitRenderer(_renderTexture);
			return _renderTexture;
		}

		public async UniTask SetVehicle(EVehicle vehicleId, EVehicleRendererMode mode, CancellationToken ct)
		{
			_activeCts?.Cancel();
			_activeCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
			CancellationToken token = _activeCts.Token;
			
			ClearRt();

			if (_rendererScene)
			{
				_rendererScene.FreezeRenderer();
			}
			await LoadRendererScene(token);

			_sceneLightingHandler.AddSceneLightingLock(_sceneLightingLock);
			
			CVehicleResourceConfig vehicleConfig = _resourceConfigs.Vehicles.GetConfig(vehicleId);
			CVehicleRendererData data = new(vehicleConfig, mode);
			await _rendererScene.SetNewData(data, token);
		}

		public void Disable()
		{
			_activeCts?.Cancel();
			_rendererScene.Disable();
			_sceneLightingHandler.RemoveSceneLightingLock(_sceneLightingLock);
		}

		private void ClearRt()
		{
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = _renderTexture;
			GL.Clear(true, true, Color.black);
			RenderTexture.active = active;
		}

		private async UniTask LoadRendererScene(CancellationToken ct)
		{
			if (_rendererSceneIsLoading)
			{
				await UniTask.WaitUntil(() => !_rendererSceneIsLoading, cancellationToken: ct);
				return;
			}
			
			if(_rendererScene)
				return;
			
			_rendererSceneIsLoading = true;
			CBundleLoadResult loadBundles = _bundleManager.LoadBundles(new[] { (int)EBundleId.VehicleDetailScene }, ct);
			await UniTask.WaitUntil(() => loadBundles.IsDone(), cancellationToken: ct);
			
			//await UniTask.WaitForSeconds(2f, cancellationToken: ct);
			
			Scene scene = await _sceneManager.LoadSceneAsync(ESceneId.VehicleDetail, LoadSceneMode.Additive, ct);
			_rendererScene = scene.GetRootGameObjects()[0].GetComponentInChildren<CVehicleRendererScene>();
			_rendererScene.InitRenderer(_renderTexture);
			_rendererSceneIsLoading = false;
		}
	}
}