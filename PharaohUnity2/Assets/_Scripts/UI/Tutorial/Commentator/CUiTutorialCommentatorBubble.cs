// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.10.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiTutorialCommentatorBubble : ValidatedMonoBehaviour, IConstructable
	{
		private enum EState
		{
			Hidden,
			Animating,
			Shown
		}
		
		private const float AnimationDuration = 0.2f;
		private const float HiddenPosY = -200f;

		[SerializeField, Self] private RectTransform _rectTransform;
		[SerializeField, Self] private CanvasGroup _canvasGroup;
		[SerializeField] private CGradualTextEffect _contentText;

		private Vector3 _defaultAnchoredPos;
		private ITranslation _translation;
		private EState _state;
		
		public bool IsAnimating => _state == EState.Animating;

		[Inject]
		private void Inject(ITranslation translation)
		{
			_translation = translation;
		}

		public void Construct()
		{
			_defaultAnchoredPos = _rectTransform.anchoredPosition;
		}

		public async UniTask SetText(string langKey, CancellationToken ct)
		{
			_state = EState.Animating;
			
			if (_state != EState.Hidden)
			{
				_canvasGroup.DOFade(0f, AnimationDuration).WithCancellation(ct).Forget();
				await _rectTransform.DOAnchorPos(new Vector2(0f, HiddenPosY), AnimationDuration).WithCancellation(ct);
			}
			
			string text = _translation.GetText(langKey);
			_contentText.Prepare(text);
			ResizeWindow();

			_canvasGroup.DOFade(1f, AnimationDuration).WithCancellation(ct).Forget();
			await _rectTransform.DOAnchorPos(_defaultAnchoredPos, AnimationDuration).WithCancellation(ct);

			_state = EState.Shown;
			await _contentText.PlayAnimationAsync(ct);
		}

		public void ResetState()
		{
			_state = EState.Hidden;
			
			_canvasGroup.DOKill();
			_canvasGroup.alpha = 0f;

			_rectTransform.anchoredPosition = new Vector2(0f, HiddenPosY);
		}

		public void FinishAnimation()
		{
			_contentText.FinishAnimation();
		}

		public async UniTask Hide(CancellationToken ct)
		{
			UniTask canvasGroupHide = _canvasGroup.DOFade(0f, AnimationDuration).WithCancellation(ct);
			UniTask rectHide = _rectTransform.DOAnchorPos(new Vector2(0f, HiddenPosY), AnimationDuration)
				.WithCancellation(ct);
			await UniTask.WhenAll(canvasGroupHide, rectHide);
		}

		private void ResizeWindow()
		{
			float textHeight = _contentText.GetTargetTextHeight();
			Vector2 currentSize = _rectTransform.sizeDelta;
			currentSize.y = textHeight;
			
			_rectTransform.sizeDelta = currentSize;
		}
	}
}