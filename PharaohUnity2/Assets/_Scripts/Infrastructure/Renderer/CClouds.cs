// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.10.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace Pharaoh
{
	public class CClouds : MonoBehaviour, IInitializable
	{
		[SerializeField] private Light _light;
		[SerializeField] private Material _combineMaterial;

		private static RenderTexture _renderTexture;
		private IEventBus _eventBus;
		private CRenderer _renderer;
		
		[Inject]
		private void Inject(CRenderer rend, IEventBus eventBus)
		{
			_eventBus = eventBus;
			_renderer = rend;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CGraphicsQualityChangedSignal>(OnGraphicsQualityChanged);
			Refresh();
		}

		private void OnGraphicsQualityChanged(CGraphicsQualityChangedSignal signal)
		{
			Refresh();
		}

		private void Refresh()
		{
			RefreshRenderTexture();
			_light.cookie = _renderTexture;
		}

		private void RefreshRenderTexture()
		{
			CreateRt();
		}

		private void CreateRt()
		{
			const int rtSize = 512;
			_renderTexture = new RenderTexture(rtSize, rtSize, 0, RenderTextureFormat.ARGB32)
			{
				wrapMode = TextureWrapMode.Repeat,
				filterMode = FilterMode.Bilinear,
				enableRandomWrite = false,
				useMipMap = false
			};
			
			_light.cookie = _renderTexture;
		}

		private void Update()
		{
			Blit();
		}

		private void Blit()
		{
			Graphics.Blit(null, _renderTexture, _combineMaterial);
		}

		private void OnDestroy()
		{
			DestroyRt();
		}

		private void DestroyRt()
		{
			if (_renderTexture)
			{
				Destroy(_renderTexture);
			}

			_renderTexture = null;
		}
	}
}