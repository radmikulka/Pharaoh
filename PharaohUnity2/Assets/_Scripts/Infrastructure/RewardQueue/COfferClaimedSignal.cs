// =========================================
// AUTHOR: Juraj Joscak
// DATE:   23.07.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder.Infrastructure
{
	public class COfferClaimedSignal : IEventBusSignal
	{
		public string OfferId { get; }
		
		public COfferClaimedSignal(string offerId)
		{
			OfferId = offerId;
		}
	}
}