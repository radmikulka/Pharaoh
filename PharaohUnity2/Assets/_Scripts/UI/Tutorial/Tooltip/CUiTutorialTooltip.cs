// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.12.2023
// =========================================

using System;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TycoonBuilder
{
	public class CUiTutorialTooltip : ValidatedMonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField, Child] private CUiTutorialTooltipSideGraphics[] _sideDependentGraphics;
		[SerializeField] private HorizontalLayoutGroup _textParentLayoutGroup;
		[SerializeField] private CGradualTextEffect _gradualTextEffect;
		[SerializeField] private GameObject _avatarGraphicsParent;
		[SerializeField] private RectTransform _tooltipWindow;
		[SerializeField] private CUiButton _continueButton;

		private CTweener _continueButtonTweener;
		private IInputLock _continueInputLock;
		private RectTransform _rectTransform;
		private CEventSystem _eventSystem;
		private ICtsProvider _ctsProvider;
		private ITranslation _translation;
		private IEventBus _eventBus;

		private CanvasGroup _canvasGroup;
		private Vector2 _defaultSize;
		private Canvas _canvas;

		[Inject]
		private void Inject(
			ITranslation translation, 
			ICtsProvider ctsProvider, 
			CEventSystem eventSystem, 
			IEventBus eventBus
			)
		{
			_ctsProvider = ctsProvider;
			_translation = translation;
			_eventSystem = eventSystem;
			_eventBus = eventBus;
		}

		public void Construct()
		{
			_rectTransform = GetComponent<RectTransform>();
			_canvasGroup = GetComponent<CanvasGroup>();
			_canvas = GetComponent<Canvas>();
			ResizeWidthByDevice();
			_defaultSize = _tooltipWindow.sizeDelta;
			_continueInputLock = new CInputLock("tutorial_tooltip_continue", EInputLockLayer.Tutorial, _continueButton.GetComponent<RectTransform>());
			_continueButton.AddClickListener(OnContinueClicked);
			_continueButtonTweener = _continueButton.GetComponent<CTweener>();
			SetActive(false);
		}

		private void OnContinueClicked()
		{
			_eventBus.Send(new CTutorialContinueClickedSignal());
		}

		private void ResizeWidthByDevice()
		{
			float mod = CMath.Min(1f, CScreen.ScreenRaito / CScreen.ReferenceScreenRaito * 1.2f);
			Vector2 size = _tooltipWindow.rect.size;
			_tooltipWindow.sizeDelta = new Vector2(size.x * mod, size.y);
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CShowTutorialTooltipTask>(OnShowTutorialTooltip);
			_eventBus.AddTaskHandler<CHideTutorialTooltipTask>(OnHideTutorialTooltip);
		}

		private void OnHideTutorialTooltip(CHideTutorialTooltipTask task)
		{
			SetActiveAnimated(false);
		}

		private void OnShowTutorialTooltip(CShowTutorialTooltipTask task)
		{
			TryShowTooltipAsync(task).Forget();
		}

		private async UniTask TryShowTooltipAsync(CShowTutorialTooltipTask task)
		{
			try
			{
				await ShowTooltipAsync(task);
			}
			catch (OperationCanceledException)
			{
				
			}
		}

		private void SetSide(ETutorialTooltipSide side)
		{
			foreach (CUiTutorialTooltipSideGraphics graphics in _sideDependentGraphics)
			{
				graphics.SetSide(side);
			}
		}

		private async UniTask ShowTooltipAsync(CShowTutorialTooltipTask task)
		{
			SetActiveAnimated(true);

			_eventBus.Send(new CTutorialTooltipShowingStarted(task.TextLangKey));
			string text = _translation.GetText(task.TextLangKey);
			
			_tooltipWindow.sizeDelta = _defaultSize + task.SizeOffset;
			LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipWindow);
			_gradualTextEffect.Prepare(text);

			SetActiveContinueButton(task.ShowContinueButton);
			SetInteractable(task.ShowContinueButton);
			if (task.ShowContinueButton)
			{
				_eventSystem.AddInputLocker(_continueInputLock);
			}
			
			ResizeWindow(task.SizeOffset, task.ShowContinueButton);
			SetSide(task.Side);
			_avatarGraphicsParent.SetActiveObject(task.ShowAvatar);
				
			_tooltipWindow.position = task.Target.position;
			_tooltipWindow.anchoredPosition += task.AnchoredOffset;
			
			Vector2 pos = _tooltipWindow.anchoredPosition;
			Vector2 size = _tooltipWindow.rect.size;
			Vector2 pivot = _tooltipWindow.pivot;
			Rect parentRect = _rectTransform.rect;
			float left   = -parentRect.width / 2f + size.x * pivot.x;
			float right  = parentRect.width / 2f - size.x * (1f - pivot.x);
			float bottom = -parentRect.height / 2f + size.y * pivot.y;
			float top    = parentRect.height / 2f - size.y * (1f - pivot.y);

			pos.x = Mathf.Clamp(pos.x, left, right);
			pos.y = Mathf.Clamp(pos.y, bottom, top);

			_tooltipWindow.anchoredPosition = pos;
			
			MarkLayoutDirtyAsync().Forget();
				
			await _gradualTextEffect.PlayAnimationAsync(_ctsProvider.Token);
		}

		private void SetInteractable(bool state)
		{
			_canvasGroup.interactable = state;
			_canvasGroup.blocksRaycasts = state;
		}

		private async UniTaskVoid MarkLayoutDirtyAsync()
		{
			bool cancelled = await UniTask.DelayFrame(1, cancellationToken: _ctsProvider.Token).SuppressCancellationThrow();
			if(!_gradualTextEffect || cancelled)
				return;
			LayoutRebuilder.MarkLayoutForRebuild((RectTransform)_gradualTextEffect.transform);
		}

		private void SetActiveAnimated(bool state)
		{
			_canvasGroup.DOKill();
			
			if (state)
			{
				SetActive(true);
				_canvasGroup.alpha = 0f;
				_canvasGroup.DOFade(1f, 0.2f);
				return;
			}
			
			_canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
			{
				SetActive(false);
			}).OnKill(() =>
			{
				SetActive(false);
			});
		}
		
		private void SetActiveContinueButton(bool state)
		{
			_continueButton.gameObject.SetActiveObject(state);
			
			if (state)
			{
				_continueButtonTweener.Enable();
				return;
			}
			
			_continueButtonTweener.Disable();
		}

		private void ResizeWindow(Vector2 sizeOffset, bool showContinueButton)
		{
			float textHeight = _gradualTextEffect.GetTargetTextHeight();
			Vector2 currentSize = _tooltipWindow.sizeDelta;
			float paddingY = TetYPadding(showContinueButton);
			currentSize.y = textHeight + paddingY;
			currentSize += sizeOffset;
			
			_tooltipWindow.sizeDelta = currentSize;
		}

		private float TetYPadding(bool showContinueButton)
		{
			int padding = _textParentLayoutGroup.padding.bottom + _textParentLayoutGroup.padding.top;
			if (showContinueButton)
			{
				padding += 135;
			}
			return padding;
		}

		private void SetActive(bool state)
		{
			_canvas.enabled = state;

			if (!state)
			{
				_eventSystem.RemoveInputLocker(_continueInputLock);
			}
		}
	}
}