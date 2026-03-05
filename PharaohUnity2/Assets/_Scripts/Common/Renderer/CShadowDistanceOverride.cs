// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.09.2025
// =========================================

namespace TycoonBuilder
{
	public class CShadowDistanceOverride
	{
		public float ShadowDistance { get; set; }
		public readonly int Priority;

		public CShadowDistanceOverride(float shadowDistance, int priority)
		{
			ShadowDistance = shadowDistance;
			Priority = priority;
		}
	}
}