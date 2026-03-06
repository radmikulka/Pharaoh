// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using AldaEngine;
using UnityEngine;

namespace Pharaoh
{
	public class CDragStartSignal : IInputSignal
	{
		public readonly Vector2 ScreenCoords;
		public readonly int PointerId;

		public CDragStartSignal(Vector2 screenCoords, int pointerId)
		{
			ScreenCoords = screenCoords;
			PointerId = pointerId;
		}
	}
}