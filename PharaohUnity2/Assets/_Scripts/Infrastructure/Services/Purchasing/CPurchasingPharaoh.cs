// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.09.2025
// =========================================

using System;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData.Hits;
using ServiceEngine.Purchasing;
using Pharaoh;
using Zenject;

namespace ServiceEngine.Purchasing
{
	public class CPurchasingPharaoh : CPurchasing
	{
		private CRequestSender _hitBuilder;

		[Inject]
		private void Inject(CRequestSender hitBuilder)
		{
			_hitBuilder = hitBuilder;
		}

		public void Initialize()
		{
			EventBus.Subscribe<CPurchaseFailedSignal>(OnPurchaseFailedSignalled);
		}

		private void OnPurchaseFailedSignalled(CPurchaseFailedSignal signal)
		{
			TryShowFailedTooltip(signal.FailureReason);
		}

		private void TryShowFailedTooltip(EPurchaseFailureReason reason)
		{
			switch (reason)
			{
				case EPurchaseFailureReason.Cancelled:
					EventBus.ProcessTask(new CShowTooltipTask("Purchasing.Failed", true));
					break;
				case EPurchaseFailureReason.Error:
					EventBus.ProcessTask(new CShowTooltipTask("Purchasing.Cancelled", true));
					break;
			}
		}

		public override async UniTask<EPurchaseState> PurchaseProduct(IPurchaseMetaData metaData)
		{
			EPurchaseState currentState = ValidatePurchasingState();
			if (currentState != EPurchaseState.Ok)
				return currentState;
			
			EventBus.Send(new CPurchaseInitiatedSignal(metaData));
			return await base.PurchaseProduct(metaData);
		}
	}
}