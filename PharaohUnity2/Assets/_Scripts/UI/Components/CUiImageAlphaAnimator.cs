// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.09.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	[RequireComponent(typeof(CUiComponentImage))]
	public class CUiImageAlphaAnimator : ValidatedMonoBehaviour, IAldaFrameworkComponent, IScreenCloseEnd
	{
		[SerializeField] private float _fadeDuration = 0.2f;
		[SerializeField] private float _waitDuration = 0.2f;
		
		[SerializeField, Self] private CUiComponentImage _image;
		
		private ICtsProvider _ctsProvider;
		private CancellationTokenSource _cts;
		
		[Inject]
		private void Inject(ICtsProvider ctsProvider)
		{
			_ctsProvider = ctsProvider;
		}

		public void OnScreenCloseEnd()
		{
			_cts?.Cancel();
		}

		public void Toggle(bool active)
		{
			_cts?.Cancel();
			
			if (!active)
			{
				_image.SetAlpha(0f);
				return;
			}

			_cts = CancellationTokenSource.CreateLinkedTokenSource(_ctsProvider.Token);
			Animate(_cts.Token).Forget();
		}

		public void SetAlpha(float alpha)
		{
			_image.SetAlpha(alpha);
		}

		private async UniTask Animate(CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{
				await Fade(0f, 1f, ct);

				await UniTask.WaitForSeconds(_waitDuration, cancellationToken: ct);
				
				await Fade(1f, 0f, ct);
				
				await UniTask.WaitForSeconds(_waitDuration, cancellationToken: ct);
			}
		}

		private async UniTask Fade(float initialAlpha, float targetAlpha, CancellationToken ct)
		{
			float time = 0f;
			while (time < _fadeDuration)
			{
				float alpha = Mathf.Lerp(initialAlpha, targetAlpha, time / _fadeDuration);
				_image.SetAlpha(alpha);
				time += Time.deltaTime;
				await UniTask.Yield(ct);
			}
			_image.SetAlpha(targetAlpha);
		}
	}
}