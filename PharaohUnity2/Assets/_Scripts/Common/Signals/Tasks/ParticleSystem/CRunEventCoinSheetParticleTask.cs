// =========================================
// AUTHOR: Juraj Joscak
// DATE:   06.01.2026
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CRunEventCoinSheetParticleTask
	{
		public readonly RectTransform Start;
		public readonly CEventCoinValuable Currency;

		public CRunEventCoinSheetParticleTask(RectTransform start, CEventCoinValuable currency)
		{
			Currency = currency;
			Start = start;
		}
	}
}