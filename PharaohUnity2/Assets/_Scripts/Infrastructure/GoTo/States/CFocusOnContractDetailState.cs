// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using AldaEngine;
using ServerData;
using TycoonBuilder;

namespace TycoonBuilder.GoToStates
{
	public class CFocusOnContractDetailState : CGoToFsmState
	{
		public CFocusOnContractDetailState(IEventBus eventBus) : base(eventBus)
		{
		}

		public override void Start()
		{
			EStaticContractId contractId = Context.GetEntry<EStaticContractId>(EGoToContextKey.ContractId);
			EventBus.Send(new CSetActiveContractDetailCameraSignal(contractId, true));
			IsCompleted = true;
		}
	}
}