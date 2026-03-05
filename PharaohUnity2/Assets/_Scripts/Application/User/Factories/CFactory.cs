// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using System;
using System.Collections.Generic;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CFactory
	{
		private readonly Dictionary<int, CFactorySlot> _slots = new();
	
		public readonly CRecharger Durability;
		public readonly EFactory Id;
		public CLevelData LevelData { get; private set; }
		public bool IsSeen { get; private set; }

		public CFactory(EFactory id, CRecharger durability, CLevelData levelData, CFactorySlot[] slots, bool isSeen)
		{
			Durability = durability;
			Id = id;
			LevelData = levelData;
			IsSeen = isSeen;

			foreach (CFactorySlot slot in slots)
			{
				_slots.Add(slot.Index, slot);
			}
		}

		public void Repair(long time)
		{
			int repairAmount = Durability.ProductionPerTick;
			Durability.Add(repairAmount, time);
		}
	
		public CFactorySlot GetOrCreateSlot(int slotIndex)
		{
			if(slotIndex > CDesignFactoryConfigs.MaxSlotsCount)
				throw new Exception($"Factory {Id} slot {slotIndex} is too large.");
		
			if (_slots.TryGetValue(slotIndex, out CFactorySlot slot)) 
				return slot;
		
			slot = CFactorySlot.NewSlot(slotIndex);
			_slots.Add(slotIndex, slot);

			return slot;
		}

		public void StartProduction(int slotIndex, SResource resource, long productionTimeInMs, long clientTime)
		{
			Durability.Remove(1, clientTime);

			CFactorySlot slot = GetOrCreateSlot(slotIndex);
			long completionTime = clientTime + productionTimeInMs;
			slot.SetProduct(resource, completionTime);
		}

		public int GetUnlockedSlotsCount()
		{
			int count = 0;
			foreach (CFactorySlot slot in _slots.Values)
			{
				if (slot.IsUnlocked)
				{
					++count;
				}
			}

			return count;
		}
		
		public int GetFinishedProductsCount(EResource product, long timestamp)
		{
			int count = 0;
			for (int i = 0; i < GetUnlockedSlotsCount(); i++)
			{
				CFactorySlot slot = GetOrCreateSlot(i);
				if(!slot.IsCrafting)
					continue;
				
				if (!slot.IsCraftingCompleted(timestamp))
					continue;
				
				if(product != EResource.None && slot.CraftingProduct.Id != product)
					continue;

				count++;
			}
			return count;
		}

		public int GetMissingDurability(long time)
		{
			return Durability.MaxCapacity(time) - Durability.CurrentAmount;
		}

		public void MarkAsSeen()
		{
			IsSeen = true;
		}
	}
}