using AldaEngine;
using UnityEngine;

namespace Pharaoh.Map
{
	public class CMapClickDetector : MonoBehaviour, IClickableItem
	{
		private CMapInstance _mapInstance;

		private static IEventBus _eventBus;

		public static void SetEventBus(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Initialize(CMapInstance mapInstance)
		{
			_mapInstance = mapInstance;
		}

		public void OnClicked(RaycastHit hit)
		{
			if (_eventBus == null || _mapInstance == null)
				return;

			int x = Mathf.RoundToInt(hit.point.x);
			int y = Mathf.RoundToInt(hit.point.z);

			if (!_mapInstance.IsValid(x, y))
				return;

			CMapCell cell = _mapInstance.GetCell(x, y);
			if (cell == null)
				return;

			_eventBus.Send(new CCellClickedSignal(new SCellCoord(x, y)));
		}
	}
}
