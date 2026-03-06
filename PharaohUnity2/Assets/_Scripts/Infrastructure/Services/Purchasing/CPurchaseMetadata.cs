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
		public CPurchasePayloads Payloads { get; private set; }
		public string OfferId;

		public CPurchaseMetadata(
			string offerId, 
			string productId, 
			string analyticsId,
			CPurchasePayloads payloads
			)
		{
			AnalyticsId = analyticsId;
			ProductId = productId;
			Payloads = payloads;
			OfferId = offerId;
		}

		public override string ToString()
		{
			return $"{nameof(OfferId)}: {OfferId}";
		}
	}
}