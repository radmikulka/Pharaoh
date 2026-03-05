// =========================================
// AUTHOR: Radek Mikulka
// DATE:   25.09.2025
// =========================================

namespace TycoonBuilder
{
	public interface IOverrideZoomValue
	{
		float ZoomValue { get; }
		int Priority { get; }
	}

	public interface IOverrideMinMaxZoomValue
	{
		float MinZoomValue { get; }
		float MaxZoomValue { get; }
		int Priority { get; }
	}
}