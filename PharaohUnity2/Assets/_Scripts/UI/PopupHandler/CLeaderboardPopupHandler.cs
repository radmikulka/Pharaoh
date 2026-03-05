// =========================================
// AUTHOR: Marek Karaba
// DATE:   27.01.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CLeaderboardPopupHandler : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Child] private CUserDetailsPopup _userDetailsPopup;
		[SerializeField] private CUiButton _backgroundButton;
		
		private IEventBus _eventBus;
		
		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			SetVisible(false);
			
			_eventBus.AddTaskHandler<CShowUserDetailsPopupTask>(OnShowUserDetailsPopupTask);
			_eventBus.AddTaskHandler<CHideUserDetailsPopupTask>(OnHideUserDetailsPopupTask);
			_backgroundButton.AddClickListener(OnBackgroundButtonClick);
		}

		private void OnShowUserDetailsPopupTask(CShowUserDetailsPopupTask task)
		{
			SetPosition(task.ItemRect, task.ContentRect);
			SetVisible(true);
			_userDetailsPopup.SetOnClickAction(task.OnClickAction);
		}

		private void OnHideUserDetailsPopupTask(CHideUserDetailsPopupTask task)
		{
			SetVisible(false);
		}

		private void OnBackgroundButtonClick()
		{
			SetVisible(false);
		}

		private void SetPosition(RectTransform itemRect, RectTransform contentRect)
		{
			Vector3[] itemCorners = new Vector3[4];
			Vector3[] contentCorners = new Vector3[4];
    
			itemRect.GetWorldCorners(itemCorners);
			contentRect.GetWorldCorners(contentCorners);

			float itemCenterY = (itemCorners[0].y + itemCorners[1].y) / 2f;
			float contentCenterY = (contentCorners[0].y + contentCorners[1].y) / 2f;

			Vector3 targetWorldPosition = (itemCorners[0] + itemCorners[2]) / 2f;

			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				transform as RectTransform,
				RectTransformUtility.WorldToScreenPoint(null, targetWorldPosition),
				null,
				out Vector2 localPoint
			);
			
			bool isBottomHalf = itemCenterY < contentCenterY;
			float xOffset = -330f;
			localPoint.x += xOffset;
			float yOffset = 80f;
			if (isBottomHalf)
			{
				localPoint.y += yOffset;
			}
			else
			{
				localPoint.y -= yOffset;
			}
			_userDetailsPopup.SetAnchoredPosition(localPoint);
			_userDetailsPopup.SetTopBottomVisual(isBottomHalf);
		}

		private void SetVisible(bool visible)
		{
			SendVisibilitySignal(visible);
			
			_userDetailsPopup.gameObject.SetActive(visible);
			_backgroundButton.gameObject.SetActive(visible);
			
			if (visible)
				return;
			
			_userDetailsPopup.SetOnClickAction(null);
		}

		private void SendVisibilitySignal(bool visible)
		{
			if (!visible)
			{
				_eventBus.Send(new CUserDetailsPopupHiddenSignal());
			}
		}
	}
}