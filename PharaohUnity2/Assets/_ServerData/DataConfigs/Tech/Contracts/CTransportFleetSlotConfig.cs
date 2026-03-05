// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.02.2026
// =========================================

namespace ServerData
{
	public class CTransportFleetSlotConfig
	{
		public readonly EMovementType MovementType;
		public readonly ETransportType TransportType;

		public CTransportFleetSlotConfig(EMovementType movementType, ETransportType transportType)
		{
			MovementType = movementType;
			TransportType = transportType;
		}
	}
}
