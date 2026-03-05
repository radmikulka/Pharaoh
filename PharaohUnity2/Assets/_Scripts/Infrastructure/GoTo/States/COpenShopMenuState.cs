// =========================================
// AUTHOR: Juraj Joscak
// DATE:   23.09.2025
// =========================================

using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.GoToStates
{
	public class COpenShopMenuState : CGoToFsmState
	{
		public COpenShopMenuState(IEventBus eventBus) : base(eventBus)
		{
			
		}

		public override void Start()
		{
			EShopTab tab = Context.GetEntry<EShopTab>(EGoToContextKey.ShopTab);
			EValuable valuable = Context.GetEntryOrDefault<EValuable>(EGoToContextKey.ValuableType);
			EventBus.ProcessTaskAsync(new COpenShopTask(tab, valuable), CancellationToken);
			IsCompleted = true;
		}
	}
}