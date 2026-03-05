// =========================================
// AUTHOR: Claude
// DATE:   04.03.2026
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CFleetSlotDto
	{
		[JsonProperty] public EMovementType MovementType { get; set; }
		[JsonProperty] public ETransportType TransportType { get; set; }

		public CFleetSlotDto()
		{
		}

		public CFleetSlotDto(EMovementType movementType, ETransportType transportType)
		{
			MovementType = movementType;
			TransportType = transportType;
		}
	}
}
