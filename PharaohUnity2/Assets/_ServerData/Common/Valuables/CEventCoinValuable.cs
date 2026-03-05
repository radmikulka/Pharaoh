// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.12.2025
// =========================================

namespace ServerData
{
	public class CEventCoinValuable : ICountableValuable
	{
		public EValuable Id => EValuable.EventCoin;
		public ELiveEvent LiveEvent { get; private set; }
		public int Amount { get; private set; }

		public int Value => Amount;

		public CEventCoinValuable(ELiveEvent liveEvent, int amount)
		{
			LiveEvent = liveEvent;
			Amount = amount;
		}

		public string GetAnalyticsValue()
		{
			return Amount.ToString();
		}
		
		public CEventCoinValuable Inverse()
		{
			return new CEventCoinValuable(LiveEvent, -Amount);
		}

		public CEventCoinValuable Double()
		{
			return new CEventCoinValuable(LiveEvent, Amount * 2);
		}
		
		public ICountableValuable Multiply(int multiplier)
		{
			return new CEventCoinValuable(LiveEvent, Amount * multiplier);
		}
		
		public string GetOfferRewardAnalyticsValue()
		{
			return $"va{(int)Id}:{Value}";
		}
	}
}