// =========================================
// NAME: Marek Karaba
// DATE: 02.10.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public class CRunVisualLossTask
	{
		public readonly RectTransform Source;
		public readonly Sprite Icon;
		public readonly int Amount;

		public CRunVisualLossTask(RectTransform source, Sprite icon, int amount)
		{
			Source = source;
			Icon = icon;
			Amount = amount;
		}
	}
}