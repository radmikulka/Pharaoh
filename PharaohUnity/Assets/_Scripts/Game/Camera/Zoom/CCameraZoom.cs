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
using Pharaoh;
using Unity.Cinemachine;
using Zenject;

namespace Pharaoh
{
	public class CCameraZoom : ValidatedMonoBehaviour, 
		IConstructable, IInitializable, ICameraZoom
	{
		private const float MinZoom = 100;
		private const float MaxZoom = 100;
		
		[SerializeField, Child] private CinemachineCameraOffset _cinemachineOffset;
		[SerializeField, Child] private CinemachineCamera _cinemachineCamera;

		private IMainCameraProvider _mainCameraProvider;
		private CCameraZoomProxy _cameraZoomProxy;
		private CEventSystem _eventSystem;
		private IEventBus _eventBus;

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

		public void SetAbsoluteCameraZoom(float absoluteZoomValue)
		{
			float zoomPercentage = CMath.InverseLerp(MinZoom, MaxZoom, absoluteZoomValue);
			SetZoomValue(zoomPercentage);
		}

		public void SetTargetZoom(float zoomPercentage)
		{
			SetZoomValue(zoomPercentage);
		}

		private void SetMaxZoom()
		{
			SetZoomValue(MaxZoom);
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
			float originalZoomDiff = MaxZoom - MinZoom;
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
		}
		
		private void UpdateCameraPositionAnimated()
		{
			(float min, float max) bounds = GetMinMaxZoom();
			float targetZoom = -CMath.Lerp(bounds.min, bounds.max, TargetZoom);
			float finalZoom = targetZoom;
			float newValue = CMath.Lerp(_cinemachineOffset.Offset.z, finalZoom, Time.deltaTime * 10f);
			_cinemachineOffset.Offset.z = newValue;
		}

		private (float min, float max) GetMinMaxZoom()
		{
			float minZoom = MinZoom;
			float maxZoom = MaxZoom;
			return (minZoom, maxZoom);
		}
		
		private void UpdateCameraPositionImmediate()
		{
			float targetZoom = CMath.Lerp(MinZoom, MaxZoom, TargetZoom);
			_cinemachineOffset.Offset.z = -targetZoom;
		}
	}
}