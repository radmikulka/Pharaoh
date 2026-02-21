// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.10.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;

namespace Pharaoh
{
	public interface ICameraZoom
	{
		float TargetZoom { get; }
		float CurrentAbsoluteZoomValue { get; }
		void SetAbsoluteCameraZoom(float absoluteZoomValue);
		void SetTargetZoom(float zoomPercentage);
	}
}