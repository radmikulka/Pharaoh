// =========================================
// AUTHOR: Marek Karaba
// DATE:   03.07.2025
// =========================================

using System.Reflection;
using AldaEngine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TycoonBuilder
{
	public class CScreenshotModeTag : MonoBehaviour
	{
		private enum ScreenshotModeEnum
		{
			SetAlpha,
			ToggleCanvas,
			ToggleGameObject,
			SetAntiAliasing,
			SetVignetteMesh,
			SetAmbientOcclusion
		}

		public EScreenshotModeObject ScreenshotModeObject => _screenshotObject;

		[SerializeField] private EScreenshotModeObject _screenshotObject;
		[SerializeField] private ScreenshotModeEnum _mode;
		public bool _defaultState = true;

		public void SetActiveStateForScreenshot(bool state, float value = 0f)
		{
			#if UNITY_EDITOR
			switch (_mode)
			{
				case ScreenshotModeEnum.SetAlpha:
					SetAlpha(state);
					break;
				case ScreenshotModeEnum.ToggleCanvas:
					ToggleCanvas(state);
					break;
				case ScreenshotModeEnum.ToggleGameObject:
					gameObject.SetActiveObject(state);
					break;
				case ScreenshotModeEnum.SetAntiAliasing:
					SetAntiAliasing(state);
					break;
				case ScreenshotModeEnum.SetVignetteMesh:
					SetVignetteMesh(state);
					break;
				case ScreenshotModeEnum.SetAmbientOcclusion:
					SetAmbientOcclusion(state, value);
					break;
			}
			#endif
		}

		private void SetAmbientOcclusion(bool state, float intensity)
		{
			int index = state ? 2 : 0;
			
			Camera targetCamera = GetComponent<Camera>();
			UniversalAdditionalCameraData camData = targetCamera.GetUniversalAdditionalCameraData();
			camData.SetRenderer(index);
			
			if (index == 0)
				return;
			
			SetSSAOIntensityViaReflection(index, intensity);
		}

		// ReSharper disable once InconsistentNaming
		private static void SetSSAOIntensityViaReflection(int index, float intensity)
		{
			UniversalRenderPipelineAsset pipeline = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            ScriptableRendererData rendererData = pipeline.rendererDataList[index];
			foreach (ScriptableRendererFeature feature in rendererData.rendererFeatures)
			{
				if (feature is not ScreenSpaceAmbientOcclusion ssao)
					continue;

				FieldInfo settingsField = typeof(ScreenSpaceAmbientOcclusion).GetField(
					"m_Settings", 
					BindingFlags.NonPublic | BindingFlags.Instance
				);

				if (settingsField == null)
					continue;
				
				object settings = settingsField.GetValue(ssao);

				FieldInfo intensityField = settings.GetType()
					.GetField("Intensity", BindingFlags.NonPublic | BindingFlags.Instance);

				if (intensityField == null) 
					continue;
				
				intensityField.SetValue(settings, intensity);
			}
		}

		private void SetAlpha(bool visible)
		{
			TryGetComponent(out CanvasGroup canvasGroup);
			if (!canvasGroup)
			{
				canvasGroup = gameObject.AddComponent<CanvasGroup>();
			}

			if (visible)
			{
				Destroy(canvasGroup);
			}
			else
			{
				canvasGroup.alpha = 0;
				canvasGroup.blocksRaycasts = true;
				canvasGroup.interactable = false;
			}
		}

		private void ToggleCanvas(bool visible)
		{
			Canvas canvas = gameObject.GetComponent<Canvas>();
			if (canvas)
			{
				canvas.enabled = visible;
			}
		}

		private void SetAntiAliasing(bool active)
		{
			UniversalAdditionalCameraData cameraData = GetComponent<UniversalAdditionalCameraData>();
			cameraData.antialiasing = active ? AntialiasingMode.SubpixelMorphologicalAntiAliasing : AntialiasingMode.None;
		}
		
		private void SetVignetteMesh(bool active)
		{
			Volume volume = GetComponent<Volume>();
			volume.profile.TryGet(out Vignette vignette);
			vignette.active = active;
		}
	}
}