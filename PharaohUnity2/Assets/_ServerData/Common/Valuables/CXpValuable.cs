// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.08.2025
// =========================================

namespace ServerData
{
	public class CXpValuable : ICountableValuable
	{
		public EValuable Id => EValuable.Xp;
		public int Amount { get; private set; }

		public int Value => Amount;

		public CXpValuable(int amount)
		{
			Amount = amount;
		}

		public ICountableValuable Multiply(int multiplier)
		{
			return new CXpValuable(Amount * multiplier);	
		}

		public string GetAnalyticsValue()
		{
			return Amount.ToString();
		}

		public string GetOfferRewardAnalyticsValue()
		{
			return $"{(int)Id}:{Value}";
		}
	}
}