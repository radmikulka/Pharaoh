// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.10.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;

namespace TycoonBuilder
{
	public interface ICameraZoom
	{
		float TargetZoom { get; }
		float GetMaxZoom();
		float GetMinZoom();
		float CurrentAbsoluteZoomValue { get; }
		void AddMinMaxZoomOverride(IOverrideMinMaxZoomValue value);
		void RemoveMinMaxZoomOverride(IOverrideMinMaxZoomValue value);
		void AddZoomOverride(IOverrideZoomValue value);
		void RemoveZoomOverride(IOverrideZoomValue value);
		void SetAbsoluteCameraZoom(float absoluteZoomValue);
		void SetTargetZoom(float zoomPercentage);
	}
}