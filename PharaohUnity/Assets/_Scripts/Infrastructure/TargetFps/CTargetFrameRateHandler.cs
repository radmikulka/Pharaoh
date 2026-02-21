// =========================================
// AUTHOR: Radek Mikulka
// DATE:   5.4.2024
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;
using Screen = UnityEngine.Device.Screen;

namespace Pharaoh
{
	public class CTargetFrameRateHandler : MonoBehaviour, IInitializable
	{
		private const int EditorFpsLimit = FpsLimit;
		private const int FpsLimit = 60;

		private int _lastKnownDeviceRefreshRate;
		
		public void Initialize()
		{
			UpdateFpsLimit();
		}

		private void LateUpdate()
		{
			UpdateFpsLimit();
		}

		private void UpdateFpsLimit()
		{
			int finalFpsLimit = GetFinalFpsLimit();
			Application.targetFrameRate = finalFpsLimit;
		}

		private int GetFinalFpsLimit()
		{
			int maxFps = GetMaxFps();
			return maxFps;
		}

		private int GetMaxFps()
		{
			if (CPlatform.IsEditor)
				return EditorFpsLimit;
			
			int screenRefreshRate = GetScreenRefreshRate();
			return CMath.Min(FpsLimit, screenRefreshRate);
		}

		private int GetScreenRefreshRate()
		{
			return (int) Screen.currentResolution.refreshRateRatio.value;
		}
	}
}