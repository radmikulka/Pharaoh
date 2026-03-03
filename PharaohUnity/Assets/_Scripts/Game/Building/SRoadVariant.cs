namespace Pharaoh.Building
{
	public readonly struct SRoadVariant
	{
		public readonly ERoadShape Shape;
		public readonly float RotationY;

		public SRoadVariant(ERoadShape shape, float rotationY)
		{
			Shape     = shape;
			RotationY = rotationY;
		}
	}
}
