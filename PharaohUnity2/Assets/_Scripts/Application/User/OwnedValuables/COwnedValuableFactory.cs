// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.12.2024
// =========================================

using System;
using ServerData;

namespace TycoonBuilder
{
	public class COwnedValuableFactory
	{
		public COwnedValuable Create(EValuable id)
		{
			switch (id)
			{
				case EValuable.HardCurrency:
					return new CConsumableOwnedValuable(id, 0);
				default:
					throw new NotImplementedException(id.ToString());
			}
		}
	}
}