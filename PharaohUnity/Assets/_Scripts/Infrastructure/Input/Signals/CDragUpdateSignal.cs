// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using AldaEngine;
using UnityEngine;

namespace Pharaoh
{
	public class CDragUpdateSignal : IInputSignal
	{
		public readonly Vector2 DragStartPos;
		public readonly Vector2 ScreenCoords;
		public readonly Vector2 MovementDelta;
		public readonly int PointerId;

		public CDragUpdateSignal(Vector2 screenCoords, int pointerId, Vector2 movementDelta, Vector2 dragStartPos)
		{
			DragStartPos = dragStartPos;
			MovementDelta = movementDelta;
			ScreenCoords = screenCoords;
			PointerId = pointerId;
		}
	}
}