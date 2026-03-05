// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System.Collections.Generic;
using UnityEngine;
using System;
using AldaEngine;
using Random = UnityEngine.Random;

namespace TycoonBuilder
{
	public class CTouchInputStartSignal : IInputSignal
	{
		public readonly Vector2 ScreenCoords;
        public readonly int PointerId;

        public CTouchInputStartSignal(Vector2 screenCoords, int pointerId)
        {
	        ScreenCoords = screenCoords;
	        PointerId = pointerId;
        }
	}
}