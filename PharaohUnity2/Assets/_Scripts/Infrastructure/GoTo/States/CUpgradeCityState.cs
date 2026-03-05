// =========================================
// AUTHOR: Marek Karaba
// DATE:   09.10.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder.GoToStates
{
	public class CUpgradeCityState : CAwaitableState
	{
		
		public CUpgradeCityState(IEventBus eventBus) : base(eventBus)
		{
			
		}

		protected override async UniTask Run(CancellationToken ct)
		{	
			await EventBus.ProcessTaskAsync(new CAnimateCityTask(), ct);
		}
	}
}