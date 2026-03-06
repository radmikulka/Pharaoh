// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.05.2024
// =========================================

using System;
using System.Reflection;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Pharaoh;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Pharaoh.Infrastructure
{
	public class CGraphicsQualityProvider : IGraphicsQualityProvider
	{
		public EGraphicsQuality Quality => _settingsData.Quality;
		private readonly ISettings _settingsData;

		public CGraphicsQualityProvider(ISettings settingsData)
		{
			_settingsData = settingsData;
		}
	}
}