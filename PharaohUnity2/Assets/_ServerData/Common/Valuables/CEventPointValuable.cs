// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.12.2025
// =========================================

namespace ServerData
{
	public class CEventPointValuable : ICountableValuable
	{
		public EValuable Id => EValuable.EventPoint;
		public ELiveEvent LiveEvent { get; private set; }
		public int Amount { get; private set; }

		public int Value => Amount;

		public CEventPointValuable(ELiveEvent liveEvent, int amount)
		{
			LiveEvent = liveEvent;
			Amount = amount;
		}

		public string GetAnalyticsValue()
		{
			return Amount.ToString();
		}

		public string GetOfferRewardAnalyticsValue()
		{
			return $"va{(int)Id}:{Value}";
		}

		public ICountableValuable Multiply(int multiplier)
		{
			return new CEventPointValuable(LiveEvent, Amount * multiplier);
		}
	}
}