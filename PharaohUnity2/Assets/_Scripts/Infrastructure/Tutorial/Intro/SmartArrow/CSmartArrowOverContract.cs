// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.10.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CSmartArrowOverContract : CBaseSmartArrow
	{
		public CSmartArrowOverContract(
			ITutorialCommentator tutorialCommentator, 
			ISmartArrowLocker smartArrowLocker, 
			IDialogueHandler dialogueHandler, 
			IEventBus eventBus, 
			CUser user) 
			: base(tutorialCommentator, smartArrowLocker, dialogueHandler, eventBus, user)
		{
		}

		public async UniTask Show(SStaticContractPointer pointer, CancellationToken ct)
		{
			CGetStoryContractNameTagPointResponse contractPosition = EventBus.ProcessTask<CGetStoryContractNameTagPointRequest, CGetStoryContractNameTagPointResponse>(
				new CGetStoryContractNameTagPointRequest(pointer.Id));
			
			ShowAt(contractPosition.Point, new Vector2(0f, 140f));
			
			while (true)
			{
				bool isCompleted = IsCompleted(pointer);
				if (isCompleted)
				{
					Destroy();
					return;
				}

				bool shouldBeVisible = ShouldBeVisible(pointer.Id);
				SetVisible(shouldBeVisible, 0.2f);

				await UniTask.Yield(ct);
			}
		}
		
		private bool ShouldBeVisible(EStaticContractId contractId)
		{
			bool shouldBeVisible = ShouldBeVisible();
			if (!shouldBeVisible)
				return false;
			
			CContract contract = User.Contracts.GetStaticContractOrDefault(contractId);
			shouldBeVisible = contract?.RemainingAmount > 0;
			if (shouldBeVisible)
				return true; 
			
			bool dispatchExists = User.Dispatches.AnyDispatchGoingToContract(contractId);
			return !dispatchExists;
		}
		
		private bool IsCompleted(SStaticContractPointer pointer)
		{
			bool isCompleted = User.Contracts.IsContractCompleted(pointer.Id);
			if (isCompleted)
				return true;
			CContract contract = User.Contracts.GetStaticContractOrDefault(pointer.Id);
			if (contract == null)
				return false;
			
			return contract.StaticData.Task > pointer.Task;
		}
	}
}