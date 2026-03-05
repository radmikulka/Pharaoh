// =========================================
// AUTHOR: Juraj Joscak
// DATE:   03.10.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CGetHudCanvasGroup
	{
		
	}
	
	public class CHudHider : ValidatedMonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField, Child] private CUiComponentCanvasGroup _canvasGroup;

		private ICtsProvider _ctsProvider;
		
		private const float FadeDuration = 0.1f;
		
		private CancellationTokenSource _cts;
		private readonly HashSet<object> _hudHideLockers = new();
		
		public CUiComponentCanvasGroup CanvasGroup => _canvasGroup;

		[Inject]
		private void Inject(ICtsProvider ctsProvider)
		{
			_ctsProvider = ctsProvider;
		}
		
		public void Show(object lockObject, bool instant, float delay)
		{
			_hudHideLockers.Remove(lockObject);
			
			if (_hudHideLockers.Count > 0)
				return;
			
			_cts?.Cancel();
			_cts = _ctsProvider.GetNewLinkedCts();
			
			_canvasGroup.SetInteractable(true);
			if(instant)
			{
				SetAlpha(1);
				return;
			}
			
			ShowAsync(FadeDuration, delay, _cts.Token).Forget();
		}

		public void Hide(object lockObject, bool instant)
		{
			if (_hudHideLockers.Count > 0)
			{
				_hudHideLockers.Add(lockObject);
				return;
			}
			
			_hudHideLockers.Add(lockObject);
			_cts?.Cancel();
			_cts = _ctsProvider.GetNewLinkedCts();

			_canvasGroup.SetInteractable(false);
			if (instant)
			{
				SetAlpha(0);
				return;
			}
			HideAsync(FadeDuration, _cts.Token).Forget();
		}

		private async UniTask ShowAsync(float fadeTime, CancellationToken token)
		{
			await ShowAsync(fadeTime, 0f, token);
		}
		
		private async UniTask ShowAsync(float fadeTime, float delay, CancellationToken token)
		{
			if (delay > 0)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
			}
			else
			{
				await UniTask.DelayFrame(1, cancellationToken: token);
			}
			await FadeTaskAsync(1, fadeTime, token);
		}

		private async UniTask HideAsync(float fadeTime, CancellationToken token)
		{
			await FadeTaskAsync(0, fadeTime, token);
		}

		private async UniTask FadeTaskAsync(float targetAlpha, float fadeTime, CancellationToken token)
		{
			await _canvasGroup.DOFade(targetAlpha ,fadeTime).WithCancellation(token);
			SetAlpha(targetAlpha);
		}
		
		private void SetAlpha(float alpha)
		{
			_canvasGroup.SetAlpha(alpha);
		}
	}
}