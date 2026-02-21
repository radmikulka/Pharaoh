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
		public EGraphicsQuality Quality => EGraphicsQuality.High;

		private readonly IEventBus _eventBus;

		public CGraphicsQualityProvider(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		private void OnSettingsChanged(int _)
		{
			_eventBus.Send(new CGraphicsQualityChangedSignal(Quality));
		}
	}
}