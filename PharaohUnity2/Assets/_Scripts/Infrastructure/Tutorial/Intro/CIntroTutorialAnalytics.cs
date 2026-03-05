// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;

namespace TycoonBuilder
{
	public class CIntroTutorialAnalytics : IInitializable
	{
		private readonly ICFtueFunnel _ftueFunnel;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;

		private readonly SStaticContractPointer _brickworks1 = new(EStaticContractId._1930_Brickworks, 1);
		private readonly SStaticContractPointer _brickworks2 = new(EStaticContractId._1930_Brickworks, 2);
		private readonly SStaticContractPointer _coalMine = new(EStaticContractId._1930_CoalMine, 1);

		public CIntroTutorialAnalytics(ICFtueFunnel ftueFunnel, IEventBus eventBus, CUser user)
		{
			_ftueFunnel = ftueFunnel;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			bool isCompleted = IsCompleted();
			if (isCompleted)
				return;

			_eventBus.Subscribe<COpenDispatchMenuForContractTriggeredSignal>(OnOpenDispatchMenuForContractTriggered);
			_eventBus.Subscribe<CDispatchForContractOpenedSignal>(OnDispatchForContractOpened);
			_eventBus.Subscribe<CStoryContractRewardsClaimedSignal>(OnStoryContractClaimed);
			_eventBus.Subscribe<CVehicleDispatchedSignal>(OnVehicleDispatched);
		}

		private bool IsCompleted()
		{
			return _user.Tutorials.IsTutorialCompleted(EIntroTutorialStep.Completed);
		}

		private void OnOpenDispatchMenuForContractTriggered(COpenDispatchMenuForContractTriggeredSignal signal)
		{
			if (signal.Contract == _brickworks1)
			{
				Send(EFtueFunnelStep.Brickworks1Clicked);
			}
			
			if (signal.Contract == _brickworks2)
			{
				Send(EFtueFunnelStep.Brickworks2Clicked);
			}
			
			if (signal.Contract == _coalMine)
			{
				Send(EFtueFunnelStep.CoalMineClicked);
			}
		}

		private void OnDispatchForContractOpened(CDispatchForContractOpenedSignal signal)
		{
			SStaticContractPointer contractPointer = ((CContract)signal.Contract).GetPointer();
			if (contractPointer == _brickworks1)
			{
				Send(EFtueFunnelStep.Brickworks1Opened);
			}
			
			if (contractPointer == _brickworks2)
			{
				Send(EFtueFunnelStep.Brickworks2Opened);
			}
			
			if (contractPointer == _coalMine)
			{
				Send(EFtueFunnelStep.CoalMineOpened);
			}
		}

		private void OnVehicleDispatched(CVehicleDispatchedSignal signal)
		{
			if (signal.Dispatch.Type != EDispatchType.Contract)
				return;

			CContract contract = _user.Contracts.GetStaticContract(signal.Dispatch.ContractData.Contract);
			SStaticContractPointer contractPointer = contract.GetPointer();
				
			if (contractPointer == _brickworks1)
			{
				Send(EFtueFunnelStep.Brickworks1Start);
			}
			
			if (contractPointer == _brickworks2)
			{
				Send(EFtueFunnelStep.Brickworks2Start);
			}
			
			if (contractPointer == _coalMine)
			{
				Send(EFtueFunnelStep.CoalMineStart);
			}
		}

		private void OnStoryContractClaimed(CStoryContractRewardsClaimedSignal signal)
		{
			SStaticContractPointer contractPointer = ((CContract)signal.Contract).GetPointer();
			if (contractPointer == _brickworks1)
			{
				Send(EFtueFunnelStep.Brickworks1Claimed);
			}
			
			if (contractPointer == _brickworks2)
			{
				Send(EFtueFunnelStep.Brickworks2Claimed);
			}
			
			if (contractPointer == _coalMine)
			{
				Send(EFtueFunnelStep.CoalMineClaimed);
			}
		}

		public void Send(EFtueFunnelStep step)
		{
			_ftueFunnel.Send(step);
		}
	}
}