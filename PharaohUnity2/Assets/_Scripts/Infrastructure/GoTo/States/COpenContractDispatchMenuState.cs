// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using TycoonBuilder;

namespace TycoonBuilder.GoToStates
{
	public class COpenContractDispatchMenuState : CAwaitableState
	{
		private readonly COpenDispatchMenuHandler _handler;
		
		public COpenContractDispatchMenuState(IEventBus eventBus, COpenDispatchMenuHandler handler) : base(eventBus)
		{
			_handler = handler;
		}

		protected override async UniTask Run(CancellationToken ct)
		{	
			EStaticContractId contractId = Context.GetEntry<EStaticContractId>(EGoToContextKey.ContractId);
			await _handler.Execute(contractId, false, CancellationToken);
		}
	}
}