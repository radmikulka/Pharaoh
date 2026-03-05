// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.10.2023
// =========================================

using System.Collections.Generic;
using System.Reflection;
using ServerData;
using ServerData.Design;
using ServiceEngine.Purchasing;
using UnityEngine.Purchasing;

namespace TycoonBuilder
{
	public class CPurchasingProductsProvider : IPurchasingProductsProvider
	{
		public ProductDefinition[] Products { get; }

		public CPurchasingProductsProvider(CInAppPrices inAppPrices)
		{
			Products = new ProductDefinition[inAppPrices.AllPrices.Length];
			for (int i = 0; i < Products.Length; i++)
			{
				Products[i] = new ProductDefinition(inAppPrices.AllPrices[i], ProductType.Consumable);
			}
		}
	}
}