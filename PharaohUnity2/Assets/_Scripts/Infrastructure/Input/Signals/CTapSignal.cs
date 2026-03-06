// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using AldaEngine;
using UnityEngine;

namespace Pharaoh
{
	public class CTapSignal : IInputSignal
	{
		public Vector2 Position;

		public CTapSignal(Vector2 position)
		{
			Position = position;
		}
	}
}