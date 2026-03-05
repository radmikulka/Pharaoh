// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.06.2024
// =========================================

using AldaEngine;
using UnityEngine;

namespace TycoonBuilder
{
	public class CWorldSpaceTutorialHighlightPoint : ITutorialHighlightPoint
	{
		private readonly Vector3 _worldPos;
		private readonly Camera _camera;
		public Vector3 Point => _camera.WorldToScreenPoint(_worldPos);
		public Vector2 Size => CVector2.Zero;

		public CWorldSpaceTutorialHighlightPoint(Vector3 worldPos, Camera camera)
		{
			_worldPos = worldPos;
			_camera = camera;
		}
	}
}