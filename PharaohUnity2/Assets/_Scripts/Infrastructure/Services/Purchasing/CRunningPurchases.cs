// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.09.2025
// =========================================

using System.Collections.Generic;

namespace ServiceEngine.Purchasing
{
	public class CRunningPurchases
	{
		private readonly List<CRunningPurchase> _runningPurchases = new();

		public CRunningPurchase GetPurchaseOrDefault(string productId)
		{
			for (int i = 0; i < _runningPurchases.Count; i++)
			{
				CRunningPurchase purchase = _runningPurchases[i];
				bool isProductIdMatch = purchase.ProductId == productId;
				if (!isProductIdMatch)
					continue;

				_runningPurchases.RemoveAt(i);
				return purchase;
			}

			return null;
		}
	}
}