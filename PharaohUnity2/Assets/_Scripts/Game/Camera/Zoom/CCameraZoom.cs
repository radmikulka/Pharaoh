// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.12.2024
// =========================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using TycoonBuilder;
using Unity.Cinemachine;
using Zenject;

namespace TycoonBuilder
{
	public class CCameraZoom : ValidatedMonoBehaviour, 
		IConstructable, IInitializable, ICameraZoom
	{
		[SerializeField, Child] private CinemachineCameraOffset _cinemachineOffset;
		[SerializeField, Child] private CinemachineCamera _cinemachineCamera;

		private IMainCameraProvider _mainCameraProvider;
		private CCameraZoomProxy _cameraZoomProxy;
		private CEventSystem _eventSystem;
		private IEventBus _eventBus;

		private readonly HashSet<IOverrideMinMaxZoomValue> _overrideMinMaxZoom = new();
		private readonly HashSet<IOverrideZoomValue> _overrideZoomValues = new();

		public float CurrentAbsoluteZoomValue => GetMinMaxZoom().max * TargetZoom;
		public float TargetZoom { get; private set; }

		[Inject]
		private void Inject(
			IMainCameraProvider mainCameraProvider, 
			CCameraZoomProxy cameraZoomProxy,
			CEventSystem eventSystem,
			IEventBus eventBus
			)
		{
			_mainCameraProvider = mainCameraProvider;
			_cameraZoomProxy = cameraZoomProxy;
			_eventSystem = eventSystem;
			_eventBus = eventBus;
		}
		
		public void Construct()
		{
			SetMaxZoom();
			UpdateCameraPositionImmediate();
		}
		
		public void Initialize()
		{
			_cameraZoomProxy.SetInstance(this);
			_eventBus.Subscribe<CPinchUpdateSignal>(OnPinchUpdate);
		}

		public void AddZoomOverride(IOverrideZoomValue value)
		{
			_overrideZoomValues.Add(value);
		}

		public void RemoveZoomOverride(IOverrideZoomValue value)
		{
			_overrideZoomValues.Remove(value);
		}

		public void SetAbsoluteCameraZoom(float absoluteZoomValue)
		{
			float zoomPercentage = CMath.InverseLerp(CDesignCameraConfig.MinZoom, CDesignCameraConfig.MaxZoom, absoluteZoomValue);
			SetZoomValue(zoomPercentage);
		}

		public void AddMinMaxZoomOverride(IOverrideMinMaxZoomValue value)
		{
			_overrideMinMaxZoom.Add(value);
		}

		public void RemoveMinMaxZoomOverride(IOverrideMinMaxZoomValue value)
		{
			_overrideMinMaxZoom.Remove(value);
		}

		public void SetTargetZoom(float zoomPercentage)
		{
			SetZoomValue(zoomPercentage);
		}

		private void SetMaxZoom()
		{
			SetZoomValue(CDesignCameraConfig.MaxZoom);
		}
		
		public void SetZoomValue(float value)
		{
			TargetZoom = CMath.Clamp01(value);
		}

		private void OnPinchUpdate(CPinchUpdateSignal signal)
		{
			bool inputLocked = IsInputLocked();
			if(inputLocked)
				return;
			
			float sensitivity = GetZoomSensitivity();
			float zoomScaleMod = GetZoomScaleMod();
			sensitivity *= zoomScaleMod;
			
			AddZoomValue(signal.Delta * sensitivity);
		}
		
		private bool IsInputLocked()
		{
			bool isLive = _mainCameraProvider.Camera.IsLiveCamera(_cinemachineCamera);
			if (!isLive)
				return true;
			
			return _eventSystem.IsInputLocked();
		}

		private float GetZoomScaleMod()
		{
			float originalZoomDiff = CDesignCameraConfig.MaxZoom - CDesignCameraConfig.MinZoom;
			var overrideMinMax = GetMinMaxZoom();
			float overrideZoomDiff = overrideMinMax.max - overrideMinMax.min;
			float mod = originalZoomDiff / overrideZoomDiff;
			return mod;
		}

		private float GetZoomSensitivity()
		{
			if (CPlatform.IsEditor)
			{
				if(!CPlatform.IsUnityRemoteConnected)
					return Time.deltaTime * 8;
				return Time.deltaTime;
			}
			return Time.deltaTime * 0.06f;
		}
		
		private void AddZoomValue(float value)
		{
			SetZoomValue(TargetZoom - value);
		}

		private void LateUpdate()
		{
			UpdateCameraPositionAnimated();
			UpdateMinHeight();
		}

		private void UpdateMinHeight()
		{
			float minGroundOffset = GetMinGroundOffset();
			Vector3 targetLocalPosition = new(0f, minGroundOffset, 0f);
			
			transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, Time.deltaTime * 6f);
		}

		private float GetMinGroundOffset()
		{
			bool isZoomOverrideActive = IsZoomOverrideActive();
			if (isZoomOverrideActive)
				return 0f;
			
			float groundHeight = GetGroundHeight();
			float cameraHeight = _cinemachineCamera.State.GetFinalPosition().y;
			float distanceToGround = cameraHeight - groundHeight;
			float minCameraLocalHeight = CMath.Max(0f, CDesignCameraConfig.MinDistanceToGround - distanceToGround);
			return minCameraLocalHeight;
		}

		private float GetGroundHeight()
		{
			Vector3 origin = _cinemachineCamera.State.GetFinalPosition();
			origin.y = 2000;
			
			bool hit = Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, 2000, CObjectLayerMask.Default);
			CExtendedGizmos.DrawSphere(hitInfo.point, Color.magenta, 1f, Time.deltaTime);
			return !hit ? 0f : hitInfo.point.y;
		}
		
		private void UpdateCameraPositionAnimated()
		{
			(float min, float max) bounds = GetMinMaxZoom();
			float targetZoom = -CMath.Lerp(bounds.min, bounds.max, TargetZoom);
			float finalZoom = GetOverrideZoom() ?? targetZoom;
			float newValue = CMath.Lerp(_cinemachineOffset.Offset.z, finalZoom, Time.deltaTime * 10f);
			_cinemachineOffset.Offset.z = newValue;
		}

		public float GetMaxZoom()
		{
			return GetMinMaxZoom().max;
		}
		
		public float GetMinZoom()
		{
			return GetMinMaxZoom().min;
		}

		private (float min, float max) GetMinMaxZoom()
		{
			float minZoom = CDesignCameraConfig.MinZoom;
			float maxZoom = CDesignCameraConfig.MaxZoom;
			int maxPriority = int.MinValue;
			
			foreach (IOverrideMinMaxZoomValue zoomValue in _overrideMinMaxZoom)
			{
				if (zoomValue.Priority < maxPriority) 
					continue;
				maxPriority = zoomValue.Priority;
				minZoom = zoomValue.MinZoomValue;
				maxZoom = zoomValue.MaxZoomValue;
			}

			return (minZoom, maxZoom);
		}

		private float? GetOverrideZoom()
		{
			float? targetZoom = null;
			int maxPriority = int.MinValue;
			foreach (IOverrideZoomValue zoomValue in _overrideZoomValues)
			{
				if (zoomValue.Priority < maxPriority) 
					continue;
				maxPriority = zoomValue.Priority;
				targetZoom = zoomValue.ZoomValue;
			}

			return -targetZoom;
		}
		
		private bool IsZoomOverrideActive()
		{
			return _overrideZoomValues.Count > 0;
		}
		
		private void UpdateCameraPositionImmediate()
		{
			float targetZoom = CMath.Lerp(CDesignCameraConfig.MinZoom, CDesignCameraConfig.MaxZoom, TargetZoom);
			_cinemachineOffset.Offset.z = -targetZoom;
		}
	}
}