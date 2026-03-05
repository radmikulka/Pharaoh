// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.08.2025
// =========================================

namespace ServerData
{
	public struct SVehicleSpeed
	{
		public readonly EMovementType MovementType;
		public readonly float MaxSpeed;
		public readonly float MaxSpeed2;
		public readonly float AccelerationDistance;
		public readonly float DecelerationDistance;

		public SVehicleSpeed(
			EMovementType movementType, 
			float maxSpeed, 
			float accelerationDistance, 
			float decelerationDistance
			)
		{
			MovementType = movementType;
			MaxSpeed = maxSpeed;
			MaxSpeed2 = maxSpeed * maxSpeed;
			AccelerationDistance = accelerationDistance;
			DecelerationDistance = decelerationDistance;
		}

		public SVehicleSpeed ModifyMaxSpeed(float mod)
		{
			return new SVehicleSpeed(MovementType, MaxSpeed * mod, AccelerationDistance, DecelerationDistance);
		}
		
		public SVehicleSpeed ModifyAccelerationDistance(float mod)
		{
			return new SVehicleSpeed(MovementType, MaxSpeed, AccelerationDistance * mod, DecelerationDistance);
		}
	}
}