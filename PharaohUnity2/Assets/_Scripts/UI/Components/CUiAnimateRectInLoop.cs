// =========================================
// AUTHOR:
// DATE:   30.09.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using TycoonBuilder.Ui;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiAnimateRectInLoop : MonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField] private RectTransform _visual;
		[Header("Settings")]
		[SerializeField] private float _delay;
		[SerializeField] private float _scaleFactor = 1.2f;
		[SerializeField] private float _scaleDuration = 0.25f;

		private CAnimationProvider _animationProvider;
		private ICtsProvider _ctsProvider;

		private CancellationTokenSource _cts;
		private UniTask _animationTask;

		[Inject]
		private void Inject(
			CAnimationProvider animationProvider,
			ICtsProvider ctsProvider)
		{
			_animationProvider = animationProvider;
			_ctsProvider = ctsProvider;
		}
		
		public void Toggle(bool active)
		{
			if (!active)
			{
				_cts?.Cancel();
				return;
			}

			if (_animationTask.Status == UniTaskStatus.Pending)
				return;
			
			_cts = CancellationTokenSource.CreateLinkedTokenSource(_ctsProvider.Token);
			_animationTask = Animate(_cts.Token);
		}

		private async UniTask Animate(CancellationToken ct)
		{
			while (Application.isPlaying && !ct.IsCancellationRequested)
			{
				await UniTask.WaitForSeconds(_delay, cancellationToken: ct);
				await _animationProvider.ScaleUpAndDown.ScaleRect(_visual, _scaleFactor, _scaleDuration, ct);
			}
		}
	}
}