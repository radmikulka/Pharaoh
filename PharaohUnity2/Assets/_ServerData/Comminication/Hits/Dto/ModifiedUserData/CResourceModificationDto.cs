// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.11.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CResourceModificationDto
	{
		[JsonProperty] public EModificationSource Source { get; set; }
		[JsonProperty] public string SourceDetail { get; set; }
		[JsonProperty] public EValuablePrice Price { get; set; }
		[JsonProperty] public SResource Resource { get; set; }
		[JsonProperty] public SResource OwnedValue { get; set; }

		public CResourceModificationDto(EModificationSource source, SResource resource, SResource ownedValue, string sourceDetail, EValuablePrice price)
		{
			Price = price;
			Source = source;
			Resource = resource;
			OwnedValue = ownedValue;
			SourceDetail = sourceDetail;
		}
	}
}