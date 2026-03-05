// =========================================
// AUTHOR: Radek Mikulka
// DATE:   17.10.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CModifiedFactoryRechargerDto : CRechargerDto
	{
		[JsonProperty] public EFactory FactoryId { get; set; }

		public CModifiedFactoryRechargerDto(
			EFactory factoryId,
			long lastTickTime,
			int currentAmount
		) : base(lastTickTime, currentAmount)
		{
			FactoryId = factoryId;
		}

		public override string ToString()
		{
			return $"{base.ToString()}, {nameof(FactoryId)}: {FactoryId}";
		}
	}
}