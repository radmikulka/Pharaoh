// =========================================
// AUTHOR: Radek Mikulka
// DATE:   5.3.2024
// =========================================

using System;
using System.Collections.Generic;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CUiLetterBox : ValidatedMonoBehaviour, IInitializable
	{
		private const float BlendDuration = 0.5f;
		
		[SerializeField, Self] private Canvas _canvas;
		[SerializeField] private RectTransform _topBar;
		[SerializeField] private RectTransform _bottomBar;

		private ICtsProvider _ctsProvider;
		private IEventBus _eventBus;
		
		public bool IsVisible { get; private set; }
		
		[Inject]
		private void Inject(IEventBus eventBus, ICtsProvider ctsProvider)
		{
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CSetActiveLetterboxTask>(ProcessActiveLetterBoxTask);
			Kill();
		}
		
		private void ProcessActiveLetterBoxTask(CSetActiveLetterboxTask task)
		{
			float duration = task.Animated ? BlendDuration : 0f;
			if (task.State)
			{
				Show(duration, _ctsProvider.Token);
				return;
			}

			Hide(duration, _ctsProvider.Token).Forget();
		}

		private void Show(float animationDuration, CancellationToken ct)
		{
			SetActiveCanvas(true);
			IsVisible = true;
			_topBar.gameObject.SetActiveObject(true);
			_bottomBar.gameObject.SetActiveObject(true);
			
			_topBar.anchoredPosition = new Vector2(0, _topBar.rect.height);
			_bottomBar.anchoredPosition = new Vector2(0, -_bottomBar.rect.height);
			
			_topBar.DOAnchorPosY(0, animationDuration).ToUniTask(cancellationToken: ct).Forget();
			_bottomBar.DOAnchorPosY(0, animationDuration).ToUniTask(cancellationToken: ct).Forget();
		}
		
		private async UniTaskVoid Hide(float animationDuration, CancellationToken ct)
		{
			_topBar.DOAnchorPosY(_topBar.rect.height, animationDuration).ToUniTask(cancellationToken: ct).Forget();
			UniTask showBottom = _bottomBar.DOAnchorPosY(-_bottomBar.rect.height, animationDuration)
				.ToUniTask(cancellationToken: ct);

			await showBottom;

			SetActiveCanvas(false);
			
			IsVisible = false;
		}
		
		private void Kill()
		{
			SetActiveCanvas(false);

			IsVisible = false;
		}
		
		private void ShowInstantly()
		{
			SetActiveCanvas(true);
			
			_topBar.anchoredPosition = new Vector2(0, 0);
			_bottomBar.anchoredPosition = new Vector2(0, 0);

			IsVisible = true;
		}
		
		private void SetActiveCanvas(bool state)
		{
			_canvas.gameObject.SetActive(state);
		}
	}
}