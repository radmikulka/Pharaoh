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
			public float CurrentAbsoluteZoomValue => 100f;
			public void AddMinMaxZoomOverride(IOverrideMinMaxZoomValue value) { }
			public void RemoveMinMaxZoomOverride(IOverrideMinMaxZoomValue value) { }
			public void AddZoomOverride(IOverrideZoomValue value) { }
			public void RemoveZoomOverride(IOverrideZoomValue value) { }
			public void SetAbsoluteCameraZoom(float absoluteZoomValue) { }
			public void SetTargetZoom(float zoomPercentage) { }
			public float GetMaxZoom() => CurrentAbsoluteZoomValue;
			public float GetMinZoom() => CurrentAbsoluteZoomValue;
		}
		
		private ICameraZoom _instance = new CNullCameraZoom();

		public float TargetZoom => _instance.TargetZoom;
		public float CurrentAbsoluteZoomValue => _instance.CurrentAbsoluteZoomValue;

		public void SetInstance(ICameraZoom instance)
		{
			_instance = instance;
		}

		public void AddMinMaxZoomOverride(IOverrideMinMaxZoomValue value)
		{
			_instance.AddMinMaxZoomOverride(value);
		}

		public void RemoveMinMaxZoomOverride(IOverrideMinMaxZoomValue value)
		{
			_instance.RemoveMinMaxZoomOverride(value);
		}

		public float GetMaxZoom()
		{
			return _instance.GetMaxZoom();
		}
		
		public float GetMinZoom()
		{
			return _instance.GetMinZoom();
		}

		public void AddZoomOverride(IOverrideZoomValue value)
		{
			_instance.AddZoomOverride(value);
		}

		public void RemoveZoomOverride(IOverrideZoomValue value)
		{
			_instance.RemoveZoomOverride(value);
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