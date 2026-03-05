// =========================================
// AUTHOR: Marek Karaba
// DATE:   21.07.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CRunCurrencyPopupNumberTask
	{
		public readonly RectTransform Source;
		public readonly int Number;

		public CRunCurrencyPopupNumberTask(RectTransform source, int number)
		{
			Number = number;
			Source = source;
		}
	}
}