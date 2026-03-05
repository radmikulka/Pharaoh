// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.12.2023
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public class CTutorialRectTarget : ITutorialGraphicsTarget
	{
		private readonly RectTransform _rectTransform;

		public CTutorialRectTarget(RectTransform rectTransform)
		{
			_rectTransform = rectTransform;
		}

		public Vector3 GetScreenPosition()
		{
			Vector2 rectCenter = _rectTransform.rect.center;
			Vector3 result = _rectTransform.TransformPoint(rectCenter);
			return result;
		}
	}
}