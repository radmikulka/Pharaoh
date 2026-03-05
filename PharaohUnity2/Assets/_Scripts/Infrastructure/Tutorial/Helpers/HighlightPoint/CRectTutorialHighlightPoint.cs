// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.06.2024
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public class CRectTutorialHighlightPoint : ITutorialHighlightPoint
	{
		private readonly RectTransform _rectTransform;
		public Vector3 Point => _rectTransform.TransformPoint(_rectTransform.rect.center);
		public Vector2 Size => _rectTransform.rect.size;

		public CRectTutorialHighlightPoint(RectTransform rectTransform)
		{
			_rectTransform = rectTransform;
		}
	}
}