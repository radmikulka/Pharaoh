// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.08.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	[DefaultExecutionOrder(CScriptExecutionOrder.CameraMover + 1)]
	public class CAudioListener : ValidatedMonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField, Self] private Transform _transform;
		[SerializeField, Self] private AudioListener _listener;
		
		private ICameraPlaneProvider _cameraPlaneProvider;
		private IMainCameraProvider _mainCamera;

		[Inject]
		private void Inject(
			ICameraPlaneProvider cameraPlaneProvider, 
			IMainCameraProvider mainCameraProvider
			)
		{
			_cameraPlaneProvider = cameraPlaneProvider;
			_mainCamera = mainCameraProvider;
		}

		private void LateUpdate()
		{
			UpdatePositionAgainstPlane();
		}

		private void UpdatePositionAgainstPlane()
		{
			Transform camTransform = _mainCamera.Camera.Camera.transform;
			Ray ray = new(camTransform.position, camTransform.forward);
			Plane cameraPlane = _cameraPlaneProvider.GetCameraPlane();
			cameraPlane.Raycast(ray, out float enter);
			Vector3 hitPoint = ray.GetPoint(enter);
			Vector3 point = Vector3.Lerp(hitPoint, camTransform.position, 0.3f);
			_transform.position = point;
		}
	}
}