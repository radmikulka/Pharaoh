// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.08.2025
// =========================================

using System;
using AldaEngine;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using BlitMaterialParameters = UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils.BlitMaterialParameters;

namespace Pharaoh
{
	public class CFreezeRendererFeature : ScriptableRendererFeature
	{
		[Serializable]
		public class CBlurSettings
		{
			[Range(1, CBlurPass.MaxIterations)] public int Iterations = 3;

			[Range(0.01f, 3f)] public float PixelOffset = 1f;
		}

		private class CBlurPass : ScriptableRenderPass
		{
			public const int MaxIterations = 6;

			private readonly RTHandle[] _rtHandles = new RTHandle[MaxIterations];
			private readonly TextureHandle[] _textureHandles = new TextureHandle[MaxIterations];

			private static readonly int PixelOffset = Shader.PropertyToID("_PixelOffset");
			private readonly CBlurSettings _blurSettings;
			private readonly Material _material;
			private RTHandle _destination;

			public CBlurPass(
				CBlurSettings blurSettings,
				Material material
			)
			{
				_blurSettings = blurSettings;
				_material = material;
			}

			public void Setup(RTHandle destination)
			{
				_destination = destination;
			}

			public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
			{
				UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
				UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

				if (cameraData.camera.cameraType != CameraType.Game)
					return;

				_material.SetFloat(PixelOffset, _blurSettings.PixelOffset);

				TextureHandle source = resourceData.activeColorTexture;
				TextureHandle destination = new();

				RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
				desc.depthBufferBits = 0;
				desc.msaaSamples = 1;

				int blurIterations = CMath.Clamp(1, _blurSettings.Iterations, MaxIterations);

				for (int i = 0; i < blurIterations; i++)
				{
					desc.width >>= 1;
					desc.height >>= 1;

					RTHandle handle = _rtHandles[i];
					RenderingUtils.ReAllocateHandleIfNeeded(ref handle, desc, FilterMode.Bilinear,
						TextureWrapMode.Clamp);
					_textureHandles[i] = renderGraph.ImportTexture(handle);
				}

				for (int i = 0; i < blurIterations; i++)
				{
					destination = _textureHandles[i];
					BlitMaterialParameters blit = new(source, destination, _material, 0);
					renderGraph.AddBlitPass(blit, "Blur");
					source = destination;
				}

				for (int i = blurIterations - 2; i >= 0; i--)
				{
					destination = _textureHandles[i];
					BlitMaterialParameters blit = new(source, destination, _material, 0);
					renderGraph.AddBlitPass(blit, "Blur");
					source = destination;
				}

				TextureHandle finalDestination = renderGraph.ImportTexture(_destination);
				BlitMaterialParameters finalBlit = new(source, finalDestination, _material, 0);
				renderGraph.AddBlitPass(finalBlit, "Final Blur");
			}

			public void Dispose()
			{
				foreach (var handle in _rtHandles)
				{
					handle?.Release();
				}
			}
		}

		private class CRenderPass : ScriptableRenderPass
		{
			private RTHandle _frame;

			public void Setup(RTHandle frame)
			{
				_frame = frame;
			}

			public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
			{
				UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
				TextureHandle frame = renderGraph.ImportTexture(_frame);
				TextureHandle destination = resourceData.activeColorTexture;
				
				//resourceData.cameraColor = frame;	// cant be used - urp forgets cameraColor is some cases :(
				BlitMaterialParameters blitParams = new(frame, destination, CDefaultBlitMaterial.Material, 0);
				renderGraph.AddBlitPass(blitParams, "Blit freeze frame");
			}
		}

		[SerializeField] private CBlurSettings _blurSettings;
		[SerializeField] private Shader _blurShader;

		private CRenderPass _renderPass;
		private CBlurPass _blurPass;
		static private Material _material;
		private RTHandle _frame;

		private bool _captureNextFrame;
		private bool _renderLastFrame;
		private bool _isEnabled;

		public void RefreshActivity(bool isEnabled)
		{
			if(_isEnabled == isEnabled)
				return;
			
			_isEnabled = isEnabled;
			_captureNextFrame = isEnabled;
			_renderLastFrame = !isEnabled;
		}

		public override void Create()
		{
			_material = CoreUtils.CreateEngineMaterial(_blurShader);

			_blurPass = new CBlurPass(_blurSettings, _material)
			{
				renderPassEvent = RenderPassEvent.AfterRenderingTransparents
			};

			_renderPass = new CRenderPass
			{
				renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
			};
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if(!Application.isPlaying)
				return;
			
			bool isFreezeCamera = renderingData.cameraData.camera.GetComponent<CFreezeRendererCamera>() != null;
			if(!isFreezeCamera)
				return;
			
			if (!_isEnabled)
			{
				FreezeRender(renderingData.cameraData.camera, false);
				if (!_renderLastFrame)
				{
					return;
				}
			}
			
			if (renderingData.cameraData.cameraType != CameraType.Game)
				return;
			
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.msaaSamples = 1;
			descriptor.depthBufferBits = 0;
			descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
			RenderingUtils.ReAllocateHandleIfNeeded(ref _frame, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp);

			if (_captureNextFrame)
			{
		
				_blurPass.Setup(_frame);
				renderer.EnqueuePass(_blurPass);
				_captureNextFrame = false;
				FreezeRender(renderingData.cameraData.camera, true);
			}
			
			RenderFrameToScreen(renderer);
			_renderLastFrame = false;
		}

		private void FreezeRender(Camera camera, bool state)
		{
			IMainCamera mainCamera = camera.GetComponent<IMainCamera>();
			mainCamera?.FreezeRender(state);
		}

		private void RenderFrameToScreen(ScriptableRenderer renderer)
		{
			_renderPass.Setup(_frame);
			renderer.EnqueuePass(_renderPass);
		}

		protected override void Dispose(bool disposing)
		{
			CoreUtils.Destroy(_material);
			_material = null;

			_blurPass?.Dispose();
			_blurPass = null;
		}
	}
}