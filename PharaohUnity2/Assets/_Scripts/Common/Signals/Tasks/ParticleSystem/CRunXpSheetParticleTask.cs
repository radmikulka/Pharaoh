// =========================================
// AUTHOR: Juraj Joscak
// DATE:   07.10.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CRunXpSheetParticleTask
	{
		public readonly RectTransform Start;
		public readonly int Amount;

		public CRunXpSheetParticleTask(RectTransform start, int amount)
		{
			Start = start;
			Amount = amount;
		}
	}
}