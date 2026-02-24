using Pharaoh.Map;
using ServerData;

namespace Pharaoh.Building
{
	public class CBuilding
	{
		public EBuildingId Id { get; }
		public CMapCell Cell { get; }
		public bool IsActive { get; set; }
		public int Level { get; set; } = 1;

		public CBuilding(EBuildingId id, CMapCell cell)
		{
			Id = id;
			Cell = cell;
			IsActive = true;
		}
	}
}
