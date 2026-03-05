// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.07.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CContractsDto
	{
		[JsonProperty] public CContractDto[] StoryContracts { get; set; }
		[JsonProperty] public CContractDto[] PassengerContracts { get; set; }
		[JsonProperty] public EStaticContractId[] CompletedStoryContracts { get; set; }

		public CContractsDto()
		{
		}

		public CContractsDto(
			CContractDto[] storyContracts,
			EStaticContractId[] completedStoryContracts,
			CContractDto[] passengerContracts
		)
		{
			StoryContracts = storyContracts;
			CompletedStoryContracts = completedStoryContracts;
			PassengerContracts = passengerContracts;
		}
	}
}
