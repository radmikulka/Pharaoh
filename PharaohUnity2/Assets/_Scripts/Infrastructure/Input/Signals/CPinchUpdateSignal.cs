// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.11.2023
// =========================================

using AldaEngine;
using UnityEngine;

namespace TycoonBuilder
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