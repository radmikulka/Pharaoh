// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.09.2025
// =========================================

using System;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using ServerData;
using ServerData.Design;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CVehicleRendererScene : MonoBehaviour, IConstructable, IInitializable
	{
		private const float ShadowDistance = 120f;
		
		[Serializable]
		private class CEnvironment
		{
			public EMovementType MovementType;
			public GameObject Environment;
			public Camera Camera;
		}
		
		[SerializeField] private CEnvironment[] _environments;
		[SerializeField] private CVehicleRotator _rotator;
		[SerializeField] private Transform _rendererPoint;
		[SerializeField] private Camera _rotatorCamera;

		private readonly CShadowDistanceOverride _shadowDistanceOverride = new(ShadowDistance, 10);
		private CDesignVehicleConfigs _vehicleConfigs;
		private CAldaInstantiator _aldaInstantiator;
		private CVehicleRendererData _rendererData;
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		private GameObject _activeVehicle;
		private GameObject _sceneRoot;
		private CRenderer _renderer;

		[Inject]
		private void Inject(
			CDesignVehicleConfigs vehicleConfigs,
			CAldaInstantiator aldaInstantiator,
			CResourceConfigs resourceConfigs,
			IBundleManager bundleManager,
			CRenderer rend
			)
		{
			_aldaInstantiator = aldaInstantiator;
			_resourceConfigs = resourceConfigs;
			_vehicleConfigs = vehicleConfigs;
			_bundleManager = bundleManager;
			_renderer = rend;
		}

		public void Construct()
		{
			SetActiveEnvironmentCamera(false);
			InitSceneRoot();
		}

		public void Initialize()
		{
			SetAllowRotation(false);
		}

		private void InitSceneRoot()
		{
			_sceneRoot = gameObject.scene.GetRootGameObjects()[0];
		}

		public void InitRenderer(RenderTexture renderTexture)
		{
			foreach (CEnvironment environment in _environments)
			{
				environment.Camera.targetTexture = renderTexture;
			}
			_rotatorCamera.targetTexture = renderTexture;
		}

		public void FreezeRenderer()
		{
			SetActiveEnvironmentCamera(false);
		}

		public async UniTask SetNewData(CVehicleRendererData data, CancellationToken ct)
		{
			bool needsUpdate = NeedsUpdate(data);
			PrepareScene();

			SetActiveMode(data);
			
			if (needsUpdate)
			{
				await LoadVehicle(data, ct);
			}

			_rendererData = data;
		}

		private void SetActiveMode(CVehicleRendererData data)
		{
			switch (data.Mode)
			{
				case EVehicleRendererMode.StaticDetail:
					InitEnvironmentCameraDistance(data);
					SetActiveEnvironmentCamera(true);
					SetAllowRotation(false);
					break;
				case EVehicleRendererMode.FullscreenDetail:
					InitRotatorBounds(data);
					SetActiveEnvironmentCamera(false);
					SetAllowRotation(true);
					_rotator.ResetCamera(
						data.VehicleConfig.FullscreenDetailDefaultCameraDistance, 
						data.VehicleConfig.FullscreenDetailCameraYOffset
						);
					break;
				default: throw new ArgumentOutOfRangeException(nameof(data.Mode), data.Mode, null);
			}
		}

		private bool NeedsUpdate(CVehicleRendererData data)
		{
			if(_rendererData == null)
				return true;
			if (!_activeVehicle)
				return true;
			return _rendererData.VehicleId != data.VehicleId || _rendererData.Mode != data.Mode;
		}

		private void InitRotatorBounds(CVehicleRendererData data)
		{
			float halfVehicleLength = data.VehicleConfig.MenuModelLenght * 0.5f;
			_rotator.SetCameraBounds(
				-halfVehicleLength, 
				halfVehicleLength, 
				data.VehicleConfig.FullscreenDetailMinCameraDistance, 
				data.VehicleConfig.FullscreenDetailMaxCameraDistance
				);
		}

		private void InitEnvironmentCameraDistance(CVehicleRendererData data)
		{
			CVehicleConfig dataConfig = _vehicleConfigs.GetConfig(data.VehicleId);
			CEnvironment vehicleEviro = GetVehicleEviro(dataConfig);
			vehicleEviro.Camera.transform.localPosition = data.VehicleConfig.DetailCameraLocalPosition;
		}

		private CEnvironment GetVehicleEviro(CVehicleConfig dataConfig)
		{
			return _environments.First(environment => environment.MovementType == dataConfig.MovementType);
		}

		private void SetAllowRotation(bool state)
		{
			_rotator.SetActive(state);
		}

		private async UniTask LoadVehicle(CVehicleRendererData data, CancellationToken ct)
		{
			TryDestroyActiveVehicle();
			
			CVehicleConfig dataConfig = _vehicleConfigs.GetConfig(data.VehicleId);
			SetActiveEnvironment(dataConfig.MovementType);
			if(!data.VehicleConfig.MenuPrefab.IsObjectAssigned)
				return;
			
			Transform prefab = await _bundleManager.LoadItemAsync<Transform>(data.VehicleConfig.MenuPrefab, EBundleCacheType.TemporaryCache, ct);
			ct.ThrowIfCancellationRequested();
			Transform vehicleTransform = _aldaInstantiator.Instantiate(prefab, _rendererPoint);
			vehicleTransform.localPosition = GetLocalPosition();
			vehicleTransform.localRotation = Quaternion.identity;
			
			_activeVehicle = vehicleTransform.gameObject;
			return;

			Vector3 GetLocalPosition()
			{
				if (data.Mode == EVehicleRendererMode.FullscreenDetail)
					return CVector3.Zero;
				CVehicleResourceConfig resourceConfig = _resourceConfigs.Vehicles.GetConfig(data.VehicleId);
				return resourceConfig.DetailSceneModelLocalPosition;
			}
		}

		private void PrepareScene()
		{
			_sceneRoot.SetActive(true);
			_renderer.AddShadowDistanceOverride(_shadowDistanceOverride);
		}

		private void SetActiveEnvironment(EMovementType movementType)
		{
			foreach (CEnvironment environment in _environments)
			{
				environment.Environment.SetActive(environment.MovementType == movementType);
			}
		}

		private void SetActiveEnvironmentCamera(bool state)
		{
			foreach (CEnvironment environment in _environments)
			{
				environment.Camera.enabled = state;
			}
		}

		private void TryDestroyActiveVehicle()
		{
			if (!_activeVehicle) 
				return;
			Destroy(_activeVehicle);
			_activeVehicle = null;
		}

		public void Disable()
		{
			_sceneRoot.SetActive(false);
			_renderer.ReleaseShadowDistanceOverride(_shadowDistanceOverride);
		}
	}
}