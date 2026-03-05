// =========================================
// AUTHOR: Radek Mikulka
// DATE:   25.09.2025
// =========================================

namespace TycoonBuilder
{
	public class COverrideZoomValue : IOverrideZoomValue
	{
		public float ZoomValue { get; set; }
		public int Priority { get; }

		public COverrideZoomValue(int priority, float zoomValue)
		{
			Priority = priority;
			ZoomValue = zoomValue;
		}
	}
}