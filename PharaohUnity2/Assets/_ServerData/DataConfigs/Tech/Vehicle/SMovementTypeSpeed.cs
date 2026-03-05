// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.08.2025
// =========================================

namespace ServerData
{
	public struct SMovementTypeSpeed
	{
		public readonly EMovementType MovementType;
		public readonly SVehicleSpeed Speed;

		public SMovementTypeSpeed(EMovementType movementType, SVehicleSpeed speed)
		{
			MovementType = movementType;
			Speed = speed;
		}
	}
}