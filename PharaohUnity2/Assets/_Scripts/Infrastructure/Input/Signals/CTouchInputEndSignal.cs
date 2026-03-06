// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using AldaEngine;
using UnityEngine;

namespace Pharaoh
{
	public class CTouchInputEndSignal : IInputSignal
	{
		public readonly Vector2 ScreenCoords;
		public readonly Vector2 PointerDownPos;
		public readonly bool WasDrag;
		public readonly int PointerId;

		public CTouchInputEndSignal(Vector2 screenCoords, int pointerId, Vector2 pointerDownPos, bool wasDrag)
		{
			WasDrag = wasDrag;
			ScreenCoords = screenCoords;
			PointerDownPos = pointerDownPos;
			PointerId = pointerId;
		}
	}
}