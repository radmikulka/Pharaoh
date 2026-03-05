// =========================================
// AUTHOR:
// DATE:   30.09.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class CClaimContractState : CAwaitableState
	{
		private readonly COpenDispatchMenuHandler _handler;
		
		public CClaimContractState(IEventBus eventBus, COpenDispatchMenuHandler handler) : base(eventBus)
		{
			_handler = handler;
		}

		protected override async UniTask Run(CancellationToken ct)
		{
			EStaticContractId contractId = Context.GetEntry<EStaticContractId>(EGoToContextKey.ContractId);
			await _handler.Execute(contractId, true, CancellationToken);
		}
	}
}