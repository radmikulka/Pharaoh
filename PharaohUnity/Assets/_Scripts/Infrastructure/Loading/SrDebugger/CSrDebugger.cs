// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.11.2025
// =========================================

using System.ComponentModel;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
#if !DISABLE_SRDEBUGGER
using CodeStage.AdvancedFPSCounter;
using SRDebugger;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ShadowQuality = UnityEngine.ShadowQuality;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

namespace Pharaoh
{
	public class CSrDebugger
	{
		private static bool _isActivated;
		
		public async UniTask TryActivate(CancellationToken ct)
		{
			#if !DISABLE_SRDEBUGGER

			if (CPlatform.IsEditor)
				return;

			if (!CBuildConfig.Instance.ShowLauncherMenu)
				return;
			
			if (!_isActivated)
			{
				SRDebug.Init();
				SRDebug.Instance.IsTriggerEnabled = true;
				SRDebug.Instance.AddOptionContainer(new COptions());
				_isActivated = true;
			}
			
			SRDebug.Instance.ShowDebugPanel(DefaultTabs.Options);

			await UniTask.WaitUntil(() => !SRDebug.Instance.IsDebugPanelVisible, cancellationToken: ct);

			#endif
		}

		#if !DISABLE_SRDEBUGGER

		private class COptions
		{
			private const string LauncherCategory = "Launcher";
			private const string PresetCategory = "Preset";
			private const string DebugCategory = "Debug";

			private UniversalRenderPipelineAsset UrpAsset => (UniversalRenderPipelineAsset) QualitySettings.renderPipeline;
			private AFPSCounter _advancedFpsCounter;

			public COptions()
			{
				CreateFpsCounter();
			}

			private void CreateFpsCounter()
			{
				GameObject go = new();
				_advancedFpsCounter = Object.FindAnyObjectByType<AFPSCounter>() ?? go.AddComponent<AFPSCounter>();
				_advancedFpsCounter.OperationMode = OperationMode.Disabled;
				Object.DontDestroyOnLoad(go);
			}

			[Category(LauncherCategory)]
			public EEditorSkips EditorSkips
			{
				get => CDebugConfig.Instance.EditorSkips;
				set => CDebugConfig.Instance.EditorSkips = value;
			}

			[Category(LauncherCategory)]
			public EServerType ServerType
			{
				get => CServerConfig.Instance.ServerType;
				set => CServerConfig.Instance.ServerType = value;
			}

			[Category(LauncherCategory)]
			public bool DeleteUser
			{
				get => CServerConfig.Instance.DeleteUser;
				set => CServerConfig.Instance.DeleteUser = value;
			}

			[Category(PresetCategory)]
			public bool LikeABoss
			{
				get => CServerConfig.Instance.LikeABoss;
				set => CServerConfig.Instance.LikeABoss = value;
			}

			[Category(PresetCategory)]
			public string OverrideUid
			{
				get => CServerConfig.Instance.OverrideUid;
				set => CServerConfig.Instance.OverrideUid = value;
			}

			[Category(PresetCategory)]
			public EUserPresetId PresetId
			{
				get => CServerConfig.Instance.PresetId;
				set => CServerConfig.Instance.PresetId = value;
			}

			[Category(DebugCategory), NumberRange(0, 4)]
			public int GlobalTextureMipmapLimit
			{
				get => QualitySettings.globalTextureMipmapLimit;
				set => QualitySettings.globalTextureMipmapLimit = value;
			}
			
			[Category(DebugCategory), Increment(0.1f)]
			public float Downsample
			{
				get => UrpAsset.renderScale;
				set => UrpAsset.renderScale = value;
			}

			[Category(DebugCategory)]
			public bool SupportsCameraOpaqueTexture
			{
				get => UrpAsset.supportsCameraOpaqueTexture;
				set => UrpAsset.supportsCameraOpaqueTexture = value;
			}

			[Category(DebugCategory)]
			public bool SupportsCameraDepthTexture
			{
				get => UrpAsset.supportsCameraDepthTexture;
				set => UrpAsset.supportsCameraDepthTexture = value;
			}

			[Category(DebugCategory)]
			public GPUResidentDrawerMode ResidentDrawerMode
			{
				get => UrpAsset.gpuResidentDrawerMode;
				set => UrpAsset.gpuResidentDrawerMode = value;
			}

			[Category(DebugCategory)]
			public bool GpuResidentDrawerEnableOcclusionCullingInCameras
			{
				get => UrpAsset.gpuResidentDrawerEnableOcclusionCullingInCameras;
				set => UrpAsset.gpuResidentDrawerEnableOcclusionCullingInCameras = value;
			}

			[Category(DebugCategory), NumberRange(1, 8)]
			public int MsaaSampleCount
			{
				get => UrpAsset.msaaSampleCount;
				set => UrpAsset.msaaSampleCount = value;
			}

			[Category(DebugCategory)]
			public float ShadowDistance
			{
				get => UrpAsset.shadowDistance;
				set => UrpAsset.shadowDistance = value;
			}

			[Category(DebugCategory)]
			public bool FpsCounter
			{
				get => _advancedFpsCounter.OperationMode == OperationMode.Normal;
				set => _advancedFpsCounter.OperationMode = value ? OperationMode.Normal : OperationMode.Disabled;
			}

			[Category(DebugCategory)]
			public bool HDR
			{
				get => UrpAsset.supportsHDR;
				set => UrpAsset.supportsHDR = value;
			}
			
			[Category(DebugCategory)]
			public ShadowResolution ShadowResolution
			{
				get => (ShadowResolution)UrpAsset.mainLightShadowmapResolution;
				set => UrpAsset.mainLightShadowmapResolution = (int)value;
			}
			
			[Category(DebugCategory)]
			public bool LowQualityShaders
			{
				get => Shader.IsKeywordEnabled(CRenderer.LowQualityShaderKeyword);
				set
				{
					if (value)
					{
						Shader.EnableKeyword(CRenderer.LowQualityShaderKeyword);
						return;
					}
					Shader.DisableKeyword(CRenderer.LowQualityShaderKeyword);
				}
			}
			
			[Category(DebugCategory)]
			public void RunMemoryDebug()
			{
				Object.FindFirstObjectByType<CBaseEditorCommands>().RunMemoryDebug();
			}
		}

		#endif
	}
}