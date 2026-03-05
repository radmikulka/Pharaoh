// =========================================
// AUTHOR: Marek Karaba
// DATE:   24.10.2025
// =========================================

using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CStretchAnimation
	{
		private readonly Vector3 _targetScale = Vector3.one * 1.3f;
		private readonly float _scaleDuration = 0.25f;
		private readonly Ease _easeType = Ease.InOutQuad;

		public async UniTask Animate(RectTransform target, CancellationToken ct)
		{
			UniTask task = GetTween(target).ToUniTask(cancellationToken: ct);
			await task;
		}
		
		private Sequence GetTween(RectTransform target)
		{
			Vector3 originalScale = target.localScale;
			Sequence tweenSequence = DOTween.Sequence()
				.Append(target.DOScale(_targetScale, _scaleDuration).SetEase(_easeType))
				.Append(target.DOScale(originalScale, _scaleDuration).SetEase(_easeType))
				.Append(target.DOScale(_targetScale, _scaleDuration).SetEase(_easeType))
				.Append(target.DOScale(originalScale, _scaleDuration).SetEase(_easeType));
			
			return tweenSequence;
		}
	}
}