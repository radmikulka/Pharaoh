// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.11.2023
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CProfileConflictingUserDto
	{
		[JsonProperty] public EYearMilestone Year { get; set; }
		[JsonProperty] public int SoftCurrency { get; set; }
		[JsonProperty] public int HardCurrency { get; set; }

		public CProfileConflictingUserDto()
		{
		}

		public CProfileConflictingUserDto(EYearMilestone year, int softCurrency, int hardCurrency)
		{
			Year = year;
			SoftCurrency = softCurrency;
			HardCurrency = hardCurrency;
		}
	}
}