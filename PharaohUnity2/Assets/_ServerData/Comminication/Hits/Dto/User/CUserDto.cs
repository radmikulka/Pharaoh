// =========================================
// AUTHOR: Radek Mikulka
// DATE:   28.11.2024
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData
{
	public class CUserDto
	{
		[JsonProperty] public COwnedValuablesDto OwnedValuables { get; set; }
		[JsonProperty] public CProgressDto Progress { get; set; }
		[JsonProperty] public CAccountDto Account { get; set; }
		[JsonProperty] public COffersDto Offers { get; set; }

		public CUserDto(
	        COwnedValuablesDto ownedValuables,
	        CAccountDto account, 
	        CProgressDto progress, 
	        COffersDto offers
	        )
        {
	        OwnedValuables = ownedValuables;
	        Progress = progress;
	        Account = account;
	        Offers = offers;
        }
	}
}