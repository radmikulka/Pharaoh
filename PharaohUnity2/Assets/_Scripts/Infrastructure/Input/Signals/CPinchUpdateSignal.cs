// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.11.2023
// =========================================

using AldaEngine;
using UnityEngine;

namespace Pharaoh
{
	public class CPinchUpdateSignal : IInputSignal
	{
		public readonly float Delta;

		public CPinchUpdateSignal(float delta)
		{
			Delta = delta;
		}
	}
}