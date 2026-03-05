// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.01.2026
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

namespace TycoonBuilder.Ui
{
	public class CUiScrollRectHandler : ValidatedMonoBehaviour
	{
		[SerializeField, Self] private ScrollRect _scrollRect;
		
		public ScrollRect ScrollRect => _scrollRect;
		
		public async UniTask AnimateScroll(Vector2 targetPosition, float duration, CancellationToken ct)
		{
			float time = 0f;
			Vector2 startingPosition = _scrollRect.content.anchoredPosition;
			while (time < duration)
			{
				ct.ThrowIfCancellationRequested();
				
				time += Time.deltaTime;
				float progress = time / duration;
				float easedProgress = CMath.SmoothStep(progress, 0f, 1f);
				Vector2 newPosition = Vector2.Lerp(startingPosition, targetPosition, easedProgress);
				_scrollRect.content.anchoredPosition = newPosition;
				await UniTask.Yield(ct);
			}
			_scrollRect.content.anchoredPosition = targetPosition;
		}
		
		public Vector2 GetViewportCenterScrollPosition(CVirtualRectTransform virtualRect)
		{
			float userPosition = -virtualRect.Position.y;
			float viewportHeight = _scrollRect.viewport.rect.height;
			float targetPosition = userPosition - viewportHeight / 2;
			float clampedPosition = Mathf.Clamp(targetPosition, 0, _scrollRect.content.rect.height - viewportHeight);
			Vector2 finalPosition = new (_scrollRect.content.anchoredPosition.x, clampedPosition);
			return finalPosition;
		}
		
		public void SetScrollPosition(Vector2 position)
		{
			_scrollRect.content.anchoredPosition = position;
		}
	}
}