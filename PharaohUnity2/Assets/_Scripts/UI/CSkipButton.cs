// =========================================
// AUTHOR: Marek Karaba
// DATE:   09.09.2025
// =========================================

using System;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CSkipButton : ValidatedMonoBehaviour, IInitializable, IConstructable
	{
		[SerializeField] private float _animationSpeed = 1f;
		
		private const float RegularXSize = 100f;
		
		[SerializeField] private CUiComponentText _skipAllText;
		[SerializeField] private Animation _skipAllAnimation;
		[SerializeField, Self] private CUiComponentGraphicGroup _graphicGroup;
		[SerializeField, Self] private RectTransform _rectTransform;
		[SerializeField, Self] private CUiButton _button;
		
		private CEventSystem _eventSystem;
		private ICtsProvider _ctsProvider;
		private IEventBus _eventBus;

		private CancellationTokenSource _cts;
		private IInputLock _inputLock;
		private bool _readyToSkip;
		
		[Inject]
		private void Inject(
			CEventSystem eventSystem,
			ICtsProvider ctsProvider,
			IEventBus eventBus)
		{
			_eventSystem = eventSystem;
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
		}
		
		public void Construct()
		{
			_inputLock = new CInputLock("SkipButton", EInputLockLayer.SkipButton, _rectTransform);
		}
		
		public void Initialize()
		{
			_button.AddClickListener(OnButtonClick);
			_eventBus.Subscribe<CBlockedEscapePressedSignal>(OnBlockedEscapePressed);
			HideImmediately();
			
			_eventBus.AddTaskHandler<CSkipButtonRectTransformRequest, RectTransform>(GetRectTransform);
			_eventBus.AddTaskHandler<CShowSkipButtonRequest>(ShowRequest);
			_eventBus.AddTaskHandler<CHideSkipButtonRequest>(HideRequest);
		}

		private void ShowRequest(CShowSkipButtonRequest request)
		{
			ResetToRegularSize();
			CancelCts();
			
			gameObject.SetActiveObject(true);
			_eventSystem.AddInputLocker(_inputLock);

			FadeIn(0.2f, _cts.Token).Forget();
		}
		
		private void HideRequest(CHideSkipButtonRequest request)
		{
			_readyToSkip = false;
			CancelCts();
			FadeOut(0.2f, _cts.Token).Forget();
		}

		private RectTransform GetRectTransform(CSkipButtonRectTransformRequest request)
		{
			return _rectTransform;
		}

		private void HideImmediately()
		{
			_readyToSkip = false;
			CancelCts();
			
			_graphicGroup.SetColor(Color.clear, true);
			gameObject.SetActiveObject(false);
			
			_eventSystem.RemoveInputLocker(_inputLock);
		}

		private void CancelCts()
		{
			_cts?.Cancel();
			_cts = _ctsProvider.GetNewLinkedCts();
		}

		private async UniTask FadeIn(float duration, CancellationToken ct)
		{
			UniTask task = _graphicGroup.DOColor(Color.white, duration).ToUniTask(cancellationToken: ct);
			await task;
		}
		
		private async UniTask FadeOut(float duration, CancellationToken ct)
		{
			UniTask task = _graphicGroup.DOColor(Color.clear, duration).ToUniTask(cancellationToken: ct);
			await task;
				
			gameObject.SetActiveObject(false);
			_eventSystem.RemoveInputLocker(_inputLock);
		}

		private void ResetToRegularSize()
		{
			_readyToSkip = false;
			_skipAllText.gameObject.SetActive(false);
			_rectTransform.sizeDelta = new Vector2(RegularXSize, _rectTransform.sizeDelta.y);
		}

		private void OnButtonClick()
		{
			if (!_readyToSkip)
			{
				_skipAllAnimation[_skipAllAnimation.clip.name].speed = _animationSpeed;
				_skipAllAnimation.Play();
				_readyToSkip = true;
				return;
			}
			
			_eventBus.Send(new CAnimationSkipRequestedSignal());
			//_eventBus.Send(new CAnimationSpeedChangeableSignal(false));
		}

		private void OnBlockedEscapePressed(CBlockedEscapePressedSignal signal)
		{
			if (!gameObject.activeSelf)
				return;
			
			if (!_readyToSkip)
			{
				_readyToSkip = true;
				return;
			}
			
			_eventBus.Send(new CAnimationSkipRequestedSignal());
			//_eventBus.Send(new CAnimationSpeedChangeableSignal(false));
		}
	}
}