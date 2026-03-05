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

namespace TycoonBuilder
{
	public class CTargetFrameRateHandler : MonoBehaviour, IInitializable
	{
		private const int EditorFpsLimit = 3333;
		private const int FpsLimit = 60;
		//private const int HighEndPhoneFpsLimit = 60;
		private const int BatterySaverFpsLimit = 40;

		private int _lastKnownDeviceRefreshRate;

		private CSettingsData _settingsData;
		
		[Inject]
		private void Inject(CSettingsData settingsData)
		{
			_settingsData = settingsData;
		}
		
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
			if (_settingsData.BatterySaverEnabled.Value)
			{
				int limit = GetBatterySaverLimit();
				return limit;
			}
			
			if (CPlatform.IsEditor)
				return EditorFpsLimit;
			
			int screenRefreshRate = GetScreenRefreshRate();
			return CMath.Min(FpsLimit, screenRefreshRate);
		}
		
		private int GetBatterySaverLimit()
		{
			/*int screenRefreshRate = GetScreenRefreshRate();
			if (screenRefreshRate >= 90)
				return HighEndPhoneFpsLimit;
			*/
			return BatterySaverFpsLimit;
		}

		private int GetScreenRefreshRate()
		{
			return (int) Screen.currentResolution.refreshRateRatio.value;
		}
	}
}