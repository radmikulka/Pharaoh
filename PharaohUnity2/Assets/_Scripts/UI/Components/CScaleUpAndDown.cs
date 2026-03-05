// =========================================
// AUTHOR: Marek Karaba
// DATE:   29.08.2025
// =========================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CScaleUpAndDown
	{
		private const float DefaultScaleFactor = 2f;
		private const float DefaultDuration = 0.25f;
		
		public async UniTask ScaleRect(RectTransform rectTransform, CancellationToken ct)
		{
			await ScaleRectAnimated(rectTransform, DefaultScaleFactor, DefaultDuration, ct);
		}

		public async UniTask ScaleRect(RectTransform rectTransform, float scaleFactor, float duration, CancellationToken ct)
		{
			await ScaleRectAnimated(rectTransform, scaleFactor, duration, ct);
		}

		private async UniTask ScaleRectAnimated(RectTransform rectTransform, float scaleFactor, float duration, CancellationToken ct)
		{
			Vector3 initialScale = Vector3.one;
			Vector3 targetScale = initialScale * scaleFactor;
			
			float halfDuration = duration / 2f;
			UniTask scaleUp = rectTransform.DOScale(targetScale, halfDuration).SetEase(Ease.Linear).ToUniTask(cancellationToken: ct);
			await scaleUp;

			UniTask scaleDown = rectTransform.DOScale(initialScale, halfDuration).SetEase(Ease.Linear).ToUniTask(cancellationToken: ct);
			await scaleDown;
		}
		
		public async UniTask AnimateScaleDown(RectTransform rectTransform, float duration, CancellationToken ct)
		{
			await rectTransform.DOScale(Vector3.zero, duration).SetEase(Ease.Linear).ToUniTask(cancellationToken: ct);
		}

		public async UniTask AnimateScaleUp(RectTransform rectTransform, float duration, CancellationToken ct)
		{
			await rectTransform.DOScale(Vector3.one, duration).SetEase(Ease.Linear).ToUniTask(cancellationToken: ct);
		}
		
		public async UniTask AnimateScaleDownAndUpWithBounce(RectTransform rectTransform, float duration, CancellationToken ct, Action onHighestScaleAction = null)
		{
			Sequence sequence = DOTween.Sequence()
				.Append(rectTransform.DOScale(Vector3.zero, duration * 0.3f).SetEase(Ease.Linear))
				.Append(rectTransform.DOScale(Vector3.one * 1.4f, duration * 0.3f).SetEase(Ease.Linear))
				.AppendCallback(() => onHighestScaleAction?.Invoke())
				.AppendInterval(duration * 0.1f)
				.Append(rectTransform.DOScale(Vector3.one, duration * 0.3f).SetEase(Ease.Linear));

			await sequence.ToUniTask(cancellationToken: ct);
		}
	}
}