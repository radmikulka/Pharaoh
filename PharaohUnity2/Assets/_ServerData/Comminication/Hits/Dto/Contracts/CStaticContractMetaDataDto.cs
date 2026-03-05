// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.11.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class CStaticContractMetaDataDto : IMapAble
	{
		[JsonProperty] public EStaticContractId Id { get;  set; }
		[JsonProperty] public EDialogueId StoryDialogue { get;  set; }
		[JsonProperty] public int TotalTasksCount { get; set; }
		[JsonProperty] public bool IsActivated { get; set; }
		[JsonProperty] public bool AllowGoTo { get; set; }
		[JsonProperty] public int Task { get; set; }

		public CStaticContractMetaDataDto()
		{
		}

		public CStaticContractMetaDataDto(EStaticContractId id, bool isActivated, int task, int totalTasksCount, bool allowGoTo, EDialogueId storyDialogue)
		{
			TotalTasksCount  = totalTasksCount;
			StoryDialogue = storyDialogue;
			IsActivated = isActivated;
			AllowGoTo = allowGoTo;
			Task = task;
			Id = id;
		}
	}
}