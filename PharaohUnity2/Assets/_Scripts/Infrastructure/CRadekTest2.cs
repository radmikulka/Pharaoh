// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.10.2025
// =========================================

using System;
using AldaEngine;
using UnityEngine;

namespace TycoonBuilder
{
	public class CRadekTest2 : MonoBehaviour
	{
		[SerializeField] private Transform _orbitRoot;

		private Quaternion _targetRotation;
		private Vector3 _targetPosition;
		private Vector2 _inputDelta;
		private Vector3 _rightMax;
		private Vector3 _leftMin;

		private void Start()
		{
			_leftMin = _orbitRoot.position + Vector3.left * 5f;
			_rightMax = _orbitRoot.position + Vector3.right * 5f;
			_targetPosition = _orbitRoot.position;
		}

		private void Update()
		{
			float horizontal = Input.GetKey(KeyCode.A) ? -0.1f : Input.GetKey(KeyCode.D) ? 0.1f : 0f;
			float vertical = Input.GetKey(KeyCode.W) ? 0.1f : Input.GetKey(KeyCode.S) ? -0.1f : 0f;
			_inputDelta.x += horizontal;
			_inputDelta.y += vertical;
			
			Vector3 orbitRootPosition = _orbitRoot.position;
			
			bool isOnRight = Vector3.Dot(_rightMax - _leftMin, _rightMax - orbitRootPosition) <= 0.01f;
			if (isOnRight)
			{
				Rotate(0f, 280f);
				return;
			}
			
			bool isOnLeft = Vector3.Dot(_leftMin - _rightMax, _leftMin - orbitRootPosition) <= 0.01f;
			if (isOnLeft)
			{
				Rotate(180f, 360f);
				return;
			}

			AddOrbitMoveVelocity();
			
		}
		
		private void AddOrbitMoveVelocity()
		{
			Vector3 localLeftPoint = _leftMin;
			Vector3 localRightPoint = _rightMax;
			Vector3 toCameraDir = _targetRotation * Vector3.forward;

			bool leftPointIsOnLeft = Vector3.Dot(transform.forward, toCameraDir) > 0f;
			if (!leftPointIsOnLeft)
			{
				localLeftPoint = _rightMax;
				localRightPoint = _leftMin;
			}

			Vector3 targetEndPoint = _inputDelta.x > 0f ? localRightPoint : localLeftPoint;
			float distanceToTarget = Vector3.Distance(_targetPosition, targetEndPoint);
			Vector3 moveDirection = (targetEndPoint - _targetPosition).normalized;

			float distanceToMove = CMath.Abs(_inputDelta.x);
			float deltaToMove = CMath.Min(distanceToTarget, distanceToMove);
			
			_targetPosition += moveDirection * deltaToMove;
			
			_inputDelta.x -= deltaToMove * CMath.Sign(_inputDelta.x);
			_orbitRoot.position = _targetPosition;
		}
		
		private void Rotate(float minY, float maxY)
		{
			float minAngleX = -8f;
			float maxAngleX = 80f;
			
			Vector3 currentEuler = _targetRotation.eulerAngles;
			if (currentEuler.x > 180f) currentEuler.x -= 360f;
			if (currentEuler.y > 180f) currentEuler.y -= 360f;
			
			Vector3 targetEuler = currentEuler + new Vector3(_inputDelta.y, -_inputDelta.x, 0f);
			targetEuler.x = Mathf.Clamp(targetEuler.x, minAngleX, maxAngleX);
			targetEuler.y = Mathf.Clamp(targetEuler.y, minY, maxY);
			
			_targetRotation = Quaternion.Euler(targetEuler);
			_orbitRoot.rotation = _targetRotation;
			
			Vector3 actualRotationDelta = targetEuler - currentEuler;
			_inputDelta.y -= actualRotationDelta.x;
			_inputDelta.x -= actualRotationDelta.y;
		}
	}
}