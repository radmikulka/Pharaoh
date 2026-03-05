// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.12.2025
// =========================================

using ServerData;

namespace TycoonBuilder.Infrastructure
{
	public interface IDispatcherOffersProvider
	{
		bool AnyOfferAvailable();
		COffer GetOfferWithHighestPriorityOrDefault();
		CDispatcherValuable GetDispatcherValuableFromOffer(COffer offer);
	}
}