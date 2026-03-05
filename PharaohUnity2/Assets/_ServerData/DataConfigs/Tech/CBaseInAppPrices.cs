// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.10.2023
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using ServerData;

namespace ServerData
{
	public class CBaseInAppPrices
	{
		private readonly Dictionary<EInAppPrice, string> _pricesDb = new();

		public string[] AllPrices => _pricesDb.Values.ToArray();

		public string GetStoreId(EInAppPrice priceId)
		{
			if (_pricesDb.TryGetValue(priceId, out string storeId))
				return storeId;
			throw new Exception($"Price not found for {priceId}");
		}
	
		protected void AddPrice(EInAppPrice priceId, string storeId)
		{
			_pricesDb.Add(priceId, storeId);
		}
	}
}

