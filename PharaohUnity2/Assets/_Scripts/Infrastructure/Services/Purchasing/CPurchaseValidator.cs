// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.09.2025
// =========================================

using System;
using System.Linq;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Hits;
using ServiceEngine.Purchasing;
using Pharaoh;
using UnityEngine;
using UnityEngine.Purchasing;

namespace ServiceEngine.Purchasing
{
	public class CPurchaseValidator : IPurchaseValidator
	{
		private readonly CPurchaseReceiptUnpacker _receiptUnpacker;
		private readonly ICrashlytics _crashlytics;
		private readonly CHitBuilder _hitBuilder;
		private readonly IAnalytics _analytics;
		private readonly ISingular _singular;
		private readonly IEventBus _eventBus;

		public CPurchaseValidator(
			CPurchaseReceiptUnpacker receiptUnpacker, 
			ICrashlytics crashlytics, 
			CHitBuilder hitBuilder, 
			IAnalytics analytics, 
			IEventBus eventBus, 
			ISingular singular
			)
		{
			_receiptUnpacker = receiptUnpacker;
			_crashlytics = crashlytics;
			_hitBuilder = hitBuilder;
			_analytics = analytics;
			_eventBus = eventBus;
			_singular = singular;
		}

		public void Validate(
			PendingOrder order, 
			IPurchaseMetaData metaData, 
			bool isPurchaseInitiatedByUser,
			Action onFailure,
			Action onSuccess
			)
		{
			if (metaData is not CPurchaseMetadata purchaseMetaData)
			{
				if (!isPurchaseInitiatedByUser)
				{
					onFailure();
					Debug.LogError("PurchaseMetaData must be provided for non user initiated purchases");
					return;
				}
				purchaseMetaData = new CPurchaseMetadata(
					string.Empty, 
					order.GetFirstProductId(), 
					string.Empty,
					new CPurchasePayloads()
					);
			}
			
			TryValidateByServerAsync(order, purchaseMetaData, isPurchaseInitiatedByUser, onFailure, onSuccess).Forget();
		}

		private async UniTask TryValidateByServerAsync(
			PendingOrder order, 
			CPurchaseMetadata metaData, 
			bool isPurchaseInitiatedByUser,
			Action onFailure,
			Action onSuccess
			)
		{
			try
			{
				await ValidateByServerAsync(order, metaData, isPurchaseInitiatedByUser, onFailure, onSuccess);
			}
			catch (Exception e)
			{
				onFailure();
				_crashlytics.LogException(e);
			}
		}
		
		private async UniTask ValidateByServerAsync(
			PendingOrder order, 
			CPurchaseMetadata metaData, 
			bool isPurchaseInitiatedByUser,
			Action onFailure,
			Action onSuccess
			)
		{
			EStoreId store = GetStoreId();
			string productId = order.GetFirstProductId();
			CPurchaseReceiptUnpacker.SPurchaseInfo purchaseData = _receiptUnpacker.GetPurchaseData(order);
			CRealMoneyPurchaseDataDto purchaseDataDto = new(
				purchaseData.Token, productId, store, purchaseData.PurchaseTimestampMs, isPurchaseInitiatedByUser
				);
			CHitRecordBuilder hitBuilder = _hitBuilder.GetBuilder(new CValidatePurchaseRequest(
				purchaseDataDto, 
				metaData.OfferId
			));

			hitBuilder.SetExecuteImmediately();

			var response = await hitBuilder.BuildAndSendAsync<CResponseHit>(CancellationToken.None);
			
			// handled on upper application layer
			if(response == null)
				return;

			switch (response.Response)
			{
				case CValidatePurchaseResponse purchaseResponse:
					if (purchaseResponse.IsTestUser)
					{
						_analytics.SetActiveAnalytics(false);
						_singular.MarkAsTester();
					}
					onSuccess();
					break;
				default:
					onFailure();
					break;
			}

			if (!isPurchaseInitiatedByUser)
			{
				_eventBus.Send(new CFailedPurchaseResolvedSignal());
			}
		}
		
		private EStoreId GetStoreId()
		{
			if (CPlatform.IsEditor)
				return EStoreId.Editor;
			return CPlatform.IsAndroidDevice ? EStoreId.Android : EStoreId.Apple;
		}
	}
}