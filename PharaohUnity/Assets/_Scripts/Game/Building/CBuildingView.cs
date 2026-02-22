using UnityEngine;

namespace Pharaoh.Building
{
	public class CBuildingView : MonoBehaviour
	{
		public CBuilding Building { get; private set; }

		public void Initialize(CBuilding building)
		{
			Building = building;
		}
	}
}
