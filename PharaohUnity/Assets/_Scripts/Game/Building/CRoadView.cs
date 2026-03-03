using UnityEngine;

namespace Pharaoh.Building
{
	public class CRoadView : MonoBehaviour
	{
		private GameObject _prefabDeadEnd;
		private GameObject _prefabCorner;
		private GameObject _prefabStraight;
		private GameObject _prefabTJunction;
		private GameObject _prefabCross;

		private GameObject _currentVariant;

		public void Initialize(
			GameObject deadEnd,
			GameObject corner,
			GameObject straight,
			GameObject tJunction,
			GameObject cross)
		{
			_prefabDeadEnd   = deadEnd;
			_prefabCorner    = corner;
			_prefabStraight  = straight;
			_prefabTJunction = tJunction;
			_prefabCross     = cross;
		}

		public void RefreshVisual(int roadNeighborMask)
		{
			if (_currentVariant != null)
				Destroy(_currentVariant);

			SRoadVariant variant = CRoadVariantResolver.Resolve(roadNeighborMask);

			GameObject src = variant.Shape switch
			{
				ERoadShape.DeadEnd    => _prefabDeadEnd,
				ERoadShape.Straight   => _prefabStraight,
				ERoadShape.Corner     => _prefabCorner,
				ERoadShape.TJunction  => _prefabTJunction,
				_                     => _prefabCross,
			};

			if (src == null)
				return;

			_currentVariant = Instantiate(src, transform);
			_currentVariant.transform.localRotation = Quaternion.Euler(0f, variant.RotationY, 0f);
		}
	}
}
