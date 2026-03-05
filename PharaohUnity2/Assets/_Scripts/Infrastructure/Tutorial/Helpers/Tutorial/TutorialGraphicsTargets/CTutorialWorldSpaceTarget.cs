// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.12.2023
// =========================================

using AldaEngine;
using UnityEngine;

namespace TycoonBuilder
{
	public class CTutorialWorldSpaceTarget : ITutorialGraphicsTarget
	{
		private readonly Transform _worldSpaceTarget;
		private readonly IMainCameraProvider _cameraProvider;

		public CTutorialWorldSpaceTarget(IMainCameraProvider cameraProvider, Transform worldSpaceTarget)
		{
			_worldSpaceTarget = worldSpaceTarget;
			_cameraProvider = cameraProvider;
		}

		public Vector3 GetScreenPosition()
		{
			Vector3 position = _worldSpaceTarget.position;
			Vector3 result =  _cameraProvider.Camera.WorldToScreenPoint(position);
			return result;
		}
	}
}