// =========================================
// NAME: Marek Karaba
// DATE: 02.10.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CRunVisualLossCurrencyTask
	{
		public readonly RectTransform Source;
		public readonly EValuable ValuableId;
		public readonly int Amount;

		public CRunVisualLossCurrencyTask(RectTransform source, EValuable valuableId, int amount)
		{
			Source = source;
			ValuableId = valuableId;
			Amount = amount;
		}
	}
}