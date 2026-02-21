// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.11.2025
// =========================================

using AldaEngine;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Pharaoh
{
	public class CDefaultGraphicsQualityProvider
	{
		public EGraphicsQuality GetDefaultQuality()
		{
			if (CPlatform.IsIosDevice)
				return GetDefaultIosQuality();
			
			return GetDefaultQualityBasedOnSystemInfo();
		}

		private EGraphicsQuality GetDefaultIosQuality()
		{
			#if UNITY_IOS
			if (Device.generation > DeviceGeneration.iPhone11)
			{
				return EGraphicsQuality.High;
			}
			#endif

			return GetDefaultQualityBasedOnSystemInfo();
		}

		private EGraphicsQuality GetDefaultQualityBasedOnSystemInfo()
		{
			bool useHigh = UseHighSettings();
			if (useHigh)
				return EGraphicsQuality.High;

			return EGraphicsQuality.Low;
		}
		
		private bool UseHighSettings()
		{
			int? sdk = CPlatformUtils.GetAndroidSDKLevel();
			const int android12Sdk = 31;
			if (sdk < android12Sdk)
				return false;

			if (SystemInfo.systemMemorySize > 1024 * 6)
				return true;
			return false;
		}
	}
}