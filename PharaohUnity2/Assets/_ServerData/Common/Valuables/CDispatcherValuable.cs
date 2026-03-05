// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CDispatcherValuable : IValuable, IOfferAnalyticsValueProvider
	{
		public EValuable Id => EValuable.Dispatcher;
		[JsonProperty] public EDispatcher Dispatcher { get; private set; }
		[JsonProperty] public long? ExpirationDurationIsSecs { get; private set; }

		public CDispatcherValuable()
		{
		}

		public CDispatcherValuable(EDispatcher dispatcher, long? expirationDurationIsSecs)
		{
			ExpirationDurationIsSecs = expirationDurationIsSecs;
			Dispatcher = dispatcher;
		}
	
		public string GetAnalyticsValue()
		{
			return Dispatcher.ToString();
		}

		public string GetOfferRewardAnalyticsValue()
		{
			return $"di{Dispatcher.ToString()}";
		}

		public override string ToString()
		{
			return $"{nameof(Id)}: {Id}, {nameof(Dispatcher)}: {Dispatcher}";
		}
	}
}