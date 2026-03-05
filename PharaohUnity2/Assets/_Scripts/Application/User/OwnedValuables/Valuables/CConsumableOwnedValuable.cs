// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.12.2024
// =========================================

using System;
using System.Numerics;
using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	[ValidatableData]
	public class CConsumableOwnedValuable : COwnedValuable
	{
		[ValidatableData] public CUserDataInt Amount { get; private set; }

		public event Action<SValueChangeArgs> ValueChanged;

		public CConsumableOwnedValuable(EValuable id, int amount) : base(id)
		{
			Amount = new CUserDataInt(amount);
		}

		public override void InitialSync(COwnedValuableData data)
		{
			base.InitialSync(data);
			CConsumableOwnedValuableData consumable = (CConsumableOwnedValuableData)data;
			Amount = new CUserDataInt(consumable.Amount);
			ValueChanged?.Invoke(new SValueChangeArgs(null, 0, Amount));
		}

		public override void Sync(IOwnedValuableData data)
		{
			CConsumableOwnedValuableData consumable = (CConsumableOwnedValuableData)data;
			Amount.ServerValue = consumable.Amount;
		}

		internal override void Modify(IValuable valuable, CValueModifyParams modifyParams)
		{
			if(valuable is not CConsumableValuable consumableValuable)
				throw new Exception($"Valuable is not {nameof(CConsumableValuable)}");
			Modify(consumableValuable, modifyParams);
		}

		public override bool HaveValuable(IValuable valuable)
		{
			if(valuable is not CConsumableValuable consumableValuable)
				throw new Exception($"Valuable is not {nameof(CConsumableValuable)}");
			return Amount >= consumableValuable.Value;
		}

		private void Modify(CConsumableValuable valuable, CValueModifyParams modifyParams)
		{
			int previousValue = Amount;
			
			Amount.LocalValue += valuable.Value;
			if (Amount < 0)
			{
				Amount.LocalValue = 0;
			}
			
			ValueChanged?.Invoke(new SValueChangeArgs(modifyParams, previousValue, Amount));
		}

		public override void Dispose()
		{
			base.Dispose();
			ValueChanged = null;
		}

		public override string ToString()
		{
			return $"{Id} - {nameof(Amount)}: {Amount}";
		}
	}
}