// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.10.2025
// =========================================

using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

namespace TycoonBuilder
{
	public class CUiTutorialCommentatorCharacter : ValidatedMonoBehaviour
	{
		[SerializeField, Self] private RectTransform _rectTransform;
		[SerializeField] private Image _icon;
		
		public async UniTask Show(ITutorialCommentator.ESide side, CancellationToken ct)
		{
			float anchorX = side == ITutorialCommentator.ESide.Left ? 0f : 1f;
			_icon.transform.localScale = new Vector3(side == ITutorialCommentator.ESide.Left ? 1f : -1f, 1f, 1f);
			_rectTransform.anchorMin = new Vector2(anchorX, 0f);
			_rectTransform.anchorMax = new Vector2(anchorX, 0f);
			_rectTransform.anchoredPosition = Vector2.zero;

			await _rectTransform.DOAnchorPos(Vector2.zero, 0.2f)
					.From(new Vector2(_rectTransform.rect.size.x, 0f))
					.WithCancellation(ct)
				;
		}

		public async UniTask Hide(CancellationToken ct)
		{
			await _rectTransform.DOAnchorPos(new Vector2(_rectTransform.rect.size.x, 0f), 0.2f)
				.WithCancellation(ct);
		}
	}
}