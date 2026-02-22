// =========================================
// DATE:   22.02.2026
// =========================================

using System;
using ServerData;

namespace Pharaoh
{
	public class COwnedResource
	{
		public EResource Id { get; }
		public int Amount { get; private set; }

		public event Action<SValueChangeArgs> ValueChanged;

		public COwnedResource(EResource id, int amount = 0)
		{
			Id = id;
			Amount = amount;
		}

		public SValueChangeArgs Modify(int delta, CValueModifyParams modifyParams = null)
		{
			int previousValue = Amount;

			Amount += delta;
			if (Amount < 0)
				Amount = 0;

			SValueChangeArgs args = new(modifyParams, previousValue, Amount);
			ValueChanged?.Invoke(args);
			return args;
		}

		public bool HasEnough(int amount)
		{
			return Amount >= amount;
		}

		public void Dispose()
		{
			ValueChanged = null;
		}

		public override string ToString()
		{
			return $"{Id} - {nameof(Amount)}: {Amount}";
		}
	}
}
