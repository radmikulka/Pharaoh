// =========================================
// AUTHOR: Marek Karaba
// DATE:   03.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace TycoonBuilder
{
	#if UNITY_EDITOR
	[AssetPath("Resources/Configs/ScreenshotModeConfig", "ScreenshotModeConfig")]
	public class CScreenshotModeConfig : CScriptableSingletonEditorOnly<CScreenshotModeConfig>, IAldaFrameworkComponent
	{
		private const float AmbientOcclusionDefaultIntensity = 7f;
		
		[Header("Enable ScreenshotMode")] 
		[SerializeField] private bool _isEnabled;

		[Header("Set Active State")] 
		[SerializeField] private bool _canvas;
		[SerializeField] private bool _developerHUD;
		[SerializeField] private bool _antiAliasing;
		[SerializeField] private bool _vignetteMesh;
		[Header("SSAO Settings")]
		[SerializeField] private bool _ambientOcclusion;
		[Range(0f, 50f)]
		[SerializeField] private float _ambientOcclusionIntensity = 7f;
		
		private CRenderer _renderer;
		
		private List<CScreenshotModeTag> _tags = new();

		public void ToggleEScreenshotModeObjects()
		{
			_tags.Clear();
			_tags = FindObjectsByType<CScreenshotModeTag>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
            
			SetToggleStates();
		}

		private void SetToggleStates()
		{
			if (!_isEnabled)
				return;
			
			SetRenderingQuality(EGraphicsQuality.High);
			foreach (CScreenshotModeTag tag in _tags)
			{
				switch (tag.ScreenshotModeObject)
				{
					case EScreenshotModeObject.Canvas:
						tag.SetActiveStateForScreenshot(_canvas);
						break;
					case EScreenshotModeObject.GameInfo:
						tag.SetActiveStateForScreenshot(_developerHUD);
						break;
					case EScreenshotModeObject.AntiAliasing:
						tag.SetActiveStateForScreenshot(_antiAliasing);
						break;
					case EScreenshotModeObject.VignetteMesh:
						tag.SetActiveStateForScreenshot(_vignetteMesh);
						break;
					case EScreenshotModeObject.AmbientOcclusion:
						tag.SetActiveStateForScreenshot(_ambientOcclusion, _ambientOcclusionIntensity);
						break;
				}
			}
		}

		private void SetRenderingQuality(EGraphicsQuality quality)
		{
			_renderer = FindFirstObjectByType<CRenderer>();
			_renderer.SetScreenshotModeGraphics(quality);
		}

		public void ResetCanvasChanges()
		{
			SetRenderingQuality(EGraphicsQuality.Medium);
			
			_isEnabled = false;
			_canvas = true;
			_developerHUD = true;
			_antiAliasing = false;
			_vignetteMesh = true;
			
			_tags.Clear();
			_tags = FindObjectsByType<CScreenshotModeTag>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
			
			foreach (CScreenshotModeTag tag in _tags)
			{
				float value = 0f;
				if (tag.ScreenshotModeObject == EScreenshotModeObject.AmbientOcclusion)
				{
					_ambientOcclusionIntensity = AmbientOcclusionDefaultIntensity;
					value = _ambientOcclusionIntensity;
				}
				tag.SetActiveStateForScreenshot(tag._defaultState, value);
			}
		}
	}
	#endif
}
