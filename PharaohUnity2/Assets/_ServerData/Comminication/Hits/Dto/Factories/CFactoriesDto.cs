// =========================================
// AUTHOR: Radek Mikulka
// DATE:   04.09.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CFactoriesDto
	{
		[JsonProperty] public CFactoryDto[] Factores { get; set; }

		public CFactoriesDto()
		{
		}

		public CFactoriesDto(CFactoryDto[] factores)
		{
			Factores = factores;
		}
	}
}