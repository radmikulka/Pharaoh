// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.10.2025
// =========================================

using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CBaseSmartArrow
	{
		private readonly ITutorialCommentator _tutorialCommentator;
		private readonly ISmartArrowLocker _smartArrowLocker;
		private readonly IDialogueHandler _dialogueHandler;
		private ISmartArrow _smartArrow;

		protected readonly IEventBus EventBus;
		protected readonly CUser User;

		protected CBaseSmartArrow(
			ITutorialCommentator tutorialCommentator, 
			ISmartArrowLocker smartArrowLocker, 
			IDialogueHandler dialogueHandler, 
			IEventBus eventBus, 
			CUser user
		)
		{
			_tutorialCommentator = tutorialCommentator;
			_smartArrowLocker = smartArrowLocker;
			_dialogueHandler = dialogueHandler;
			EventBus = eventBus;
			User = user;
		}

		protected void ShowAt(Vector3 position, Vector2 offset)
		{
			CGetSmartArrowResponse response = EventBus.ProcessTask<CGetSmartArrowRequest, CGetSmartArrowResponse>();
			_smartArrow = response.SmartArrow;
			_smartArrow.ShowAt(position, offset);
		}

		protected void SetVisible(bool state, float blendDuration)
		{
			_smartArrow.SetVisible(state, blendDuration);
		}
		
		protected bool ShouldBeVisible()
		{
			CIsAnyScreenActiveResponse isAnyScreenActive = EventBus.ProcessTask<CIsAnyScreenActiveRequest, CIsAnyScreenActiveResponse>(new CIsAnyScreenActiveRequest());
			if (isAnyScreenActive.IsActive)
				return false;

			if (_dialogueHandler.IsRunning || _tutorialCommentator.IsRunning)
				return false;
			
			CIsTutorialArrowActiveResponse isTutorialArrowActive = EventBus.ProcessTask<CIsTutorialArrowActiveRequest, CIsTutorialArrowActiveResponse>(new CIsTutorialArrowActiveRequest());
			if (isTutorialArrowActive.IsActive)
				return false;

			if (_smartArrowLocker.IsLocked)
				return false;

			return true;
		}
		
		public virtual void Destroy()
		{
			_smartArrow.DestroySelf();
		}
	}
}