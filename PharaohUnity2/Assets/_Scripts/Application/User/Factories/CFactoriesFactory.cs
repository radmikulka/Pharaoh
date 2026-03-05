// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using AldaEngine;
using ServerData;
using ServerData.Design;
using ServerData.Dto;

namespace TycoonBuilder
{
	public class CFactoriesFactory
	{
		private readonly CDesignFactoryConfigs _configs;
		private readonly IMapper _mapper;

		public CFactoriesFactory(CDesignFactoryConfigs configs, IMapper mapper)
		{
			_configs = configs;
			_mapper = mapper;
		}

		public CFactory GetNew(EFactory id)
		{
			CRecharger durability = GetNewDurability(id);
			CFactorySlot[] initialUnlockedSlots = GetInitialUnlockedSlots();
			return new CFactory(id, durability, CLevelData.New(), initialUnlockedSlots, false);
		}

		public CFactory GetExisting(CFactoryDto dto)
		{
			CRecharger durability = GetExistingDurability(dto);
			CLevelData levelData = _mapper.Map<CLevelDataDto, CLevelData>(dto.LevelData);
			CFactorySlot[] slots = _mapper.Map<CFactorySlotDto, CFactorySlot>(dto.Slots);
			return new CFactory(dto.Id, durability, levelData, slots, dto.IsSeen);
		}

		private CFactorySlot[] GetInitialUnlockedSlots()
		{
			CFactorySlot[] unlockedSlots = new CFactorySlot[CDesignFactoryConfigs.InitiallyUnlockedSlots];
			for (int i = 0; i < unlockedSlots.Length; i++)
			{
				CFactorySlot slot = CFactorySlot.NewSlot(i);
				slot.Unlock();
				unlockedSlots[i] = slot;
			}

			return unlockedSlots;
		}
		
		private CRecharger GetExistingDurability(CFactoryDto dto)
		{
			SRechargerLevelConfig durabilityConfig = _configs.GetDurabilityConfig(dto.Id, dto.LevelData.Level);
			return CRecharger.Existing(dto.Durability.LastTickTime, dto.Durability.CurrentAmount, durabilityConfig);
		}

		private CRecharger GetNewDurability(EFactory factory)
		{
			SRechargerLevelConfig durabilityConfig = _configs.GetDurabilityConfig(factory, 1);
			return CRecharger.Existing(0, durabilityConfig.MaxCapacity, durabilityConfig);
		}
	}
}