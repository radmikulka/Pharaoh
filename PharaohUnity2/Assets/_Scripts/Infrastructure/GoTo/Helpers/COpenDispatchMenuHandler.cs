// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;
using TycoonBuilder;
using UnityEngine;

namespace TycoonBuilder
{
	public class COpenDispatchMenuHandler
	{
		private readonly CDesignStoryContractConfigs _contractConfigs;
		private readonly IDialogueHandler _dialogueHandler;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;
		
		private readonly CLockObject _dialogueLock =new ("DispatchMenuDialogueLock");

		public COpenDispatchMenuHandler(
			CDesignStoryContractConfigs contractConfigs, 
			IDialogueHandler dialogueHandler, 
			IEventBus eventBus, 
			CUser user
			)
		{
			_dialogueHandler = dialogueHandler;
			_contractConfigs = contractConfigs;
			_eventBus = eventBus;
			_user = user;
		}

		public void Execute(ECity city)
		{
			_eventBus.Send(new COpenDispatchMenuTriggeredSignal());
			_eventBus.ProcessTask(new COpenCityDispatchMenuTask(city));
		}
		
		public void Execute(EIndustry industry)
		{
			_eventBus.Send(new COpenDispatchMenuTriggeredSignal());
			_eventBus.ProcessTask(new COpenResourceDispatchMenuTask(industry));
		}
		
		public async UniTask Execute(EStaticContractId contractId, bool claimContract, CancellationToken ct)
		{
			try
			{
				await DoExecute(contractId, claimContract, ct);
			}
			catch (OperationCanceledException)
			{
				// Ignore cancellation
			}
			catch (Exception e)
			{
				Debug.LogError(new CUnityReadableException(e));
				throw;
			}
		}

		private async UniTask DoExecute(EStaticContractId contractId, bool claimContract, CancellationToken ct)
		{
			bool claimHandled = TryHandleClaimContract(contractId, claimContract);
			if (claimHandled)
				return;
			
			CContract contract = _user.Contracts.GetStaticContractOrDefault(contractId);
			SStaticContractPointer contractPointer = new(contractId, contract?.StaticData.Task ?? 1);
			bool hasDialogue = contract != null && contract.StaticData.StoryDialogue != EDialogueId.None;
			bool isActivated = contract is { StaticData: { IsActivated: true } };
			_user.Contracts.TryActivateContract(contractId);
			
			_eventBus.Send(new COpenDispatchMenuTriggeredSignal());
			_eventBus.Send(new COpenDispatchMenuForContractTriggeredSignal(contractPointer));

			if (!isActivated && hasDialogue)
			{
				await ShowDialogue(contractPointer, ct);
				return;
			}
			
			OpenDispatchMenu(contractPointer);
		}

		private bool TryHandleClaimContract(EStaticContractId contractId, bool claimContract)
		{
			if (!claimContract)
				return false;
			
			bool isCompleted = _user.Contracts.IsCurrentTaskCompleted(contractId);
			if (!isCompleted)
				throw new Exception($"Contract {contractId} is not completed and cannot be claimed.");
			
			_user.Contracts.CompleteContractTier(contractId);
			return true;
		}

		private async UniTask ShowDialogue(SStaticContractPointer contractId, CancellationToken ct)
		{
			CContract contract = _user.Contracts.GetStaticContract(contractId.Id);
			EDialogueId dialogueId = contract.StaticData.StoryDialogue;
			
			SetActiveNameTags(false);
			SetActiveHud(false);

			try
			{
				await _dialogueHandler.AddDialogue(dialogueId, ct);
			}
			finally
			{
				OpenDispatchMenu(contractId);
			
				SetActiveNameTags(true);
				SetActiveHud(true);
			}
		}

		private void OpenDispatchMenu(SStaticContractPointer contractId)
		{
			_eventBus.ProcessTask(new COpenContractDispatchMenuTask(contractId));
		}
		
		private void SetActiveNameTags(bool state)
		{
			if (state)
			{
				_eventBus.ProcessTask(new CRemoveFloatingWindowsBlockerTask(_dialogueLock));
				return;
			}
			_eventBus.ProcessTask(new CAddFloatingWindowsBlockerTask(_dialogueLock));
		}

		private void SetActiveHud(bool state)
		{
			if (state)
			{
				_eventBus.ProcessTask(new CHudShowTask(_dialogueLock, false, true));
				return;
			}
			_eventBus.ProcessTask(new CHudHideTask(_dialogueLock, false, true));
		}
	}
}