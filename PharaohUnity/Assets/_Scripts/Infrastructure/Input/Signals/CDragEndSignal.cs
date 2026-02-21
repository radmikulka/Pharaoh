// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using AldaEngine;
using UnityEngine;

namespace Pharaoh
{
	public class CDragEndSignal : IInputSignal
	{
		public readonly Vector2 ScreenCoords;
		public readonly Vector2 Delta;
		public readonly int PointerId;

		public CDragEndSignal(Vector2 screenCoords, Vector2 delta, int pointerId)
		{
			ScreenCoords = screenCoords;
			PointerId = pointerId;
			Delta = delta;
		}
	}
}