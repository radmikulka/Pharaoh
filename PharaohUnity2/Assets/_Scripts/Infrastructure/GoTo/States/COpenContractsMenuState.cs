// =========================================
// AUTHOR: Juraj Joscak
// DATE:   16.02.2026
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.GoToStates
{
	public class COpenContractsMenuState : CAwaitableState
	{
		public COpenContractsMenuState(IEventBus eventBus) : base(eventBus)
		{
			
		}

		protected override async UniTask Run(CancellationToken ct)
		{
			//EShopTab tab = Context.GetEntry<EShopTab>(EGoToContextKey.ShopTab);
			await EventBus.ProcessTaskAsync(new CShowScreenTask(EScreenId.Contracts), CancellationToken);
			IsCompleted = true;
		}
	}
}