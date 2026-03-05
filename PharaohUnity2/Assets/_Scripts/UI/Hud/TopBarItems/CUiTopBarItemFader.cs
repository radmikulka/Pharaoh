// =========================================
// AUTHOR: Juraj Joscak
// DATE:   25.09.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	[RequireComponent(typeof(CanvasGroup))]
	public class CUiTopBarItemFader : ValidatedMonoBehaviour, IConstructable
	{
		[SerializeField, Self] private CUiTopBarItem _topBarItem;
		[SerializeField, Self] private CUiComponentCanvasGroup _canvasGroup;

		private const float FadeInDuration = 0.15f;
		private const float FadeOutDuration = 0.15f;
		public ETopBarItem Id => _topBarItem.Id;
		public RectTransform RectTransform { get; private set; }
		
		private Tween _fadeTween;
		private Tween _positionTween;
		private bool _visible;
		
		public void Construct()
		{
			RectTransform = (RectTransform)transform;
		}
		
		public void SetVisible(bool state)
		{
			_canvasGroup.SetAlpha(state ? 1 : 0);
			_canvasGroup.SetInteractable(state);
			_visible = state;
		}
		
		public void FadeIn(bool showButton)
		{
			if(_visible)
				return;
			
			_visible = true;
			_canvasGroup.SetInteractable(true);
			_topBarItem.ShowButton(showButton);
			_fadeTween?.Kill();
			
			_fadeTween = _canvasGroup.DOFade(1, FadeInDuration)
				.OnComplete(() =>
				{
					_fadeTween = null;
				})
				;
		}
		
		public void FadeOut()
		{
			if(!_visible)
				return;
			
			_visible = false;
			_fadeTween?.Kill();
			_fadeTween = _canvasGroup.DOFade(0, FadeOutDuration)
				.OnComplete(() =>
				{
					_fadeTween = null;
					_topBarItem.ShowButton(true);
					_canvasGroup.SetInteractable(false);
				})
				;
		}
		
		public void MoveToPosition(float targetPosition)
		{
			_positionTween?.Kill();
			RectTransform.DOLocalMoveX(targetPosition, FadeInDuration)
				.SetRelative(false)
				.OnComplete(() =>
				{
					_positionTween = null;
				})
				;
		}
	}
}