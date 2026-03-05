// =========================================
// AUTHOR: Marek Karaba
// DATE:   16.10.2025
// =========================================

using UnityEngine;
using UnityEngine.Animations;

namespace TycoonBuilder.Infrastructure
{
	public class CRotationAnimator : MonoBehaviour
	{
		[SerializeField] private Axis _rotationAxis = Axis.Y;
		[SerializeField] private float _rotationsPerSecond = 1f;
		[SerializeField] private bool _isClockwise = true;

		private float _rotationSpeed;
		
		private void Update()
		{
			float rotationDirection = _isClockwise ? 1f : -1f;
			Vector3 rotation = Vector3.zero;
			_rotationSpeed = 360f * _rotationsPerSecond;
			
			switch (_rotationAxis)
			{
				case Axis.X:
					rotation = new Vector3(_rotationSpeed * rotationDirection * Time.deltaTime, 0f, 0f);
					break;
				case Axis.Y:
					rotation = new Vector3(0f, _rotationSpeed * rotationDirection * Time.deltaTime, 0f);
					break;
				case Axis.Z:
					rotation = new Vector3(0f, 0f, _rotationSpeed * rotationDirection * Time.deltaTime);
					break;
			}
			transform.Rotate(rotation, Space.Self);
		}
	}
}