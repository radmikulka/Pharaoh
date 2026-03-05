// =========================================
// AUTHOR: Radek Mikulka
// DATE:   04.09.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CFactoryDto
	{
		[JsonProperty] public CFactorySlotDto[] Slots { get; set; }
		[JsonProperty] public CRechargerDto Durability { get; set; }
		[JsonProperty] public CLevelDataDto LevelData { get; set; }
		[JsonProperty] public bool IsSeen { get; set; }
		[JsonProperty] public EFactory Id { get; set; }

		public CFactoryDto()
		{
		}

		public CFactoryDto(CFactorySlotDto[] slots, CRechargerDto durability, CLevelDataDto levelData, EFactory id, bool isSeen)
		{
			IsSeen = isSeen;
			Slots = slots;
			Durability = durability;
			LevelData = levelData;
			Id = id;
		}
	}
}