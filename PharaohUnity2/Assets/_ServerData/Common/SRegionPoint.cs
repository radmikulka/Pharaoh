// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.01.2026
// =========================================

using System;
using ServerData;

namespace ServerData
{
	[Serializable]
	public struct SRegionPoint : IEquatable<SRegionPoint>
	{
		public ERegionPoint PointId;
		public ERegion Region;

		public SRegionPoint(ERegionPoint pointId, ERegion region)
		{
			PointId = pointId;
			Region = region;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(PointId, Region);
		}

		public bool Equals(SRegionPoint other)
		{
			return GetHashCode() == other.GetHashCode();
		}

		public override string ToString()
		{
			return $"{Region} {PointId}";
		}
	}
}