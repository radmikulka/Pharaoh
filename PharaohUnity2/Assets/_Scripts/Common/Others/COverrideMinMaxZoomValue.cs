// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.10.2025
// =========================================

namespace TycoonBuilder
{
	public class COverrideMinMaxZoomValue : IOverrideMinMaxZoomValue
	{
		public float MinZoomValue { get; }
		public float MaxZoomValue { get; }
		public int Priority { get; }

		public COverrideMinMaxZoomValue(float minZoomValue, float maxZoomValue, int priority)
		{
			MinZoomValue = minZoomValue;
			MaxZoomValue = maxZoomValue;
			Priority = priority;
		}
	}
}