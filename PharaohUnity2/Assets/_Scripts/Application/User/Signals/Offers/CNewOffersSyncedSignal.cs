// =========================================
// AUTHOR: Juraj Joscak
// DATE:   14.11.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder.Offers
{
	public class CNewOffersSyncedSignal : IEventBusSignal
	{
		public readonly COffer[] NewOffers;

		public CNewOffersSyncedSignal(COffer[] newOffers)
		{
			NewOffers = newOffers;
		}
	}
}