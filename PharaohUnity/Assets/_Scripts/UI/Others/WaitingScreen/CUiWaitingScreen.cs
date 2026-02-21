// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.09.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Pharaoh
{
	public class CUiWaitingScreen : ValidatedMonoBehaviour, IWaitingScreen, IInitializable
	{
		private const float BlendDuration = 0.1f;
		
		[SerializeField, Self] private CanvasGroup _canvasGroup;
		[SerializeField] private RectTransform _rotablePoint;
		
		private CWaitingScreenProvider _waitingScreenProvider;
		
		[Inject]
		private void Inject(CWaitingScreenProvider waitingScreenProvider)
		{
			_waitingScreenProvider = waitingScreenProvider;
		}

		public void Initialize()
		{
			_waitingScreenProvider.SetWaitingScreen(this);
			gameObject.SetActive(false);
		}

		public void Show()
		{
			gameObject.SetActive(true);
			_canvasGroup.alpha = 0f;

			_canvasGroup.DOFade(1f, BlendDuration);
		}

		private void Update()
		{
			Rotate();
		}

		private void Rotate()
		{
			_rotablePoint.Rotate(0f, 0f, Time.deltaTime * 120f);
		}

		public void Hide()
		{
			_canvasGroup.DOFade(0f, BlendDuration * _canvasGroup.alpha).OnComplete(() =>
			{
				gameObject.SetActive(false);
			});
		}
	}
}