// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using System;
using ServerData;

namespace TycoonBuilder
{
	public class CFactorySlot
	{
		public readonly int Index;
		public SResource CraftingProduct { get; private set; }
		public long? CompletionTime { get; private set; }
		public bool IsUnlocked { get; private set; }
	
		public bool IsCrafting => CompletionTime.HasValue;

		public static CFactorySlot NewSlot(int index)
		{
			return new CFactorySlot(index, default, null, false);
		}

		public CFactorySlot(int index, SResource craftingProduct, long? completionTime, bool isUnlocked)
		{
			Index = index;
			CraftingProduct = craftingProduct;
			CompletionTime = completionTime;
			IsUnlocked = isUnlocked;
		}

		public void SetProduct(SResource resource, long completionTime)
		{
			if (!IsUnlocked)
			{
				throw new Exception($"Cannot set product on locked factory slot {Index}");
			}
		
			CraftingProduct = resource;
			CompletionTime = completionTime;
		}

		public void Unlock()
		{
			IsUnlocked = true;
		}

		public bool IsCraftingCompleted(long time)
		{
			return time >= CompletionTime;
		}

		public void Claim()
		{
			CompletionTime = null;
			CraftingProduct = default;
		}
	}
}