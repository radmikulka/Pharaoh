// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.10.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;

namespace TycoonBuilder
{
	public class CCameraZoomProxy : ICameraZoom
	{
		private class CNullCameraZoom : ICameraZoom
		{
			public float TargetZoom => 1f;
			public void SetAbsoluteCameraZoom(float absoluteZoomValue) { }
			public void SetTargetZoom(float zoomPercentage) { }
		}
		
		private ICameraZoom _instance = new CNullCameraZoom();

		public float TargetZoom => _instance.TargetZoom;

		public void SetInstance(ICameraZoom instance)
		{
			_instance = instance;
		}

		public void SetAbsoluteCameraZoom(float absoluteZoomValue)
		{
			_instance.SetAbsoluteCameraZoom(absoluteZoomValue);
		}

		public void SetTargetZoom(float zoomPercentage)
		{
			_instance.SetTargetZoom(zoomPercentage);
		}
	}
}