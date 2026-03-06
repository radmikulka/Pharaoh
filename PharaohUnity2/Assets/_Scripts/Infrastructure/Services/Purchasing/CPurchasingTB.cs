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
	public class CPurchasingTB : CPurchasing, IInitializable
	{
		private CHitBuilder _hitBuilder;

		[Inject]
		private void Inject(CHitBuilder hitBuilder)
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
			
			if(signal.MetaData is not CPurchaseMetadata metaData)
				return;
			_hitBuilder.GetBuilder(new CCancelPurchaseRequest(metaData.OfferId, metaData.ProductId))
				.SetExecuteImmediately()
				.BuildAndSend();
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
			
			CPurchaseMetadata tbMetaData = (CPurchaseMetadata)metaData;
			var response = await _hitBuilder.GetBuilder(new CInitiatePurchaseRequest(
					tbMetaData.OfferId, 
					tbMetaData.ProductId, 
					tbMetaData.Payloads
					))
				.SetExecuteImmediately()
				.BuildAndSendAsync<CInitiatePurchaseResponse>(CancellationToken.None);

			if (response.Response == null)
				return EPurchaseState.Failed;
			
			return await base.PurchaseProduct(metaData);
		}
	}
}