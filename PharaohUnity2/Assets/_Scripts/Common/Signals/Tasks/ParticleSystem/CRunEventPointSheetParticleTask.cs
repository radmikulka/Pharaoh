// =========================================
// AUTHOR: Marek Karaba
// DATE:   19.02.2026
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CRunEventPointSheetParticleTask
	{
		public readonly RectTransform Start;
		public readonly CEventPointValuable Currency;

		public CRunEventPointSheetParticleTask(RectTransform start, CEventPointValuable currency)
		{
			Currency = currency;
			Start = start;
		}
	}
}