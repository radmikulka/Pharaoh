// =========================================
// AUTHOR: Marek Karaba
// DATE:   27.01.2026
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CUserDetailsPopup : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private RectTransform _rectTransform;
		[SerializeField] private GameObject _bottomHalfVisual;
		[SerializeField] private GameObject _topHalfVisual;
		[SerializeField] private CUiButton _viewProfileButton;
		
		private IEventBus _eventBus;
		
		private Action _onClickAction;

		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_viewProfileButton.AddClickListener(OnViewProfileButtonClicked);
		}

		public void SetAnchoredPosition(Vector2 newPos)
		{
			_rectTransform.anchoredPosition = newPos;
		}

		public void SetTopBottomVisual(bool isBottomHalf)
		{
			_bottomHalfVisual.SetActive(isBottomHalf);
			_topHalfVisual.SetActive(!isBottomHalf);
		}

		public void SetOnClickAction(Action action)
		{
			_onClickAction = action;
		}

		private void OnViewProfileButtonClicked()
		{
			_onClickAction?.Invoke();
			_eventBus.ProcessTask(new CHideUserDetailsPopupTask());
		}
	}
}