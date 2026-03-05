// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.10.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CVehicleRotator : MonoBehaviour, IInitializable
	{
		[SerializeField] private Transform _orbitRoot;
		[SerializeField] private Transform _zoomHolder;

		private CVehicleRotatorInput _input;
		private Quaternion _targetRotation;
		private float _localCameraYOffset;
		private float _localZPostiion;
		private Vector2 _minMaxZoom;
		private Vector2 _inputDelta;
		private float _targetZoom;
		private float _frontMax;
		private float _backMin;

		[Inject]
		private void Construct(CVehicleRotatorInput input)
		{
			_input = input;
		}

		public void Initialize()
		{
			
		}

		public void SetActive(bool state)
		{
			gameObject.SetActive(state);
		}

		public void ResetCamera(float cameraDistance, float cameraYOffset)
		{
			_zoomHolder.localPosition = new Vector3(0f, 0f, -cameraDistance);
			_targetZoom = cameraDistance;
			_localCameraYOffset = cameraYOffset;
			_targetRotation = Quaternion.Euler(0f, 120f, 0f);
			UpdateOrbitTransform(1f);
		}

		public void SetCameraBounds(float leftMin, float rightMax, float minZoom, float maxZoom)
		{
			_backMin = leftMin;
			_frontMax = rightMax;
			_minMaxZoom = new Vector2(minZoom, maxZoom);
			_localZPostiion = _frontMax;
		}

		private void HandleInput()
		{
			_inputDelta += _input.InputDelta * Time.deltaTime;

			bool canMove = true;
			
			bool isOnBack = _localZPostiion <= _backMin;
			if (isOnBack)
			{
				RotateHorizontal();
				canMove = Vector3.Dot(Vector3.forward, GetPlanarCameraDirection()) <= 0f;
			}
			
			bool isOnFront = _localZPostiion >= _frontMax;
			if (isOnFront)
			{
				RotateHorizontal();
				canMove = Vector3.Dot(Vector3.forward, GetPlanarCameraDirection()) > 0f;
			}

			bool facingFromLeft = Vector3.Dot(GetPlanarCameraDirection(), Vector3.right) > 0f;
			bool isInMiddle = !isOnFront && !isOnBack;

			if (isInMiddle)
			{
				ForcePerpendicularAngle(facingFromLeft);
			}

			if (canMove)
			{
				AddOrbitMoveVelocity(facingFromLeft);
			}

			RotateVertical();
		}
		
		private Vector3 GetPlanarCameraDirection()
		{
			Vector3 forward = _targetRotation * Vector3.forward;
			return Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
		}

		private void ForcePerpendicularAngle(bool facingFromLeft)
		{
			Vector3 currentRotation = _targetRotation.eulerAngles;
			_targetRotation = Quaternion.Euler(currentRotation.x, facingFromLeft ? 90f : -90f, currentRotation.z);
		}

		private void AddOrbitMoveVelocity(bool facingFromLeft)
		{
			const float moveSensitivity = 0.004f;
			float inputToApply = _inputDelta.x * moveSensitivity;
			
			float distanceToMove = CMath.Abs(_inputDelta.x);
			float deltaToMove = inputToApply * distanceToMove * (facingFromLeft ? 1f : -1f);
			
			_localZPostiion = CMath.Clamp(_localZPostiion + deltaToMove, _backMin, _frontMax);
		}

		private void RotateVertical()
		{
			const float sensitivity = 0.4f;
			Vector2 rotationInput = _inputDelta * sensitivity;
				
			float minAngle = -8f;
			float maxAngle = 80f;
			
			Vector3 euler = _targetRotation.eulerAngles;
			if (euler.x > maxAngle)
			{
				euler.x -= 360f;
			}
			if(euler.x < minAngle)
			{
				euler.x += 360f;
			}
			
			euler += new Vector3(-rotationInput.y, 0f, 0f);
			euler.x = CMath.Clamp(euler.x, minAngle, maxAngle);
			_targetRotation = Quaternion.Euler(euler);
		}

		private void RotateHorizontal()
		{
			const float sensitivity = 0.2f;
			Vector2 rotationInput = _inputDelta * sensitivity;
			Vector3 euler = _targetRotation.eulerAngles;
			euler += new Vector3(0f, rotationInput.x, 0f);
			_targetRotation = Quaternion.Euler(euler);
		}

		private void Update()
		{
			HandleInput();
			UpdateZoomInput();
			UpdateWhileNoKeyIsPressed();
		}

		private void UpdateZoomInput()
		{
			const float zoomSensitivity = 80f;
			_targetZoom = CMath.Clamp(_targetZoom - _input.Zoom * Time.deltaTime * zoomSensitivity, _minMaxZoom.x, _minMaxZoom.y);
		}

		private void LateUpdate()
		{
			UpdateOrbitTransform(Time.deltaTime);
		}

		private void UpdateWhileNoKeyIsPressed()
		{
			if (_input.IsKeyPressed)
				return;

			DecreaseDeltaOverTime();
		}

		private void UpdateOrbitTransform(float delta)
		{
			_orbitRoot.rotation = Quaternion.Slerp(_orbitRoot.rotation, _targetRotation, delta * 25f);
			_orbitRoot.localPosition = Vector3.Lerp(_orbitRoot.localPosition, new Vector3(0f, _localCameraYOffset, _localZPostiion), delta * 25f);
			_zoomHolder.localPosition = Vector3.Lerp(_zoomHolder.localPosition, new Vector3(0f, 0f, -_targetZoom), delta * 10f);
		}

		private void DecreaseDeltaOverTime()
		{
			_inputDelta = Vector3.Lerp(_inputDelta, CVector3.Zero, Time.deltaTime * 5f);
		}
	}
}