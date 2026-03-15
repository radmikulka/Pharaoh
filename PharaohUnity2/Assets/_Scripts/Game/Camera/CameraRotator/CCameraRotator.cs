// =========================================
// AUTHOR: Claude
// DATE:   15.03.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CCameraRotator : ValidatedMonoBehaviour, IInitializable
	{
		private const float SmoothSpeed = 25f;
		private const float InertiaDecay = 5f;

		[SerializeField] private float _sensitivity = 0.15f;
		[SerializeField] private float _minPitch = -8f;
		[SerializeField] private float _maxPitch = 80f;

		private const float DeltaMagnitudeThreshold = 0.01f;

		private Quaternion _targetRotation;
		private Vector2 _movementDelta;
		private bool _isDragging;
		private bool _receivedDragThisFrame;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_targetRotation = transform.rotation;
			_eventBus.Subscribe<CDragStartSignal>(OnDragStart);
			_eventBus.Subscribe<CDragUpdateSignal>(OnDragUpdate);
			_eventBus.Subscribe<CDragEndSignal>(OnDragEnd);
		}

		private void OnDragStart(CDragStartSignal signal)
		{
			_isDragging = true;
			_movementDelta = Vector2.zero;
		}

		private void OnDragUpdate(CDragUpdateSignal signal)
		{
			_movementDelta = signal.MovementDelta;
			_receivedDragThisFrame = true;
		}

		private void OnDragEnd(CDragEndSignal signal)
		{
			_isDragging = false;
		}

		private void LateUpdate()
		{
			if (!_isDragging || !_receivedDragThisFrame)
			{
				_movementDelta = Vector2.Lerp(_movementDelta, Vector2.zero, Time.deltaTime * InertiaDecay);
			}

			if (_movementDelta.sqrMagnitude < DeltaMagnitudeThreshold * DeltaMagnitudeThreshold)
			{
				_movementDelta = Vector2.zero;
			}

			Rotate(-_movementDelta.y * _sensitivity, _movementDelta.x * _sensitivity);
			SmoothRotation();
			_receivedDragThisFrame = false;
		}

		private void Rotate(float pitchDelta, float yawDelta)
		{
			if (Mathf.Approximately(pitchDelta, 0f) && Mathf.Approximately(yawDelta, 0f))
			{
				return;
			}

			Vector3 euler = _targetRotation.eulerAngles;
			float pitch = euler.x > 180f ? euler.x - 360f : euler.x;
			pitch = Mathf.Clamp(pitch + pitchDelta, _minPitch, _maxPitch);
			float yaw = euler.y + yawDelta;
			_targetRotation = Quaternion.Euler(pitch, yaw, 0f);
		}

		private void SmoothRotation()
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * SmoothSpeed);
		}

		public void RotateImmediately(Quaternion rotation)
		{
			_targetRotation = rotation;
			transform.rotation = rotation;
		}
	}
}
