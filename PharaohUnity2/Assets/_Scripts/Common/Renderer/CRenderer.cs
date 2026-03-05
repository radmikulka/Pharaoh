// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Reflection;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zenject;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

namespace TycoonBuilder
{
	public class CRenderer : MonoBehaviour, IConstructable, IInitializable
	{
		public const string LowQualityShaderKeyword = "_LOW_QUALITY";
		public const string MediumQualityShaderKeyword = "_MEDIUM_QUALITY";
		
		private const float ShadowDistance = 1200f;
		
		private readonly HashSet<CShadowDistanceOverride> _overrides = new();
		private readonly Dictionary<int, UniversalRenderPipelineAsset> _urpAssetsPerQuality = new();
		private IGraphicsQualityProvider _graphicsQualityProvider;
		private IEventBus _eventBus;
		
		public EGraphicsQuality GraphicsQuality => _graphicsQualityProvider.Quality;

		[Inject]
		private void Inject(IEventBus eventBus, IGraphicsQualityProvider graphicsQualityProvider)
		{
			_graphicsQualityProvider = graphicsQualityProvider;
			_eventBus = eventBus;
		}

		public void Construct()
		{
			RefreshUrpAsset();
		}

		public void Initialize()
		{
			SetQuality(_graphicsQualityProvider.Quality);
			_eventBus.Subscribe<CGraphicsQualityChangedSignal>(OnQualityChanged);
		}

		private void OnQualityChanged(CGraphicsQualityChangedSignal signal)
		{
			SetQuality(signal.Quality);
		}

		private void SetQuality(EGraphicsQuality quality)
		{
			QualitySettings.SetQualityLevel((int)quality);
			
			RefreshUrpAsset();
			RefreshDownSample(quality);
			SetShaderQuality(quality);
			UpdateShadowDistance();
		}

		private void RefreshUrpAsset()
		{
			int currentQuality = QualitySettings.GetQualityLevel();
			if (_urpAssetsPerQuality.TryGetValue(currentQuality, out var existingAsset))
			{
				QualitySettings.renderPipeline = existingAsset;
				return;
			}
			
			UniversalRenderPipelineAsset original = (UniversalRenderPipelineAsset) QualitySettings.renderPipeline;
			UniversalRenderPipelineAsset urpAsset = Instantiate(original);
			_urpAssetsPerQuality.Add(currentQuality, urpAsset);
			QualitySettings.renderPipeline = urpAsset;
		}
		
		private UniversalRenderPipelineAsset GetActiveUrpAsset()
		{
			int currentQuality = QualitySettings.GetQualityLevel();
			return _urpAssetsPerQuality[currentQuality];
		}

		public float GetRenderTextureDownsample()
		{
			UniversalRenderPipelineAsset urpAsset = GetActiveUrpAsset();
			return CMath.Lerp(urpAsset.renderScale, 1f, 0.5f);
		}
		
		public void AddShadowDistanceOverride(CShadowDistanceOverride distanceOverride)
		{
			_overrides.Add(distanceOverride);
			UpdateShadowDistance();
		}

		public void ReleaseShadowDistanceOverride(CShadowDistanceOverride distanceOverride)
		{
			_overrides.Remove(distanceOverride);
			UpdateShadowDistance();
		}
		
		private void RefreshDownSample(EGraphicsQuality quality)
		{
			float maxScreenHeight;
			switch (quality)
			{
				case EGraphicsQuality.Low:
					maxScreenHeight = CMath.Min(550, Screen.height * 0.6f);
					break;
				case EGraphicsQuality.Medium:
					maxScreenHeight = CMath.Min(600, Screen.height * 0.7f);
					break;
				case EGraphicsQuality.High:
					maxScreenHeight = CMath.Min(720, Screen.height * 0.75f);
					break;
				default: throw new ArgumentOutOfRangeException();
			}
			
			float downSample = maxScreenHeight / Screen.height;
			
			if (CPlatform.IsEditor)
			{
				downSample = 1f;
			}
			
			UniversalRenderPipelineAsset urpAsset = GetActiveUrpAsset();
			urpAsset.renderScale = downSample;
		}

		private void SetShaderQuality(EGraphicsQuality quality)
		{
			Shader.DisableKeyword(LowQualityShaderKeyword);
			Shader.DisableKeyword(MediumQualityShaderKeyword);

			switch (quality)
			{
				case EGraphicsQuality.Low:
					Shader.EnableKeyword(LowQualityShaderKeyword);
					return;
				case EGraphicsQuality.Medium:
					Shader.EnableKeyword(MediumQualityShaderKeyword);
					return;
			}
		}

		public void UpdateShadowDistance()
		{
			float targetDistance = GetTargetShadowDistance();
			UniversalRenderPipelineAsset urpAsset = GetActiveUrpAsset();
			urpAsset.shadowDistance = targetDistance;
		}

		private float GetTargetShadowDistance()
		{
			CShadowDistanceOverride activeOverride = GetHighestPriorityOverrideOrDefault();
			if (activeOverride != null)
				return activeOverride.ShadowDistance;
			
			return ShadowDistance;
		}

		private CShadowDistanceOverride GetHighestPriorityOverrideOrDefault()
		{
			CShadowDistanceOverride result = null;
			foreach (CShadowDistanceOverride distanceOverride in _overrides)
			{
				if (result != null && result.Priority >= distanceOverride.Priority) 
					continue;
				result = distanceOverride;
			}

			return result;
		}

		public ScriptableRendererData GetRendererData()
		{
			UniversalRenderPipelineAsset urpAsset = GetActiveUrpAsset();
			return urpAsset.rendererDataList[0];
		}

		public void SetScreenshotModeGraphics(EGraphicsQuality quality)
		{
			SetQuality(quality);
		}
	}
}