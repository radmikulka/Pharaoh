// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.10.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public class CVehicleRotatorInput
	{
		public Vector2 InputDelta { get; private set; }
		public float Zoom { get; private set; }
		public bool IsKeyPressed { get; private set; }
		
		public void SetInputDelta(Vector2 inputDelta)
		{
			InputDelta = inputDelta;
		}
		
		public void SetKeyPressed(bool isPressed)
		{
			IsKeyPressed = isPressed;
		}

		public void SetZoom(float zoom)
		{
			Zoom = zoom;
		}
	}
}