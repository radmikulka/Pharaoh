// =========================================
// AUTHOR: Marek Karaba
// DATE:   06.11.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CRateUsCustomRatingSignal : IEventBusSignal
	{
		public int StarsAmount { get; }

		public CRateUsCustomRatingSignal(int starsAmount)
		{
			StarsAmount = starsAmount;
		}
	}
}