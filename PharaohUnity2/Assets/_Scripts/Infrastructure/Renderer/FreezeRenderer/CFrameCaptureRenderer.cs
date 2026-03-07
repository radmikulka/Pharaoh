// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.08.2025
// =========================================

using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace Pharaoh
{
	public class CFrameCaptureRenderer : MonoBehaviour, IInitializable
	{
		private readonly HashSet<CLockObject> _lockObjects = new();
		private CFrameCaptureRendererFeature _rendererFeature;
		private CRenderer _renderer;
		private IEventBus _eventBus;
		
		private bool _isEnabled;

		[Inject]
		private void Inject(IEventBus eventBus, CRenderer rend)
		{
			_eventBus = eventBus;
			_renderer = rend;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CActivateFrameCaptureTask>(HandleActivateFrameCapture);
			_eventBus.AddTaskHandler<CDeactivateFrameCaptureTask>(HandleDeactivateFrameCapture);
			
			LoadRenderFeature();
			_rendererFeature.RefreshActivity(false);
		}

		private void HandleDeactivateFrameCapture(CDeactivateFrameCaptureTask task)
		{
			_lockObjects.Remove(task.LockObject);
			Refresh();
		}

		private void HandleActivateFrameCapture(CActivateFrameCaptureTask task)
		{
			_lockObjects.Add(task.LockObject);
			Refresh();
		}

		private void Refresh()
		{
			bool shouldFreeze = _lockObjects.Count > 0;
			
			if(_isEnabled != shouldFreeze)
			{
				_rendererFeature.RefreshActivity(shouldFreeze);
			}
			
			_isEnabled = shouldFreeze;
		}

		private void LoadRenderFeature()
		{
			ScriptableRendererData rendererData = _renderer.GetRendererData();
			rendererData.TryGetRendererFeature(out _rendererFeature);
			Assert.IsNotNull(_rendererFeature);
		}
	}
}