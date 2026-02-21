// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.10.2023
// =========================================

using System;
using AldaEngine;
using ServerData;

// ReSharper disable InconsistentNaming

namespace Pharaoh
{
	[Serializable]
	public class CPurchaseMetadata : IPurchaseMetaData
	{
		public string ProductId { get; }
		public string AnalyticsId { get; private set; }
		public EModificationSource OverrideSource;
		public long PurchaseTimeInMs;
		public string OfferId;

		public CPurchaseMetadata(
			string offerId, 
			string productId, 
			string analyticsId,
			long purchaseTimeInMs, 
			EModificationSource overrideSource
			)
		{
			PurchaseTimeInMs = purchaseTimeInMs;
			OverrideSource = overrideSource;
			AnalyticsId = analyticsId;
			ProductId = productId;
			OfferId = offerId;
		}

		public override string ToString()
		{
			return $"{nameof(OfferId)}: {OfferId}";
		}
	}
}